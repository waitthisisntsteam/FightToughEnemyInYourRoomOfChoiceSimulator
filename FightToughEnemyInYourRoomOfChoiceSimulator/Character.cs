using Microsoft.VisualBasic.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text.Json.Serialization.Metadata;

namespace FightToughEnemyInYourRoomOfChoiceSimulator
{
    public enum CharacterState
    {
        Jumping,
        DoubleJumping,
        Crouching,
        CrouchMoving,
        Idling,
        Running,
        Falling
    }

    public class Character : AnimatedSprite
    {
        private float charXSpeed;
        private float charYSpeed;

        private int jumpCount;
        private bool upPressed;
        private bool idle;
        private float gravity;

        public Character(Vector2 Position, Texture2D Image, List<List<Frame>> Frames, float charXSpeed, float charYSpeed)

            : base(Position, Color.White, Image, SpriteEffects.None)
        {
            for (int i = 0; i < Frames.Count; i++)
            {
                AddFrames((CharacterState)i, Frames[i]);
            }

            this.charXSpeed = charXSpeed;
            this.charYSpeed = charYSpeed;

            jumpCount = 0;
            upPressed = false;
            gravity = 0.2f;
            idle = true;

            characterState = CharacterState.Idling;
        }




        public void Update(GameTime gameTime, List<Rectangle> hitBoxes)
        {
            currentFrame++;
            if (characterState == CharacterState.Crouching)
            {
                charYSpeed += gravity * 4;
            }
            else
            {
                charYSpeed += gravity;
            }
            idle = true;

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                idle = false;
                upPressed = true;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Up) && upPressed && jumpCount < 2)
            {
                jumpCount++;
                if (jumpCount == 1)
                {
                    characterState = CharacterState.Jumping;
                }
                if (jumpCount == 2)
                {
                    characterState = CharacterState.DoubleJumping;
                }
                charYSpeed = -7;
                upPressed = false;
                currentFrame = 0;
            }
            Position.Y += charYSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                characterState = CharacterState.Crouching;
                idle = false;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Down) && jumpCount > 0)
            {
                if (jumpCount == 1)
                {
                    characterState = CharacterState.Jumping;
                }
                if (jumpCount == 2)
                {
                    characterState = CharacterState.DoubleJumping;
                }
                idle = false;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    Position.X -= charXSpeed / 2;
                    if (jumpCount == 0)
                    {
                        characterState = CharacterState.CrouchMoving;
                    }
                }
                else
                {
                    Position.X -= charXSpeed;
                }

                if (jumpCount == 0 && Keyboard.GetState().IsKeyUp(Keys.Down))
                {
                    characterState = CharacterState.Running;
                }

                Direction = SpriteEffects.FlipHorizontally;
                idle = false;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    Position.X += charXSpeed / 2;
                    if (jumpCount == 0)
                    {
                        characterState = CharacterState.CrouchMoving;
                    }
                }
                else
                {
                    Position.X += charXSpeed;
                }

                if (jumpCount == 0 && Keyboard.GetState().IsKeyUp(Keys.Down))
                {
                    characterState = CharacterState.Running;
                }

                Direction = SpriteEffects.None;
                idle = false;
            }
            if (idle && jumpCount == 0)
            {
                characterState = CharacterState.Idling;
            }
            if (charYSpeed > 1 && jumpCount == 0)
            {
                characterState = CharacterState.Falling;
            }



            Rectangle characterHB = new Rectangle((int)Position.X, (int)Position.Y, 32, 32);
            foreach (Rectangle hB in hitBoxes)
            {
                if (characterHB.Intersects(hB))
                {
                    if (characterHB.Bottom >= hB.Top && characterHB.Top < hB.Top && characterHB.Right < hB.Right && characterHB.Left > hB.Left)
                    {
                        //Position.Y = hB.Top - characterHB.Height;

                        Position.Y -= charYSpeed;
                        charYSpeed = 0;
                        jumpCount = 0;
                    }
                    else if (characterHB.Top <= hB.Bottom && characterHB.Bottom > hB.Bottom && characterHB.Right < hB.Right && characterHB.Left > hB.Left)
                    {
                        Position.Y = hB.Bottom;
                    }
                    else if (characterHB.Left <= hB.Right && Direction == SpriteEffects.FlipHorizontally && characterHB.Right > hB.Right && characterHB.Left < hB.Left)
                    {
                        //Position.X = hB.Right;

                        Position.X += charXSpeed;
                    }
                    else if (characterHB.Right >= hB.Left && Direction == SpriteEffects.None && characterHB.Right > hB.Right && characterHB.Left < hB.Left)
                    {
                        //Position.X = hB.Left - characterHB.Width;

                        Position.X -= charXSpeed;
                    }
                }
            }
        }
    }
}
