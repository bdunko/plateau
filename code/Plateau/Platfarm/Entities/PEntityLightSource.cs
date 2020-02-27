using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Components;
using Platfarm.Items;

namespace Platfarm.Entities
{
    public class PEntityLightSource : PlacedEntity
    {
        private Area.LightSource.Strength lightStrength;
        private PartialRecolorSprite sprite;

        public PEntityLightSource(PartialRecolorSprite sprite, Vector2 tilePosition, Area.LightSource.Strength lightStrength, PlaceableItem sourceItem, DrawLayer drawLayer) : base(tilePosition, sourceItem, drawLayer)
        {
            this.sprite = sprite;
            this.tileHeight = sprite.GetFrameHeight() / 8;
            this.tileWidth = sprite.GetFrameWidth() / 8;
            this.itemForm = sourceItem;
            this.lightStrength = lightStrength;
        }

        public Color GetLightColor()
        {
            return Color.White;
            //return ((PlaceableItem)itemForm).GetColor();
        }

        public Area.LightSource.Strength GetLightStrength()
        {
            return lightStrength;
        }

        public Vector2 GetLightPosition()
        {
            Vector2 lightPos = new Vector2(position.X, position.Y);
            lightPos.X += sprite.GetFrameWidth() / 2;
            lightPos.Y += 6;
            return lightPos;
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            sprite.Draw(sb, new Vector2(position.X, position.Y + 1), Color.White, layerDepth);
        }

        public override void LoadSave(SaveState state)
        {
            //nothing to load
        }

        public override void Update(float deltaTime, Area area)
        {
            sprite.Update(deltaTime);
            if(sprite.IsCurrentLoopFinished())
            {
                sprite.SetLoop("anim");
            }
        }

        public override void OnRemove(EntityPlayer player, Area area, World world)
        {
            if (area.GetCollisionTypeAt((int)this.tilePosition.X, (int)this.tilePosition.Y) == Area.CollisionTypeEnum.SOLID)
            {
                this.position.Y -= 8;
            }
            base.OnRemove(player, area, world);
        }

        public override RectangleF GetCollisionRectangle()
        {
            return new RectangleF(position, new Size2(sprite.GetFrameWidth(), sprite.GetFrameHeight()));
        }
    }
}
