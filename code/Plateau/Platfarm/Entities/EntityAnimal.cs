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
    public class EntityAnimal : EntityCollidable, IInteract, ITickDaily, ITick
    {
        public enum Type
        {
            COW, CHICKEN, SHEEP, PIG, CAT, DOG
        }

        private static float SPEED = 0.25f;
        private static float HERD_SPEED = 0.3f;
        private static float HERD_TIME = 4.5f;
        private static float GRAVITY = 8;
        private static float JUMP_SPEED = 1.7f;
        private static int COLLISION_STEPS = 3;
        private bool triedJump;

        private AnimatedSprite sprite;
        private LootTables.LootTable lootTable;
        private Item harvestedWith;
        private bool harvestable;
        private Type animalType;
        private float velocityX, velocityY;
        private float herdTimer;
        private DirectionEnum direction;
        private bool grounded;

        public EntityAnimal(AnimatedSprite sprite, Vector2 position, LootTables.LootTable lootTable, Item harvestedWith, Type type)
        {
            this.grounded = false;
            this.velocityX = 0;
            this.velocityY = 0;
            this.herdTimer = 0;
            this.triedJump = false;
            this.sprite = sprite;
            this.position = position;
            this.lootTable = lootTable;
            this.harvestedWith = harvestedWith;
            this.animalType = type;
            this.direction = DirectionEnum.LEFT;
            if (animalType == Type.PIG) {
                this.harvestable = false;
            } else {
                this.harvestable = true;
            }
            this.drawLayer = DrawLayer.PRIORITY;
            UpdateAnimation();
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            sprite.Draw(sb, position, Color.White, layerDepth);
        }

        public override RectangleF GetCollisionRectangle()
        {
            if(animalType == Type.CHICKEN)
            {
                return new RectangleF(position.X+2, position.Y+6, 10, 10);
            } else if (animalType == Type.PIG || animalType == Type.COW)
            {
                return new RectangleF(position.X + 3, position.Y, 16, 16);
            }
            return new RectangleF(position.X, position.Y, sprite.GetFrameWidth(), sprite.GetFrameHeight());
        }

        private void Jump()
        {
            if (grounded)
            {
                velocityY = -JUMP_SPEED;
            }
        }

        public override void Update(float deltaTime, Area area)
        {
            sprite.Update(deltaTime);
            velocityY += GRAVITY * deltaTime;

            if(herdTimer >= 0)
            {
                herdTimer -= deltaTime;
                if(herdTimer < 0)
                {
                    velocityX = 0;
                }
            }

            if(herdTimer < 0)
            {
                if(velocityX == 0 && Util.RandInt(0, 50) == 0)
                {
                    if (Util.RandInt(1, 2) == 1)
                    {
                        velocityX = SPEED;
                        direction = DirectionEnum.RIGHT;
                        velocityY = -0.6f;
                        triedJump = false;
                    }
                    else
                    {
                        velocityX = -SPEED;
                        direction = DirectionEnum.LEFT;
                        velocityY = -0.6f;
                        triedJump = false;
                    }
                } else
                {
                    if (Util.RandInt(0, 100) == 0 && grounded)
                    {
                        velocityX = 0;
                    }
                }
            }

            //calculate collisions
            RectangleF collisionBox = GetCollisionRectangle();
            float stepX = velocityX / COLLISION_STEPS;
            for (int step = 0; step < COLLISION_STEPS; step++)
            {
                if (stepX != 0) //move X
                {
                    collisionBox = GetCollisionRectangle();
                    RectangleF stepXCollisionBox = new RectangleF(collisionBox.X + stepX, collisionBox.Y, collisionBox.Width, collisionBox.Height);
                    bool xCollision = CollisionHelper.CheckCollision(stepXCollisionBox, area, true);
                    RectangleF stepXCollisionBoxForesight = new RectangleF(collisionBox.X + (stepX*18), collisionBox.Y, collisionBox.Width, collisionBox.Height);
                    bool xCollisionSoon = CollisionHelper.CheckCollision(stepXCollisionBoxForesight, area, true);
                    RectangleF stepXCollisionBoxBoundary = new RectangleF(collisionBox.X + (stepX*55), collisionBox.Y, collisionBox.Width, collisionBox.Height);
                    bool xCollisionBoundary = CollisionHelper.CheckForCollisionType(stepXCollisionBoxBoundary, area, Area.CollisionTypeEnum.BOUNDARY);
                    bool solidFound = false;
                    Vector2 tenativePos = this.position + new Vector2(stepX, 0);
                    if (direction == DirectionEnum.LEFT)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            Area.CollisionTypeEnum collType = area.GetCollisionTypeAt((int)(tenativePos.X) / 8, ((int)(tenativePos.Y + sprite.GetFrameHeight()) / 8) + i);
                            if (collType == Area.CollisionTypeEnum.SOLID ||
                                collType == Area.CollisionTypeEnum.BRIDGE ||
                                collType == Area.CollisionTypeEnum.SCAFFOLDING_BLOCK ||
                                collType == Area.CollisionTypeEnum.SCAFFOLDING_BRIDGE)
                            {
                                solidFound = true;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            Area.CollisionTypeEnum collType = area.GetCollisionTypeAt((int)(tenativePos.X + sprite.GetFrameWidth()) / 8, ((int)(tenativePos.Y + sprite.GetFrameHeight()) / 8) + i);
                            if (collType == Area.CollisionTypeEnum.SOLID ||
                                collType == Area.CollisionTypeEnum.BRIDGE ||
                                collType == Area.CollisionTypeEnum.SCAFFOLDING_BLOCK ||
                                collType == Area.CollisionTypeEnum.SCAFFOLDING_BRIDGE)
                            {
                                solidFound = true;
                            }
                        }
                    }

                    if (xCollisionSoon && !triedJump)
                    {
                        Jump();
                        triedJump = true;
                    }

                    if (xCollision || xCollisionBoundary || !solidFound) //if next movement = collision
                    {
                        if(!triedJump)
                        {
                            Jump();
                            triedJump = true;
                        }
                        stepX = 0; //stop moving if collision
                        if (grounded)
                        {
                            velocityX = 0;
                        }
                    } 
                    else
                    {
                        this.position.X += stepX;
                        triedJump = false;
                    }
                }
            }

            
            float stepY = velocityY / COLLISION_STEPS;
            for (int step = 0; step < COLLISION_STEPS; step++)
            {
                if (stepY != 0) //move Y
                {
                    collisionBox = GetCollisionRectangle();
                    RectangleF stepYCollisionBox = new RectangleF(collisionBox.X, collisionBox.Y + stepY, collisionBox.Width, collisionBox.Height);
                    bool yCollision = CollisionHelper.CheckCollision(stepYCollisionBox, area, stepY > 0);

                    if (yCollision)
                    {
                        if(velocityY > 0)
                        {
                            grounded = true;
                        } 
                        stepY = 0;
                        velocityY = 0;
                        
                    }
                    else
                    {
                        this.position.Y += stepY;
                        grounded = false;
                    }
                }
            }

            UpdateAnimation();
        }

        public string GetLeftShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public string GetRightShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public void SetHarvestable(bool harvestable)
        {
            this.harvestable = harvestable;
        }

        public bool IsHarvestable()
        {
            return harvestable;
        }

        public string GetLeftClickAction(EntityPlayer player)
        {
            if (this.harvestable)
            {
                return "Gather";
            }
            return "";
        }

        public string GetRightClickAction(EntityPlayer player)
        {
            return "Herd";
        }

        private void UpdateAnimation()
        {
            if (velocityX == 0)
            {
                if (animalType == Type.SHEEP && !harvestable) {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? "idle2L": "idle2R");
                }
                else
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT? "idleL" : "idleR");
                }
            } else
            {
                if(animalType == Type.SHEEP && !harvestable) {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? "walk2L" : "walk2R");
                } else
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? "walkL" : "walkR");
                }
             }
        }

        public void InteractRight(EntityPlayer player, Area area, World world)
        {
            if (player.GetDirection() == DirectionEnum.LEFT)
            {
                velocityX = -HERD_SPEED;
                herdTimer = HERD_TIME;
                direction = DirectionEnum.LEFT;
            }
            else
            {
                velocityX = HERD_SPEED;
                herdTimer = HERD_TIME;
                direction = DirectionEnum.RIGHT;
            }
        }

        public void InteractLeft(EntityPlayer player, Area area, World world)
        {
            Item item = player.GetHeldItem().GetItem();
            if (harvestedWith != ItemDict.NONE && item == harvestedWith)
            {
                if (harvestable)
                {
                    List<Item> drops = lootTable.RollLoot(player, area, world.GetTimeData());
                    foreach (Item drop in drops)
                    {
                        area.AddEntity(new EntityItem(drop, new Vector2(position.X, position.Y - 10)));
                    }
                    harvestable = false;
                }
                else
                {
                    player.AddNotification(new EntityPlayer.Notification("I've already gathered from this one recently.", Color.Red));
                }
            }
            else
            {
                player.AddNotification(new EntityPlayer.Notification("I need the proper tool to do this.", Color.Red));
            }
        }

        public void InteractRightShift(EntityPlayer player, Area area, World world)
        {
            
        }

        public void InteractLeftShift(EntityPlayer player, Area area, World world)
        {
            
        }

        public void TickDaily(World world, Area area, EntityPlayer player)
        {
            if(this.animalType != Type.PIG)
            {
                harvestable = true;
            }
        }

        public void Tick(int minutesTicked, EntityPlayer player, Area area, World world)
        {
            if(animalType == Type.PIG)
            {
                if (world.GetTimeData().timeOfDay != World.TimeOfDay.NIGHT)
                {
                    if (Util.RandInt(1, 55) == 1)
                    {
                        List<Item> drops = lootTable.RollLoot(player, area, world.GetTimeData());
                        foreach (Item drop in drops)
                        {
                            area.AddEntity(new EntityItem(drop, new Vector2(position.X, position.Y - 10)));
                        }
                    }
                }
            }
        }

        protected override void OnXCollision()
        {
            velocityX = 0;
        }

        protected override void OnYCollision()
        {
            velocityY = 0;
        }
    }
}
