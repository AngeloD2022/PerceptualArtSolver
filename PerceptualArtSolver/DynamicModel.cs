using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PerceptualArtSolver
{
    public class DynamicModel : IModel, ICloneable
    {
        public BasicEffect Effect;
        public List<short> ibuf;
        public IndexBuffer indexBuffer;
        public List<VertexPositionNormalTexture> vbuf;
        public VertexBuffer vertexBuffer;
        
        #if DEBUG
        public FastLogger logger;
        #endif

        public DynamicModel()
        {
            ibuf = new List<short>();
            vbuf = new List<VertexPositionNormalTexture>();
        }

        public object Clone()
        {
            var o = new DynamicModel();
            o.ibuf.AddRange(ibuf);
            o.vbuf.AddRange(vbuf);
            o.Effect = Effect;
            return o;
        }

        public void Draw(Matrix world, Camera camera)
        {
            Effect.World = world;
            Effect.View = camera.GetView();
            Effect.Projection = camera.GetProjection();
            Effect.CurrentTechnique.Passes[0].Apply();

            var backup = Effect.GraphicsDevice.RasterizerState;


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

            return ab >= 0 && bc >= 0 && ca >= 0 &&
                   ab * ab >= xa.LengthSquared() * xb.LengthSquared() - threshold &&
                   bc * bc >= xb.LengthSquared() * xc.LengthSquared() - threshold &&
                   ca * ca >= xc.LengthSquared() * xa.LengthSquared() - threshold;
        }


        public static bool FindCoplanar(Vector3 e1, Vector3 e2, Vector3 t1, Vector3 t2, Vector3 t3,
            out Vector3 intersection)
        {
            //O(1)
            var normal = Vector3.Cross(t2 - t1, t3 - t1);

            var denominator = Vector3.Dot(e2 - e1, normal);

            if (denominator == 0)
            {
                intersection = new Vector3();
                return false;
            }

            var t = Vector3.Dot(t1 - e1, normal) / denominator;

            if (t > 1 || t < 0)
            {
                intersection = new Vector3();
                return false;
            }

            intersection = e1 + t * (e2 - e1);

            return true;
        }

        public DynamicModel SubtractSolid(DynamicModel B)
        {
            
            #if DEBUG
            logger = new FastLogger("Subtract Solid");
            #endif

            DynamicModel A2 = (DynamicModel) this.Clone();
            DynamicModel B2 = (DynamicModel) B.Clone();
            int vbufMaxA = vbuf.Count;
            int vbufMaxB = B.vbuf.Count;
            
            #if DEBUG
            if (A2 != this && B2 != B)
                logger.Log(FLMessageType.CONFIRM, "A and B cloned properly.");
            else
                logger.Log(FLMessageType.ERROR, "A and B not cloned properly (addresses match)");
            
            logger.Log(FLMessageType.INFO, $"vbufA: {vbufMaxA}, vbufMaxB: {vbufMaxB}");
            #endif

            A2.SplitAtIntersections(B);
            B2.SplitAtIntersections(this);



            // Remove from A2 any triangles that are inside B...
            for (var i = A2.ibuf.Count-3; i >= 0; i-=3)
            {
                Vector3 a, b, c;
                a = A2.vbuf[A2.ibuf[i]].Position;
                b = A2.vbuf[A2.ibuf[i+1]].Position;
                c = A2.vbuf[A2.ibuf[i+2]].Position;
                
                // Remove the triangle from A2 if and only if:
                // * one or more of the selected vertices:
                //      - are not resultants of a split
                //      - and are inside solid B

                if (A2.ibuf[i] < vbufMaxA && B.Contains(a)
                    || A2.ibuf[i + 1] < vbufMaxA && B.Contains(b)
                    || A2.ibuf[i + 2] < vbufMaxA && B.Contains(c))
                {
                    A2.RemoveTriangle(i/3);
                }

            }
            
            // Add to A2 inverted: any triangles from B2 that reside within A...
            for (var i = 0; i < B2.ibuf.Count-3; i++)
            {
                Vector3 a, b, c;
                a = B2.vbuf[B2.ibuf[i]].Position;
                b = B2.vbuf[B2.ibuf[i+1]].Position;
                c = B2.vbuf[B2.ibuf[i+2]].Position;
                
                // Add triangle to A2 if and only if:
                // * One or more of the selected vertices from B2:
                //      - not the result of a split 
                //      - & inside us
                if (B2.ibuf[i] < vbufMaxB && this.Contains(a)
                    || B2.ibuf[i + 1] < vbufMaxB && this.Contains(b)
                    || B2.ibuf[i + 2] < vbufMaxB && this.Contains(c))
                {
                    int count = A2.vbuf.Count;
                    A2.vbuf.Add(B2.vbuf[B2.ibuf[i]]);
                    A2.vbuf.Add(B2.vbuf[B2.ibuf[i+1]]);
                    A2.vbuf.Add(B2.vbuf[B2.ibuf[i+2]]);
                    A2.ibuf.Add((short)(count + 2));
                    A2.ibuf.Add((short)(count + 1));
                    A2.ibuf.Add((short)(count));
                }
            }

            return A2;
        }

        public void SplitAtIntersections(DynamicModel other)
        {
            // where does other's edges intersect this's faces?...
            // for each of other's faces...
            for (var i = 0; i < other.ibuf.Count; i += 3)
            {
                
                var vA = other.vbuf[other.ibuf[i]].Position;
                var vB = other.vbuf[other.ibuf[i + 1]].Position;
                var vC = other.vbuf[other.ibuf[i + 2]].Position;
                
#if DEBUG
                logger.Info($"1-B-{i/3}: {vA}, {vB}, {vC}");
#endif
                
                // go through our faces...
                for (var j = ibuf.Count - 3; j >= 0; j -= 3)
                {
                    var tA = vbuf[ibuf[j]].Position;
                    var tB = vbuf[ibuf[j + 1]].Position;
                    var tC = vbuf[ibuf[j + 2]].Position;

#if DEBUG
                    logger.Info($"1-A-{j/3}: {tA}, {tB}, {tC}");
#endif
                    
                    Vector3 intersection;
                    // TODO: make this look like the below loop.
                    // if the intersection equals any of our triangle vertices, we don't need to split.
                    if (FindCoplanar(vA, vB, tA, tB, tC, out intersection) && intersection != tA &&
                        intersection != tB && intersection != tC && IsInTriangle(intersection, j / 3))
                    {
#if DEBUG
                        logger.Warning($"BA - SplitTriangle called: coplanar(vA,bB) = {intersection}, thisTri: {j/3}, otherTri: {i/3}");
#endif
                        SplitTriangle(intersection, j / 3);
                    }

                    if (FindCoplanar(vB, vC, tA, tB, tC, out intersection) && intersection != tA &&
                        intersection != tB && intersection != tC && IsInTriangle(intersection, j / 3))
                    {
#if DEBUG
                        logger.Warning($"BA - SplitTriangle called: coplanar(vB,vC) = {intersection}, thisTri: {j/3}, otherTri: {i/3}");
#endif
                        SplitTriangle(intersection, j / 3);
                    }

                    if (FindCoplanar(vC, vA, tA, tB, tC, out intersection) && intersection != tA &&
                        intersection != tB && intersection != tC && IsInTriangle(intersection, j / 3))
                    {
#if DEBUG
                        logger.Warning($"BA - SplitTriangle called: coplanar(vC,vA) = {intersection}, thisTri: {j/3}, otherTri: {i/3}");
#endif
                        SplitTriangle(intersection, j / 3);
                    }
                }
            }

            // where does this's edges intersect other's faces?...
            // for each of other's faces...
            for (var i = ibuf.Count - 3; i >= 0; i -= 3)
            {
                var vA = vbuf[ibuf[i]].Position;
                var vB = vbuf[ibuf[i + 1]].Position;
                var vC = vbuf[ibuf[i + 2]].Position;

                // go through other's faces...
                for (var j = 0; j < other.ibuf.Count; j += 3)
                {
                    var tA = other.vbuf[other.ibuf[j]].Position;
                    var tB = other.vbuf[other.ibuf[j + 1]].Position;
                    var tC = other.vbuf[other.ibuf[j + 2]].Position;
                
                    Vector3 intersection;
                
                    // TODO: Something is terribly incorrect here...
                
                    if (FindCoplanar(vA, vB, tA, tB, tC, out intersection) && intersection != vA &&
                        intersection != vB && intersection != vC && other.IsInTriangle(intersection, j / 3))
                        SplitTriangle(intersection, i / 3);
                
                    if (FindCoplanar(vB, vC, tA, tB, tC, out intersection) && intersection != vA &&
                        intersection != vB && intersection != vC && other.IsInTriangle(intersection, j / 3))
                    {
                        SplitTriangle(intersection, i / 3);
                    }
                
                    if (FindCoplanar(vC, vA, tA, tB, tC, out intersection) && intersection != vA &&
                        intersection != vB && intersection != vC &&
                        other.IsInTriangle(intersection, j / 3)) SplitTriangle(intersection, i / 3);
                }
            }
        }

        public bool Contains(Vector3 point)
        {
            // int nt = 0;
            var nearestDistance = float.PositiveInfinity;
            for (var i = 0; i < ibuf.Count; i += 3)
            {
                // Using the X-Z plane for projection (bottom-top view).
                Vector2 vertexA = new Vector2(vbuf[ibuf[i]].Position.X, vbuf[ibuf[i]].Position.Z);
                Vector2 vertexB = new Vector2(vbuf[ibuf[i + 1]].Position.X, vbuf[ibuf[i + 1]].Position.Z);
                Vector2 vertexC = new Vector2(vbuf[ibuf[i + 2]].Position.X, vbuf[ibuf[i + 2]].Position.Z);
                Vector2 point2d = new Vector2(point.X, point.Z);

                Vector2 sideA = vertexB - vertexA;
                Vector2 sideB = vertexC - vertexB;
                Vector2 sideC = vertexA - vertexC;

                if (sideA.X * sideB.Y - sideA.Y * sideB.X == 0)
                    continue;

                Vector2 segmentA = point2d - vertexA;
                Vector2 segmentB = point2d - vertexB;
                Vector2 segmentC = point2d - vertexC;

                // Ax*By - Ay*Bx = the z component of a cross product
                float xa = segmentA.X * sideA.Y - segmentA.Y * sideA.X;
                float xb = segmentB.X * sideB.Y - segmentB.Y * sideB.X;
                float xc = segmentC.X * sideC.Y - segmentC.Y * sideC.X;

                bool inTriangle = xa >= 0 && xb >= 0 && xc >= 0 || xa <= 0 && xb <= 0 && xc <= 0;

                if (!inTriangle)
                    continue;

                Vector3 normal = Vector3.Normalize(Vector3.Cross(vbuf[ibuf[i + 2]].Position - vbuf[ibuf[i]].Position,
                    vbuf[ibuf[i + 1]].Position - vbuf[ibuf[i]].Position));
                float dot = Vector3.Dot(normal, point - vbuf[ibuf[i]].Position);

                if (Math.Abs(nearestDistance) > Math.Abs(dot)) nearestDistance = dot;
            }

            return nearestDistance <= 0;
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
            var sideA = vbuf[ibuf[triangle * 3 + 1]].Position - vbuf[ibuf[triangle * 3]].Position;
            var sideB = vbuf[ibuf[triangle * 3 + 2]].Position - vbuf[ibuf[triangle * 3 + 1]].Position;
            var sideC = vbuf[ibuf[triangle * 3]].Position - vbuf[ibuf[triangle * 3 + 2]].Position;

            Matrix to, from;
            BarycentricMatrices(vbuf[ibuf[triangle * 3]].Position, vbuf[ibuf[triangle * 3 + 1]].Position,
                vbuf[ibuf[triangle * 3 + 2]].Position, out to, out from);

            var barycentric = Vector3.Transform(point, to);

            // normal = b_x * vB 
            var normal = barycentric.X * vbuf[ibuf[triangle * 3 + 1]].Normal +
                         barycentric.Y * vbuf[ibuf[triangle * 3 + 2]].Normal + (1 - barycentric.Z - barycentric.Y) *
                         barycentric.X * vbuf[ibuf[triangle * 3]].Normal;

            var textureCoordinate = barycentric.X * vbuf[ibuf[triangle * 3 + 1]].TextureCoordinate +
                                    barycentric.Y * vbuf[ibuf[triangle * 3 + 2]].TextureCoordinate +
                                    (1 - barycentric.X - barycentric.Y) *
                                    vbuf[ibuf[triangle * 3]].TextureCoordinate;

            vbuf.Add(new VertexPositionNormalTexture(point, Vector3.Normalize(normal), textureCoordinate));


            if (Vector3.Cross(sideA, point - vbuf[ibuf[triangle * 3]].Position).LengthSquared() > 0)
                AddTriangle((short) (vbuf.Count - 1), ibuf[triangle * 3], ibuf[triangle * 3 + 1]);

            if (Vector3.Cross(sideB, point - vbuf[ibuf[triangle * 3 + 1]].Position).LengthSquared() > 0)
                AddTriangle(ibuf[triangle * 3 + 2], (short) (vbuf.Count - 1), ibuf[triangle * 3 + 1]);

            if (Vector3.Cross(sideC, point - vbuf[ibuf[triangle * 3 + 2]].Position).LengthSquared() > 0)
                AddTriangle(ibuf[triangle * 3 + 2], ibuf[triangle * 3], (short) (vbuf.Count - 1));

            RemoveTriangle(triangle);
        }

        /// <summary>
        ///     Produces a matrix that transforms from world coordinates to barycentric coordinates.
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="to"></param>
        public static void BarycentricMatrices(Vector3 p0, Vector3 p1, Vector3 p2, out Matrix to, out Matrix from)
        {
            var d1 = p1 - p0;
            var d2 = p2 - p0;
            var cross = Vector3.Cross(d2, d1);

            from = new Matrix(d1.X, d1.Y, d1.Z, 0, d2.X, d2.Y, d2.Z, 0, cross.X, cross.Y, cross.Z, 0, p0.X, p0.Y, p0.Z,
                1);

            Matrix.Invert(ref from, out to);
        }
    }
}