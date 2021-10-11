using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PerceptualArtSolver
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private readonly Camera camera;
        private readonly List<IDrawObject> drawObjs;
        private readonly List<IUpdateObject> updateObjs;

        private CubeModel cube;

#if DEBUG
        private Matrix debugTransformation;
#endif
        private GraphicsDeviceManager graphics;
        private DynamicModel negative;
        private DynamicModel positive;
        private SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            camera = new Camera();

            drawObjs = new List<IDrawObject>();
            updateObjs = new List<IUpdateObject>();
        }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

#if DEBUG
            GraphicsDevice.RasterizerState = new RasterizerState {FillMode = FillMode.WireFrame};
            DynamicModel dynModel;

            if (File.Exists("/Users/angelodeluca/Desktop/ModelDebug/difference.obj") &&
                File.Exists("/Users/angelodeluca/Desktop/ModelDebug/positive.obj") &&
                File.Exists("/Users/angelodeluca/Desktop/ModelDebug/negative.obj"))
            {
                dynModel = ModelSerializer.FromObjectFile("/Users/angelodeluca/Desktop/ModelDebug/positive.obj");
                positive = ModelSerializer.FromObjectFile("/Users/angelodeluca/Desktop/ModelDebug/positive.obj");
                negative = ModelSerializer.FromObjectFile("/Users/angelodeluca/Desktop/ModelDebug/negative.obj");
            }
            else
            {
                var cubeModel = new CubeModel();
                dynModel = new DynamicModel();
                dynModel.vbuf.AddRange(cubeModel.vbuf);
                dynModel.ibuf.AddRange(cubeModel.ibuf);
                negative.ibuf.AddRange(cubeModel.ibuf);
            }
            // UNIT TEST: DynamicModel

            // Vector3 offset = new Vector3(1f, 1f, 1f);


            dynModel.Effect = CubeModel.Effect;
            dynModel.Effect.Texture = Content.Load<Texture2D>("14376136-pack_l");
            dynModel.Effect.TextureEnabled = true;
            dynModel.Effect.EnableDefaultLighting();


            // debugTransformation = new Matrix(0.99652976f, 0, -0.08323707f, 0, -0.0103775775f, 0.99219763f, -0.1242423f,
            //     0, 0.08258762f, 0.12467495f, 0.98875445f, 0, 0.09079224f, -1.0134584f, -0.55807877f, 1);

            negative.Effect = CubeModel.Effect;
            negative.Effect.EnableDefaultLighting();


            negative.Effect.Texture = Content.Load<Texture2D>("14376136-pack_l");
            negative.Effect.TextureEnabled = true;


            negative.vbuf.AddRange(dynModel.vbuf.Select(v =>
                new VertexPositionNormalTexture(Vector3.Transform(v.Position, debugTransformation), v.Normal,
                    v.TextureCoordinate)));

            dynModel = dynModel.SubtractSolid(negative);

            drawObjs.Add(new GenericModelInstance(dynModel)
            {
                Position = new Vector3(0, 0, 0)
            });

            drawObjs.Add(new GenericModelInstance(negative)
            {
                Position = new Vector3(0, 0, 0)
            });

            // drawObjs.Add(new DualPerspectiveObject {Position = new Vector3(0, 0, -50)});
            // updateObjs.Add((IUpdateObject) drawObjs.Last());

#elif TEST
            cube = new CubeModel();
            positive = new DynamicModel();
            positive.vbuf.AddRange(cube.vbuf);
            positive.ibuf.AddRange(cube.ibuf);
            positive.Effect = CubeModel.Effect;
            // RasterizerState backup = GraphicsDevice.RasterizerState;
            GraphicsDevice.RasterizerState = new RasterizerState() {FillMode = FillMode.WireFrame};
            // GraphicsDevice.RasterizerState = backup;
            positive.Effect.Texture = Content.Load<Texture2D>("14376136-pack_l");
            positive.Effect.TextureEnabled = true;
            positive.Effect.EnableDefaultLighting();
            
            negative = new DynamicModel();
            negative.vbuf.AddRange(cube.vbuf.Select(v =>
                new VertexPositionNormalTexture(v.Position + Vector3.One, v.Normal, v.TextureCoordinate)));
            negative.ibuf.AddRange(cube.ibuf);
            negative.Effect = positive.Effect;
            
            drawObjs.Add(new GenericModelInstance(positive));
            drawObjs.Add(new GenericModelInstance(negative));
