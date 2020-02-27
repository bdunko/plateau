using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using static Platfarm.Components.Util;

namespace Platfarm.Items
{
    public class DyeItem : Item
    {
        private RecolorMap color;

        public DyeItem(string name, string texturePath, int stackCapacity, RecolorMap color, string description, int value, params Tag[] tags) : base(name, texturePath, stackCapacity, description, value, tags)
        {
            this.color = color;
        }

        public string GetDyedNameAdjustment()
        {
            return " (" + color.name + ")";
        }
    }
}
