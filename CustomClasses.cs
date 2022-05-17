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
		public Vector3 DirectionVect { get; }

		public Ray(Vector3 entryPoint, Vector3 direction)
		{
			this.EntryPoint = entryPoint;
			this.DirectionVect = direction;
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
		public static Vector3 Pos				{ get; private set; } = new Vector3(0, 0, -10);
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
		public abstract bool TryIntersect(Ray ray, out float t);

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
			Vector3 d = ray.DirectionVect;
			Vector3 p = Pos;

			float a = d.X*d.X + d.Y*d.Y + d.Z*d.Z;
		
			float b = 2 * (d.X * (e.X - p.X) + d.Y * (e.Y - p.Y) + d.Z * (e.Z - p.Z));
			float c = e.X*(e.X -2*p.X) + p.X *p.X + e.Y  * (e.Y  - 2 * p.Y) + p.Y * p.Y + e.Z * (e.Z - 2 * p.Z) + p.Z * p.Z -Radius*Radius;//p.X * (p.X - 2 * e.X) + p.Y * (p.Y - 2 * e.Y) + p.Z * (p.Z - 2 * e.Z) - Radius * Radius;
			float dis = b * b - (4 * a * c);
			
			if (dis == 0)
			{
				//t = -b / 6
				t = -b / (2 * a);
				return true;
			}
			else if (dis > 0)
			{
				//t = (float)(Math.Min((-b + Math.Sqrt(D)) / 6, (-b - Math.Sqrt(D)) / 6));
				t = (float)(-b - Math.Sqrt(dis)) / (2 * a);	// uitgaande van dat elke oplossing (met + of - wortel(d) positief is, gaat deze formule standaard voor de kleinste waarde.
															// wanneer oplossingen negatief worden (zoals bij verkeerde orientatie) zal dit het punt het verste weg van 'e' geven.
				return true;
			}
			t = 0; 
			return false;
		}
		public bool Contains(Vector3 point) => (point.X - Pos.X) * (point.X - Pos.X) + (point.Y - Pos.Y) * (point.Y - Pos.Y) + (point.Z - Pos.Z) * (point.Z - Pos.Z) <= Radius * Radius; //:)
	}
	public class Point : Object
    {
		public Vector3 Pos { get; }
		public Point(Vector3 pos)
        {
			this.Pos = pos;
        }

        public override bool TryIntersect(Ray ray, out float t)
        {
			float xLamda = ray.DdirectionVect.X / (Pos.X - ray.EntryPoint.X);
			float yLamda = ray.DdirectionVect.Y / (Pos.Y - ray.EntryPoint.Y);
			float zLamda = ray.DdirectionVect.Z / (Pos.Z - ray.EntryPoint.Z);
			t = 0;
			return (xLamda == yLamda &&  yLamda == zLamda);
        }

        public override bool Contains(Vector3 point)
        {
            throw new NotImplementedException();
        }
    }
	//public class Cube : Object
	//{
	//	//are we doing this?
	//}
	public class Plane : Object
    {
		private Vector3 Normal;
		private Vector3 Pos;
        public Plane(Vector3 normal, Vector3 pos)
        {
			this.Normal = normal;
			this.Pos = pos;
        } 
        public override bool TryIntersect(Ray ray, out float t)
        {
			Vector3 e = ray.EntryPoint; 
			Vector3 dir = ray.DirectionVect; 
			float d = -1*(Normal.X * Pos.X+ Normal.Y * Pos.Y + Normal.Z * Pos.Z);
			float bot = (Normal.X * dir.X + Normal.Y * dir.Y + Normal.Z * dir.Z);
			if(bot!= 0)
            {
				t = (Normal.X * e.X + Normal.Y *e.Y + Normal.Z * e.Z +d)/bot;
				return true;
			}

			else
            {
				t = 0;
				return false;
            }
		}
    }
	#endregion Objects
	#region Colors
	public static class Colors
	{
		public static int Blend(byte r, byte g, byte b) => (r << 16) | (g << 8) | b;
		public static byte[] SplitRGB(int color) => new byte[] { (byte)(color >> 16), (byte)(color >> 8), (byte)color };
	}
	#endregion Colors
}
