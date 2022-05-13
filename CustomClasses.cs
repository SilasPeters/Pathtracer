using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Raytracer
{
	#region Rays
	public abstract class Ray
	{
		public Vector3 EntryPoint { get; }
		public Vector3 DdirectionVect { get; }

		public Ray(Vector3 entryPoint, Vector3 direction)
		{
			this.EntryPoint = entryPoint;
			this.DdirectionVect = direction;
		}
	}

	public class LightRay : Ray
	{
		float freq;

		public LightRay(Vector3 entryPoint, Vector3 direction) : base(entryPoint, direction)
		{

		}
	}
	public class ViewRay : Ray
	{
		public ViewRay(Vector3 entryPoint, Vector3 direction) : base(entryPoint, direction)
		{

		}
	}
	public class ShadowRay : Ray
	{
		public ShadowRay(Vector3 entryPoint, Vector3 direction) : base(entryPoint, direction)
		{

		}
	}

	#endregion Rays
	#region Lights
	public class LightSource
	{
		public Vector3 Pos { get; }
		public float Intensity { get; }
		public float Freq { get; }

		public LightSource(Vector3 pos, float intensity, float freq)
		{
			this.Pos = pos;
			this.Intensity = intensity;
			this.Freq = freq;
		}
	}
	#endregion Lights
	#region Cameraworks
	public static class Camera
	{
		public static Vector3 Pos				{ get; private set; } = new Vector3(0, 5, -10);
		public static Vector3 Direction			{ get; private set; } = Vector3.UnitX;
		public static Vector3 ScreenRelativePos { get; private set; } = new Vector3(0, 0, 5);
		public static float FOV					{ get; private set; } = 60;

		public static List<Object> RenderedObjects = new List<Object>();

		public static void Render(Template.Surface screen)
		{
			for (int y = 0; y < screen.height; y++)
			{
				for (int x = 0; x < screen.width; x++)
				{
					Vector3 angle = new Vector3(ScreenToObjX(x, screen),
												ScreenToObjY(y, screen),
												ScreenRelativePos.Z
					);
					ViewRay ray = new ViewRay(Pos, angle);

					foreach (var obj in RenderedObjects)
						if (obj.TryIntersect(ray, out float t))
							screen.pixels[x + y * screen.width] = Colors.Blend(0, (byte)(255 - t * 60), 0);
				}
			}
		}

		///<summary>Methode voor het omzetten van object-space naar screenspace coordinaten</summary>
		private static int ObjToScreenX(float x, int screenWidth, float centerOffset = 0) => (int)(screenWidth / 2 * (x + 1 + centerOffset));
		///<summary>Methode voor het omzetten van object-space naar screenspace coordinaten</summary>
		private static int ObjToScreenY(float y, int screenWidth, float centerOffset = 0) => ObjToScreenX(-y, screenWidth, centerOffset);
		///<summary>Methode voor het omzetten van screencoordinates naar object-space coordinaten</summary>
		private static float ScreenToObjX(int x, Template.Surface screen) => (float)x / screen.width * 2 - 1f;
		///<summary>Methode voor het omzetten van screencoordinates naar object-space coordinaten</summary>
		private static float ScreenToObjY(int y, Template.Surface screen) => (float)y / screen.height * 2 - 1f;
		public static void Set(Vector3 pos, Vector3 direction, float fov = 60) {
			Pos = pos;
			Direction = direction;
			FOV = fov;
		}
		public static void Translate(Vector3 movement) => Pos += movement;
		public static void Rotate(Vector3 rotation) => Direction += rotation;
	}
	#endregion Cameraworks
	#region Objects
	public abstract class Object
	{
		public Object()
		{

		}
		public abstract bool TryIntersect(Ray ray, out float d);
		public abstract bool Contains(Vector3 point);
	}
	public class Sphere : Object
	{
		public Vector3 Pos { get; }
		public float Radius { get; }
		/// <summary>Determines how much of each RGB component is reflected and thus rendered. Each component must be 0..1</summary>
		public Vector3 ReflectionConstant { get; }

		/// <param name="reflectionConstant">Determines how much of each RGB component is reflected and thus rendered. Each component must be 0..1</param>
		public Sphere(Vector3 pos, float radius, Vector3 reflectionConstant)
		{
			this.Pos = pos;
			this.Radius = radius;
			this.ReflectionConstant = reflectionConstant;
		}
		public override bool TryIntersect(Ray ray, out float t)
		{
			Vector3 e = ray.EntryPoint;
			Vector3 d = ray.DdirectionVect;
			Vector3 p = Pos;

			float b = 2 * (d.X * (1 + e.X - p.X) + d.Y * (1 + e.Y - p.Y) + d.Z * (1 + e.Z - p.Z));
			float dis = b * b - 12 * (p.X * (p.X - 2 * e.X) + p.Y * (p.Y - 2 * e.Y) + p.Z * (p.Z - 2 * e.Z) +e.X * e.X + d.X *d.X + e.Y * e.Y +d.Y * d.Y +e.Z *e.Z + d.Z *d.Z -Radius * Radius);
			//t = 0;
			//return true;
			
			if (dis == 0)
			{
				t = -b / 6;
				return true;
			}
			else if(dis > 0)
			{
				t = (float)(Math.Min((-b + Math.Sqrt(dis)) / 6, (-b - Math.Sqrt(dis)) / 6));
				return true;
			}
			t = 0; 
			return false;
		}
		public override bool Contains(Vector3 point) => (point.X - Pos.X) * (point.X - Pos.X) + (point.Y - Pos.Y) * (point.Y - Pos.Y) + (point.Z - Pos.Z) * (point.Z - Pos.Z) <= Radius * Radius; //:)
	}
	//public class Cube : Object
	//{
	//	//are we doing this?
	//}
	#endregion Objects
	#region Colors
	public static class Colors
	{
		public static int Blend(byte r, byte g, byte b) => (r << 16) | (g << 8) | b;
		public static byte[] SplitRGB(int color) => new byte[] { (byte)(color >> 16), (byte)(color >> 8), (byte)color };
	}
	#endregion Colors
}
