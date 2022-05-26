using System.Drawing;
using EpicRaytracer;
using OpenTK;
using OpenTK.Input;

namespace Template
{
	static class Raytracer
	{
		public static Display Display;
		public static Size DisplaySize = new Size(711, 400);
		public static float Epsilon = 0.001f;

		private static BasicCamera[][] _cameraStances;
		private static int _currentCamStance = 0;
		//private static KeyboardState currentKeyboardState, lastKeyboardState;

		public static void Init()
		{
			Scene.AddObject(new Sphere(new Vector3(-4,     0,  0), 1.5f, Vector3.One));
			Scene.AddObject(new Sphere(new Vector3(0,      0,  0), 1f, new Vector3(0,1,1)));
			Scene.AddObject(new Sphere(new Vector3(4,      0,  0), 2f, Vector3.One));
			Scene.AddLight(new LightSource(new Vector3(0f, 1.3f, 0f), new Vector3(1,1,0), 1));
			//Scene.AddLight(new LightSource(new Vector3(-3f, 2f, -3f), 1, 1));
			Scene.AddObject(new Plane(new Vector3(0, -1f, 0), new Vector3(0.2f,-1, 0f), new Vector3(0.2f, 0.2f, 0.2f)));

			_cameraStances = new []
			{
				new BasicCamera[] { // normal state 1
					new MainCamera (new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, new Rectangle(0, 0, 711, 200), 1),
					//new DebugCamera(new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, new Rectangle(400, 0, 400, 400))
				},
				new BasicCamera[] { // normal state 2
					new MainCamera (new Vector3(3, 5, -5), new Vector3(-0.5f, -0.5f, 0.5f), Vector3.UnitY, new Rectangle(0, 0, 711, 400), 1),
					//new DebugCamera(new Vector3(0, 0, -10), Vector3.UnitZ, Vector3.UnitY, new Rectangle(400, 0, 400, 400))
				}
			};
		}
		
		
		public static void Tick()
		{
			Display.Clear(0);
			HandleUserInput();

			foreach (var cam in _cameraStances[_currentCamStance]) {
				cam.RenderImage();
			}
			Display.Print(_currentCamStance.ToString(), 5, 5, 0xffffff);
		}

		private static void HandleUserInput()
		{
			//currentKeyboardState = Keyboard.GetState();

			if (Keyboard.GetState()[Key.Space])
				_currentCamStance = (_currentCamStance + 1) % _cameraStances.Length;

			//lastKeyboardState = currentKeyboardState;

			//bool keyPressed(Key key) => currentKeyboardState[key] && !lastKeyboardState[key];
		}
	}
}