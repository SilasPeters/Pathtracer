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
			Sphere bigboi = new Sphere(Vector3.Zero, 3, Vector3.One);
			Camera.RenderedObjects.Add(bigboi);
		}
		// tick: renders one frame
		public void Tick()
		{
			screen.Clear(0);
			screen.Print("hello world", 2, 2, 0xffffff);
			screen.Line(2, 20, 160, 20, 0xff0000);

			Camera.Render(screen);

		}
	}
}