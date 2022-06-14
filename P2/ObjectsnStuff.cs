using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using Template;

namespace JackNSilo
{
	public class Texture
	{
	}

	public class Shader
	{
	}

	public class Triangle
	{
		protected Vector3[] vertices;
		protected Vector3 Normal;

		public Triangle(params Vector3[] vertices)
		{
			if (vertices.Length != 3) throw new Exception("Spin meee!");
			this.vertices = vertices;
		}
	}
}