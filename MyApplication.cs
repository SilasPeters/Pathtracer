using Raytracer;
using OpenTK;

namespace Template
{
	class MyApplication
	{
		public Display Display;

		public void Init()
		{
			Scene scene = new Scene();
			scene.AddObject(new Sphere(new Vector3(0, 0, 0), 2, Vector3.One));


			Screen screen = new Screen(new Vector3(1, 0, 0), new Vector3(0, (float)Display.height/Display.width, 0));
			Raytracer.Raytracer.Set(scene, Display);
			Camera.Set(screen, new Vector3(0, 0, -10), Vector3.UnitZ, Vector3.UnitY);
		}
		
		public void Tick()
		{
			Display.Clear(0);
			Raytracer.Raytracer.RenderImage();
		}
	}
}