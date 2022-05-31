using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Template;

namespace EpicRaytracer
{
	public static class Scene
	{
		public static IList<Object> renderedObjects = new List<Object>();
		public static IList<LightSource> lightSources = new List<LightSource>();

		public static void AddObject(Object o)     => renderedObjects.Add(o);
		public static void AddLight(LightSource o) => lightSources.Add(o);

		// Given a ray and more, calculates what color that ray should return (which is used to display a single pixel)
		public static Vector3 CalculatePixel(Ray ray, int layers, float n, Object self = null)
		{
			Vector3 color = Vector3.Zero;
			if (TryIntersect(ray, out IntersectionInfo ii, self)) //we hit an object, but not the object from which the ray comes
			{
				Material objMat = ii.Object.Mat;
				if (layers >= 20) return Vector3.Zero; //end recursion
				
				if (ii.Object.Mat.Type == "Mirror") {
					//calculate new reflected ray
					Vector3 V = ii.Point - ii.IntersectedRay.EntryPoint;
					Vector3 R = V - 2 * Vector3.Dot(V.Normalized(), ii.Normal) * ii.Normal;
					ray.SetPoint(ii.Point);
					ray.SetDir(R);
					
					// enter recursion with new ray
					return objMat.DiffuseCo * CalculatePixel(ray, ++layers, n, ii.Object);
				}

				if (ii.Object.Mat.Type == "Refractive")
				{
					// calculate new refracted ray
					float nBreuk = n / objMat.N;
					if (nBreuk > 1) return Vector3.Zero; //internal reflection
					
					Vector3  d      = ray.DirectionVect;
					float    dot    = Vector3.Dot(d, ii.Normal);
					Vector3  t = nBreuk * (d - dot * ii.Normal) - (float)Math.Sqrt(1 - nBreuk * nBreuk * (1 - dot * dot)) * ii.Normal;
					
					ray.SetPoint(ii.Point);
					ray.SetDir(t);
					
					//enter recursion
					return objMat.DiffuseCo * CalculatePixel(ray, ++layers, objMat.N, ii.Object);
				}

				// if not some special object type, determine for each light source how much this surface is lit
				foreach (LightSource ls in lightSources)
				{
					Vector3 toLight = ls.Pos - ii.Point;
					if (!TryIntersect(new Ray(ii.Point, toLight.Normalized()), out IntersectionInfo iii, ii.Object)) // no intersection, object is lit
					{
						//determine how lit the object is at ii.Point using the lecture notes
						Vector3 V = (ii.IntersectedRay.EntryPoint - ii.Point).Normalized();
						Vector3 R = (toLight - 2 * Vector3.Dot(toLight.Normalized(), ii.Normal) * ii.Normal).Normalized();
						color += new Vector3(1 / toLight.LengthSquared * ls.Color *
						                     (objMat.DiffuseCo * Math.Max(0, Vector3.Dot(ii.Normal, toLight.Normalized()))
						                    + objMat.SpecularCo * (float)Math.Pow(Math.Max(0, Vector3.Dot(V, R)), Raytracer.Glossyness)
						                     ));
					}
				}
				// If the hit object is a normal diffuse object, return at least an ambient light level
				color += Raytracer.AmbientLightLevel * objMat.DiffuseCo * objMat.AmbientCo;
			}
			return color;
		}

		/*public static Vector3 TracePixel(Ray ray, int depth, Object self = null)
		{
			if (depth >= 80)
				return Vector3.Zero; // Bounced enough times.

			if (TryIntersect(ray, out IntersectionInfo ii, self))
			{
				Material material  = ii.Object.Mat;
				Vector3  emittance = material.DiffuseCo;

				// Pick a random direction from here and keep going.
				ray.SetPoint(ii.Point);
				// This is NOT a cosine-weighted distribution!
				ray.SetDir(RandomUnitVectorInHemisphereOf(ii.Normal));

				// Probability of the newRay
				float p = 1f / (float)(2 * Math.PI);

				// Compute the BRDF for this ray (assuming Lambertian reflection)
				float   cos_theta = Vector3.Dot(ray.DirectionVect, ii.Normal);
				Vector3 BRDF      = material.SpecularCo / (float)Math.PI;

				// Recursively trace reflected light sources.
				Vector3 incoming = TracePixel(ray, depth + 1, ii.Object);

				// Apply the Rendering Equation here.
				return emittance + BRDF * incoming * cos_theta / p;	
			}

			return Vector3.Zero; // Nothing was hit.

			Vector3 RandomUnitVectorInHemisphereOf(Vector3 Normal)
			{
				Random r = new Random();
				return new Vector3(
					Normal.X * r.Next(0, 20),
					Normal.Y * r.Next(0, 20),
					Normal.Z * r.Next(0, 20)
				).Normalized();
			}*/
			
