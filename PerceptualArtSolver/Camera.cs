using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PerceptualArtSolver
{
    public class Camera
    {
        public float AspectRatio;
        public float FarPlane;
        public float FieldOfView;
        public float NearPlane;
        public Vector3 Position;

        public Camera()
        {
            FieldOfView = MathHelper.PiOver4;
            AspectRatio = 16.0f / 9.0f;
            NearPlane = .1f;
            FarPlane = 100f;
        }

        public Matrix GetView()
        {
            return Matrix.CreateTranslation(-Position);
        }

        public Matrix GetProjection()
        {
            return Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
        }

        public void Apply(BasicEffect effect)
        {
            effect.View = GetView();
            effect.Projection = GetProjection();
        }
    }
}