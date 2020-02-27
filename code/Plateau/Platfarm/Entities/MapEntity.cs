using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Components;

namespace Platfarm.Entities
{
    public class MapEntity : Entity
    {
        protected AnimatedSprite sprite;
        private string id;

        public MapEntity(string id, Vector2 position, AnimatedSprite sprite)
        {
            this.id = id;
            this.position = position;
            this.sprite = sprite;
            this.drawLayer = DrawLayer.NORMAL;
        }

        public string GetID()
        {
            return id;
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            sprite.Draw(sb, this.position, Color.White, layerDepth);
        }

        public override RectangleF GetCollisionRectangle()
        {
            return new RectangleF(this.position, new Vector2(sprite.GetFrameWidth(), sprite.GetFrameHeight()));
        }

        public override void Update(float deltaTime, Area area)
        {
            sprite.Update(deltaTime);
        }
    }
}
