using OpenTK;
using OpenTK.Input;
using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Template
{
	class MyApplication
	{
		public Surface screen;
		private DateTime timeStart;
		private double t;

		private float[,] heightMap;
		private Color[] alternatingColors = { Color.CornflowerBlue, Color.Black };
		private int huidigeKleur = 0;
		private float[] vertexData;
		private float[] vertexColor;

		//shaders
		private int programID;
		private int vertexShaderID;
		private int fragmentShaderID;
		private int attribute_vpos;
		private int attribute_vcol;
		private int uniform_mview;
		private int attribute_color;

		public void Init()
		{
			timeStart = DateTime.Now;

			//load heightmap
			Surface source = new Surface("../../assets/coin.png");
			heightMap = new float[256, 256];
			for (int y = 0; y < 256; y++) for (int x = 0; x < 256; x++)
					heightMap[x, y] = ((float)(source.pixels[x + y * 256] & 255)) / 256;
			vertexData = new float[255 * 255 * 4 * 3];
			vertexColor = new float[255 * 255 * 4 * 3];
		}
		public void Tick()
		{
			screen.Clear(0);
			t = (DateTime.Now - timeStart).Duration().TotalSeconds;
			float rotationAmount = (float)t / 6;

			Matrix4 M = Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), rotationAmount);
			M *= Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), 1.9f);
			M *= Matrix4.CreateTranslation(0, 0, -1);
			M *= Matrix4.CreatePerspectiveFieldOfView(1.6f, 1.3f, .1f, 1000);
			GL.UseProgram(programID);
			GL.UniformMatrix4(uniform_mview, false, ref M);
		}
		public void LoadShaders()
		{
			programID = GL.CreateProgram();
			loadShader("../../shaders/vs.glsl", ShaderType.VertexShader, programID, out vertexShaderID);
			loadShader("../../shaders/fs.glsl", ShaderType.FragmentShader, programID, out fragmentShaderID);
			GL.LinkProgram(programID);

			//vertex shader
			attribute_vpos = GL.GetAttribLocation(programID, "vPosition");
			attribute_vcol = GL.GetAttribLocation(programID, "vColor");
			uniform_mview = GL.GetUniformLocation(programID, "M");

			//fragment shader
			attribute_color = GL.GetAttribLocation(programID, "color");
		}
		void loadShader(string name, ShaderType type, int program, out int ID)
		{
			ID = GL.CreateShader(type);
			using (System.IO.StreamReader sr = new System.IO.StreamReader(name))
				GL.ShaderSource(ID, sr.ReadToEnd());
			GL.CompileShader(ID);
			GL.AttachShader(program, ID);
			Console.WriteLine(GL.GetShaderInfoLog(ID));
		}

		public void RenderGL()
		{
			GL.Clear(ClearBufferMask.ColorBufferBit);
			
			heightmapRendererLegacy(0f, 0f, 0f, 0.15f);

			void heightmapRendererBuffer(float centerX, float centerY, float centerZ, float depthScale)
			{
				//prepare buffer
				int VBO = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
				GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * 4, vertexData, BufferUsageHint.StaticDraw);
				GL.EnableClientState(ArrayCap.VertexArray);
				GL.VertexPointer(3, VertexPointerType.Float, 12, 0);

				//generate vertices
				int i = 0;
				for (int y = 0; y < heightMap.GetLength(1) - 1; y++) for (int x = 0; x < heightMap.GetLength(0) - 1; x++)
					for (int yOffset = 0; yOffset < 2; yOffset++) for (int xOffset = 0; xOffset < 2; xOffset++) {
						vertexData[i++] = HMToObjX(x + Math.Abs(xOffset - yOffset)) + centerX;
						vertexData[i++] = HMToObjY(y + yOffset) + centerY;
						vertexData[i++] = heightMap[x + Math.Abs(xOffset - yOffset), y + yOffset] * depthScale + centerZ;
					} //patroon: [x, y], [x+1, y], [x+1, y+1], [x, y+1]
				
				//draw buffer
				GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
				GL.DrawArrays(PrimitiveType.Quads, 0, vertexData.Length);

				//generate colors
				for (int c = 0; c < vertexColor.Length; c++)
					vertexColor[c] = 0x00ffff;

				//shaders
				int vbo_pos = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_pos);
				GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * 4, vertexData, BufferUsageHint.StaticDraw );
				//GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0 );
				//GL.EnableVertexAttribArray(attribute_vpos);
				//GL.DrawArrays(PrimitiveType.Quads, 0, vertexData.Length);

				int vbo_col = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_col);
				GL.BufferData(BufferTarget.ArrayBuffer, vertexColor.Length * 4, vertexColor, BufferUsageHint.StaticDraw );
				//GL.VertexAttribPointer(attribute_vcol, 3, VertexAttribPointerType.Float, false, 0, 0 );
				//GL.EnableVertexAttribArray(attribute_vcol);
				//GL.DrawArrays(PrimitiveType.Quads, 0, vertexColor.Length);

				//==========>
				GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_col);
				GL.ColorPointer(4, ColorPointerType.Float, 0, 0);
				GL.EnableClientState(ArrayCap.ColorArray);

				GL.EnableClientState(ArrayCap.VertexArray);
				GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_pos);
				GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
				GL.DrawArrays(PrimitiveType.Quads, 0, vertexData.Length);
			}

			void heightmapRendererLegacy(float centerX, float centerY, float centerZ, float depthScale)
			{
				huidigeKleur = 0;
				double rotationAmount = t / 6;
				var M = Matrix4.CreatePerspectiveFieldOfView(1.6f, 1.3f, .1f, 1000);
				GL.LoadMatrix(ref M);
				GL.Translate(0, 0, -1);
				GL.Rotate(-20, 1, 0, 0);
				GL.Rotate(rotationAmount * 180 / 3.141593, 0, 0.05, 1);

				for (int y = 0; y < heightMap.GetLength(1) - 1; y++) for (int x = 0; x < heightMap.GetLength(0) - 1; x++)
						//if (heightMap[y, x] != 0)
						drawShape3D(PrimitiveType.Quads, alternatingColors[huidigeKleur++ % 2],
							new Vector3(HMToObjX(x) + centerX, HMToObjY(y) + centerY, heightMap[x, y] * depthScale + centerZ),
							new Vector3(HMToObjX(x + 1) + centerX, HMToObjY(y) + centerY, heightMap[x + 1, y] * depthScale + centerZ),
							new Vector3(HMToObjX(x + 1) + centerX, HMToObjY(y + 1) + centerY, heightMap[x + 1, y + 1] * depthScale + centerZ),
							new Vector3(HMToObjX(x) + centerX, HMToObjY(y + 1) + centerY, heightMap[x, y + 1] * depthScale + centerZ));

			}

			void drawShape3D(PrimitiveType type, Color c, params Vector3[] vertices)
			{
				GL.Color3(c);
				GL.Begin(type);
				foreach (var v in vertices)
					GL.Vertex3(v);
			}
			///<summary>Methode voor het omzetten van screencoordinates naar object-space coordinaten</summary>
			float HMToObjX(int x) => (float)x / heightMap.GetLength(0) * 2 - 1f;
			///<summary>Methode voor het omzetten van screencoordinates naar object-space coordinaten</summary>
			float HMToObjY(int y) => (float)y / heightMap.GetLength(1) * 2 - 1f;
		}

		private void spinningRectangle()
		{
			Vector2 center = new Vector2(0.5f, 0);
			float radius = 0.5f;
			float rotationSpeed = 4f;

			Vector2[] Hoeken = { Corner(0.275f, 0.25f),
								 Corner(0.525f, 0.25f),
								 Corner(0.275f, 0.416f),
								 Corner(0.525f, 0.416f) };

			screen.Line(TX(Hoeken[0].X, screen.width), TY(Hoeken[0].Y, screen.width), TX(Hoeken[1].X, screen.width), TY(Hoeken[1].Y, screen.width), 0xffffff);
			screen.Line(TX(Hoeken[1].X, screen.width), TY(Hoeken[1].Y, screen.width), TX(Hoeken[3].X, screen.width), TY(Hoeken[3].Y, screen.width), 0xffffff);
			screen.Line(TX(Hoeken[0].X, screen.width), TY(Hoeken[0].Y, screen.width), TX(Hoeken[2].X, screen.width), TY(Hoeken[2].Y, screen.width), 0xffffff);
			screen.Line(TX(Hoeken[2].X, screen.width), TY(Hoeken[2].Y, screen.width), TX(Hoeken[3].X, screen.width), TY(Hoeken[3].Y, screen.width), 0xffffff);

			Vector2 Corner(float x, float y)
			{
				float rx = center.X + radius * (float)(x * Math.Cos(t * rotationSpeed) - y * Math.Sin(t * rotationSpeed));
				float ry = center.Y + radius * (float)(x * Math.Sin(t * rotationSpeed) + y * Math.Cos(t * rotationSpeed));
				return new Vector2(rx, ry);
			}

			int TX(float x, int screenWidth, float centerOffset = 0) => (int)(screenWidth / 2 * (x + 1 + centerOffset));
			int TY(float y, int screenWidth, float centerOffset = 0) => TX(-y, screenWidth, centerOffset);
		}

		private void yellowPipe(int Y, int Height, int color)
		{
			for (int y = Y; y <= Y + Height; y++)
			{
				int centerY = Height / 2;
				int distanceFromCentre = Math.Abs(centerY - (y - Y));
				float transparency = (centerY - distanceFromCentre) / (float)centerY;

				byte[] rgb = splitRGB(color);
				int c = this.color((byte)(rgb[0] * transparency), (byte)(rgb[1] * transparency), (byte)(rgb[2] * transparency));
				screen.Line(0, y, screen.width - 1, y, c);
			}
		}

		private void bluePipe()
		{
			int Y = 100;
			int Height = 200;

			for (int y = Y; y <= Y + Height; y++)
			{
				int centerY = Height / 2;
				int distanceFromCentre = Math.Abs(centerY - (y - Y));

				byte blue = (byte)(255 * ((centerY - distanceFromCentre) / (float)centerY));
				screen.Line(0, y, screen.width - 1, y, blue);
			}
		}

		private int color(byte r, byte g, byte b) => (r << 16) | (g << 8) | b;
		private byte[] splitRGB(int color) => new byte[] { (byte)(color >> 16), (byte)(color >> 8), (byte)color };

		private void wiskundeBros()
		{
			Vector2 topleft = new Vector2(100, 100);
			Vector2 boxPadding = new Vector2(5, 5);
			Vector2 boxSize = new Vector2(385, 16);
			Vector2 rotationArea = new Vector2(200, 200);
			double rotationSpeedGeneral = 1f;

			double rotationSpeedX = rotationSpeedGeneral * 1.0;
			double rotationSpeedY = rotationSpeedGeneral * 1.0;

			Vector2 pos = new Vector2(rotationArea.X / 2 * (1 + (float)Math.Cos(t * rotationSpeedX)),
									  rotationArea.Y / 2 * (1 + (float)Math.Sin(t * rotationSpeedY)));

			screen.Print($"t: {t}", 5, 5, 0xffffff);
			screen.Print("Wiskunde bros - " + DateTime.Now.TimeOfDay.ToString(),
						(int)(topleft.X + pos.X),
						(int)(topleft.Y + pos.Y),
						0xffffff);
			screen.Box((int)(topleft.X + pos.X - boxPadding.X),
					   (int)(topleft.Y + pos.Y - boxPadding.Y),
					   (int)(topleft.X + pos.X + boxPadding.X + boxSize.X),
					   (int)(topleft.Y + pos.Y + boxPadding.Y + boxSize.Y),
					   0xff0000);
		}

		private void debug()
		{
			int mouseX = Math.Min(Math.Max(Mouse.GetState().X, 0), screen.width - 1);
			int mouseY = Math.Min(Math.Max(Mouse.GetState().Y, 0), screen.height - 1);

			int colorAtCursor = screen.pixels[mouseX + mouseY * screen.width];
			screen.Print($"x: {mouseX}, y: {mouseY}", 5, 5, 0xffffff);
			screen.Print($"Pixel: {colorAtCursor}", 5, 30, 0xffffff);
		}
	}
}