			/*if (depth > 20) return Vector3.Zero; 
			Vector2 N                  = ..., P                    = ...; //normal and position of the shaded point 
			Vector3 directLightContrib = 0,   indirectLightContrib = 0; 
			// compute direct illumination
			foreach (var ls in lightSources)
			{
				Vector2 L   = ls.getLightDirection(P); 
				Vector3 L_i = ls.intensity * scene->lights[i]->color; 
				// we assume the surface at P is diffuse
				directLightContrib += shadowRay(P, -L) * std::max(0.f, N.dotProduct(L)) * L_i;
			}
				// compute indirect illumination
			float rotMat[2][2] =  {{N.y, -N.x}, {N.x, N.y}}; //compute a rotation matrix 
			uint32_t N = 16;
			for (uint32_t n = 0; n < N; ++n) {
				// step 1: draw a random sample in the half-disk
				float theta    = drand48() * M_PI; 
				float cosTheta = cos(theta); 
				float sinTheta = sin(theta); 
				// step 2: rotate the sample direction
				float sx = cosTheta  * rotMat[0][0] + sinTheta  * rotMat[0][1]; 
				float sy = cosTheta  * rotMat[1][0] + sinTheta  * rotMat[1][1]; 
				// step 3: cast the ray into the scene
				Vec3f sampleColor = castRay(P, Vec2f(sx, sy), depth + 1); //trace a ray from P in random direction 
				// step 4 and 5: treat the return color as if it was a light (we assume our shaded surface is diffuse)
				IndirectLightContrib += sampleColor * cosTheta; //diffuse shading = L_i * cos(N.L) 
			} 
			// step 6: divide the result of indirectLightContrib by the number of samples N (Monte Carlo integration)
			indirectLightContrib /= N; 
 
			// final result is diffuse from direct and indirect lighting multiplied by the object color at P
			return (indirectLightContrib + directLightContrib) * objectAlbedo / M_PI; 
		}*/

		/*void Render(Image finalImage, count numSamples) {
			foreach (pixel in finalImage) {
				foreach (i in numSamples) {
					Ray r = camera.generateRay(pixel);
					pixel.color += TracePath(r, 0);
				}
				pixel.color /= numSamples; // Average samples.
			}
		}*/

		/// <summary>Returns true if an object intersects with a given ray. If so, the out parameter supplies you with all the info you need</summary>
		/// <param name="ray">The ray which is tested for intersections</param>
		/// <param name="iiiiiiiiiiiiiiiiiii">i</param>
		/// <param name="self">The object to ignore when raycasting</param>
		/// <returns></returns>
		public static bool TryIntersect(Ray ray, out IntersectionInfo iiiiiiiiiiiiiiiiiii, Object self = null)
		{
			// Declare output variables
			float  t = float.MaxValue;
			Object o = null;
			
			// Test for intersections for each object
			foreach (Object obj in renderedObjects)
				if (obj != self && obj.TryIntersect(ray, out iiiiiiiiiiiiiiiiiii) && iiiiiiiiiiiiiiiiiii.t < t) {
					//stores only the nearest object which is not equal to self
					t = iiiiiiiiiiiiiiiiiii.t;
					o = obj;
				}

			// return all information gathered
			iiiiiiiiiiiiiiiiiii = new IntersectionInfo(ray, t, o);
			return o != null && t >= 0;
		}
	}
	
	public struct Lens
	{
		private readonly BasicCamera cam;
		private float height, aspectRatio;
		public float Distance;

		// Stores vectors which represent the dimensions of the lens in object space. Aka, the distance to be traveled
		// to reach a point on the lens from its topleft coordinate. These dimensions are relative to the camera, so
		// that at all times the lens follows the camera
		public Vector3 horizontal => cam.Right * height * aspectRatio;
		public Vector3 vertical   => cam.Up * height;
		public Vector3 topLeft    => cam.Front * Distance - horizontal / 2 + vertical / 2;
		
