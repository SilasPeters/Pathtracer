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
		public const float DebugScale = 10f;
		public const float Epsilon = 0.001f;
		public const float Glossyness = 50;
		public const float AmbientLightLevel = 0.2f;
		private const float movementSpeed = 1f;
		private const float pivotDegrees = 10f;

		public static BasicCamera[][] _cameraStances;
		public static int _currentCamStance;
		private static KeyboardState currentKeyboardState, lastKeyboardState;
		public static BasicCamera CurrentCam => _cameraStances[_currentCamStance][0];

		public static void Init()
		{
			Material m11 = new Material(new Vector3(0, 1, 0), Vector3.One/2,   new Vector3(1, 1, 1)*10);
			Material m12 = new Material(new Vector3(1, 0, 1), Vector3.One/2,   new Vector3(1, 1, 1)*10);
			Material mir = new Material(new Vector3(1, 1, 1), Vector3.One / 2, new Vector3(1, 1, 1) * 10, true);

			Scene.AddObject(new Sphere(new Vector3(0, 0.5f,   0), 1f, mir));
			Scene.AddObject(new Sphere(new Vector3(2, 2f,  0), 1.3f, m11));
			Scene.AddObject(new Plane(new Vector3(0,  -1f, 0), new Vector3(0, -1, 0), m12));

			Scene.AddLight(new LightSource(new Vector3(-3,  2, 0f), new Vector3(1, 1, 1f), 1));
			//Scene.AddLight(new LightSource(new Vector3(-2, 1, 0f), new Vector3(1, 1, 0.4f), 1));

			_cameraStances = new []
			{
				new BasicCamera[] { // normal state
					new MainCamera (new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, new Rectangle(0, 0, DisplaySize.Width, DisplaySize.Height), 60)
				},
				new BasicCamera[] { // debug state
					new MainCamera (new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, new Rectangle(0, 0, DisplaySize.Width/2, DisplaySize.Height), 60),
					new DebugCamera(new Vector3(0, 0, 0), -Vector3.UnitY, Vector3.UnitY, new Rectangle(DisplaySize.Width/2, 0, DisplaySize.Width/2, DisplaySize.Height), 60)
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
			currentKeyboardState = Keyboard.GetState();
			BasicCamera cam = CurrentCam;
			
			//camstances
			if (firstPressed(Key.Space))
				_currentCamStance = (_currentCamStance + 1) % _cameraStances.Length;
			//movement
			if (currentKeyboardState[Key.S])
				cam.MoveForward(-movementSpeed);
			if (currentKeyboardState[Key.W])
				cam.MoveForward(movementSpeed);
			if (currentKeyboardState[Key.A])
				cam.MoveSidewards(-movementSpeed);
			if (currentKeyboardState[Key.D])
				cam.MoveSidewards(movementSpeed);
			//rotation
			if (currentKeyboardState[Key.Right])
				cam.PivotY(cam.Rotation.Y + pivotDegrees);
			if (currentKeyboardState[Key.Left])
				cam.PivotY(cam.Rotation.Y - pivotDegrees);
			if (currentKeyboardState[Key.Up])
				cam.PivotX(cam.Rotation.X - pivotDegrees);
			if (currentKeyboardState[Key.Down])
				cam.PivotX(cam.Rotation.X + pivotDegrees);
			if (currentKeyboardState[Key.Q])
				cam.PivotZ(cam.Rotation.Z + pivotDegrees);
			if (currentKeyboardState[Key.E])
				cam.PivotZ(cam.Rotation.Z - pivotDegrees);
			//zoom
			if (currentKeyboardState[Key.Z])
				cam.Lens.Distance *= 1.5f;
			if (currentKeyboardState[Key.X])
				cam.Lens.Distance *= 0.5f;
			
			//logics to check whether this is the first frame on which a button is pressed  
			lastKeyboardState = currentKeyboardState;
			bool firstPressed(Key key) => currentKeyboardState[key] && !lastKeyboardState[key];
		}
	}
}