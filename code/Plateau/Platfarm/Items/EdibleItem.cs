using Platfarm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Items
{
    public class EdibleItem : Item
    {
        private AppliedEffects.Effect whenEaten;
        private string onEatDescription;
        private float effectLength;

        public EdibleItem(string name, string texturePath, int stackCapacity, string description, AppliedEffects.Effect whenEaten, float effectLength, string onEatDescription, int value, params Tag[] tags) : base(name, texturePath, stackCapacity, description, value, tags)
        {
            this.whenEaten = whenEaten;
            this.effectLength = effectLength;
            this.onEatDescription = onEatDescription;
        }

        public AppliedEffects.Effect GetEffect()
        {
            return this.whenEaten;
        }

        public float GetEffectLength()
        {
            return this.effectLength;
        }

        public string GetOnEatDescription()
        {
            return this.onEatDescription;
        }
    }
}