		public Lens(BasicCamera cam, float distance, float height = 1f) {
			this.cam      = cam;
			this.Distance = distance;
			this.height   = height;
			
			this.aspectRatio = (float)cam.DisplayRegion.Width / cam.DisplayRegion.Height;
		}

		// Knowing how much to go to the 'right' to reach the right edge of the lens etc., we multiply it by the percentage of where we want
		// to be in that dimension. So 70% to the right returns the topleft + 0.7 * horizontal
		public Vector3 GetDirToPixel(float xPercentage, float yPercentage) => topLeft + horizontal * xPercentage - vertical * yPercentage;
	}

	public abstract class BasicCamera
	{
		public Vector3   Pos           { get; protected set; }
		public Vector3   Front         { get; protected set; }
		public Vector3   Up            { get; protected set; }
		public Vector3   Right         { get; protected set; }
		public Rectangle DisplayRegion { get; protected set; }
		public Lens Lens;
		public Vector3   Rotation      { get; protected set; }

		public BasicCamera(Vector3 pos, Vector3 front, Vector3 up, Rectangle displayRegion, float FOV) {
			Pos           = pos;
			Front         = front.Normalized();
			Up            = up.Normalized();
			DisplayRegion = displayRegion;

			Right = Vector3.Cross(Up, Front).Normalized(); //big brain
			Lens  = new Lens(this, 45f/FOV);
		}
		
		public abstract void RenderImage();

		public void MoveForward(float amount)  => Pos += Front * amount;
		public void MoveSidewards(float amount) => Pos += Right * amount;

		public void Pivot(float x, float y, float z)
		{
			PivotX(x);
			PivotY(y);
			PivotZ(z);
		}
		public void PivotX(float degree)
		{
			Up         = new Vector3(Up.X, Cos(degree), Sin(degree));
			Front      = Vector3.Cross(Right, Up).Normalized();
			Rotation = new Vector3(degree, Rotation.Y, Rotation.Z);
		}
		public void PivotY(float degree) {
			Front = new Vector3(Sin(degree), Front.Y, Cos(degree)).Normalized();
			Right = Vector3.Cross(Up, Front).Normalized();
			Rotation = new Vector3(Rotation.X, degree, Rotation.Z);
		}
		public void PivotZ(float degree) {
			Right = new Vector3(Cos(degree), Sin(degree), Right.Z).Normalized();
			Up    = Vector3.Cross(Front, Right).Normalized();
			Rotation = new Vector3(Rotation.X, Rotation.Y, degree);
		}
		
		float Sin(float degree) => (float)Math.Sin(MathHelper.DegreesToRadians(degree));
		float Cos(float degree) => (float)Math.Cos(MathHelper.DegreesToRadians(degree));
	}
	public class MainCamera : BasicCamera
	{
		public MainCamera(Vector3 pos, Vector3 front, Vector3 up, Rectangle displayRegion, float FOV)
			: base(pos, front, up, displayRegion, FOV)
		{
		}

		public override void RenderImage()
		{
			// Apply multithreading: start N that are sharing the workload
			Thread[] threads = new Thread[Raytracer.Threads];
			for (int i = 0; i < threads.Length; i++)
			{
				// each thread is assigned a subregion within the DisplayRegion of this camera which it will draw
				int       subregionWidth = DisplayRegion.Width / Raytracer.Threads;
				Rectangle subregion = new Rectangle(
					DisplayRegion.Left + subregionWidth * i,
					DisplayRegion.Top,
					subregionWidth,
					DisplayRegion.Height
				);
				threads[i] = new Thread(DrawSubregion);
				threads[i].Start(subregion);
			}
			foreach (Thread thread in threads) thread.Join(); //Wait for all threads to be finished before continuing

			// The actual drawing method
			void DrawSubregion(object subregion)
			{
				Rectangle Subregion = (Rectangle)subregion;
				Ray       viewRay   = new Ray(Pos, Vector3.Zero);
			
				for (int y = 0; y < Subregion.Height; y++)
					for (int x = Subregion.Left; x < Subregion.Right; x++)
					{
						// for each pixel in the given region to draw, calculate a new ray
						viewRay.SetDir(Lens.GetDirToPixel(
							(float)x / (DisplayRegion.Width - 1),
							(float)y / (DisplayRegion.Height - 1)));
						// for each pixel, shoot the ray and store the returned color
						Raytracer.Display.pixels[DisplayRegion.Left + x + (DisplayRegion.Top + y) * Raytracer.Display.width]
							= Colors.Make(Scene.CalculatePixel(viewRay, 0, 1));
					}
			}
		}
	}

