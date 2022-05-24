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

		//float freq;

		public Ray(Vector3 entryPoint, Vector3 direction) {
			this.EntryPoint    = entryPoint;
			this.DirectionVect = direction.Normalized();
		}

		public void SetDir(Vector3 dir) => DirectionVect = dir.Normalized();
	}
	
	public class LightSource
	{
		public Vector3 Pos       { get; }
		public float   Radius    { get; }
		public float   Freq      { get; }
		
		private readonly Vector3 Color;

		/// <param name="radius">Only used for the debugcam</param>
		public LightSource(Vector3 pos, Vector3 color, float freq, float radius = 1) {	
			this.Pos    = pos;
			this.Color  = color;
			this.Freq   = freq;
			this.Radius = radius;
		}

		/// <summary>Returns the color that is dimmed based on distance 't'</summary>
		public Vector3 CalculateColor(float t)
		{
			//todo: moet worden bijgewerkt zodat afstand een factor is op intensity, maar dit gaat gepaard met andere
			//changes dus ik laat dit voor nu

			return Color;
		}
	}
	
	#region Objects
	public abstract class Object
	{
		public Vector3 Pos   { get; protected set; }
		public Vector3 Color { get; protected set; }

		protected Object(Vector3 pos, Vector3 color) {
			Pos   = pos;
			Color = color;
		}

		public abstract bool    TryIntersect(Ray ray, out IntersectionInfo ii);
		public abstract Vector3 GetNormalAt(Vector3 pointOnObject);
		//todo: normal vector is always to the outside [REFRACTION] - wat bedoelden we hier mee? Wanneer was dit van toepassing?
	}
	public class Sphere : Object
	{
		public float Radius { get; protected set; }
		public Sphere(Vector3 pos, float radius, Vector3 color) : base(pos, color) {
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
				Vector3 normal   = GetNormalAt(intPoint);

				ii = new IntersectionInfo(intPoint, normal, t, this);
				return true;
			}
			else if (dis > 0)
			{
				double magicNumber = Math.Abs(-Math.Sqrt(dis)) < Math.Abs(Math.Sqrt(dis)) ? Math.Abs(-Math.Sqrt(dis)) : Math.Abs(Math.Sqrt(dis));
				float t = (float)(-b + magicNumber) / (2 * a);  // uitgaande van dat elke oplossing (met + of - wortel(d) positief is, gaat deze formule standaard voor de kleinste waarde.
																// wanneer oplossingen negatief worden (zoals bij verkeerde orientatie) zal dit het punt het verste weg van 'e' geven.
				Vector3 intPoint = ray.EntryPoint + ray.DirectionVect * t;
				Vector3 normal   = GetNormalAt(intPoint);

				ii = new IntersectionInfo(intPoint, normal, t, this);
				return true;
			}
			ii = IntersectionInfo.None;
			return false;
		}

		public override Vector3 GetNormalAt(Vector3 pointOnObject) => (pointOnObject - Pos) / Radius; //accurate enough

		public bool Contains(Vector3 point) => (point.X - Pos.X) * (point.X - Pos.X) + (point.Y - Pos.Y) * (point.Y - Pos.Y) + (point.Z - Pos.Z) * (point.Z - Pos.Z) <= Radius * Radius; //:)
	}
	
	public class Plane : Object
	{
		public Vector3 Normal { get; protected set; }

		public Plane(Vector3 pos, Vector3 normal, Vector3 color) : base(pos, color) {
			Normal = normal.Normalized();
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

		public override Vector3 GetNormalAt(Vector3 pointOnObject) => Normal;
	}
	public class Quad : Plane
	{
		public Vector3 Min { get; protected set;} //todo: Shouldn't we ask for vertexes? Otherwise a quad is always square
		public Vector3 Max { get; protected set;}
		public Quad(Vector3 pos, Vector3 normal, Vector3 min, Vector3 max, Vector3 color) : base(pos, normal, color) {
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
		public static int Make(byte r, byte g, byte b) => (r << 16) | (g << 8) | b;
		public static int Make(Vector3 vec) {
			return Make((byte)(Math.Min(vec.X, 1) * 255), //todo: assume (safely...) that the values won't clip beyond 1
						(byte)(Math.Min(vec.Y, 1) * 255),
						(byte)(Math.Min(vec.Z, 1) * 255));
		}

		public static Vector3 GetVector(int c) => new Vector3(GetR(c), GetB(c), GetB(c));
		public static byte[]  SplitRGB(int c)  => new byte[] { GetR(c), GetG(c), GetB(c) };
		public static byte    GetR(int color)  => (byte)(color >> 16);
		public static byte    GetG(int color)  => (byte)(color >> 8 );
		public static byte    GetB(int color)  => (byte)(color      );
	}

	public class IntersectionInfo //todo: make this a struct. This might prevents issues when passing this as an
								  //argument in recursion when afterwards using this in the same context. (As a reference
								  //type, values might change in deeper recursion levels affecting all levels as a result.)
								  //Yet, all items are readonly anyway so this is pratically a struct.
	{
		public Vector3 Point  { get; }
		public Vector3 Normal { get; }
		public float   t      { get; } //todo: gebruik t om te bepalen of een intersection wel relevant is bij 100 objecten etc. dinges
		public Object  Object { get; }

		public const IntersectionInfo None = null;

		public IntersectionInfo(Vector3 point, Vector3 normal, float t, Object obj)
		{
			this.Point  = point;
			this.Normal = normal;
			this.t      = t;
			this.Object = obj;
		}

		
		public override string ToString() => $"IntPoint: {Point}, Normal: {Normal}";
	}
}
