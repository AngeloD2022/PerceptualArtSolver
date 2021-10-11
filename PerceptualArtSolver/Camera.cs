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
        public float Pitch;
        public Vector3 Position;
        public float Yaw;


        public Camera()
        {
            FieldOfView = MathHelper.PiOver4;
            AspectRatio = 16.0f / 9.0f;
            NearPlane = .1f;
            FarPlane = 100f;
        }

        public Matrix GetWorld()
        {
            return Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw) * Matrix.CreateTranslation(Position);
        }

        public Matrix GetView()
        {
            return Matrix.CreateTranslation(-Position) * Matrix.CreateRotationY(-Yaw) * Matrix.CreateRotationX(-Pitch);
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