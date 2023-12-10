using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace FightToughEnemyInYourRoomOfChoiceSimulator
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager gfx;
        private SpriteBatch spriteBatch;
        private Texture2D SpriteSheet;

        private Texture2D HitBoxSheet;

        private List<Rectangle> hitBoxes = new List<Rectangle>();

        Character Kirby;
        List<Frame> kirbyIdleFrames;
        List<Frame> kirbyRunningFrames;
        List<Frame> kirbyJumpingFrames;
        List<Frame> kirbyDoubleJumpingFrames;
        List<Frame> kirbyCrouchingFrames;
        List<Frame> kirbyCrouchMovingFrames;

        public Game1()
        {
            gfx = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            
            spriteBatch = new SpriteBatch(GraphicsDevice);

            SpriteSheet = Content.Load<Texture2D>("kirby");

            HitBoxSheet = Content.Load<Texture2D>("hitbox");

            kirbyIdleFrames = new List<Frame>();
            kirbyRunningFrames = new List<Frame>();
            kirbyJumpingFrames = new List<Frame>();
            kirbyDoubleJumpingFrames = new List<Frame>();
            kirbyCrouchingFrames = new List<Frame>();
            kirbyCrouchMovingFrames = new List<Frame>();         

            kirbyIdleFrames.Add(new Frame(Vector2.Zero, new Rectangle(149, 126, 16, 16)));
            kirbyIdleFrames.Add(new Frame(Vector2.Zero, new Rectangle(191, 126, 16, 16)));
            kirbyIdleFrames.Add(new Frame(Vector2.Zero, new Rectangle(171, 126, 16, 16)));
            kirbyIdleFrames.Add(new Frame(Vector2.Zero, new Rectangle(191, 126, 16, 16)));
            kirbyIdleFrames.Add(new Frame(Vector2.Zero, new Rectangle(211, 126, 16, 16)));

            kirbyRunningFrames.Add(new Frame(Vector2.Zero, new Rectangle(24, 19, 16, 16)));
            kirbyRunningFrames.Add(new Frame(Vector2.Zero, new Rectangle(45, 19, 20, 16)));
            kirbyRunningFrames.Add(new Frame(Vector2.Zero, new Rectangle(68, 19, 16, 16)));
            kirbyRunningFrames.Add(new Frame(Vector2.Zero, new Rectangle(89, 19, 21, 16)));

            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(141, 564, 21, 19)));
            kirbyJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(113, 563, 22, 19)));

            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(141, 564, 21, 19)));
            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(113, 563, 22, 19)));
            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(113, 563, 22, 19)));
            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(113, 563, 22, 19)));

            kirbyCrouchingFrames.Add(new Frame(Vector2.Zero, new Rectangle(74, 228, 16, 16)));

            kirbyCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(100, 228, 17, 16)));
            kirbyCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(100, 228, 17, 16)));
            kirbyCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(127, 228, 17, 16)));
            kirbyCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(127, 228, 17, 16)));
            kirbyCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(74, 228, 16, 16)));

          
            Kirby = new Character(new Vector2((GraphicsDevice.Viewport.Width - 32)/2, (GraphicsDevice.Viewport.Height - 32)/2), Content.Load<Texture2D>("kirby"), new List<List<Frame>>() { kirbyJumpingFrames, kirbyDoubleJumpingFrames, kirbyCrouchingFrames, kirbyCrouchMovingFrames, kirbyIdleFrames, kirbyRunningFrames, kirbyJumpingFrames }, 4f, 0.2f);

            hitBoxes.Add(new Rectangle(200, GraphicsDevice.Viewport.Height - 200, 300, 20)); //platform

            hitBoxes.Add(new Rectangle(0, GraphicsDevice.Viewport.Height-40, GraphicsDevice.Viewport.Width + 40, 20)); //floor
            hitBoxes.Add(new Rectangle(0, 20, GraphicsDevice.Viewport.Width + 40, 20)); //roof
            hitBoxes.Add(new Rectangle(20, -20, 20, GraphicsDevice.Viewport.Height + 40)); //left
            hitBoxes.Add(new Rectangle(GraphicsDevice.Viewport.Width-40, -20, 20, GraphicsDevice.Viewport.Height + 40)); //right


           
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Kirby.Update(/*GraphicsDevice,*/ gameTime, hitBoxes);
          
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            Kirby.Draw(spriteBatch);

            foreach (var hb in hitBoxes)
            {
                spriteBatch.Draw(Content.Load<Texture2D>("hitbox"), new Rectangle(hb.X, hb.Y, hb.Width, hb.Height), Color.White);
            }
            //spriteBatch.Draw(Content.Load<Texture2D>("hitbox"), new Rectangle((int)Kirby.Position.X, (int)Kirby.Position.Y, 32, 32), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}