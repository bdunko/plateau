using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Platfarm.Items.Item;

namespace Platfarm.Items
{
    public class DamageDealingItem : Item
    {
        private int damage;

        public DamageDealingItem(string name, string texturePath, int stackCapacity, int damage, string description, int value, params Tag[] tags) : base(name, texturePath, stackCapacity, description, value, tags)
        {
            this.damage = damage;
        }

        public int GetDamage()
        {
            return damage;
        }
    }
}
