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
		//todo: lens (een object maken zodat hij) een afstand kan hebben tot de camera etc.
		public Vector3 HalfRight { get; }
		public Vector3 HalfUp { get; }
		
		/// <summary>Creates a lens (formerly known as screen) which has a height of 1 and a width of 1 * 'AspectRatio' (both multiplied by 'scale')</summary>
		public Lens(float AspectRatio, float scale = 1f)
		{
			HalfRight = Vector3.UnitX * scale * AspectRatio;
			HalfUp    = Vector3.UnitY * scale;
		}
	}

	public abstract class BasicCamera
	{
		public Vector3   Pos           { get; protected set; }
		public Vector3   Front         { get; protected set; }
		public Vector3   Up            { get; protected set; }
		public Vector3   Right         { get; protected set; }
		public Rectangle DisplayRegion { get; protected set; }
		public Lens      Lens          { get; protected set; }

		protected readonly float HalfW;
		protected readonly float HalfH;
		
		public BasicCamera(Vector3 pos, Vector3 front, Vector3 up, Rectangle displayRegion) { //todo: ask for a scale for the lens
			Pos           = pos;
			Front         = front;
			Up            = up;
			DisplayRegion = displayRegion;

			Right = -Vector3.Cross(front, up);
			Lens  = new Lens((float)DisplayRegion.Width / DisplayRegion.Height);
			HalfW = DisplayRegion.Width  >> 1;
			HalfH = DisplayRegion.Height >> 1;
		}

		/// <summary>Updates everything so that the camera now renders its content to other pixels</summary>
		public void SetDisplayRegion(Rectangle displayRegion) {
			DisplayRegion = DisplayRegion;
			Lens          = new Lens((float)DisplayRegion.Width / DisplayRegion.Height);
		}
		public abstract void RenderImage();

		public void Translate(Vector3 movement)          => Pos += movement;
		public void Translate(float x, float y, float z) => Pos += new Vector3(x, y, z);
		public void Rotate(Vector3 rotation)             => Front += rotation;
		public void Rotate(float x, float y, float z)    => Front += new Vector3(x, y, z);
	}
	public class MainCamera : BasicCamera
	{
		public MainCamera(Vector3 pos, Vector3 front, Vector3 up, Rectangle displayRegion)
			: base(pos, front, up, displayRegion)
		{
		}

		public override void RenderImage()
		{
			Ray viewRay = new Ray(Pos, Vector3.Zero);
			
			for (int y = DisplayRegion.Top; y < DisplayRegion.Bottom; y++)
				for (int x = DisplayRegion.Left; x < DisplayRegion.Right; x++)
				{
					viewRay.SetDir(
						Right * Lens.HalfRight * -(HalfW - (x - DisplayRegion.Left)) / HalfW +
						Up * Lens.HalfUp * (HalfH - (y - DisplayRegion.Top)) / HalfH +
						Front);

					if (Scene.TryIntersect(viewRay, out IntersectionInfo ii))
					{
						Raytracer.Display.pixels[x + y * Raytracer.Display.width] = Colors.Make(Scene.GetColor(viewRay));
					}
				}
		}
	}

	public class DebugCamera : BasicCamera
	{
		public DebugCamera(Vector3 pos, Vector3 front, Vector3 up, Rectangle displayRegion)
			: base(pos, front, up, displayRegion)
		{
		}

		public override void RenderImage()
		{
			return;
			throw new NotImplementedException();
		}
	}
}