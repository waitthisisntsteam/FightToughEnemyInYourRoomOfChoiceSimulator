﻿using Microsoft.VisualBasic.Logging;
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

namespace FightToughEnemyInYourRoomOfChoiceSimulator
{
    public enum CharacterState
    {
        Jumping,
        DoubleJumping,
        Crouching,
        CrouchMoving,
        Idling,
        Running
    }

    public class Character : AnimatedSprite
    {
        private float charXSpeed;
        private float charYSpeed;

        private int jumpCount;
        private bool upPressed;
        private bool idle;
        private float gravity;

        //private Rectangle hitbox => new Rectangle((int)Position.X, (int)Position.Y, Image.Width * 2, Image.Height * 2);

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

        public void Update(/*GraphicsDevice graphicsDevice,*/ GameTime gameTime, List<Rectangle> hitBoxes)
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

            Rectangle characterHB = new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

            foreach (Rectangle hB in hitBoxes)
            {
                if (characterHB.Intersects(hB))
                {
                    /*
                    if (characterHB.X <= hB.X + hB.Width)
                    {
                        Position.X = hB.X + hB.Width + 1;
                    }
                    else if (characterHB.X + characterHB.Width >= hB.X)
                    {
                        Position.X = hB.X - characterHB.Width - 1;
                    }

                    if (characterHB.Y + characterHB.Height >= hB.Y)
                    {
                        Position.Y = hB.Y - characterHB.Height - 1;
                    }
                    else if (characterHB.Y <= hB.Y + hB.Height)
                    {
                        Position.Y = hB.Y + hB.Height + 1;
                    }*/

                    if (characterHB.Y + characterHB.Height >= hB.Y) //on top
                    {
                        if (charYSpeed > 0)
                        {
                            if (characterHB.X <= hB.X + hB.Width && characterHB.X + characterHB.Width >= hB.X)
                            {
                                Position.Y = hB.Y - characterHB.Height;
                                jumpCount = 0;
                            }
                        }
                    }
                

                    else if (Direction == SpriteEffects.None) //right of character hits left of hitbox
                    {
                        Position.X -= charXSpeed / 2;
                    }
                    else if (Direction == SpriteEffects.FlipHorizontally) //left of character hits right of hitbox
                    {
                        Position.X += charXSpeed / 2;
                    }
                }
            }

                //if (characterHB.Y + characterHB.Height >= hB.Y)
                //{
                //    if (charYSpeed > 0)
                //    {
                //        if (characterHB.X <= hB.X + hB.Width && characterHB.X + characterHB.Width >= hB.X)
                //        {
                //            Position.Y = hB.Y + hB.Height - characterHB.Height;
                //            jumpCount = 0;
                //        }
                //    }

                //    if (characterHB.X <= hB.X + hB.Width)
                //    {
                //        Position.X = hB.X + hB.Width + 1;
                //    }
                //    else if (characterHB.X + characterHB.Width >= hB.X)
                //    {
                //        Position.X = hB.X - characterHB.Width-1;
                //    }
                //}

                /*
                else if (characterHB.X <= hB.X + hB.Width && characterHB.Y + characterHB.Height < hB.Y)
                {
                    Position.X = hB.X + hB.Width;
                }
                else if (characterHB.X + characterHB.Width >= hB.X && characterHB.Y + characterHB.Height < hB.Y)
                {
                    Position.X = hB.X - characterHB.Width;               
                }*/
        }
    }
}