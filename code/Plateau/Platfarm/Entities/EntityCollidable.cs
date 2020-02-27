using MonoGame.Extended;
using Platfarm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Entities
{
    public abstract class EntityCollidable : Entity
    {
        protected abstract void OnXCollision();
        protected abstract void OnYCollision();

        public bool IsColliding(RectangleF solidRect)
        {
            return solidRect.Intersects(GetCollisionRectangle());
        }

        public virtual bool IsRiding(RectangleF solidRect)
        {
            RectangleF ridingHitbox = GetCollisionRectangle();
            ridingHitbox.Y -= 0.5f;
            return solidRect.Intersects(ridingHitbox) && GetCollisionRectangle().Bottom - 0.2f < solidRect.Top;
        }

        public virtual bool PushX(float pushX, Area area)
        {
            RectangleF collisionBox = GetCollisionRectangle();
            collisionBox.X += pushX;
            if (CollisionHelper.CheckCollision(collisionBox, area, true)) //if next movement = collision
            {
                OnXCollision();
                return false;
            }
            else
            {
                this.position.X += pushX;
                return true;
            }
        }

        public virtual bool PushY(float pushY, Area area)
        {
            RectangleF collisionBox = GetCollisionRectangle();
            collisionBox.Y += pushY;
            if (CollisionHelper.CheckCollision(collisionBox, area, true)) //if next movement = collision
            {
                OnYCollision();
                return false;
            }
            else
            {
                this.position.Y += pushY;
                return true;
            }
        }
    }
}
