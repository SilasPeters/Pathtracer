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
		public Cam(Matrix4 localPos, Matrix4 localRotation, bool enabled = true)
			: base(localPos, localRotation, enabled)
		{
		}
	}
	
	public static class SceneGraph
	{
		private static ISmashable root = Game.Cam;
		
		public static void Render(Matrix4 toScreenConverter)
		{
			ICollection<Matrix4> worldSpaces = new Collection<Matrix4>();
			GetAllWorldSpaces(Matrix4.Identity, root, in worldSpaces); // fills screenSpaces
			ICollection<Matrix4> screenSpaces = ConvertToScreenSpace(in worldSpaces, toScreenConverter);
			
			
		}

		private static void GetAllWorldSpaces(Matrix4 parentWorldSpace, ISmashable currentSmashable, in ICollection<Matrix4> screenSpaces)
		{
			parentWorldSpace = currentSmashable.Transform.GetWorldSpace(parentWorldSpace);
			
			if (currentSmashable is Smash)
				screenSpaces.Add(parentWorldSpace);

			foreach (var child in currentSmashable.Children)
				GetAllWorldSpaces(parentWorldSpace, child, screenSpaces);
		}

		private static ICollection<Matrix4> ConvertToScreenSpace(in ICollection<Matrix4> worldSpaces, Matrix4 converter)
		{
			ICollection<Matrix4> screenSpaces = new Collection<Matrix4>();
			foreach (var worldSpace in worldSpaces)
				screenSpaces.Add(converter * worldSpace);
			return screenSpaces;
		}

		public static void AddToRoot(ISmashable addition) => root.AddChild(addition);
	}

	public class Smash : Mesh, ISmashable
	{
		// Native fields of a general mesh
		public Texture Texture;
		public Shader Shader;
		
		// Fields for implementing ISmashable
		public Transform               Transform { get; set; }
		public ICollection<ISmashable> Children  { get; set; } // This was not intended
		public bool                    Enabled   { get; set; }

		public Smash(string fileName, bool enabled = true) : base(fileName)
		{
			Enabled   = enabled;
			
			Children  = new Collection<ISmashable>();
			Transform = new Transform();
		}
		public Smash(string fileName, Matrix4 localPos, Matrix4 localRotation, bool enabled = true) : base(fileName)
		{
			Enabled = enabled;
			
			Children  = new Collection<ISmashable>();
			Transform = new Transform(localPos, localRotation);
		}

		public void AddChild(ISmashable addition) => Children.Add(addition);
	}

	public class Pass : ISmashable
	{
		public Transform               Transform { get; set; }
		public ICollection<ISmashable> Children  { get; set; } // This was not intended
		public bool                    Enabled   { get; set; }

		public Pass(bool enabled = true)
		{
			Enabled = enabled;
			
			Children  = new Collection<ISmashable>();
			Transform = new Transform();
		}
		public Pass(Matrix4 localPos, Matrix4 localRotation, bool enabled = true)
		{
			Enabled = enabled;
			
			Children  = new Collection<ISmashable>();
			Transform = new Transform(localPos, localRotation);
		}
		
		public void AddChild(ISmashable addition) => Children.Add(addition);
	}

	public interface ISmashable
	{
		Transform               Transform { get; set; }
		ICollection<ISmashable> Children  { get; set; } // This was not intended
		bool                    Enabled   { get; set; }

		void AddChild(ISmashable addition);
	}

	public class Transform //todo: struct?
	{
		public Matrix4 LocalTranslation { get; private set; }
		public Matrix4 LocalRotation    { get; private set; }

		public Matrix4 FullMatrix          => LocalTranslation * LocalRotation;
		public Vector3 LocalPosVector      => LocalTranslation.ExtractTranslation();
		public Vector3 LocalRotationVector => LocalRotation.ExtractRotation().Xyz;

		// source: https://gamedev.stackexchange.com/questions/104862/how-to-find-the-up-direction-of-the-view-matrix-with-glm
		public Vector3 Right => -LocalRotation.Column0.Xyz;
		public Vector3 Up    => -LocalRotation.Column1.Xyz;
		public Vector3 Front => LocalRotation.Column2.Xyz;

		public Transform() {
			LocalTranslation      = Matrix4.Identity;
			LocalRotation = Matrix4.Identity;
		}
		public Transform(Matrix4 localTranslation, Matrix4 localRotation) {
			LocalTranslation      = localTranslation;
			LocalRotation = localRotation;
		}

		public void Translate(Matrix4 translation) => LocalTranslation *= translation;
		public void Translate(Vector3 translation) => LocalTranslation *= Matrix4.CreateTranslation(translation);
		public void Rotate(Matrix4 rotation)       => LocalRotation *= rotation;
		public void Rotate(Quaternion rotation)    => LocalRotation *= Matrix4.CreateFromQuaternion(rotation);
		
		public Matrix4 GetWorldSpace(Matrix4 parentWorldSpace) => parentWorldSpace * FullMatrix;
		
		//return Matrix4.CreateTranslation((baseMatrix * LocalPos).ExtractTranslation()) *
		//       Matrix4.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2);
	}
}