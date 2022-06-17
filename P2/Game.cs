using System.Diagnostics;
using JackNSilo;
using OpenTK;
using System;
using JackNSilo;
using OpenTK.Input;

namespace Template
{
	class Game
	{
		// member variables
		public Surface screen;                  // background surface for printing etc.
		Mesh mesh, floor;                       // a mesh to draw using OpenGL
		const float PI = 3.1415926535f;         // PI
		float angle90degrees = PI / 2;
		float a = 0;                            // teapot rotation angle
		Stopwatch timer;                        // timer for measuring frame duration
		Shader shader;                          // shader to use for rendering
		Shader postproc;                        // shader to use for post processing
		Texture wood;                           // texture to use for rendering
		RenderTarget target;                    // intermediate render target
		ScreenQuad quad;                        // screen filling quad for post processing
		const bool useRenderTarget = true;
		
		public static Cam Cam;
		public const float MovementSpeed = 100;
		public const float RotationSpeed = 50f / 10;



		// initialize
		public void Init()
		{
			Cam = new Cam(Matrix4.CreateTranslation(0, -14, 0), Matrix4.CreateRotationX(0));
			Smash child = new Smash("../../assets/teapot.obj", Matrix4.CreateTranslation(2, 0, 2), Matrix4.Identity);
			Smash grandChild = new Smash("../../assets/teapot.obj", Matrix4.CreateTranslation(1, 2, 0), Matrix4.Identity);
			child.AddChild(grandChild);
			SceneGraph.AddToRoot(child);

			// load teapot
			mesh = new Mesh( "../../assets/teapot.obj" );
			floor = new Mesh( "../../assets/floor.obj" );
			// initialize stopwatch
			timer = new Stopwatch();
			timer.Reset();
			timer.Start();
			// create shaders
			shader = new Shader( "../../shaders/vs.glsl", "../../shaders/fs.glsl" );
			postproc = new Shader( "../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl" );
			// load a texture
			wood = new Texture( "../../assets/wood.jpg" );
			// create the render target
			target = new RenderTarget( screen.width, screen.height );
			quad = new ScreenQuad();
		}

		// tick for background surface
		public void Tick()
		{
			screen.Clear( 0 );
			screen.Print( "hello world", 2, 2, 0xffff00 );
		}
		private static void HandleUserInput(Cam cam, float deltaTime)
		{
			var currentKeyboardState = Keyboard.GetState();
			deltaTime /= 1000; //ms

			// Translation
			if (currentKeyboardState[Key.W])
				cam.Transform.Translate(cam.Transform.Front * MovementSpeed * deltaTime);
			if (currentKeyboardState[Key.A])
				cam.Transform.Translate(cam.Transform.Right * -MovementSpeed * deltaTime);
			if (currentKeyboardState[Key.S])
				cam.Transform.Translate(cam.Transform.Front * -MovementSpeed * deltaTime);
			if (currentKeyboardState[Key.D])
				cam.Transform.Translate(cam.Transform.Right * MovementSpeed * deltaTime);
			if (currentKeyboardState[Key.Space])
				cam.Transform.Translate(cam.Transform.Up * MovementSpeed * deltaTime);
			if (currentKeyboardState[Key.ShiftLeft])
				cam.Transform.Translate(cam.Transform.Up * -MovementSpeed * deltaTime);
			
			// Looking around
			if (currentKeyboardState[Key.Up])
				cam.Transform.Rotate(Matrix4.CreateRotationX(-RotationSpeed * deltaTime));
			if (currentKeyboardState[Key.Left])
				cam.Transform.Rotate(Matrix4.CreateRotationY(-RotationSpeed * deltaTime));
			if (currentKeyboardState[Key.Down])
				cam.Transform.Rotate(Matrix4.CreateRotationX(RotationSpeed * deltaTime));
			if (currentKeyboardState[Key.Right])
				cam.Transform.Rotate(Matrix4.CreateRotationY(RotationSpeed * deltaTime));
			
			// Tilting
			if (currentKeyboardState[Key.Q])
				cam.Transform.Rotate(Matrix4.CreateRotationZ(-RotationSpeed * deltaTime));
			if (currentKeyboardState[Key.E])
				cam.Transform.Rotate(Matrix4.CreateRotationZ(RotationSpeed * deltaTime));
		}
		// tick for OpenGL rendering code
		public void RenderGL()
		{
			// measure frame duration
			float frameDuration = timer.ElapsedMilliseconds;
			timer.Reset();
			timer.Start();

			// prepare matrix for vertex shader
			Matrix4 Tpot = Matrix4.CreateScale( 0.5f ) * Matrix4.CreateFromAxisAngle( new Vector3( 0, 1, 0 ), a );
			Matrix4 Tfloor = Matrix4.CreateScale( 4.0f ) * Matrix4.CreateFromAxisAngle( new Vector3( 0, 1, 0 ), a );
			Matrix4 Tview = Matrix4.CreatePerspectiveFieldOfView( 1.2f, 1.3f, .1f, 1000 );


			HandleUserInput(Cam, frameDuration);
			SceneGraph.Render(Matrix4.Identity);

			// update rotation
			a += 0.001f * frameDuration;
			if( a > 2 * PI ) a -= 2 * PI;

			if( useRenderTarget )
			{
				// enable render target
				target.Bind();

				// render scene to render target
				mesh.Render( shader, Tpot * Cam.Transform.FullMatrix * Tview, wood );
				floor.Render( shader, Tfloor * Cam.Transform.FullMatrix * Tview, wood );

				// render quad
				target.Unbind();
				quad.Render( postproc, target.GetTextureID() );
			}
			else
			{
				// render scene directly to the screen
				mesh.Render( shader, Tpot * Cam.Transform.FullMatrix * Tview, wood );
				floor.Render( shader, Tfloor * Cam.Transform.FullMatrix * Tview, wood );
			}
		}
	}
}