using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PerceptualArtSolver
{
    internal class Cube : WorldObject
    {
        public static CubeModel Model;
        public Texture2D Texture;

        static Cube()
        {
            Model = new CubeModel();
        }

        public override void Draw(Camera camera)
        {
            if (Texture != null)
            {
                CubeModel.Effect.TextureEnabled = true;
                CubeModel.Effect.Texture = Texture;
            }
            else
            {
                CubeModel.Effect.TextureEnabled = false;
            }

            Model.Draw(Matrix.CreateTranslation(Position), camera);
        }

        public override void Update(float time)
        {
            throw new NotImplementedException();
        }
    }
}