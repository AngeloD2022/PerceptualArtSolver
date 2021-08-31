using System;
using Microsoft.Xna.Framework;

namespace PerceptualArtSolver
{
    public class Square : WorldObject
    {
        private static readonly SquareModel Model;

        static Square()
        {
            Model = new SquareModel();
        }

        public override void Draw(Camera camera)
        {
            Model.Draw(Matrix.CreateTranslation(Position), camera);
        }

        public override void Update(float time)
        {
            throw new NotImplementedException();
        }
    }
}