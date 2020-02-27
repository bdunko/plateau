using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Entities
{
    public enum DrawLayer
    {
        FOREGROUND_CARPET, FOREGROUND, PRIORITY, NORMAL, BACKGROUND_WALL, BACKGROUND_WALLPAPER
    }

    public abstract class Entity
    {
        protected Vector2 position;
        protected DrawLayer drawLayer = DrawLayer.NORMAL;

        public DrawLayer GetDrawLayer()
        {
            return drawLayer;
        }

        public void SetDrawLayer(DrawLayer layer)
        {
            drawLayer = layer;
        }

        public virtual void SetPosition(Vector2 position)
        {
            this.position = position;
        }
        public Vector2 GetPosition()
        {
            return this.position;
        }

        public abstract RectangleF GetCollisionRectangle();
        public abstract void Draw(SpriteBatch sb, float layerDepth);
        public abstract void Update(float deltaTime, Area area);
    }
}
