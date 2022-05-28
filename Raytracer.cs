using System.Drawing;
using EpicRaytracer;
using OpenTK;
using OpenTK.Input;

namespace Template
{
	static class Raytracer
	{
		public static Display Display;
		public static Size DisplaySize = new Size(800, 400);
		public const float Epsilon = 0.001f;
		public const float Glossyness = 50;
		public const float AmbientLightLevel = 0.2f;

		public static BasicCamera[][] _cameraStances;
		public static int _currentCamStance;
		//private static KeyboardState currentKeyboardState, lastKeyboardState;

		public static void Init()
		{
			Material m11 = new Material(new Vector3(0, 1, 0), Vector3.UnitX, new Vector3(1, 1, 1));
			Material m12 = new Material(new Vector3(1, 0, 0), Vector3.One/2,  new Vector3(1, 1, 1));
			Material m13 = new Material(new Vector3(0, 0, 1), Vector3.One/2,  new Vector3(1, 1, 1));
			Material m2  = new Material(new Vector3(1, 1, 1), Vector3.One/2,  new Vector3(1, 1, 1));
			//Scene.AddObject(new Sphere(new Vector3(-2.5f,  0,  0), 1f, m13));
			Scene.AddObject(new Sphere(new Vector3(0,      0,  0), 1f, m11));
			Scene.AddObject(new Sphere(new Vector3(0,      2f,  0), 0.3f, m11));
			//Scene.AddObject(new Sphere(new Vector3(1,      2f,  3), 0.3f, m11));
			//Scene.AddObject(new Sphere(new Vector3(2,      2f,  2), 0.3f, m11));
			//Scene.AddObject(new Sphere(new Vector3(3,      2f,  1), 0.3f, m11));
			//Scene.AddObject(new Sphere(new Vector3(2.5f,   0,  0), 1f, m12));
			Scene.AddLight(new LightSource(new Vector3(0, 4, 0f), new Vector3(1, 1, 0.4f), 1));
			//Scene.AddLight(new LightSource(new Vector3(-2, 1, 0f), new Vector3(1, 1, 0.4f), 1));
			//Scene.AddLight(new LightSource(new Vector3(0f, 2f, 0f), new Vector3(0,0,1), 1));
			//Scene.AddLight(new LightSource(new Vector3(-3f, 2f, -3f), Vector3.One, 1));
			Scene.AddObject(new Plane(new Vector3(0, -1f, 0), new Vector3(0,-1, 0), m2));

			_cameraStances = new []
			{
				//new BasicCamera[] { // normal state 1
				//	new MainCamera (new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, new Rectangle(0, 0, 800, 400), 1),
					//new DebugCamera(new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, new Rectangle(400, 0, 400, 400))
				//},
				new BasicCamera[] { // normal state 2
					new MainCamera (new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, new Rectangle(0, 0, 400, 400), 1),
					new DebugCamera(new Vector3(0, 0, 0), -Vector3.UnitY, Vector3.UnitY, new Rectangle(400, 0, 400, 400), 1)
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