#endif
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            SquareModel.Effect = new BasicEffect(GraphicsDevice);
            CubeModel.Effect = new BasicEffect(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            var ks = Keyboard.GetState();
            var speed = 5.0f;
            var time = (float) gameTime.ElapsedGameTime.TotalSeconds;

            if (ks.IsKeyDown(Keys.W))
                // camera.Position.Z -= speed * time;
                camera.Position += camera.GetWorld().Forward * speed * time;
            if (ks.IsKeyDown(Keys.S))
                // camera.Position.Z += speed * time;
                camera.Position += camera.GetWorld().Backward * speed * time;
            if (ks.IsKeyDown(Keys.A))
                // camera.Position.Z += speed * time;
                camera.Position += camera.GetWorld().Left * speed * time;
            if (ks.IsKeyDown(Keys.D))
                // camera.Position.Z += speed * time;
                camera.Position += camera.GetWorld().Right * speed * time;

            // Ignores camera angle...
            if (ks.IsKeyDown(Keys.Q))
                camera.Position.Y += speed * time;
            if (ks.IsKeyDown(Keys.E))
                camera.Position.Y -= speed * time;

            if (ks.IsKeyDown(Keys.Down))
                camera.Pitch -= 0.5f * time;

            if (ks.IsKeyDown(Keys.Up))
                camera.Pitch += 0.5f * time;

            if (ks.IsKeyDown(Keys.Left))
                camera.Yaw += 0.5f * time;

            if (ks.IsKeyDown(Keys.Right))
                camera.Yaw -= 0.5f * time;


            foreach (var o in updateObjs)
                o.Update(time);

#if TEST
            Matrix inFrontOfCamera = Matrix.CreateTranslation(0, 0, -10) * Matrix.CreateRotationX(camera.Pitch) *
                                     Matrix.CreateRotationY(camera.Yaw) * Matrix.CreateTranslation(camera.Position);
            
            if (ks.IsKeyDown(Keys.L))
            {
                Console.WriteLine($"new Matrix({inFrontOfCamera.M11},{inFrontOfCamera.M12},{inFrontOfCamera.M13},{inFrontOfCamera.M14},{inFrontOfCamera.M21},{inFrontOfCamera.M22},{inFrontOfCamera.M23},{inFrontOfCamera.M24},{inFrontOfCamera.M31},{inFrontOfCamera.M32},{inFrontOfCamera.M33},{inFrontOfCamera.M34},{inFrontOfCamera.M41},{inFrontOfCamera.M42},{inFrontOfCamera.M43},{inFrontOfCamera.M44});");
            }
            
            
            negative.vbuf.Clear();
            negative.vbuf.AddRange(cube.vbuf.Select(v =>
                new VertexPositionNormalTexture(Vector3.Transform(v.Position, inFrontOfCamera), v.Normal,
                    v.TextureCoordinate)));
            
            ((GenericModelInstance) drawObjs[0]).Model = positive.SubtractSolid(negative);
#endif

            var prev = false;
            if (ks.IsKeyDown(Keys.K) && !prev)
            {
                Console.WriteLine("Dumping model files...");
                ModelSerializer.ToObjectFile("/Users/angelodeluca/Desktop/ModelDebug/positive.obj", positive);
                ModelSerializer.ToObjectFile("/Users/angelodeluca/Desktop/ModelDebug/negative.obj", negative);
                ModelSerializer.ToObjectFile("/Users/angelodeluca/Desktop/ModelDebug/difference.obj",
                    positive.SubtractSolid(negative));
                prev = true;
            }
            else
            {
                prev = ks.IsKeyDown(Keys.K);
            }


            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            SquareModel.Effect.CurrentTechnique.Passes[0].Apply();

            foreach (var obj in drawObjs) obj.Draw(camera);

#if DEBUG
            var backup = GraphicsDevice.RasterizerState;
            GraphicsDevice.RasterizerState = new RasterizerState {FillMode = FillMode.WireFrame};
            // ((GenericModelInstance)negative).Draw(camera);
            GraphicsDevice.RasterizerState = backup;
#elif TEST
#endif

            base.Draw(gameTime);
        }
    }
}