using System.Diagnostics;
using JackNSilo;
using OpenTK;
using System;
using JackNSilo;
using OpenTK.Input;
using System.Collections.Generic;

namespace Template
{
	class Game
	{
		// General member-veriables
		private Stopwatch timer;			// timer for measuring frame duration
		public  Surface screen;				// background surface for printing etc.
		private RenderTarget target;		// intermediate render target
		private ScreenQuad quad;			// screen filling quad for post processing
		private Shader postproc;			// Shader which is used during post-processing
		private const bool useRenderTarget = true;
		
		// Custom member-variables
		public static Cam Cam;
		public static Matrix4 perspective;
		public static Matrix4 ortho;
		public static Matrix4 viewPort;
		private const float Fov = 1f;
		private const float AspectRatio = 1.3f;
		private const float ZNear = 0.1f;
		private const float ZFar = 2000f;
		private const float MovementSpeed = 100;
		private const float RotationSpeed = 50f / 10;
		public List<LightSource> LightSources = new List<LightSource>();

		// Misc
		float a = 0;				// teapot rotation angle
		
		public void Init()
		{
			// Store global variables
			timer = new Stopwatch();
			timer.Start();
			target = new RenderTarget( screen.width, screen.height );
			quad = new ScreenQuad();
			postproc = new Shader( "../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl" );
			LightSources.Add(new LightSource(Vector3.UnitY, Vector3.One, 0.2f, new Smash("../../assets/teapot.obj")));
			
			// Pre-calculate matrices
			// source: slides
			//Vector3 sbMin = new Vector3(-4, -4, -4); //sceneBoundingBoxMin
			//Vector3 sbMax = new Vector3(4,  4,  4);  //sceneBoundingBoxMax
			//Matrix4 camera = Cam.Transform.LocalRotation *
			//                (Cam.Transform.LocalTranslation * Matrix4.CreateTranslation(-1, -1, -1)); //Matrix4.LookAt()
			//ortho = Matrix4.CreateScale(new Vector3(2 / (sbMax.X - sbMin.X), 2 / (sbMax.Y - sbMin.Y), 2 / (sbMax.Z - sbMin.Z)))
			//                * Matrix4.CreateTranslation(-(sbMax - sbMin) / 2);
			//viewPort = Matrix4.CreateTranslation(screen.width / 2f, screen.height / 2f, 0)
			//                   * Matrix4.CreateScale(screen.width / 2f, screen.height / 2f, 1);
			perspective = Matrix4.CreatePerspectiveFieldOfView(Fov, AspectRatio, ZNear, ZFar);
			
			// create shaders
			Shader shader = new Shader( "../../shaders/vs.glsl", "../../shaders/fs.glsl" );
			// load a texture
			Texture wood = new Texture( "../../assets/wood.jpg" );
			
			// Define hierarchy
			Cam = new Cam(Matrix4.CreateTranslation(0, 0, 0), Matrix4.CreateRotationX(0));
			Smash child = new Smash("../../assets/teapot.obj", Matrix4.CreateTranslation(0, 0, 0), Matrix4.Identity, wood, shader);
			Smash grandChild = new Smash("../../assets/floor.obj", Matrix4.CreateTranslation(10, 0, 0), Matrix4.Identity, wood, shader);
			child.AddChild(grandChild);
			SceneGraph.AddToRoot(child);
		}

		// tick for background surface
		public void Tick()
		{
			screen.Clear( 0 );
			screen.Print( "hello world", 2, 2, 0xffff00 );
		}
		private static void HandleUserInput(float deltaTime)
		{
			var currentKeyboardState = Keyboard.GetState();
			deltaTime /= 1000; //ms

			// Translation
			if (currentKeyboardState[Key.W])
				Cam.Transform.Translate(Cam.Transform.Front * MovementSpeed * deltaTime);
			if (currentKeyboardState[Key.A])
				Cam.Transform.Translate(Cam.Transform.Right * -MovementSpeed * deltaTime);
			if (currentKeyboardState[Key.S])
				Cam.Transform.Translate(Cam.Transform.Front * -MovementSpeed * deltaTime);
			if (currentKeyboardState[Key.D])
				Cam.Transform.Translate(Cam.Transform.Right * MovementSpeed * deltaTime);
			if (currentKeyboardState[Key.Space])
				Cam.Transform.Translate(Cam.Transform.Up * MovementSpeed * deltaTime);
			if (currentKeyboardState[Key.ShiftLeft])
				Cam.Transform.Translate(Cam.Transform.Up * -MovementSpeed * deltaTime);
			
			// Looking around
			if (currentKeyboardState[Key.Up])
				Cam.Transform.Rotate(Matrix4.CreateRotationX(-RotationSpeed * deltaTime));
			if (currentKeyboardState[Key.Left])
				Cam.Transform.Rotate(Matrix4.CreateRotationY(-RotationSpeed * deltaTime));
			if (currentKeyboardState[Key.Down])
				Cam.Transform.Rotate(Matrix4.CreateRotationX(RotationSpeed * deltaTime));
			if (currentKeyboardState[Key.Right])
				Cam.Transform.Rotate(Matrix4.CreateRotationY(RotationSpeed * deltaTime));
			
			// Tilting
			if (currentKeyboardState[Key.Q])
				Cam.Transform.Rotate(Matrix4.CreateRotationZ(-RotationSpeed * deltaTime));
			if (currentKeyboardState[Key.E])
				Cam.Transform.Rotate(Matrix4.CreateRotationZ(RotationSpeed * deltaTime));
		}
		// tick for OpenGL rendering code
		public void RenderGL()
		{
			// measure frame duration
			float frameDuration = timer.ElapsedMilliseconds;
			timer.Restart();

			// update rotation
			//a += 0.001f * frameDuration;
			//if( a > 2 * PI ) a -= 2 * PI;

			HandleUserInput(frameDuration);

			if( useRenderTarget )
			{
				// enable render target
				target.Bind();

				// render scene to render target
				//mesh.Render( shader, Tpot * Cam.Transform.FullMatrix * Tview, wood );
				//floor.Render( shader, Tfloor * Cam.Transform.FullMatrix * Tview, wood );
				SceneGraph.Render();
				
				// render quad
				target.Unbind();
				quad.Render( postproc, target.GetTextureID() );
			}
			else
			{
				// render scene directly to the screen
				//mesh.Render( shader, Tpot * Cam.Transform.FullMatrix * Tview, wood );
				//floor.Render( shader, Tfloor * Cam.Transform.FullMatrix * Tview, wood );
				//SceneGraph.Render();
			}
		}
	}
}