using System.Diagnostics;
using OpenTK;
using System;
using JackNSilo;
using OpenTK.Input;

namespace Template
{
	class MyApplication
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
		bool useRenderTarget = true;
		Cam Tcam;



		// initialize
		public void Init()
		{
			Tcam = new Cam(Matrix4.CreateTranslation(new Vector3(0, -14.5f, 0)) * Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), angle90degrees), new Vector3(0,0,1), new Vector3(0,-1,0), new Vector3(0, -14, 0));
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
		private static void HandleUserInput(Cam Tcamera)
		{
			var currentKeyboardState = Keyboard.GetState();

			//movement
			if (currentKeyboardState[Key.W])
            {
				Tcamera.pos += Tcamera.front * -Vector3.UnitY; //front * amount
				Tcamera.transform = Matrix4.CreateTranslation(Tcamera.pos) * Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), PI / 2);
			}
			else if (currentKeyboardState[Key.A])
            {
				Tcamera.pos -= Tcamera.right * -Vector3.UnitZ; //front * amount
				Tcamera.transform = Matrix4.CreateTranslation(Tcamera.pos) * Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), PI / 2);
			}
			else if (currentKeyboardState[Key.S])
			{
				Tcamera.pos -= Tcamera.front * -Vector3.UnitY; //front * amount
				Tcamera.transform = Matrix4.CreateTranslation(Tcamera.pos) * Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), PI / 2);
			}
			else if (currentKeyboardState[Key.D])
			{
				Tcamera.pos += Tcamera.right * -Vector3.UnitZ; //front * amount
				Tcamera.transform = Matrix4.CreateTranslation(Tcamera.pos) * Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), PI / 2);
			}

				//Tcamera.Column3 = new Vector4(Tcamera.Column3.X, Tcamera.Column3.Y, Tcamera.Column3.Z + 1, Tcamera.Column3.W);
			/*
			//rotation
			if (currentKeyboardState[Key.Right])
				Tcamera.PivotY(Tcamera.Rotation.Y + pivotDegrees);
			if (currentKeyboardState[Key.Left])
				Tcamera.PivotY(Tcamera.Rotation.Y - pivotDegrees);
			if (currentKeyboardState[Key.Up])
				Tcamera.PivotX(Tcamera.Rotation.X - pivotDegrees);
			if (currentKeyboardState[Key.Down])
				Tcamera.PivotX(Tcamera.Rotation.X + pivotDegrees);
			if (currentKeyboardState[Key.Q])
				Tcamera.PivotZ(Tcamera.Rotation.Z + pivotDegrees);
			if (currentKeyboardState[Key.E])
				Tcamera.PivotZ(Tcamera.Rotation.Z - pivotDegrees);
			//zoom
			if (currentKeyboardState[Key.Z])
				Tcamera.Lens.Distance *= 1.5f;
			if (currentKeyboardState[Key.X])
				Tcamera.Lens.Distance *= 0.5f;
			
			//logics to check whether this is the first frame on which a button is pressed  
			lastKeyboardState = currentKeyboardState;
			bool firstPressed(Key key) => currentKeyboardState[key] && !lastKeyboardState[key];
			*/
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

			HandleUserInput(Tcam);

			// update rotation
			a += 0.001f * frameDuration;
			if( a > 2 * PI ) a -= 2 * PI;

			if( useRenderTarget )
			{
				// enable render target
				target.Bind();

				// render scene to render target
				mesh.Render( shader, Tpot * Tcam.transform * Tview, wood );
				floor.Render( shader, Tfloor * Tcam.transform * Tview, wood );

				// render quad
				target.Unbind();
				quad.Render( postproc, target.GetTextureID() );
			}
			else
			{
				// render scene directly to the screen
				mesh.Render( shader, Tpot * Tcam.transform * Tview, wood );
				floor.Render( shader, Tfloor * Tcam.transform * Tview, wood );
			}
		}
	}
}