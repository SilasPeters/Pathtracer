﻿using System;
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

		public Ray(Vector3 entryPoint, Vector3 direction) {
			this.EntryPoint    = entryPoint;
			this.DirectionVect = direction.Normalized();
		}

		public void    SetDir(Vector3 dir)     => DirectionVect = dir.Normalized();
		public void    SetPoint(Vector3 point) => EntryPoint = point;
		public Vector3 GetPoint(float t)       => EntryPoint + t * DirectionVect;
	}
	
	public class LightSource
	{
		public Vector3 Pos    { get; }
		public float   Radius { get; }
		public Vector3 Color  { get; }

		/// <param name="radius">Only used for the debugcam</param>
		public LightSource(Vector3 pos, Vector3 color, float radius = 0.2f) {	
			this.Pos    = pos;
			this.Color  = color * 30; //todo
			this.Radius = radius;
		}
	}
	
	#region Objects
	public abstract class Object
	{
		public Vector3  Pos { get; protected set; }
		public Material Mat { get; }

		protected Object(Vector3 pos, Material material) {
			Pos = pos;
			Mat = material;
		}

		public abstract bool    TryIntersect(Ray ray, out IntersectionInfo ii);
		public abstract Vector3 GetNormalAt(Vector3 pointOnObject);
	}
	public class Sphere : Object
	{
		public float Radius { get; protected set; }
		public Sphere(Vector3 pos, float radius, Material material) : base(pos, material) {
			Radius = radius;
		}
		
		public override bool TryIntersect(Ray ray, out IntersectionInfo ii)
		{
			// source: lecture notes
			Vector3 c  = Pos - ray.EntryPoint;
			float   t  = Vector3.Dot(c, ray.DirectionVect);
			Vector3 q  = c - t * ray.DirectionVect;
			float   p2 = q.LengthSquared;
			if (p2 > Radius * Radius)
			{
				ii = new IntersectionInfo(ray, t, this);
				return false;
			}
			t -= (float)Math.Sqrt(Radius * Radius - p2);

			ii = new IntersectionInfo(ray, t, this);
			return true;
		}

		public override Vector3 GetNormalAt(Vector3 pointOnObject) => (pointOnObject - Pos) / Radius; //accurate enough for normalization

		public bool Contains(Vector3 point) => (point.X - Pos.X) * (point.X - Pos.X) + (point.Y - Pos.Y) * (point.Y - Pos.Y) + (point.Z - Pos.Z) * (point.Z - Pos.Z) <= Radius * Radius; //:)
	}
	
	public class Plane : Object
	{
		public Vector3 Normal { get; protected set; }

		public Plane(Vector3 pos, Vector3 normal, Material material) : base(pos, material) {
			Normal = normal.Normalized();
		}

		public override bool TryIntersect(Ray ray, out IntersectionInfo ii)
		{
			/// Source: https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection
			float denom = Vector3.Dot(ray.DirectionVect, Normal);
			if (denom > 1e-6)
			{
				Vector3 p0l0 = Pos - ray.EntryPoint;
				float t = Vector3.Dot(p0l0, Normal) / denom;
				ii = new IntersectionInfo(ray, t, this);
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
		public Quad(Vector3 pos, Vector3 normal, Vector3 min, Vector3 max, Material material) : base(pos, normal, material) {
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
		public static byte[]  SplitRGB(int c)  => new byte[]{ GetR(c), GetG(c), GetB(c) };
		public static byte    GetR(int color)  => (byte)(color >> 16);
		public static byte    GetG(int color)  => (byte)(color >> 8 );
		public static byte    GetB(int color)  => (byte)(color      );
	}

	public class IntersectionInfo //todo: make this a struct. This might prevents issues when passing this as an
								  //argument in recursion when afterwards using this in the same context. (As a reference
								  //type, values might change in deeper recursion levels affecting all levels as a result.)
								  //Yet, all items are readonly anyway so this is pratically a struct.
	{
		public Ray     IntersectedRay { get; }
		public Object  Object         { get; }
		public Vector3 Point          { get; }
		public Vector3 Normal         { get; }
		public float   t              { get; }

		public const IntersectionInfo None = null;

		public IntersectionInfo(Ray intersectedRay, float t, Object obj)
		{
			this.IntersectedRay = intersectedRay;
			this.Object         = obj;
			this.t              = t;
			
			this.Point          = intersectedRay.GetPoint(t);
			if(obj != null)
				this.Normal         = obj.GetNormalAt(Point);
		}
	}

	public struct Material
	{
		public readonly string Type;
		public readonly Vector3 SpecularCo;
		public readonly Vector3 DiffuseCo;
		public readonly Vector3 AmbientCo;
		public readonly float N;
		
		public Material(Vector3 diffuseCo, Vector3 specularCo, Vector3 ambientCo, string type = "Normal", float n = 0) {
			SpecularCo      = specularCo;
			DiffuseCo       = diffuseCo;
			AmbientCo       = ambientCo;
			Type            = type;
			N = n;
		}
	}
}
