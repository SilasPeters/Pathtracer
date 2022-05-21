using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Template;

namespace Raytracer
{
	public static class Raytracer
	{
		public static Scene Scene;
		public static Display Display;
		
		public static void Set(Scene scene, Display display){
			Scene = scene;
			Display = display;
		}

		public static void RenderImage()
		{
			Camera.RenderImage(Scene, Display);
		}
	}

	public class Scene
	{
		public IList<Object> renderedObjects = new List<Object>();
		public IList<LightSource> lightSources = new List<LightSource>();

		public void AddObject(Object o) => renderedObjects.Add(o);
		public void AddLight(LightSource o) => lightSources.Add(o);

		public bool TryIntersect(Ray ray, Object obj, out IntersectionInfo iiiiiiiiiiiiiiiiiii)
		{
			if (obj.TryIntersect(ray, obj, out iiiiiiiiiiiiiiiiiii))
				return true;


			iiiiiiiiiiiiiiiiiii = IntersectionInfo.None;
			return false;
		}

        internal bool isLit(IntersectionInfo i, out IntersectionInfo ii)
        {
			//TODO: make recursive for reflective surfaces
			foreach(LightSource ls in lightSources)
            {
				Vector3 ray = new Vector3(ls.Pos - i.IntPoint);
				ii = IntersectionInfo.None;
				TryIntersect(ray, i.);
			}
			
        }
    }

	public class Screen
	{
		public Vector3 HalfRight { get; }
		public Vector3 HalfUp { get; }
		public Screen(Vector3 halfRight, Vector3 halfUp)
		{
			this.HalfRight = halfRight;
			this.HalfUp = halfUp;
		}
	}

	public static class Camera
	{
		public static Vector3 Pos				{ get; private set; }
		public static Vector3 LookDirection		{ get; private set; }
		public static Vector3 UpDirection		{ get; private set; }

		public static Screen Screen				{ get; private set; }

		public static void RenderImage(Scene scene, Display display)
		{
			float halfW = display.width >> 1;
			float halfH = display.height >> 1;
			for (int y = 0; y < display.height; y++)
				for (int x = 0; x < display.width; x++)
				{
					Ray viewRay = new Ray(Pos, Screen.HalfRight	* -(halfW - x)/halfW +
											   Screen.HalfUp	*  (halfH - y)/halfH +
											   Vector3.UnitZ
										  );
					foreach (Object obj in scene.renderedObjects)
					{
						if (scene.TryIntersect(viewRay, obj, out IntersectionInfo ii))
						{
							if (scene.isLit(ii, out IntersectionInfo iii))
							{
								display.pixels[x + y * display.width] = 0x00ff00;
							}
						}
					}
						
				}
				
		}

		public static void Set(Screen screen, Vector3 pos, Vector3 lookDirection, Vector3 upDirection)
		{
			Screen = screen;
			Pos = pos;
			LookDirection = lookDirection;
			UpDirection = upDirection;
		}
		public static void Translate(Vector3 movement) => Pos += movement;
		public static void Translate(float x, float y, float z) => Pos += new Vector3(x, y, z);
		public static void Rotate(Vector3 rotation) => LookDirection += rotation;
		public static void Rotate(float x, float y, float z) => LookDirection += new Vector3(x, y, z);
	}
}
