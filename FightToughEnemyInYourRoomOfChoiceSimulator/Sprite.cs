using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightToughEnemyInYourRoomOfChoiceSimulator
{
    public class Sprite
    {
        public Vector2 Position;
        public Color Tint;
        public Texture2D Image;

        public Sprite(Vector2 Position, Color Tint, Texture2D Image)
        {
            this.Position = Position;
            this.Tint = Tint;
            this.Image = Image;
        }
    }
}
