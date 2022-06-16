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
		private SceneGraph SceneGraph = new SceneGraph();
		public static ulong CurrentFrame;
		public static ulong LastUpdateFrame;
		
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



		// initialize
		public void Init()
		{
			Cam = new Cam(
				Vector3.UnitZ,
				-Vector3.UnitY,
				Vector3.UnitX,
				new Vector3(0, -14, 0));
			
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
			CurrentFrame += 1;
			screen.Clear( 0 );
			screen.Print( "hello world", 2, 2, 0xffff00 );
		}
		private static void HandleUserInput(Cam cam)
		{
			var currentKeyboardState = Keyboard.GetState();

			//movement angles still need work
			if (currentKeyboardState[Key.W])
            {
	            cam.pos += cam.front * -Vector3.UnitY; //front * amount
				cam.camMatrix = Matrix4.CreateTranslation(cam.pos) * Matrix4.CreateFromAxisAngle(Vector3.UnitX, PI / 2);
			}
			if (currentKeyboardState[Key.A])
            {
				cam.pos -= cam.right * -Vector3.UnitZ; //front * amount
				cam.camMatrix = Matrix4.CreateTranslation(cam.pos) * Matrix4.CreateFromAxisAngle(Vector3.UnitX, PI / 2);
			}
			if (currentKeyboardState[Key.S])
			{
				cam.pos -= cam.front * -Vector3.UnitY; //front * amount
				cam.camMatrix = Matrix4.CreateTranslation(cam.pos) * Matrix4.CreateFromAxisAngle(Vector3.UnitX, PI / 2);
			}
			if (currentKeyboardState[Key.D])
			{
				cam.pos += cam.right * -Vector3.UnitZ; //front * amount
				cam.camMatrix = Matrix4.CreateTranslation(cam.pos) * Matrix4.CreateFromAxisAngle(Vector3.UnitX, PI / 2);
			}
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


			HandleUserInput(Cam);
			
			//SceneGraph.Render(Cam.camMatrix);

			// update rotation
			a += 0.001f * frameDuration;
			if( a > 2 * PI ) a -= 2 * PI;

			if( useRenderTarget )
			{
				// enable render target
				target.Bind();

				// render scene to render target
				mesh.Render( shader, Tpot * Cam.camMatrix * Tview, wood );
				floor.Render( shader, Tfloor * Cam.camMatrix * Tview, wood );

				// render quad
				target.Unbind();
				quad.Render( postproc, target.GetTextureID() );
			}
			else
			{
				// render scene directly to the screen
				mesh.Render( shader, Tpot * Cam.camMatrix * Tview, wood );
				floor.Render( shader, Tfloor * Cam.camMatrix * Tview, wood );
			}
		}
	}
}