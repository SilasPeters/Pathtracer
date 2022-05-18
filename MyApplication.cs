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
			Sphere bigboi = new Sphere(new Vector3(0, 0.5f, 0), 2, Vector3.One);
			Camera.RenderedObjects.Add(bigboi);
		}
		// tick: renders one frame
		public void Tick()
		{
			screen.Clear(0);

			Camera.Render(screen);
		}
	}
}