	public class DebugCamera : BasicCamera
	{
		public override void RenderImage()
		{
			// Yes, this is a try catch. A rare scenario in which it's actually a neat solution in my opinion
			try
			{
				float numberOfRays     = 20;
				int   colorViewray     = 0xff00ff;
				int   colorCam         = 0xffff00;
				int   colorLens      = 0xffffff;
				int   colorSphere      = 0xffffff;
				int   colorLightSource = 0x00ffff;
				
				BasicCamera cam    = Raytracer.CurrentCam;
				Point       camPos = to2D(cam.Pos);
				
				// Draw each viewray
				for (int i = 0; i < numberOfRays; i++)
				{
					float t   = 20 * Raytracer.DebugScale; //default valye will always be out of bounds
					// Calculate intersection point by shooting a new viewray, and see at what distance with this
					// given direction an intersection occurs
					Ray   ray = new Ray(cam.Pos, cam.Lens.GetDirToPixel(i / (numberOfRays - 1), 0.5f));
					if (Scene.TryIntersect(ray, out IntersectionInfo ii)) //hit something
						t = ii.t; //overwrites default value

					// Draw the line for a length which might be default, might be shorter
					Point iPos = to2D(ray.GetPoint(t));
					Raytracer.Display.Line(camPos.X, camPos.Y, iPos.X, iPos.Y, colorViewray);
				}
			
				//draw objects except for the planes
				foreach (Object o in Scene.renderedObjects)
					if (o is Sphere s)
						circle(s.Pos, s.Radius, colorSphere);

				// Draw the lens through which lines are shot
				Point lensLeft  = to2D(cam.Lens.topLeft + cam.Pos);
				Point lensRight = to2D(cam.Lens.topLeft + cam.Lens.horizontal + cam.Pos);
				Raytracer.Display.Line(lensLeft.X, lensLeft.Y, lensRight.X, lensRight.Y, colorLens);
				
				// Draw all lightsources
				foreach (var ls in Scene.lightSources)
					circle(ls.Pos, ls.Radius, colorLightSource);
				// Draw the camera
				Raytracer.Display.Box(camPos.X - 1, camPos.Y - 1, camPos.X, camPos.Y, colorCam);

				//draws a circle
				void circle(Vector3 point, float Radius, int c)
				{
					for (float d = 0; d < 1; d += 1f/360)
					{
						Vector3 pos = point + new Vector3(
							(float)(Radius * Math.Cos(d * 2 * Math.PI)), 0,
							(float)(Radius * Math.Sin(d * 2 * Math.PI)));
						
						draw(to2D(pos), c);
					}
				}
				
				// Converts a 3D coordinate to screenspace 2D coordinate
				Point to2D(Vector3 objectPos)
				{
					Point p = new Point((int)(objectPos.X / Raytracer.DebugScale * DisplayRegion.Height),
						-(int)(objectPos.Z / Raytracer.DebugScale * DisplayRegion.Height));
					return p + new Size(DisplayRegion.Width/2 + DisplayRegion.Left, DisplayRegion.Height/2 + DisplayRegion.Top);
				}

				void draw(Point pos, int c) => Raytracer.Display.pixels[pos.X + pos.Y * Raytracer.Display.width] = c;
			}
			catch (Exception e)
			{
				// In the try block above, all objects are rendered in order of importance to be displayed.
				// If an object is to be displayed outside of the given region of the Debug Camera, the would be
				// an IndexOutOfRangeException. This may occur when the camera or an object is outside the window.
				// The window can be scaled by changing the scale value Raytracer.DebugScale, so that more can be
				// displayed. Still, there shouldn't be a crash. Thus, the rendering just stops after an exception.
				// Due to the ordered rendering, the objects actually inside the window are still displayed but the
				// ones outside not. Thus, the exception is no big deal (most of the time)!
				Console.WriteLine("Attempted to draw an object outside of the display.\nIncrease the scale of the Debug Cam to fix missing objects");
			}
		}

		public DebugCamera(Vector3 pos, Vector3 front, Vector3 up, Rectangle displayRegion, float FOV)
			: base(pos, front, up, displayRegion, FOV)
		{
		}
	}
}