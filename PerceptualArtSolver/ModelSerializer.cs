using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PerceptualArtSolver
{
    public class ModelSerializer
    {
        public static bool ToObjectFile(string filePath, DynamicModel model)
        {
            var buffer = "";
            // Specify geometric vertices...
            var vbuf = "";
            var vnbuf = "";
            for (var i = 0; i < model.vbuf.Count; i++)
            {
                var vtx = model.vbuf[i].Position;
                var vn = model.vbuf[i].Normal;
                vbuf += (i > 0 ? "\n" : "") + $"v {vtx.X} {vtx.Y} {vtx.Z}";
                vnbuf += $"\nvn {vn.X} {vn.Y} {vn.Z}";
            }

            buffer = vbuf + vnbuf;

            for (var i = 0; i < model.ibuf.Count; i += 3)
                buffer += $"\nf {model.ibuf[i] + 1} {model.ibuf[i + 1] + 1} {model.ibuf[i + 2] + 1}";

            File.WriteAllText(filePath, buffer);

            return false;
        }

        public static DynamicModel FromObjectFile(string filePath)
        {
            var result = new DynamicModel();
            var file = File.ReadAllLines(filePath);

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();

            for (var i = 0; i < file.Length; i++)
            {
                var command = file[i].Split(' ', 2);
                var param = command[1].Split(' ');
                var opcode = command[0];

                switch (opcode)
                {
                    case "v":
                        vertices.Add(new Vector3(float.Parse(param[0]), float.Parse(param[1]), float.Parse(param[2])));
                        break;
                    case "vn":
                        normals.Add(new Vector3(float.Parse(param[0]), float.Parse(param[1]), float.Parse(param[2])));
                        break;
                    case "f":
                        if (param[0].Contains("/"))
                        {
                            foreach (var face in param)
                            {
                                var indices = face.Split('/');
                                result.ibuf.Add((short) (short.Parse(indices[0]) - 1));
                                result.ibuf.Add((short) (short.Parse(indices[1]) - 1));
                                result.ibuf.Add((short) (short.Parse(indices[2]) - 1));
                            }

                            break;
                        }

                        result.ibuf.Add((short) (short.Parse(param[0]) - 1));
                        result.ibuf.Add((short) (short.Parse(param[1]) - 1));
                        result.ibuf.Add((short) (short.Parse(param[2]) - 1));
                        break;
                }
            }

            for (var i = 0; i < vertices.Count; i++)
                result.vbuf.Add(new VertexPositionNormalTexture(vertices[i], normals[i], Vector2.Zero));

            return result;
        }
    }
}