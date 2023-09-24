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
    enum CharacterState
    { 
        Jumping,
        Crouching,
        Idling,
        Running
    }


    public class Character
    {
        private Sprite charSprite;
        private float charXSpeed;
        private float charYSpeed;

        CharacterState characterState;

        private List<Frame> idleFrames;
        private List<Frame> runningFrames;
        private Frame crouchFrame;
        private Frame jumpFrame;

        private int jumpCount;
        private bool upPressed;
        private float gravity;

        private int currentFrame;
        private TimeSpan updateTime;
        private TimeSpan elapsedTime;

        private int idle;
        private int run;

        private SpriteEffects direction;

        //private Rectangle hitbox;

        public Character(Sprite charSprite, float charXSpeed, float charYSpeed, List<Frame> idleFrames, List<Frame> runningFrames, Frame jumpFrame, Frame crouchFrame)
        {
            this.charSprite = charSprite;
            this.charXSpeed = charXSpeed;
            this.charYSpeed = charYSpeed;

            this.idleFrames = idleFrames;
            this.runningFrames = runningFrames;
            this.crouchFrame = crouchFrame;
            this.jumpFrame = jumpFrame;

            jumpCount = 0;
            upPressed = false;
            gravity = 0.2f;

            direction = SpriteEffects.None;

            idle = 0;
            run = 0;
            currentFrame = 0;
            updateTime = TimeSpan.FromMilliseconds(400);
            elapsedTime = TimeSpan.Zero;

            characterState = CharacterState.Idling;

            //hitbox = new Rectangle((int)charSprite.Position.X, (int)charSprite.Position.Y, 32, 32);
        }

        public void Update(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            charYSpeed += gravity;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                upPressed = true;
            }
            else
            {
                characterState = CharacterState.Idling;
                idle++;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Up) && upPressed && jumpCount < 2)
            {
                jumpCount++;
                characterState = CharacterState.Jumping;
                charYSpeed = -7;
                idle = 0;
                run = 0;
                upPressed = false;
            }
            charSprite.Position.Y += charYSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                characterState = CharacterState.Crouching;
                run = 0;
                idle = 0;
            }
            else
            {
                characterState = CharacterState.Crouching;
                idle++;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    charSprite.Position.X -= charXSpeed/2;
                }
                else
                {
                    charSprite.Position.X -= charXSpeed;
                }
                idle = 0;
                direction = SpriteEffects.FlipHorizontally;
            }
            else
            {
                characterState = CharacterState.Idling;
                idle++;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    charSprite.Position.X += charXSpeed/2;
                }
                else
                {
                    charSprite.Position.X += charXSpeed;
                }
                idle = 0;
                direction = SpriteEffects.None;
            }
            else
            {
                characterState = CharacterState.Idling;
                idle++;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                characterState = CharacterState.Running;
                run++;
            }
            else
            {
                characterState = CharacterState.Running;
                run++;
            }

            if (idle > 3 || characterState == CharacterState.Running)
            {
                elapsedTime += gameTime.ElapsedGameTime;
                if (elapsedTime > updateTime)
                {
                    elapsedTime = TimeSpan.Zero;
                    currentFrame++;
                    if (idle > 0 && currentFrame == idleFrames.Count)
                    {
                        currentFrame = 0;
                    }
                    if (run > 0 && currentFrame == runningFrames.Count)
                    {
                        currentFrame = 0;
                    }
                }

            }
            else
            {
                currentFrame = 0;
            }

            if (charSprite.Position.Y >= graphicsDevice.Viewport.Height - 32)
            {
                charSprite.Position.Y = graphicsDevice.Viewport.Height - 32;
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
            else if (charSprite.Position.X + 32 > graphicsDevice.Viewport.Width)
            {
                charSprite.Position.X = graphicsDevice.Viewport.Width - 32;
            }
        }

        public string getState()
        {
            if (characterState == CharacterState.Jumping)
            {
                return "jumping";
            }
            else if (characterState == CharacterState.Crouching)
            {
                return "crouhing";
            }
            else if (characterState == CharacterState.Idling)
            {
                return "idling";
            }
            else if (characterState == CharacterState.Running)
            {
                return "running";
            }
            return "how did you break the code ???";
        }

        public SpriteEffects getDirection()
        {
            return direction;
        }

        public Vector2 getCharacterPosition()
        {
            return charSprite.Position;
        }

        public int getCurrentFrame()
        {
            return currentFrame;
        }
    }
}
