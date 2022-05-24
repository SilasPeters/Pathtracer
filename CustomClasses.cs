using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace EpicRaytracer
{
	public struct Ray
	{
		public Vector3 EntryPoint    { get; set; }
		public Vector3 DirectionVect { get; set; }

		//private float t;  //todo: gebruik t om te bepalen of een intersection wel relevant is bij 100 objecten etc. dinges
							//todo: in intersection zetten?
		//float freq;

		public Ray(Vector3 entryPoint, Vector3 direction) {
			this.EntryPoint = entryPoint;
			this.DirectionVect = direction.Normalized();
		}

		public void Set(Vector3 entryPoint, Vector3 direction) { //todo: stelt niet alles in
			EntryPoint    = entryPoint;
			DirectionVect = direction;
		}

		public void SetDir(Vector3 dir) => DirectionVect = dir.Normalized();
	}
	
	public class LightSource
	{
		public Vector3 Pos       { get; }
		public float   Intensity { get; }
		public float   Freq      { get; }

		public LightSource(Vector3 pos, float intensity, float freq) {
			this.Pos       = pos;
			this.Intensity = intensity;
			this.Freq      = freq;
		}
	}
	
	#region Objects
	public abstract class Object
	{
		public Vector3 Pos                { get; }
		public Vector3 ReflectionConstant { get; }

		/// <param name="reflectionConstant">Determines how much of each RGB component is reflected and thus rendered. Each component must be 0..1</param>
		protected Object(Vector3 pos, Vector3 reflectionConstant) {
			Pos                = pos;
			ReflectionConstant = reflectionConstant;
		}

		public abstract bool TryIntersect(Ray ray, out IntersectionInfo ii);
		//todo: normal vector is always to the outside (REFRACTION)
	}
	public class Sphere : Object
	{
		public float Radius { get; }
		public Sphere(Vector3 pos, float radius, Vector3 reflectionConstant) : base(pos, reflectionConstant) {
			Radius = radius;
		}
		
		public override bool TryIntersect(Ray ray, out IntersectionInfo ii)
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
				float t = -b / (2 * a);

				Vector3 intPoint = ray.EntryPoint + ray.DirectionVect * t;
				Vector3 normal = (intPoint - Pos).Normalized();

				ii = new IntersectionInfo(intPoint, normal, t, this);
				return true;
			}
			else if (dis > 0)
			{
				double magicNumber = Math.Abs(-Math.Sqrt(dis)) < Math.Abs(Math.Sqrt(dis)) ? Math.Abs(-Math.Sqrt(dis)) : Math.Abs(Math.Sqrt(dis));
				float t = (float)(-b + magicNumber) / (2 * a);  // uitgaande van dat elke oplossing (met + of - wortel(d) positief is, gaat deze formule standaard voor de kleinste waarde.
																// wanneer oplossingen negatief worden (zoals bij verkeerde orientatie) zal dit het punt het verste weg van 'e' geven.
				Vector3 intPoint = ray.EntryPoint + ray.DirectionVect * t;
				Vector3 normal = (intPoint - Pos).Normalized();

				ii = new IntersectionInfo(intPoint, normal, t, this);
				return true;
			}
			ii = IntersectionInfo.None;
			return false;
		}
		public bool Contains(Vector3 point) => (point.X - Pos.X) * (point.X - Pos.X) + (point.Y - Pos.Y) * (point.Y - Pos.Y) + (point.Z - Pos.Z) * (point.Z - Pos.Z) <= Radius * Radius; //:)
	}
	
	public class Plane : Object
	{
		protected Vector3 Pos;
		protected Vector3 Normal;

		public Plane(Vector3 pos, Vector3 normal, Vector3 reflectionConstant) : base(pos, reflectionConstant) {
			Normal = normal;
			Pos    = pos;
		}

		public override bool TryIntersect(Ray ray, out IntersectionInfo ii)
		{
			/*float d = -Vector3.Dot(Normal, Pos);
			float bot = Vector3.Dot(Normal, ray.DirectionVect);

			if (bot != 0)
			{
				float t = (Vector3.Dot(Normal, ray.EntryPoint) + d)/bot;

				Vector3 intPoint = ray.EntryPoint + ray.DirectionVect * t;
				ii = new IntersectionInfo(intPoint, Normal);
				return true;
			}
			else
			{
				ii = IntersectionInfo.None;
				return false;
			}*/

			// assuming vectors are all normalized
			float denom = Vector3.Dot(Normal, ray.DirectionVect);
			if (denom > 1e-6)
			{
				Vector3 p0l0 = Pos - ray.EntryPoint;
				float t = Vector3.Dot(p0l0, Normal) / denom;
				ii = new IntersectionInfo(ray.EntryPoint + ray.DirectionVect * t, Normal, t, this); //todo: krom
				return t >= 0;
			}

			ii = IntersectionInfo.None;
			return false;
			
		}
	}
	public class FinPlane : Plane
	{
		Vector3 Min;
		Vector3 Max;
		public FinPlane(Vector3 pos, Vector3 normal, Vector3 min, Vector3 max, Vector3 reflectionConstant) : base(pos, normal, reflectionConstant) {
			this.Min = min;
			this.Max = max;
		}
		
		public override bool TryIntersect(Ray ray, out IntersectionInfo ii)
		{
			if(base.TryIntersect(ray, out ii))
			{
				if((ii.Point - Pos).LengthSquared <= 1 * 1)
				{
					//Console.WriteLine(t);
					return true;
				}
			}
			ii = IntersectionInfo.None;
			return false;
		}
	}
	#endregion Objects
	
	public static class Colors
	{
		public static int    Make(byte r, byte g, byte b) => (r << 16) | (g << 8) | b;
		public static int Make(Vector3 vec) {
			return Make(
				(byte)(Math.Min(vec.X, 1) * 255),
				(byte)(Math.Min(vec.Y, 1) * 255),
				(byte)(Math.Min(vec.Z, 1) * 255));
		}
		public static byte[] SplitRGB(int color) => new byte[] { (byte)(color >> 16), (byte)(color >> 8), (byte)color };
	}
	
	public class IntersectionInfo
	{
		public Vector3 Point  { get; }
		public Vector3 Normal { get; }
		public float   T      { get; }
		public Object  Object { get; }

		public const IntersectionInfo None = null;

		public IntersectionInfo(Vector3 point, Vector3 Normal, float t, Object obj)
		{
			this.Point  = point;
			this.Normal = Normal;
			this.T      = t;
			this.Object = obj;
		}

		public override string ToString() => $"IntPoint: {Point}, Normal: {Normal}";
	}
}
