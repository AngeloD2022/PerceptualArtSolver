using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PerceptualArtSolver
{
    public class SquareModel : IModel
    {
        public static BasicEffect Effect;
        public VertexPositionColor[] vbuf;

        public SquareModel()
        {
            vbuf = new VertexPositionColor[4];
            vbuf[0] = new VertexPositionColor(new Vector3(-1, 1, 0), Color.Blue);
            vbuf[1] = new VertexPositionColor(new Vector3(1, 1, 0), Color.Red);
            vbuf[2] = new VertexPositionColor(new Vector3(-1, -1, 0), Color.Green);
            vbuf[3] = new VertexPositionColor(new Vector3(1, -1, 0), Color.Black);
        }

        public void Draw(Matrix world, Camera camera)
        {
            camera.Apply(Effect);
            Effect.World = world;
            Effect.VertexColorEnabled = true;
            Effect.CurrentTechnique.Passes[0].Apply();
            Effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vbuf, 0, 2);
        }
    }
}