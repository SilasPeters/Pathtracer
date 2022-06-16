using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Template;

namespace JackNSilo
{
	public class Cam : Pass
	{
		public Matrix4 camMatrix;
		public Vector3 up;
		public Vector3 front;
		public Vector3 right;
		public Vector3 pos; //todo: in transform dit alles doen
		
		public Cam(Matrix4 transform, Vector3 up, Vector3 front, Vector3 right, Vector3 pos, bool enabled = true) : base(new Transform(), null, enabled)
		{
			this.up = up;	
			this.front = front;
			this.right = right;
			this.pos = pos;
		}
	}
	
	public class SceneGraph
	{
		private ISmashable cam;

		public SceneGraph(Cam cam) => this.cam = cam;
		
		public void Render(Matrix4 camMatrix)
		{
			ICollection<Matrix4> sceenSpaces = new Collection<Matrix4>();
			GetAllScreenSpaces(camMatrix, cam, sceenSpaces); // fill screenSpaces
			
			// meer stuff
		}

		private void GetAllScreenSpaces(Matrix4 camMatrix, ISmashable currentSmashable, in ICollection<Matrix4> screenSpaces)
		{
			if (currentSmashable is Smash s)
			{
				Matrix4 screenSpace = s.ModelMatrix * s.Transform.GetWorldSpace() * camMatrix;
				screenSpaces.Add(screenSpace);
			}
			
			foreach (var child in currentSmashable.Children)
				GetAllScreenSpaces(camMatrix, child, screenSpaces);
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
		
		public Smash(string fileName, Transform transform, ISmashable parent, bool enabled = true) : base(fileName) {
			Transform = transform;
			Parent    = parent;
			Enabled   = enabled;
			
			Transform.belongingSmashable = this;
			Parent.Children.Add(this);
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
		public ICollection<ISmashable> Children  { get; set; } // This was not intended
		public bool                    Enabled   { get; set; }

		public Pass(Transform transform, ISmashable parent, bool enabled = true)
		{
			Transform = transform;
			Enabled   = enabled;
			Parent    = parent;
			
			Transform.belongingSmashable = this;
		}
	}

	public interface ISmashable
	{
		Transform               Transform { get; set; }
		ISmashable              Parent    { get; set; }
		ICollection<ISmashable> Children  { get; set; } // This was not intended
		bool                    Enabled   { get; set; }
	}

	public class Transform
	{
		public Matrix4 LocalPos;
		public Matrix4 LocalRotation;
		
		// Fields to calculate world space
		private ulong _frameLastUpdated = ulong.MaxValue;
		private Transform currentWorldSpace;
		public ISmashable belongingSmashable;

		public Transform()
		{
			LocalPos          = Matrix4.Zero;
			LocalRotation     = Matrix4.Zero;
			_frameLastUpdated = Game.CurrentFrame;
		}
		public Transform(Matrix4 locapPos, Matrix4 localRotation)
		{
			LocalPos          = locapPos;
			LocalRotation     = localRotation;
			_frameLastUpdated = Game.CurrentFrame;
		}

		public static Transform operator +(Transform a, Transform b) =>
			new Transform(a.LocalPos + b.LocalPos, a.LocalRotation + b.LocalRotation);

		public void Translate(Matrix4 translation) => LocalPos *= translation;
		public void Rotate(Matrix4 rotation)       => LocalRotation *= rotation;

		public Transform GetWorldSpace()
		{
			if (Game.LastUpdateFrame == _frameLastUpdated) // No need to recalculate, pass old values
			{
				return this.currentWorldSpace;
			}
			else // need to recalculate currentWorldSpace Transform
			{
				_frameLastUpdated = Game.CurrentFrame;
				return currentWorldSpace = this + belongingSmashable.Parent.Transform.GetWorldSpace();
			}
		}
	}
}