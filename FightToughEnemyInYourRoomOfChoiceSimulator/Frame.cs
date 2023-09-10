using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightToughEnemyInYourRoomOfChoiceSimulator
{
    public class Frame
    {
        public Vector2 Origin;
        public Rectangle SourceRectangle;

        public Frame(Vector2 Origin, Rectangle SourceRectangle)
        {
            this.Origin = Origin;
            this.SourceRectangle = SourceRectangle;
        }
    }
}
