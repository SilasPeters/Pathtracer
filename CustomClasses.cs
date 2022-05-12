using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Raytracer
{
    internal class CustomClasses
    {
        #region Rays
        public abstract class Ray
        {
            public Vector3 EntryPoint { get; }
            public Vector3 DdirectionVect { get; }

            public Ray(Vector3 entryPoint, Vector3 direction)
            {
                this.EntryPoint = entryPoint;
                this.DdirectionVect = direction;
            }
        }

        public class LightRay : Ray
        {
            float freq;

            public LightRay(Vector3 entryPoint, Vector3 direction) : base(entryPoint, direction)
            {

            }
        }
        public class ViewRay : Ray
        {
            public ViewRay(Vector3 entryPoint, Vector3 direction) : base(entryPoint, direction)
            {

            }
        }
        public class ShadowRay : Ray
        {
            public ShadowRay(Vector3 entryPoint, Vector3 direction) : base(entryPoint, direction)
            {

            }
        }

        #endregion Rays
        #region Lights
        public class LightSource
        {
            public Vector3 Pos { get; }
            public float Intensity { get; }
            public float Freq { get; }

            public LightSource(Vector3 pos, float intensity, float freq)
            {
                this.Pos = pos;
                this.Intensity = intensity;
                this.Freq = freq;
            }
        }
        #endregion Lights
        #region Objects
        public abstract class Object
        {
            public Object()
            {

            }
        }
        public class Sphere
        {
            public Vector3 pos { get; }
            public Sphere()
            {

            }
        }
        public class Cube
        {
            public Vector3[] vertices { get; }
            public Cube()
            {

            }
        }
        #endregion Objects
    }
}
