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
    class EntityItem : Entity
    {
        private Item itemForm;

        private float velocityY;
        private float velocityX;
        private static float GRAVITY = 8;
        private static float FRICTION_X = 1;
        private bool grounded;
        private float timeElapsed;

        private static float TIME_BEFORE_COLLECTION = 0.5f;
        private static int COLLISION_STEPS = 3;
        private static float MINIMUM_BOUNCE = 1.0F;
        private static float BOUNCE_MULTIPLIER = 0.6F;

        public EntityItem(Item itemForm, Vector2 position, Vector2 velocityReplacement) : this(itemForm, position)
        {
            if (velocityReplacement.X != 0)
            {
                velocityX = velocityReplacement.X;
            }
            if (velocityReplacement.Y != 0)
            {
                velocityY = velocityReplacement.Y;
            }
        }

        public EntityItem(Item itemForm, Vector2 position)
        {
            this.drawLayer = DrawLayer.PRIORITY;
            this.itemForm = itemForm;
            this.position = position;
            this.grounded = false;
            velocityX = Util.RandInt(-50, 50) / 100.0f;
            velocityY = Util.RandInt(-31, -24) / 10.0f;
            this.timeElapsed = 0;
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            itemForm.Draw(sb, new Vector2(position.X, position.Y + 2), Color.White, layerDepth);
        }

        public bool CanBeCollected()
        {
            return grounded || timeElapsed >= TIME_BEFORE_COLLECTION;
        }

        public Item GetItemForm()
        {
            return itemForm;
        }

        public override void Update(float deltaTime, Area area)
        {
            timeElapsed += deltaTime;
            velocityY += GRAVITY * deltaTime;
            velocityX = Util.AdjustTowards(velocityX, 0, FRICTION_X * deltaTime);


            //calculate collisions
            float stepX = velocityX / COLLISION_STEPS;
            float stepY = velocityY / COLLISION_STEPS;

            for (int step = 0; step < COLLISION_STEPS; step++)
            {
                if (stepX != 0) //move X
                {
                    bool xCollision = CollisionHelper.CheckCollision(new RectangleF(position.X+stepX, position.Y, 12, 16), area, stepY >= 0);
                    if (xCollision) //if next movement = collision
                    {
                        stepX = 0; //stop moving if collision
                    }
                    else
                    {
                        this.position.X += stepX;
                    }
                }
                if (stepY != 0) //move Y
                {
                    bool yCollision = CollisionHelper.CheckCollision(new RectangleF(position.X + 6, position.Y + 15 + stepY, 6, 1), area, stepY >= 0);
                    if (yCollision)
                    {
                        stepY = 0;
                        grounded = true;
                        if (velocityY > MINIMUM_BOUNCE)
                        {
                            velocityY = -velocityY * BOUNCE_MULTIPLIER;
                            //velocityX -= velocityX / 3;
                        }
                        else
                        {
                            velocityY = 0;
                        }
                    }
                    else
                    {
                        this.position.Y += stepY;
                    }
                }
            }
        }

        public override RectangleF GetCollisionRectangle()
        {
            return new RectangleF(position.X - 12, position.Y - 18, 24, 36);
        }
    }
}
