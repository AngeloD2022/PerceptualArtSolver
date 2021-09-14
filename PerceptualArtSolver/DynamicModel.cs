﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace PerceptualArtSolver
{
    public class DynamicModel : IModel
    {
        public BasicEffect Effect;
        public List<short> ibuf;
        public IndexBuffer indexBuffer;
        public List<VertexPositionNormalTexture> vbuf;
        public VertexBuffer vertexBuffer;

        public DynamicModel()
        {
            ibuf = new List<short>();
            vbuf = new List<VertexPositionNormalTexture>();
        }

        public void Draw(Matrix world, Camera camera)
        {
            Effect.World = world;
            Effect.View = camera.GetView();
            Effect.Projection = camera.GetProjection();
            Effect.CurrentTechnique.Passes[0].Apply();

            RasterizerState backup = Effect.GraphicsDevice.RasterizerState;

            Effect.GraphicsDevice.RasterizerState = new RasterizerState() {FillMode = FillMode.WireFrame};

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
                Effect.GraphicsDevice.RasterizerState = backup;
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

        public bool IsInTriangle(Vector3 point, int triangle, float threshold = 0.001f)
        {
            // O(1)
            var triangleVertexA = vbuf[ibuf[triangle * 3]].Position;
            var triangleVertexB = vbuf[ibuf[triangle * 3 + 1]].Position;
            var triangleVertexC = vbuf[ibuf[triangle * 3 + 2]].Position;

            var sideA = triangleVertexB - triangleVertexA;
            var sideB = triangleVertexC - triangleVertexB;
            var sideC = triangleVertexA - triangleVertexC;

            var a = point - triangleVertexA;
            var b = point - triangleVertexB;
            var c = point - triangleVertexC;

            var xa = Vector3.Cross(sideA, a);
            var xb = Vector3.Cross(sideB, b);
            var xc = Vector3.Cross(sideC, c);

            var ab = Vector3.Dot(xa, xb);
            var bc = Vector3.Dot(xb, xc);
            var ca = Vector3.Dot(xc, xa);

            return ab >= 0 && bc >=0 && ca >= 0 &&
                   ab * ab >= xa.LengthSquared() * xb.LengthSquared() - threshold &&
                   bc * bc >= xb.LengthSquared() * xc.LengthSquared() - threshold &&
                   ca * ca >= xc.LengthSquared() * xa.LengthSquared() - threshold;
        }
        
        
        public static bool FindCoplanar(Vector3 e1, Vector3 e2, Vector3 t1, Vector3 t2, Vector3 t3, out Vector3 intersection)
        {
            //O(1)
            Vector3 normal = Vector3.Cross(t2 - t1, t3 - t1);

            float t = Vector3.Dot(t1 - e1, normal) / Vector3.Dot(e2 - e1, normal);

            if (t > 1 || t < 0)
            {
                intersection = new Vector3();
                return false;
            }
            intersection = e1 + t * (e2 - e1);
            return true;
        }
        
        public DynamicModel SubtractSolid(DynamicModel model)
        {
            // O(N^2)
            
            // First, add a vertex where each triangle side on the model intersects...
            return this.UnionWith(model.Invert());
            // Now, we remove any vertex inside the subtraction.
        }

        public void SplitAtIntersections(DynamicModel other)
        {
            for (int i = 0; i < other.ibuf.Count; i+=3)
            {
                Vector3 vA = other.vbuf[other.ibuf[i]].Position;
                Vector3 vB = other.vbuf[other.ibuf[i+1]].Position;
                Vector3 vC = other.vbuf[other.ibuf[i+2]].Position;

                for (int j = 0; j < ibuf.Count; j+=3)
                {
                    Vector3 tA = vbuf[ibuf[i]].Position;
                    Vector3 tB = vbuf[ibuf[i+1]].Position;
                    Vector3 tC = vbuf[ibuf[i+2]].Position;
                    
                    Vector3 intersection;
                    if (FindCoplanar(vA, vB, tA, tB, tC, out intersection))
                        AddVertex(intersection);
                    if (FindCoplanar(vB, vC, tA, tB, tC, out intersection))
                        AddVertex(intersection);
                    if (FindCoplanar(vC, vA, tA, tB, tC, out intersection))
                        AddVertex(intersection);
                }
            }
        }
        
        public bool Contains(Vector3 point)
        {
            // int nt = 0;
            float nd = float.PositiveInfinity;

            for (int i = 0; i < ibuf.Count/3; i++)
            {
                if (IsInTriangle(point, i, float.PositiveInfinity))
                {
                    // (C x A) - (B x A)
                    int t = i * 3;
                    Vector3 normal = Vector3.Cross(vbuf[ibuf[t + 2]].Position - vbuf[ibuf[t]].Position,
                                     vbuf[ibuf[t + 1]].Position - vbuf[ibuf[t]].Position);
                    normal = Vector3.Normalize(normal);
                    
                    float dot = Vector3.Dot(point-vbuf[ibuf[t + 2]].Position, normal);
                    
                    if (i == 0)
                    {
                        // nt = 0;
                        nd = dot;
                        continue;
                    }

                    if (Math.Abs(nd) > Math.Abs(dot))
                    {
                        // nt = i;
                        nd = dot;
                    }
                    
                }
            }

            return nd <= 0;
        }

        public DynamicModel UnionWith(DynamicModel other)
        {
            DynamicModel a = this;
            DynamicModel b = other;
            this.SplitAtIntersections(b);
            other.SplitAtIntersections(a);
            
            // eliminate extraneous triangles... 
            
            
            
        }
        
        public void AddVertex(Vector3 position)
        {
            // O(triangles)
            // go through all existing triangles
            // if this point is on or inside the triangle,
            // split it at that position.
            int triangles = ibuf.Count / 3;
            for (int i = triangles - 1; i >= 0; i--)
            {
                if (IsInTriangle(position, i))
                {
                    SplitTriangle(position, i);
                }
            }

        }

        public void AddTriangle(short index1, short index2, short index3)
        {
            ibuf.Add(index1);
            ibuf.Add(index2);
            ibuf.Add(index3);
        }

        public void RemoveTriangle(int triangle)
        {
            ibuf.RemoveRange(triangle * 3, 3);
        }

        public void SplitTriangle(Vector3 point, int triangle)
        {
            // O(1)
            Vector3 sideA = vbuf[ibuf[triangle * 3 + 1]].Position - vbuf[ibuf[triangle * 3]].Position;
            Vector3 sideB = vbuf[ibuf[triangle * 3 + 2]].Position - vbuf[ibuf[triangle * 3 + 1]].Position;
            Vector3 sideC = vbuf[ibuf[triangle * 3]].Position - vbuf[ibuf[triangle * 3 + 2]].Position;

            Matrix to, from;
            BarycentricMatrices(vbuf[ibuf[triangle * 3]].Position, vbuf[ibuf[triangle * 3 + 1]].Position,
                vbuf[ibuf[triangle * 3 + 2]].Position, out to, out from);

            Vector3 barycentric = Vector3.Transform(point, to);
            Vector3 normal = barycentric.X * vbuf[ibuf[triangle * 3 + 1]].Normal +
                             barycentric.Y * vbuf[ibuf[triangle * 3 + 2]].Normal + (1 - barycentric.Z - barycentric.Y) *
                             barycentric.X * vbuf[ibuf[triangle * 3]].Normal;

            Vector2 textureCoordinate = barycentric.X * vbuf[ibuf[triangle * 3 + 1]].TextureCoordinate +
                                        barycentric.Y * vbuf[ibuf[triangle * 3 + 2]].TextureCoordinate +
                                        (1 - barycentric.X - barycentric.Y) *
                                        vbuf[ibuf[triangle * 3]].TextureCoordinate;

            vbuf.Add(new VertexPositionNormalTexture(point, Vector3.Normalize(normal), textureCoordinate));


            if (Vector3.Cross(sideA, point - vbuf[ibuf[triangle * 3]].Position).LengthSquared() > 0)
            {
                AddTriangle((short) (vbuf.Count - 1), (short) ibuf[(triangle * 3)], (short) ibuf[(triangle * 3 + 1)]);
            }

            if (Vector3.Cross(sideB, point - vbuf[ibuf[triangle * 3 + 1]].Position ).LengthSquared() > 0)
            {
                AddTriangle((short) ibuf[(triangle * 3 + 2)], (short) (vbuf.Count - 1), (short) ibuf[(triangle * 3 + 1)]);
            }

            if (Vector3.Cross(sideC, point- vbuf[ibuf[triangle * 3 + 2]].Position).LengthSquared() > 0)
            {
                AddTriangle((short) ibuf[(triangle * 3 + 2)], (short) ibuf[(triangle * 3)], (short) (vbuf.Count - 1));
            }

            RemoveTriangle(triangle);
        }

        /// <summary>
        /// Produces a matrix that transforms from world coordinates to barycentric coordinates.
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="to"></param>
        public static void BarycentricMatrices(Vector3 p0, Vector3 p1, Vector3 p2, out Matrix to, out Matrix from)
        {
            Vector3 d1 = p1 - p0;
            Vector3 d2 = p2 - p0;
            from = new Matrix(d1.X, d1.Y, d1.Z, 0, d2.X, d2.Y, d2.Z, 0, 0, 0, 1, 0, p0.X, p0.Y, p0.Z, 1);
            Matrix.Invert(ref from, out to);
        }
    }
}