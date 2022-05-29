using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Template;

namespace EpicRaytracer
{
	public static class Scene
	{
		public static IList<Object> renderedObjects = new List<Object>();
		public static IList<LightSource> lightSources = new List<LightSource>();

		public static void AddObject(Object o) => renderedObjects.Add(o);
		public static void AddLight(LightSource o) => lightSources.Add(o);

		public static Vector3 CalculatePixel(Ray viewRay, Vector3 camPos, int layers)
		{
			Vector3 color = Vector3.Zero;
			if (TryIntersect(viewRay, out IntersectionInfo ii)) //we hit an object
			{
				//if (mirror) > return recursie met gespiegeld langs de Normal
				if (ii.Object.Mat.IsMirror && layers<10) 
				{
					
					Vector3 V = camPos - ii.Point;
					Vector3 R = V - 2*(Vector3.Dot(V.Normalized(),ii.Normal) * ii.Normal);
					viewRay.SetDir(R.Normalized());
					viewRay.SetPoint(ii.Point);
					return ii.Object.Mat.DiffuseCo * CalculatePixel(viewRay, camPos, ++layers); ;
				}

				foreach (LightSource ls in lightSources)
				{
					Vector3 toLight = ls.Pos - ii.Point;
					if (!TryIntersect(new Ray(ii.Point, toLight.Normalized()), out IntersectionInfo iii, ii.Object)) //not lit
					{
						Vector3 V = (camPos - ii.Point).Normalized();
						Vector3 R = (toLight - 2 * Vector3.Dot(toLight.Normalized(), ii.Normal) * ii.Normal).Normalized();
						color += new Vector3(1 / toLight.LengthSquared * ls.Color *
						                     (ii.Object.Mat.DiffuseCo * Math.Max(0,
							                      Vector3.Dot(ii.Normal, toLight.Normalized()))
						                      + ii.Object.Mat.SpecularCo *
						                      (float)Math.Pow(Math.Max(0, Vector3.Dot(V, R)), Raytracer.Glossyness)
						                     ));
					}
				}
				color += Raytracer.AmbientLightLevel * ii.Object.Mat.DiffuseCo;
			}
			return color;
		}

		public static bool TryIntersect(Ray ray, out IntersectionInfo iiiiiiiiiiiiiiiiiii, Object self = null)
		{
			float  t = float.MaxValue;
			Object o = null;
			
			foreach (Object obj in renderedObjects)
				if (obj != self && obj.TryIntersect(ray, out iiiiiiiiiiiiiiiiiii) && iiiiiiiiiiiiiiiiiii.t < t) {
					t = iiiiiiiiiiiiiiiiiii.t;
					o = obj;
				}

			iiiiiiiiiiiiiiiiiii = new IntersectionInfo(ray, t, o);
			return o != null;
		}
	}
	
	public struct Lens
	{
		public readonly Vector3 horizontal, vertical;
		public readonly Vector3 topLeft;
		
		public Lens(BasicCamera cam, float distance, float height = 1f) {
			var aspectRatio = (float)cam.DisplayRegion.Width / cam.DisplayRegion.Height;
			horizontal = cam.Right * height * aspectRatio;
			vertical   = cam.Up * height;
			topLeft    = cam.Front * distance - horizontal / 2 + vertical / 2;
		}

		public Vector3 GetDirToPixel(float xPercentage, float yPercentage) => topLeft + horizontal * xPercentage - vertical * yPercentage;
	}

	public abstract class BasicCamera
	{
		public Vector3   Pos           { get; protected set; }
		public Vector3   Front         { get; protected set; }
		public Vector3   Up            { get; protected set; }
		public Vector3   Right         { get; protected set; }
		public Rectangle DisplayRegion { get; protected set; }
		public Lens      Lens          { get; protected set; }

		public BasicCamera(Vector3 pos, Vector3 front, Vector3 up, Rectangle displayRegion, float lensDistance) {
			Pos           = pos;
			Front         = front.Normalized();
			Up            = up.Normalized();
			DisplayRegion = displayRegion;

			Right = Vector3.Cross(Up, Front).Normalized(); //big brain
			Lens  = new Lens(this, lensDistance);
		}
		
