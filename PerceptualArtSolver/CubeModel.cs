using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PerceptualArtSolver
{
    public class CubeModel : IModel
    {
        public static BasicEffect Effect;
        public readonly short[] ibuf;
        public readonly VertexPositionNormalTexture[] vbuf;

        public CubeModel()
        {
            ibuf = new short[36]
            {
                // +0, +1, +4, +2, +3
                0, 1, 2, 1, 3, 2,
                4, 5, 6, 5, 7, 6,
                8, 9, 10, 9, 11, 10,
                12, 13, 14, 13, 15, 14,
                16, 17, 18, 17, 19, 18,
                20, 21, 22, 21, 23, 22
            };

            vbuf = new VertexPositionNormalTexture[24]
            {
                // Back
                new VertexPositionNormalTexture(new Vector3(-1, 1, 1), new Vector3(0, 0, 1), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(1, 1, 1), new Vector3(0, 0, 1), new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, 1), new Vector3(0, 0, 1), new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(1, -1, 1), new Vector3(0, 0, 1), new Vector2(1, 1)),

                // front
                new VertexPositionNormalTexture(new Vector3(1, 1, -1), Vector3.Forward, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(-1, 1, -1), Vector3.Forward, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(1, -1, -1), Vector3.Forward, new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, -1), Vector3.Forward, new Vector2(1, 1)),

                // left
                new VertexPositionNormalTexture(new Vector3(-1, 1, -1), Vector3.Left, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(-1, 1, 1), Vector3.Left, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, -1), Vector3.Left, new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, 1), Vector3.Left, new Vector2(1, 1)),

                new VertexPositionNormalTexture(new Vector3(1, 1, 1), Vector3.Right, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(1, 1, -1), Vector3.Right, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(1, -1, 1), Vector3.Right, new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(1, -1, -1), Vector3.Right, new Vector2(1, 1)),

                new VertexPositionNormalTexture(new Vector3(-1, -1, 1), Vector3.Down, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(1, -1, 1), Vector3.Down, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, -1), Vector3.Down, new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(1, -1, -1), Vector3.Down, new Vector2(1, 1)),

                // 20, 21, 22, 20, 23, 22
                new VertexPositionNormalTexture(new Vector3(-1, 1, -1), Vector3.Up, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(1, 1, -1), Vector3.Up, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-1, 1, 1), Vector3.Up, new Vector2(0, 1)),

                new VertexPositionNormalTexture(new Vector3(1, 1, 1), Vector3.Up, new Vector2(1, 1))
            };
        }

        public void Draw(Matrix world, Camera camera)
        {
            camera.Apply(Effect);
            Effect.World = world;
            Effect.LightingEnabled = true;
            Effect.EnableDefaultLighting();
            Effect.CurrentTechnique.Passes[0].Apply();
            Effect.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vbuf, 0, 24, ibuf, 0, 12);
        }
    }
}