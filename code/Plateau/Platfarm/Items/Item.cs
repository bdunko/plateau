using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Items
{
    public class Item
    {
        public enum Tag
        {
            TOOL, AXE, PICKAXE, FISHING_ROD, WATERING_CAN, HOE, RARE,
            SILVER_CROP, GOLDEN_CROP, PHANTOM_CROP, SHINING_SEED, SAND_PLANT_ONLY, SOIL_PLANT_ONLY,
            FORAGE, CROP, SEED, COMPOST, FLOWER, VEGETABLE, FRUIT, INSECT, FISH, MUSHROOM, WOOD, GEM, ORE,
            BAR, FOOD, CLOTHING, PERFUME, ALCHEMY, DYE, BEE_PRODUCT, BAT_PRODUCT, BIRD_PRODUCT, ANIMAL_PRODUCT, TEXTILE, MATERIAL,
            HAT, PANTS, SHOES, SOCKS, EARRINGS, SAILCLOTH, SHIRT, OUTERWEAR, BACK, GLASSES, GLOVES, ACCESSORY, SCARF, SKIN, HAIR, DYEABLE, NO_TRASH, EYES, ALWAYS_HIDE_HAIR, ALWAYS_SHOW_HAIR, HIDE_WHEN_HAT, DRAW_OVER_SHOES,
            PLACEABLE
        }
        //NO_TRASH - prevents item from being thrown away; ex Tools
        //REMOVABLE - used on all placeable items that can be removed using edit mode right click
        //COMPOSTABLE - indicates that an item is compostable

        private string name;
        private int stackCapacity;
        protected Texture2D texture;
        private string texturePath;
        private bool isLoaded;
        private string description;
        private int value;
        private Tag[] tags;

        public Item(string name, string texturePath, int stackCapacity, string description, int value, params Tag[] tags)
        {
            this.name = name;
            this.texture = texture;
            this.stackCapacity = stackCapacity;
            this.description = description;
            this.tags = tags;
            this.value = value;
            this.isLoaded = false;
            this.texturePath = texturePath;
        }

        protected bool IsLoaded()
        {
            return isLoaded;
        }

        public int GetValue()
        {
            return this.value;
        }

        public string GetName()
        {
            return name;
        }

        public int GetStackCapacity()
        {
            return stackCapacity;
        }

        public virtual void Load()
        {
            texture = Plateau.CONTENT.Load<Texture2D>(texturePath);
            isLoaded = true;
        }

        public Texture2D GetTexture()
        {
            if(!this.IsLoaded())
            {
                this.Load();
            }
            return texture;
        }

        public string GetDescription()
        {
            return description;
        }

        public Tag[] GetTags()
        {
            return tags;
        }

        public bool HasTag(Tag tag)
        {
            for(int i = 0; i < tags.Length; i++)
            {
                if(tag == tags[i])
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void Draw(SpriteBatch sb, Vector2 position, Color color, float layerDepth)
        {
            if (!this.IsLoaded())
            {
                Load();
            }
            sb.Draw(texture, position, texture.Bounds, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerDepth);
        }
    }
}
