using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PerceptualArtSolver
{
    public class DynamicModel : IModel
    {
        public BasicEffect Effect;
        private List<short> ibuf;
        public IndexBuffer indexBuffer;
        private List<VertexPositionNormalTexture> vbuf;
        public VertexBuffer vertexBuffer;

        public void Draw(Matrix world, Camera camera)
        {
            Effect.World = world;
            Effect.View = camera.GetView();
            Effect.Projection = camera.GetProjection();
            Effect.CurrentTechnique.Passes[0].Apply();

            if (vertexBuffer != null)
            {
                Effect.GraphicsDevice.SetVertexBuffer(vertexBuffer);
                Effect.GraphicsDevice.Indices = indexBuffer;
                Effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                    indexBuffer.IndexCount / 3);
            }
            else
            {
                Effect.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vbuf.ToArray(), 0,
                    vbuf.Count, ibuf.ToArray(), 0, ibuf.Count / 3);
            }
        }

        public void Bake(GraphicsDevice graphicsDevice)
        {
            var vbuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), vbuf.Count,
                BufferUsage.WriteOnly);
            vbuffer.SetData(vbuf.ToArray());
            var ibuffer = new IndexBuffer(graphicsDevice, typeof(short), ibuf.Count, BufferUsage.WriteOnly);
            ibuffer.SetData(ibuf.ToArray());
        }


        protected bool IsInTriangle(Vector3 point, int triangle, float threshold = 0.001f)
        {
            var triangleVertexA = vbuf[ibuf[triangle * 3]].Position;
            var triangleVertexB = vbuf[ibuf[triangle * 3 + 1]].Position;
            var triangleVertexC = vbuf[ibuf[triangle * 3 + 2]].Position;

            var sideA = triangleVertexB - triangleVertexA;
            var sideB = triangleVertexC - triangleVertexB;
            var sideC = triangleVertexA - triangleVertexC;

            var a = point - sideA;
            var b = point - sideB;
            var c = point - sideC;

            var xa = Vector3.Cross(sideA, a);
            var xb = Vector3.Cross(sideB, b);
            var xc = Vector3.Cross(sideC, c);

            var ab = Vector3.Dot(xa, xb);
            var bc = Vector3.Dot(xb, xc);
            var ca = Vector3.Dot(xc, xa);

            return ab * ab >= xa.LengthSquared() * xb.LengthSquared() - threshold &&
                   bc * bc >= xb.LengthSquared() * xc.LengthSquared() - threshold &&
                   ca * ca >= xc.LengthSquared() * xa.LengthSquared() - threshold;
        }

        protected short AddVertex(VertexPositionNormalTexture vertex)
        {
            vbuf.Add(vertex);
            return (short) (vbuf.Count - 1);
        }

        protected void AddTriangle(short index1, short index2, short index3)
        {
            ibuf.Add(index1);
            ibuf.Add(index2);
            ibuf.Add(index3);
        }

        protected void RemoveTriangle(int triangle)
        {
            ibuf.RemoveRange(triangle * 3, 3);
        }

        protected void SplitTriangle(Vector3 point, int triangle)
        {
            RemoveTriangle(triangle);

            vbuf.Add(new VertexPositionNormalTexture(point, new Vector3(0, 0, 0), new Vector2(0, 0)));

            Vector3 sideA = vbuf[ibuf[triangle * 3 + 1]].Position - vbuf[ibuf[triangle * 3]].Position;
            Vector3 sideB = vbuf[ibuf[triangle * 3 + 2]].Position - vbuf[ibuf[triangle * 3 + 1]].Position;
            Vector3 sideC = vbuf[ibuf[triangle * 3]].Position - vbuf[ibuf[triangle * 3 + 2]].Position;

            if (Vector3.Cross(sideA, point).LengthSquared() > 0)
            {
                AddTriangle((short) (vbuf.Count - 1), (short) (triangle * 3), (short) (triangle * 3 + 1));
            }

            if (Vector3.Cross(sideB, point).LengthSquared() > 0)
            {
                AddTriangle((short) (triangle * 3 + 2), (short) (vbuf.Count - 1), (short) (triangle * 3 + 1));
            }

            if (Vector3.Cross(sideC, point).LengthSquared() > 0)
            {
                AddTriangle((short) (triangle * 3 + 2), (short) (triangle * 3), (short) (vbuf.Count - 1));
            }
            
        }
    }
}