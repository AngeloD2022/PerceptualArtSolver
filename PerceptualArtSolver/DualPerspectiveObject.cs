using System.Drawing;
using Microsoft.Xna.Framework;

namespace PerceptualArtSolver
{
    internal class DualPerspectiveObject : WorldObject
    {
        private readonly bool[,,] bits;
        private readonly CubeModel cube;
        private Matrix myWorld;
        private float rotation;


        public DualPerspectiveObject()
        {
            cube = Cube.Model;
            bits = new bool[756, 360, 756];

            var image1 = getBitmap("image1.jpg");
            var image2 = getBitmap("image2.png");

            //image1[0, 0] = true;
            //image1[0, 1] = true;
            //image1[1, 0] = true;
            //image1[1, 0] = true;
            //image1[1, 1] = true;
            //image1[1, 2] = true;
            //image1[2, 1] = true;
            //image1[2, 2] = true;
            //image1[2, 3] = true;
            //image1[3, 2] = true;
            //image1[3, 3] = true;
            //image1[3, 4] = true;
            //image1[4, 3] = true;
            //image1[4, 4] = true;
            //image1[4, 5] = true;
            //image1[5, 4] = true;
            //image1[5, 5] = true;
            //image1[2, 6] = true;
            //image1[3, 6] = true;
            //image1[4, 6] = true;
            //image1[6, 2] = true;
            //image1[6, 3] = true;
            //image1[6, 4] = true;
            //image1[6, 6] = true;
            //image1[7, 7] = true;


            //image2[1, 1] = true;
            //image2[2, 1] = true;
            //image2[1, 2] = true;
            //image2[2, 2] = true;

            //image2[5, 1] = true;
            //image2[6, 1] = true;
            //image2[5, 2] = true;
            //image2[6, 2] = true;

            //image2[2, 4] = true;
            //image2[2, 5] = true;
            //image2[2, 6] = true;

            //image2[3, 3] = true;
            //image2[3, 4] = true;
            //image2[3, 5] = true;

            //image2[4, 3] = true;
            //image2[4, 4] = true;
            //image2[4, 5] = true;

            //image2[5, 4] = true;
            //image2[5, 5] = true;
            //image2[5, 6] = true;


            // do the magic...
            for (var z = 0; z < image2.GetLength(0); z++)
            for (var y = 0; y < image1.GetLength(1); y++)
            for (var x = 0; x < image2.GetLength(0); x++)
                bits[x, y, z] = image1[x, y] && image2[z, y];
        }

        private static bool[,] getBitmap(string filename)
        {
            var threshold = 5;

            var b = (Bitmap) Image.FromFile(filename);

            var result = new bool[b.Width, b.Height];

            for (var y = 0; y < b.Height; y++)
            for (var x = 0; x < b.Width; x++)
            {
                var p = b.GetPixel(x, y);
                if (p.R < 255 - threshold || p.G < 255 - threshold || p.B < 255 - threshold)
                    result[x, y] = true;
            }

            return result;
        }

        public override void Draw(Camera camera)
        {
            for (var z = 0; z < bits.GetLength(2); z++)
            for (var y = 0; y < bits.GetLength(1); y++)
            for (var x = 0; x < bits.GetLength(0); x++)
                if (bits[x, y, z])
                    cube.Draw(
                        Matrix.CreateTranslation(x * 2 - bits.GetLength(0), -1 * (y * 2 - bits.GetLength(1)),
                            z * 2 - bits.GetLength(2)) * myWorld, camera);
        }

        public override void Update(float time)
        {
            rotation += time;
            myWorld = Matrix.CreateScale(.05f) * Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(Position);
        }
    }
}