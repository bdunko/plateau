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
    public class PEntityWallpaper : PlacedEntity
    {
        private PartialRecolorSprite sprite;

        public PEntityWallpaper(PartialRecolorSprite sprite, Vector2 tilePosition, PlaceableItem sourceItem, DrawLayer drawLayer) : base(tilePosition, sourceItem, drawLayer)
        {
            this.sprite = sprite;
            this.tileHeight = sprite.GetFrameHeight() / 8;
            this.tileWidth = sprite.GetFrameWidth() / 8;
            this.position = position;
            this.drawLayer = DrawLayer.BACKGROUND_WALLPAPER;
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            sprite.Draw(sb, position, Color.White, layerDepth);
        }

        public override RectangleF GetCollisionRectangle()
        {
            return new RectangleF(position, new Vector2(sprite.GetFrameWidth(), sprite.GetFrameHeight()));
        }

        public override void Update(float deltaTime, Area area)
        {
            sprite.Update(deltaTime);
        }
    }
}
