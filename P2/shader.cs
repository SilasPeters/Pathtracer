using System;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace Template
{
	public class Shader
	{
		// data members
		public int programID, vsID, fsID;
		public int attribute_vpos;
		public int attribute_vnrm;
		public int attribute_vuvs;
		public int uniform_mview;
		public int attribute_fuv;
		public int attribute_fnrm;
		public int uniform_pixels;
		public int test;

		// constructor
		public Shader( String vertexShader, String fragmentShader )
		{
			// compile shaders
			programID = GL.CreateProgram();
			Load( vertexShader, ShaderType.VertexShader, programID, out vsID );
			Load( fragmentShader, ShaderType.FragmentShader, programID, out fsID );
			GL.LinkProgram( programID );
			Console.WriteLine( GL.GetProgramInfoLog( programID ) );

			// Vertex shader
			attribute_vpos = GL.GetAttribLocation( programID, "vertexPosition" );
			attribute_vnrm = GL.GetAttribLocation( programID, "vertexNormal" );
			attribute_vuvs = GL.GetAttribLocation( programID, "vertexUV" );
			uniform_mview  = GL.GetUniformLocation( programID, "objectToScreen" );
			
			// Fragment shader
			attribute_fuv  = GL.GetAttribLocation( programID, "uv" );
			attribute_fnrm = GL.GetAttribLocation( programID, "normal" );
			uniform_pixels = GL.GetUniformLocation( programID, "pixels" );
			test           = GL.GetAttribLocation( programID, "vertexColor" );
		}

		// loading shaders
		void Load( String filename, ShaderType type, int program, out int ID )
		{
			// source: http://neokabuto.blogspot.nl/2013/03/opentk-tutorial-2-drawing-triangle.html
			ID = GL.CreateShader( type );
			using( StreamReader sr = new StreamReader( filename ) ) GL.ShaderSource( ID, sr.ReadToEnd() );
			GL.CompileShader( ID );
			GL.AttachShader( program, ID );
			Console.WriteLine( GL.GetShaderInfoLog( ID ) );
		}
	}
}