		public abstract void RenderImage();

		public void Translate(Vector3 movement)          => Pos += movement;
		public void Translate(float x, float y, float z) => Pos += new Vector3(x, y, z);
		public void Rotate(Vector3 rotation)             => Front += rotation;
		public void Rotate(float x, float y, float z)    => Front += new Vector3(x, y, z);
	}
	public class MainCamera : BasicCamera
	{
		public MainCamera(Vector3 pos, Vector3 front, Vector3 up, Rectangle displayRegion, float lensDistance)
			: base(pos, front, up, displayRegion, lensDistance)
		{
		}

		public override void RenderImage()
		{
			Ray viewRay = new Ray(Pos, Vector3.Zero);
			
			for (int y = 0; y < DisplayRegion.Height; y++)
				for (int x = 0; x < DisplayRegion.Width; x++)
				{
					viewRay.SetDir(Lens.GetDirToPixel((float)x / (DisplayRegion.Width - 1),
													  (float)y / (DisplayRegion.Height - 1)));
						Raytracer.Display.pixels[DisplayRegion.Left + x + (DisplayRegion.Top + y) * Raytracer.Display.width]
							= Colors.Make(Scene.CalculatePixel(viewRay, Pos, 0));
				}
		}
	}

	public class DebugCamera : BasicCamera
	{
		public override void RenderImage()
		{
			float   scale        = 12;
			float   numberOfRays = 20;
			int     colorViewray = 0xff00ff;
			int     colorSphere  = 0xffffff;
			int     colorCam	 = 0xffff00;
			int     colorScreen	 = 0xffffff;
			Vector2 objectSize   = new Vector2(scale, scale / DisplayRegion.Width / DisplayRegion.Height);

			MainCamera cam    = (MainCamera)Raytracer._cameraStances[Raytracer._currentCamStance][0];
			Point      camPos = To2D(cam.Pos);
			for (int i = 0; i < numberOfRays; i++)
			{
				float t   = 200 * scale; //always out of bounds
				Ray   ray = new Ray(cam.Pos, cam.Lens.GetDirToPixel(i / (numberOfRays - 1), 0.5f));
				if (Scene.TryIntersect(ray, out IntersectionInfo ii))
					t = ii.t;

				Point iPos = To2D(ray.GetPoint(t));
				Raytracer.Display.Line(camPos.X, camPos.Y, iPos.X, iPos.Y, colorViewray);
			}
			Raytracer.Display.Box(camPos.X - 1, camPos.Y - 1, camPos.X, camPos.Y, colorCam);
			
			foreach (Object o in Scene.renderedObjects)
			{
				if (o is Sphere s)
				{
					for (float d = 0; d < 1; d += 1f/360)
					{
						Vector3 pos = s.Pos + new Vector3(
							(float)(s.Radius * Math.Cos(d * 2 * Math.PI)), 0,
							(float)(s.Radius * Math.Sin(d * 2 * Math.PI)));
						
						Draw(To2D(pos), colorSphere);
					}

				}
			}

			Point screenLeft = To2D(cam.Lens.topLeft + cam.Pos);
			Point screenRight = To2D(cam.Lens.topLeft + cam.Lens.horizontal + cam.Pos);
			Raytracer.Display.Line(screenLeft.X, screenLeft.Y, screenRight.X, screenRight.Y, colorScreen);

			Point To2D(Vector3 objectPos)
			{
				Point p = new Point((int)(objectPos.X / scale * DisplayRegion.Height),
					-(int)(objectPos.Z / scale * DisplayRegion.Height));
				return p + new Size(DisplayRegion.Width/2 + DisplayRegion.Left, DisplayRegion.Height/2 + DisplayRegion.Top);
			}
			
			void Draw(Point pos, int c) => Raytracer.Display.pixels[pos.X + pos.Y * Raytracer.Display.width] = c;
		}

		public DebugCamera(Vector3 pos, Vector3 front, Vector3 up, Rectangle displayRegion, float lenseDistance)
			: base(pos, front, up, displayRegion, lenseDistance)
		{
		}
	}
}