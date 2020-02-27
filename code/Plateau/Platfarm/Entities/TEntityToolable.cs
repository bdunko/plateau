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
using Platfarm.Particles;

namespace Platfarm.Entities
{
    class TEntityToolable : TileEntity, IInteractTool
    {
        private Texture2D texture;
        private EntityType type;
        private HealthBar healthBar;
        private LootTables.LootTable lootTable;
        private Item.Tag toolUsed;
        private Color particlePrimary, particleSecondary;

        private static float SHAKE_DURATION = 0.19f;
        private static float SHAKE_INTERVAL = 0.06f;
        private const float SHAKE_AMOUNT = 0.75f;
        private float shakeTimeLeft;
        private float shakeTimer;
        private float shakeModX;

        public TEntityToolable(Texture2D texture, Vector2 tilePosition, EntityType type, Item.Tag toolUsed, HealthBar hb, LootTables.LootTable lootTable, Color particlePrimary, Color particleSecondary)
        {
            this.toolUsed = toolUsed;
            this.healthBar = hb;
            this.texture = texture;
            this.type = type;
            this.position = new Vector2(tilePosition.X * 8, tilePosition.Y * 8);
            this.position.Y += 1;
            this.tilePosition = tilePosition;
            this.tileHeight = texture.Height / 8;
            this.tileWidth = texture.Width / 8;
            this.particlePrimary = particlePrimary;
            this.particleSecondary = particleSecondary;
            this.lootTable = lootTable;
            this.shakeTimeLeft = 0;
            this.shakeTimer = 0;
            this.shakeModX = 0;
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            sb.Draw(texture, position + new Vector2(shakeModX, 0), Color.White);
            healthBar.Draw(sb, this.position + new Vector2((texture.Width/2) - (healthBar.GetWidth()/2), -8), 1.0f);
        }

        public override RectangleF GetCollisionRectangle()
        {
            return new RectangleF(position, new Size2(texture.Width, texture.Height));
        }

        public string GetLeftClickAction(EntityPlayer player)
        {
            return "";
        }

        public string GetLeftShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public string GetRightClickAction(EntityPlayer player)
        {
            return "";
        }

        public string GetRightShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public void InteractTool(EntityPlayer player, Area area, World world)
        {
            Item tool = player.GetHeldItem().GetItem();
            if (tool.HasTag(toolUsed))
            {
                shakeTimeLeft = SHAKE_DURATION;
                shakeTimer = 0;
                healthBar.Damage(((DamageDealingItem)tool).GetDamage());
                for (int i = 0; i < 6; i++)
                {
                    area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(texture.Width/2, texture.Height/2), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                        particlePrimary, ParticleFactory.DURATION_MEDIUM));
                }

                if (healthBar.IsDepleted())
                {
                    area.RemoveTileEntity(player, (int)tilePosition.X, (int)tilePosition.Y, world);
                    List<Item> drops = lootTable.RollLoot(player, area, world.GetTimeData());
                    foreach (Item drop in drops)
                    {
                        area.AddEntity(new EntityItem(drop, new Vector2(position.X, position.Y - 10)));
                    }

                    for (int i = 0; i < 7; i++)
                    {
                        area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(texture.Width / 2, texture.Height / 2), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.SMALL,
                            particlePrimary, ParticleFactory.DURATION_MEDIUM));
                    }
                    for(int i = 0; i < 4; i++)
                    {
                        area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(texture.Width / 2, texture.Height / 2), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.SMALL,
                            particleSecondary, ParticleFactory.DURATION_MEDIUM));
                    }
                }
            }
        }

        public override SaveState GenerateSave()
        {
            SaveState save =  base.GenerateSave();
            save.AddData("entitytype", type.ToString());
            return save;
        }

        public override void Update(float deltaTime, Area area)
        {
            healthBar.Update(deltaTime);

            if (shakeTimeLeft > 0)
            {
                shakeTimer += deltaTime;
                if (shakeTimer >= SHAKE_INTERVAL)
                {
                    switch (shakeModX)
                    {
                        case 0:
                            shakeModX = SHAKE_AMOUNT;
                            break;
                        case SHAKE_AMOUNT:
                            shakeModX = -SHAKE_AMOUNT;
                            break;
                        case -SHAKE_AMOUNT:
                            shakeModX = 0;
                            break;
                    }
                    shakeTimer = 0;
                }
                shakeTimeLeft -= deltaTime;
            }
        }
    }
}
