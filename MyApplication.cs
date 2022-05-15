using Raytracer;
using OpenTK;

namespace Template
{
	class MyApplication
	{
		// member variables
		public Surface screen;
		// initialize
		public void Init()
		{
			//Sphere bigboi = new Sphere(new Vector3(0, 0, 20f), 0.0005f, Vector3.One);
			//Camera.RenderedObjects.Add(bigboi);

			Point pointboi = new Point(new Vector3(0, 5, 0));
			Camera.RenderedObjects.Add(pointboi);
		}
		// tick: renders one frame
		public void Tick()
		{
			screen.Clear(0);

			Camera.Render(screen);
		}
	}
}