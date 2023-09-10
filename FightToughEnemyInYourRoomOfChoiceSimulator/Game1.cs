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

        private List<Rectangle> hitBoxes = new List<Rectangle>();

        private Rectangle FloorHB;

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
        private bool jump = false;
        private int jumpHeight = 0;
        private Frame jumpFrame;
        private int jumpCount = 0;
        bool upPressed = false;

        private TimeSpan updateRunningTime = TimeSpan.FromMilliseconds(150);
        private TimeSpan elapsedRunningTime = TimeSpan.Zero;
        private bool run = false;
        private int running = 0;
        private int currentRunningFrame = 0;

        private TimeSpan updateIdleTime = TimeSpan.FromMilliseconds(400);
        private TimeSpan elapsedIdleTime = TimeSpan.Zero;
        private int idle = 0;
        private int currentIdleFrame = 0;

        private bool attack = false;

        private bool crouch = false;
        private Frame crouchFrame;

        private int gravity = 2;

        /*public void Jump()
        {
            if (goUp == true)
            {
                jump = true;
                Kirby.Position.Y -= jumpHeight;
                if (jumpHeight < 15)
                {
                    jumpHeight += 1;
                }
                else if (jumpHeight == 15)
                {
                    jumpHeight = 0;
                    goUp = false;
                }
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

            // TODO: use this.Content to load your game content here

            SpriteSheet = Content.Load<Texture2D>("kirby");

            Kirby = new Sprite(new Vector2(0, GraphicsDevice.Viewport.Height - 32), Color.White, Content.Load<Texture2D>("kirby"));


            idleFrames.Add(new Frame(Vector2.Zero, new Rectangle(149, 126, 16, 16)));
            idleFrames.Add(new Frame(Vector2.Zero, new Rectangle(191, 126, 16, 16)));
            idleFrames.Add(new Frame(Vector2.Zero, new Rectangle(171, 126, 16, 16)));
            idleFrames.Add(new Frame(Vector2.Zero, new Rectangle(191, 126, 16, 16)));
            idleFrames.Add(new Frame(Vector2.Zero, new Rectangle(211, 126, 16, 16)));

            runningFrames.Add(new Frame(Vector2.Zero, new Rectangle(24, 19, 16, 16)));
            runningFrames.Add(new Frame(Vector2.Zero, new Rectangle(68, 19, 16, 16)));

            jumpFrame = new Frame(Vector2.Zero, new Rectangle(113, 563, 22, 19));
            crouchFrame = new Frame(Vector2.Zero, new Rectangle(74, 228, 16, 16));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                upPressed = true;
            }
            else
            {
                idle++;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Up) && upPressed)
            {
                goUp = true;
                gravity = -gravity;
                idle = 0;
                running = 0;
                upPressed = false;
            }
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


            //Jump();

            if (idle > 3)
            {
                elapsedIdleTime += gameTime.ElapsedGameTime;
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
                elapsedRunningTime += gameTime.ElapsedGameTime;
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

            if (Kirby.Position.Y < GraphicsDevice.Viewport.Height - 32 && Kirby.Position.Y > 0)
            {
                Kirby.Position.Y += gravity;
            }

            if (goUp)
            {
                gravity -= 2;
                if (gravity == -20)
                {
                    gravity = 2;
                    goUp = false;
                    jumpCount--;
                }           
            }

            /*if (goUp == false && Kirby.Position.Y < GraphicsDevice.Viewport.Height - 32)
            {
                Kirby.Position.Y += 6;
                jump = true;
                
            }
            if (Kirby.Position.Y < GraphicsDevice.Viewport.Height - 32)
            {
                jump = false;
            }*/

            if (Kirby.Position.X < 0)
            {
                Kirby.Position.X = 0;
            }
            if (Kirby.Position.X + 32 > GraphicsDevice.Viewport.Width)
            {
                Kirby.Position.X = GraphicsDevice.Viewport.Width - 32;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            if (jump)
            {
                spriteBatch.Draw(SpriteSheet, Kirby.Position, jumpFrame.SourceRectangle, Color.White, 0f, jumpFrame.Origin, 2f, direction, 0f);
            }
            else if (idle > 3)
            {
                spriteBatch.Draw(SpriteSheet, Kirby.Position, idleFrames[currentIdleFrame].SourceRectangle, Color.White, 0f, idleFrames[currentIdleFrame].Origin, 2f, direction, 0f);
            }
            else if (run && !jump)
            {
                spriteBatch.Draw(SpriteSheet, Kirby.Position, runningFrames[currentRunningFrame].SourceRectangle, Color.White, 0f, runningFrames[currentRunningFrame].Origin, 2f, direction, 0f);
            }
            else if (crouch)
            {
                spriteBatch.Draw(SpriteSheet, Kirby.Position, crouchFrame.SourceRectangle, Color.White, 0f, crouchFrame.Origin, 2f, direction, 0f);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}