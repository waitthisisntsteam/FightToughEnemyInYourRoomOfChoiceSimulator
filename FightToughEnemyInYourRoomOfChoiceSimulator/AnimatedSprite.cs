using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace FightToughEnemyInYourRoomOfChoiceSimulator
{
    public abstract class AnimatedSprite : Sprite
    {
        public Dictionary<CharacterState, List<Frame>> map = new Dictionary<CharacterState, List<Frame>>();
        public int currentFrame;
        public CharacterState characterState;

        public AnimatedSprite(Vector2 Position, Color Tint, Texture2D Image, SpriteEffects Direction) : base(Position, Tint, Image, Direction)
        {
            
        }

        public void AddFrames(CharacterState state, List<Frame> frames)
        {
            map.Add(state, frames);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(currentFrame/24 >= map[characterState].Count)
            {
                currentFrame = 0;
            }
            spriteBatch.Draw(Image, Position, map[characterState][currentFrame/24].SourceRectangle, Color.White, 0f, map[characterState][currentFrame/24].Origin, 2f, Direction, 0f);
        }
    }
}
