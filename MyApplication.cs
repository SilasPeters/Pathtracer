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
			//Sphere bigboi = new Sphere(new Vector3(0, 0.5f, 0), 2, Vector3.One);
			//Camera.RenderedObjects.Add(bigboi);

			FinPlane bigplane = new FinPlane(new Vector3(0, 2, 0), new Vector3(0,0,1), Vector3.Zero, Vector3.Zero);
			Camera.RenderedObjects.Add(bigplane);
		}
		// tick: renders one frame
		public void Tick()
		{
			screen.Clear(0);

			Camera.Render(screen);
		}
	}
}