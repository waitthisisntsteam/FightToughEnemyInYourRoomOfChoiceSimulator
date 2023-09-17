using Microsoft.VisualBasic.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace FightToughEnemyInYourRoomOfChoiceSimulator
{
    public class Character
    {
        private Sprite charSprite;
        private int charXSpeed;
        private int charYSpeed;

        private List<Frame> idleFrames;
        private List<Frame> runningFrames;


        private int jumpCount;
        private bool upPressed;
        private bool goUp;
        private float speed;
        private float gravity;
        private SpriteEffects direction;

        private TimeSpan updateIdleTime;
        private TimeSpan elapsedIdleTime;
        private int idle;
        private int currentIdleFrame;

        private TimeSpan updateRunTime;
        private TimeSpan elapsedRunTime;
        private bool runDraw;
        private int run;
        private int currentRunFrame;

        private TimeSpan updateCrouchTime;
        private TimeSpan elapsedCrouchTime;
        private bool crouch = false;
        private Frame crouchFrame;

        private Rectangle hitbox;



        public Character(Sprite charSprite, int charXSpeed, int charYSpeed, List<Frame> idleFrames, List<Frame> runningFrames)
        {
            this.charSprite = charSprite;
            this.charXSpeed = charXSpeed;
            this.charYSpeed = charYSpeed;

            this.idleFrames = idleFrames;
            this.runningFrames = runningFrames;


            jumpCount = 0;
            upPressed = false;
            goUp = false;
            speed = 0f;
            gravity = 0.2f;

            direction = SpriteEffects.None;

            updateIdleTime = TimeSpan.FromMilliseconds(400);
            elapsedIdleTime = TimeSpan.Zero;                                
            idle = 0;
            currentIdleFrame = 0;

            updateRunTime = TimeSpan.FromMilliseconds(150);
            elapsedRunTime = TimeSpan.Zero;
            runDraw = false;
            run = 0;
            currentRunFrame = 0;

            hitbox = new Rectangle(charSprite.Position.X, charSprite.Position.Y, 32, 32);
        }

        public void Update()
        {
            speed += gravity;
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
                speed = -7;
                idle = 0;
                run = 0;
                upPressed = false;
            }
            charSprite.Position.Y += speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                crouch = true;
                run = 0;
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
                    charSprite.Position.X -= 2;
                }
                else
                {
                    charSprite.Position.X -= 4;
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
                    charSprite.Position.X += 4;
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
                elapsedRunTime += gameTime.ElapsedGameTime;
                if (elapsedRunTime > updateRunTime)
                {
                    elapsedRunTime = TimeSpan.Zero;
                    if (currentRunFrame == 1)
                    {
                        currentRunFrame--;
                    }
                    if (currentRunFrame == 0)
                    {
                        currentRunFrame++;
                    }
                }
            }

            if (charSprite.Position.Y >= GraphicsDevice.Viewport.Height - 32)
            {
                charSprite.Position.Y = GraphicsDevice.Viewport.Height - 32;
                goUp = false;
                jumpCount = 0;
            }
            else if (charSprite.Position.Y <= 0)
            {
                charSprite.Position.Y = 0;
            }

            if (charSprite.Position.X < 0)
            {
                charSprite.Position.X = 0;
            }
            else if (charSprite.Position.X + 32 > GraphicsDevice.Viewport.Width)
            {
                charSprite.Position.X = GraphicsDevice.Viewport.Width - 32;
            }
        }
    }
}
