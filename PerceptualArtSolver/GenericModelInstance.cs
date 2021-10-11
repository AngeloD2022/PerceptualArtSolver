using Microsoft.Xna.Framework;

namespace PerceptualArtSolver
{
    public class GenericModelInstance : WorldObject
    {
        public IModel Model;

        public GenericModelInstance(IModel model)
        {
            Model = model;
        }

        public override void Draw(Camera camera)
        {
            Model.Draw(Matrix.CreateTranslation(Position), camera);
        }

        public override void Update(float time)
        {
            // throw new System.NotImplementedException();
        }
    }
}