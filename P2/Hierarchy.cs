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
		public static Cam root = Game.Cam;
		
		private static ICollection<Smash> convertedSmashes = new Collection<Smash>();
		
		public static void Render(Game g)
		{
			convertedSmashes.Clear();
			StoreAllWorldSpaces(Matrix4.Identity, root); // fills convertedSmashes

			foreach (var Smosh in convertedSmashes)
				Smosh.Render(g);
		}

		private static void StoreAllWorldSpaces(Matrix4 parentWorldSpace, ISmashable currentSmashable)
		{
			parentWorldSpace = currentSmashable.Transform.GetWorldSpace(parentWorldSpace);

			if (currentSmashable is Smash s) {
				s.LastWorldSpace = parentWorldSpace;
				convertedSmashes.Add(s);
			}

			foreach (var child in currentSmashable.Children)
				StoreAllWorldSpaces(parentWorldSpace, child);
		}

		//private static void ConvertToScreenSpace(Matrix4 converter)
		//{
		//	foreach (var worldSmash in convertedSmashes)
		//		worldSmash.LastWorldSpace *= converter;
		//}

		public static void AddToRoot(ISmashable addition) => root.AddChild(addition);
	}

	public class Smash : Mesh, ISmashable
	{
		// Native fields of a general mesh
		public Template.Texture Texture;
		public Template.Shader Shader;
		public Matrix4 LastWorldSpace;
		
		// Fields for implementing ISmashable
		public Transform               Transform { get; set; }
		public ICollection<ISmashable> Children  { get; set; } // This was not intended
		public bool                    Enabled   { get; set; }

		public Smash(string fileName, bool enabled = true) : base(fileName)
		{
			Enabled   = enabled;
			
			Children  = new Collection<ISmashable>();
			Transform = new Transform();
			Texture   = new Template.Texture( "../../assets/wood.jpg" );
			Shader    = new Template.Shader("../../shaders/vs.glsl", "../../shaders/fs.glsl");
		}
		public Smash(string fileName, Matrix4 localPos, Matrix4 localRotation, Template.Texture texture, Template.Shader shader, bool enabled = true) : base(fileName)
		{
			Enabled = enabled;
			
			Children  = new Collection<ISmashable>();
			Transform = new Transform(localPos, localRotation);
			Texture   = texture;
			Shader    = shader;
		}

		public void Render(Game g)
		{
			//Console.WriteLine(Game.ortho * LastWorldSpace * Game.perspective);
			base.Render(Shader, LastWorldSpace * Game.perspective ,Transform.FullMatrix, Texture, g);
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
		public Vector3 Front =>  LocalRotation.Column2.Xyz;

		public Transform() {
			LocalTranslation = Matrix4.Identity;
			LocalRotation    = Matrix4.Identity;
		}
		public Transform(Matrix4 localTranslation, Matrix4 localRotation) {
			LocalTranslation = localTranslation;
			LocalRotation    = localRotation;
		}

		public void Translate(Matrix4 translation) => LocalTranslation *= translation;
		public void Translate(Vector3 translation) => LocalTranslation *= Matrix4.CreateTranslation(translation);
		public void Rotate(Matrix4 rotation)       => LocalRotation *= rotation;
		public void Rotate(Quaternion rotation)    => LocalRotation *= Matrix4.CreateFromQuaternion(rotation);
		
		public Matrix4 GetWorldSpace(Matrix4 parentWorldSpace) => FullMatrix * parentWorldSpace;
		
		//return Matrix4.CreateTranslation((baseMatrix * LocalPos).ExtractTranslation()) *
		//       Matrix4.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2);
	}
	public class LightSource 
	{
		Smash Smash;
		public Vector3 Pos { get; }
		public float Radius { get; }
		public Vector3 Color { get; }

		/// <param name="radius">Only used for the debugcam</param>
        public LightSource(Vector3 pos, Vector3 color, float radius = 0.2f, Smash smash = null)
        {
            this.Pos = pos;
            this.Color = color * 30; //todo
            this.Radius = radius;
            Smash = smash;
        }
    }
}