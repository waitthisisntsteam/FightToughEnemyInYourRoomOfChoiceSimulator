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

        private List<Rectangle> hitBoxes = new List<Rectangle>();

        Character Kirby;
        List<Frame> kirbyIdleFrames;
        List<Frame> kirbyRunningFrames;
        List<Frame> kirbyJumpingFrames;
        List<Frame> kirbyDoubleJumpingFrames;
        List<Frame> kirbyCrouchingFrames;
        List<Frame> kirbyCrouchMovingFrames;

        /*private Rectangle FloorHB;

        private Sprite Kirby;
        private Rectangle KirbyHB;
        private SpriteEffects direction = SpriteEffects.None;
        private Texture2D SpriteSheet;

        private List<Frame> idleFrames = new List<Frame>();
        private List<Frame> runningFrames = new List<Frame>();
        private List<Frame> attackFrames = new List<Frame>();

        private Frame hurtFrame;
        private Frame hurtInAirFrame;

        private bool goUp = false;
        private int jumpHeight = 0;
        private Frame jumpFrame;
        private int jumpCount = 0;
        bool upPressed = false;

        private TimeSpan updateRunningTime = TimeSpan.FromMilliseconds(400);
        private TimeSpan elapsedRunningTime = TimeSpan.Zero;
        private bool run = false;
        private int running = 0;
        private int currentRunningFrame = 0;

        private TimeSpan updateIdleTime = TimeSpan.FromMilliseconds(500);
        private TimeSpan elapsedIdleTime = TimeSpan.Zero;
        private int idle = 0;
        private int currentIdleFrame = 0;

        private bool attack = false;

        private bool crouch = false;
        private Frame crouchFrame;

        private float kirbySpeedY = 0;
        private float gravity = .2f;
        */

        /*public void KirbyUpdate(GameTime time)
        {
            kirbySpeedY += gravity;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                upPressed = true;
            }
            else
            {
                idle++;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Up) && upPressed && jumpCount < 2)
            {
                jumpCount++;
                goUp = true;
                kirbySpeedY = -7;
                idle = 0;
                running = 0;
                upPressed = false;
            }
            Kirby.Position.Y += kirbySpeedY;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                crouch = true;
                running = 0;
                idle = 0;
            }
            else
            {
                idle++;
                crouch = false;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    crouch = true;
                    Kirby.Position.X -= 2;
                }
                else
                {
                    Kirby.Position.X -= 4;
                }
                idle = 0;
                direction = SpriteEffects.FlipHorizontally;
            }
            else
            {
                idle++;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    crouch = true;
                    Kirby.Position.X += 2;
                }
                else
                {
                    Kirby.Position.X += 4;
                }
                idle = 0;
                direction = SpriteEffects.None;
            }
            else
            {
                idle++;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                run = true;
            }
            else
            {

                run = false;
            }

            if (idle > 3)
            {
                elapsedIdleTime += time.ElapsedGameTime;
                if (elapsedIdleTime > updateIdleTime)
                {
                    elapsedIdleTime = TimeSpan.Zero;
                    currentIdleFrame++;
                    if (currentIdleFrame == 4)
                    {
                        currentIdleFrame = 0;
                    }
                }

            }
            else
            {
                currentIdleFrame = 0;
            }
            if (run)
            {
                elapsedRunningTime += time.ElapsedGameTime;
                if (elapsedRunningTime > updateRunningTime)
                {
                    elapsedRunningTime = TimeSpan.Zero;
                    if (currentRunningFrame == 1)
                    {
                        currentRunningFrame--;
                    }
                    if (currentRunningFrame == 0)
                    {
                        currentRunningFrame++;
                    }
                }
            }

            if (Kirby.Position.Y >= GraphicsDevice.Viewport.Height - 32)
            {
                Kirby.Position.Y = GraphicsDevice.Viewport.Height - 32;
                goUp = false;
                jumpCount = 0;
            }
            else if (Kirby.Position.Y <= 0)
            {
                Kirby.Position.Y = 0;
            }

            if (Kirby.Position.X < 0)
            {
                Kirby.Position.X = 0;
            }
            else if (Kirby.Position.X + 32 > GraphicsDevice.Viewport.Width)
            {
                Kirby.Position.X = GraphicsDevice.Viewport.Width - 32;
            }

        }*/

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

            kirbyJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(113, 563, 22, 19)));

            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(141, 564, 21, 19)));
            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(113, 563, 22, 19)));
            kirbyDoubleJumpingFrames.Add(new Frame(Vector2.Zero, new Rectangle(113, 563, 22, 19)));

            kirbyCrouchingFrames.Add(new Frame(Vector2.Zero, new Rectangle(74, 228, 16, 16)));

            kirbyCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(100, 228, 17, 16)));
            kirbyCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(127, 228, 17, 16)));
            kirbyCrouchMovingFrames.Add(new Frame(Vector2.Zero, new Rectangle(74, 228, 16, 16)));

            //Kirby = new Character(new Sprite(new Vector2(0, GraphicsDevice.Viewport.Height - 32), Color.White, Content.Load<Texture2D>("kirby")), 4f, 0.2f, kirbyIdleFrames, kirbyRunningFrames, kirbyJumpingFrames, kirbyCrouchingFrames);
            Kirby = new Character(new Vector2(0, GraphicsDevice.Viewport.Height - 32), Content.Load<Texture2D>("kirby"), new List<List<Frame>>() { kirbyJumpingFrames, kirbyDoubleJumpingFrames, kirbyCrouchingFrames, kirbyCrouchMovingFrames, kirbyIdleFrames, kirbyRunningFrames }, 4f, 0.2f);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Kirby.Update(GraphicsDevice, gameTime);
          
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            Kirby.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}