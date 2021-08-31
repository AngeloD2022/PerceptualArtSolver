using Microsoft.Xna.Framework;

namespace PerceptualArtSolver
{
    public abstract class WorldObject : IDrawObject, IUpdateObject
    {
        public Vector3 Position;
        public abstract void Draw(Camera camera);
        public abstract void Update(float time);
    }
}