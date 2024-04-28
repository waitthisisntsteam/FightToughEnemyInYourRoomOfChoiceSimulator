using Microsoft.VisualBasic.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Net;
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

        private bool phaseThrough;

        private int jumpCount;
        private bool upPressed;
        public bool idle;
        private float gravity;

        private Keys up;
        private Keys down;
        private Keys left;
        private Keys right;

        private Rectangle characterHB;

        public Character(Vector2 Position, Texture2D Image, List<List<Frame>> Frames, float charXSpeed, float charYSpeed, Keys up, Keys down, Keys left, Keys right)

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

            this.up = up;
            this.down = down;
            this.left = left;
            this.right = right;

            phaseThrough = false;

            characterState = CharacterState.Idling;
        }



        public void Update(GameTime gameTime, List<Rectangle> hitBoxes, HashSet<Keys> keysDown)
        {
            phaseThrough = false;

            if (characterState == CharacterState.Crouching)
            {
                charYSpeed += gravity * 4;
            }
            else
            {
                charYSpeed += gravity;
            }

            if (keysDown.Contains(up))
            {
                idle = false;
                upPressed = true;
            }
            if (!keysDown.Contains(up) && upPressed && jumpCount < 2)
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
            if (keysDown.Contains(down))
            {
                phaseThrough = true;
                idle = false;
            }
            if (!keysDown.Contains(down) && jumpCount > 0)
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
            if (keysDown.Contains(left))
            {
                if (keysDown.Contains(down))
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

                if (jumpCount == 0 && !keysDown.Contains(down))
                {
                    characterState = CharacterState.Running;
                }

                Direction = SpriteEffects.FlipHorizontally;
                idle = false;
            }
            if (keysDown.Contains(right))
            {
                if (keysDown.Contains(down))
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

                if (jumpCount == 0 && !keysDown.Contains(down))
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



            characterHB = GetHitbox();
            foreach (Rectangle hB in hitBoxes)
            {
                if (characterHB.Intersects(hB))
                {
                    if (Math.Abs(hB.Height) > 20)
                    {
                        phaseThrough = false;
                    }

                    if (!phaseThrough && characterHB.Bottom >= hB.Top && characterHB.Y < hB.Top - characterHB.Height + charYSpeed + 10 && characterHB.Right < hB.Right && characterHB.Left > hB.Left)
                    {
                        Position.Y -= charYSpeed;
                        charYSpeed = 0;
                        jumpCount = 0;
                    }
                    //else if (characterHB.Top <= hB.Bottom && characterHB.Bottom > hB.Bottom && characterHB.Right < hB.Right && characterHB.Left > hB.Left)
                    //{
                    //    phaseThrough = false;
                    //    Position.Y = hB.Bottom;
                    //}
                    else if (characterHB.Left <= hB.Right && Direction == SpriteEffects.FlipHorizontally && characterHB.Right > hB.Right && characterHB.Left < hB.Left)
                    {
                        Position.X += charXSpeed;
                    }
                    else if (characterHB.Right >= hB.Left && Direction == SpriteEffects.None && characterHB.Right > hB.Right && characterHB.Left < hB.Left)
                    {
                        Position.X -= charXSpeed;
                    }
                }
            }
        }

        public Rectangle GetHitbox()
        {
            if (up == Keys.Up)
            {
               characterHB = new Rectangle((int)Position.X, (int)Position.Y, 32, 32);
            }
            if (up == Keys.W)
            {
                characterHB = new Rectangle((int)Position.X + 16, (int)Position.Y + 18, 32, 32);
            }

            return characterHB;
        }
    }
}
