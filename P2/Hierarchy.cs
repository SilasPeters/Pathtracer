﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using Template;

namespace JackNSilo
{
	public class Cam : Pass
	{
		public Matrix4 transform;
		public Vector3 up;
		public Vector3 front;
		public Vector3 pos;
		public Cam(Matrix4 transform, Vector3 up, Vector3 front, Vector3 pos, bool enabled = true) : base(null, enabled)
		{
			this.transform = transform;
			this.up = up;	
			this.front = front;
			this.pos = pos;
		}
	}
	
	public class SceneGraph
	{
		private ISmashable Camera = new Pass(null); // root
		
		public void Render(Matrix4 camMatrix)
		{
			
		}
	}

	public class Smash : Mesh, ISmashable
	{
		// Native fields of a general mesh
		public Texture Texture;
		public Shader Shader;
		public Matrix4 ModelMatrix;
		
		// Fields for implementing ISmashable
		public Transform               Transform { get; set; }
		public ISmashable              Parent    { get; set; }
		public ICollection<ISmashable> Children  { get; set; } // This was not intended
		public bool                    Enabled   { get; set; }
		
		public Smash(string fileName, ISmashable parent, bool enabled = true) : base(fileName) {
			Parent  = parent;
			Parent.Children.Add(this);
			Enabled = enabled;
		}

		public void Kill() {
			Parent.Children.Remove(this); // Garbage collector will move every child to an orphanage
			Enabled = false;
		}
	}

	public class Pass : ISmashable
	{
		public Transform               Transform { get; set; }
		public ISmashable              Parent    { get; set; }
		public ICollection<ISmashable> Children  { get; set; }
		public bool                    Enabled   { get; set; }

		public Pass(ISmashable parent, bool enabled = true) {
			Enabled = enabled;
			Parent  = parent;
		}
	}

	public interface ISmashable
	{
		Transform               Transform { get; set; }
		ISmashable              Parent    { get; set; }
		ICollection<ISmashable> Children  { get; set; } // This was not intended
		bool                    Enabled   { get; set; }
	}

	public struct Transform
	{
		public Matrix4 localPos;
		public Matrix4 localRotation;

		public void Translate(Matrix4 translation) => localPos *= translation;
		public void Rotate(Matrix4 rotation)       => localRotation *= rotation;
	}
}