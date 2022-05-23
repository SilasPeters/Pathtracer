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
	public static class Raytracer
	{
		private static Scene _scene;
		
		public static void Set(Scene scene) {
			_scene       = scene;
		}

		public static void RenderImage(BasicCamera[] cams)
		{
			foreach (var cam in cams)
			{
				cam.RenderImage(_scene);
			}
		}
	}

	public class Scene
	{
		private IList<Object> renderedObjects = new List<Object>();
		private IList<LightSource> lightSources = new List<LightSource>();

		public void AddObject(Object o) => renderedObjects.Add(o);
		public void AddLight(LightSource o) => lightSources.Add(o);

		public bool TryIntersect(Ray ray, out IntersectionInfo iiiiiiiiiiiiiiiiiii)
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

			Right = Vector3.Cross(front, up);
			Lens  = new Lens((float)DisplayRegion.Width / DisplayRegion.Height);
			HalfW = DisplayRegion.Width  >> 1;
			HalfH = DisplayRegion.Height >> 1;
		}

		/// <summary>Updates everything so that the camera now renders its content to other pixels</summary>
		public void SetDisplayRegion(Rectangle displayRegion) {
			DisplayRegion = DisplayRegion;
			Lens          = new Lens((float)DisplayRegion.Width / DisplayRegion.Height);
		}
		public abstract void RenderImage(Scene scene);

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

		public override void RenderImage(Scene scene)
		{
			ViewRay viewRay = new ViewRay(Pos, Vector3.Zero);
			
			for (int y = DisplayRegion.Top; y < DisplayRegion.Bottom; y++)
				for (int x = DisplayRegion.Left; x < DisplayRegion.Right; x++)
				{
					viewRay.DirectionVect =
						Lens.HalfRight * -(HalfW - (x - DisplayRegion.Left)) / HalfW +
						Lens.HalfUp    *  (HalfH - (y - DisplayRegion.Top )) / HalfH +
						Vector3.UnitZ;

					if (scene.TryIntersect(viewRay, out IntersectionInfo ii))
					{
						if (scene.isLit(ii, out IntersectionInfo iii))
						{
							display.pixels[x + y * display.width] = 0x00ff00;
						}
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

		public override void RenderImage(Scene scene)
		{
			return;
			throw new NotImplementedException();
		}
	}
}