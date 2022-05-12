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
		public static Vector3 Pos		{ get; private set; } = Vector3.Zero;
		public static Vector3 Direction { get; private set; } = Vector3.UnitZ;
		public static float FOV			{ get; private set; } = 60;

		public static List<Object> RenderedObjects = new List<Object>();

		public static void Render(Template.Surface screen)
		{
			
		}
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
		public abstract bool TryIntersect(Ray ray, out int d);
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
		public override bool TryIntersect(Ray ray, out int d)
		{
			throw new NotImplementedException();
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
