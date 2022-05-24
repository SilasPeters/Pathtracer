using System.Drawing;
using EpicRaytracer;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace Template
{
	static class MyApplication
	{
		public static Display Display;
		public static float Epsilon = 0.001f;

		private static BasicCamera[][] _cameraStances;
		private static int _currentCamStance = 0;

		public static void Init()
		{
			Scene.AddObject(new Sphere(new Vector3(0, 0, 0), 2f, Vector3.One));
			Scene.AddLight(new LightSource(new Vector3(3f, 3f, 3f), 1, 1));
			Scene.AddLight(new LightSource(new Vector3(-3f, 2f, -3f), 1, 1));
			//scene.AddObject(new Plane(Vector3.UnitY, new Vector3(0, 5f, 0)));

			_cameraStances = new []
			{
				new BasicCamera[] { // normal state
					new MainCamera (new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, new Rectangle(0, 0, 400, 400)),
					new DebugCamera(new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, new Rectangle(400, 0, 400, 400))
				},
				new BasicCamera[] { // debug state
					new MainCamera (new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, new Rectangle(400, 0, 400, 400)),
					new DebugCamera(new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, new Rectangle(0, 0, 400, 400))
				}
			};
		}
		
		public static void Tick()
		{
			Display.Clear(0);

			Raytracer.RenderImage(Keyboard.GetState()[Key.Space] ? _cameraStances[1] : _cameraStances[0]);
		}
	}
}