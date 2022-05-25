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
		private static IList<Object> renderedObjects = new List<Object>();
		private static IList<LightSource> lightSources = new List<LightSource>();

		public static void AddObject(Object o) => renderedObjects.Add(o);
		public static void AddLight(LightSource o) => lightSources.Add(o);

		public static Vector3 GetColor(Ray ray)
		{
			if (TryIntersect(ray, out IntersectionInfo ii)) //intersection
			{
				return ii.Object.Color * getLighting(ii.Point);
			}
			else //no intersection
			{
				return Vector3.Zero;
			}
		}

		private static Vector3 getLighting(Vector3 point)
		{
			Vector3 lightColor = new Vector3(0.5f, 0.5f, 0.5f);

			Ray shadowray = new Ray(point, Vector3.Zero);
			foreach (var lightSource in lightSources)
			{
				float d = (lightSource.Pos - point).LengthFast;
				shadowray.SetDir(lightSource.Pos - point);
				if (TryIntersect(shadowray, out IntersectionInfo ii)) //intersection
				{
					if (!(ii.t > Raytracer.Epsilon && ii.t < d - Raytracer.Epsilon)) //incorrect intersection
						lightColor += lightSource.CalculateColor(ii.t);
				}
				else
				{
					lightColor += lightSource.CalculateColor(d);
				}
			}

			return lightColor;
		}

		public static bool TryIntersect(Ray ray, out IntersectionInfo iiiiiiiiiiiiiiiiiii)
		{
			foreach (Object obj in renderedObjects)
				if (obj.TryIntersect(ray, out iiiiiiiiiiiiiiiiiii))
					return true;

			iiiiiiiiiiiiiiiiiii = IntersectionInfo.None;
			return false;
		}
	}
	
	public struct Lens
	{
		private readonly Vector3 horizontal, vertical;
		private readonly Vector3 topLeft;
		
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

			Right = Vector3.Cross(Up, Front).Normalized();
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
					if (Scene.TryIntersect(viewRay, out IntersectionInfo ii))
					{
						Raytracer.Display.pixels[DisplayRegion.Left + x + (DisplayRegion.Top + y) * Raytracer.Display.width]
							= Colors.Make(Scene.GetColor(viewRay));
					}
				}
		}
	}

	public class DebugCamera : BasicCamera
	{
		public DebugCamera(Vector3 pos, Vector3 front, Vector3 up, Rectangle displayRegion, float lenseDistance)
			: base(pos, front, up, displayRegion, lenseDistance)
		{
		}

		public override void RenderImage()
		{
			Vector3 filter = Vector3.One - Front; //doe Math.Abs(Front)
			
			return;
			throw new NotImplementedException();
		}
	}
}