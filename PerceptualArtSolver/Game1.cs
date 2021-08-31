using System;
using System.Collections.Generic;
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
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private readonly List<IUpdateObject> updateObjs;

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
            // TODO: Add your initialization logic here


            var r = new Random();

            //for (int i = 0; i < 10; i++)
            //{
            //    drawObjs.Add(new Cube()
            //    {
            //        Position = new Vector3(r.Next(21) - 10, r.Next(21) - 10, r.Next(21) - 10),
            //        Texture = Content.Load<Texture2D>("grass")
            //    });
            //}


            drawObjs.Add(new DualPerspectiveObject {Position = new Vector3(0, 0, -50)});
            updateObjs.Add((IUpdateObject) drawObjs.Last());
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
            var speed = 3.0f;
            var time = (float) gameTime.ElapsedGameTime.TotalSeconds;

            if (ks.IsKeyDown(Keys.W))
                camera.Position.Z -= speed * time;
            if (ks.IsKeyDown(Keys.S))
                camera.Position.Z += speed * time;

            foreach (var o in updateObjs)
                o.Update(time);

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

            base.Draw(gameTime);
        }
    }
}