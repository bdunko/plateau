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
    public class TEntityBush : TileEntity, IInteract, IInteractTool, ITickDaily
    {
        private AnimatedSprite sprite;
        private LootTables.LootTable lootTable;
        private EntityType type;
        private LootTables.LootTable fruitTable;
        private bool fruitAvailable;
        private HealthBar hb;

        private static float SHAKE_DURATION = 0.19f;
        private static float SHAKE_INTERVAL = 0.06f;
        private const float SHAKE_AMOUNT = 0.75f;
        private float shakeTimeLeft;
        private float shakeTimer;
        private float shakeModX;

        public TEntityBush(AnimatedSprite sprite, Vector2 tilePosition, HealthBar hb, EntityType type, LootTables.LootTable lootTable)
        {
            this.sprite = sprite;
            this.position = new Vector2(tilePosition.X * 8, tilePosition.Y * 8);
            this.position.Y += 1;
            this.tilePosition = tilePosition;
            this.tileHeight = sprite.GetFrameHeight() / 8;
            this.tileWidth = sprite.GetFrameWidth() / 8;
            this.type = type;
            this.lootTable = lootTable;
            this.hb = hb;
            this.fruitAvailable = false;
            this.shakeTimer = 0;
            this.shakeModX = 0;
            this.shakeTimeLeft = 0;
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            sprite.Draw(sb, position + new Vector2(shakeModX, 0), Color.White, layerDepth);
            hb.Draw(sb, this.position + new Vector2((sprite.GetFrameWidth() / 2) - (hb.GetWidth() / 2), -8), 1.0f);
        }

        public override RectangleF GetCollisionRectangle()
        {
            return new RectangleF(position, new Size2(sprite.GetFrameWidth(), sprite.GetFrameHeight()));
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
            if (fruitAvailable)
            {
                return "Gather";
            }
            return "";
        }

        public string GetRightShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public void InteractLeft(EntityPlayer player, Area area, World world)
        {
            //do nothing
        }

        public void InteractLeftShift(EntityPlayer player, Area area, World world)
        {
            //do nothing
        }

        bool harvested = false;

        public void InteractRight(EntityPlayer player, Area area, World world)
        {
            if (fruitAvailable)
            {
                shakeTimeLeft = SHAKE_DURATION;
                List<Item> drops = fruitTable.RollLoot(player, area, world.GetTimeData());
                foreach (Item drop in drops)
                {
                    area.AddEntity(new EntityItem(drop, new Vector2(position.X, position.Y - 4)));
                }
                Color leaf1 = Color.White, leaf2 = Color.White;
                switch (area.GetSeason())
                {
                    case World.Season.SPRING:
                        leaf1 = Util.PARTICLE_GRASS_SPRING_PRIMARY.color;
                        leaf2 = Util.PARTICLE_GRASS_SPRING_SECONDARY.color;
                        break;
                    case World.Season.SUMMER:
                        leaf1 = Util.PARTICLE_GRASS_SUMMER_PRIMARY.color;
                        leaf2 = Util.PARTICLE_GRASS_SUMMER_SECONDARY.color;
                        break;
                    case World.Season.AUTUMN:
                        leaf1 = Util.PARTICLE_GRASS_FALL_PRIMARY.color;
                        leaf2 = Util.PARTICLE_GRASS_FALL_SECONDARY.color;
                        break;
                    case World.Season.WINTER:
                        leaf1 = Util.PARTICLE_GRASS_WINTER_PRIMARY.color;
                        leaf2 = Util.PARTICLE_GRASS_WINTER_SECONDARY.color;
                        break;
                }
                for (int i = 0; i < 4; i++)
                {
                    area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(sprite.GetFrameWidth() / 2, sprite.GetFrameHeight() / 2), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                        leaf1, ParticleFactory.DURATION_MEDIUM));
                    area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(0, 1) + new Vector2(sprite.GetFrameWidth() / 2 + Util.RandInt(-2, 2)),
                        ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                        leaf2, ParticleFactory.DURATION_MEDIUM));
                }
                fruitAvailable = false;
                UpdateSprite(area.GetSeason());
            }
        }

        public override SaveState GenerateSave()
        {
            SaveState save = base.GenerateSave();
            save.AddData("entitytype", type.ToString());
            save.AddData("sprite", sprite.GetCurrentLoop());
            save.AddData("fruitAvailable", fruitAvailable.ToString());
            return save;
        }

        public override void LoadSave(SaveState state)
        {
            base.LoadSave(state);
            sprite.SetLoop(state.TryGetData("sprite", "spring"));
            fruitAvailable = state.TryGetData("fruitAvailable", false.ToString()) == true.ToString();
            UpdateFruitTable();
        }

        public void InteractRightShift(EntityPlayer player, Area area, World world)
        {
            //do nothing
        }

        public override void Update(float deltaTime, Area area)
        {
            if(shakeTimeLeft > 0)
            {
                shakeTimer += deltaTime;
                if(shakeTimer >= SHAKE_INTERVAL)
                {
                    switch(shakeModX)
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

            sprite.Update(deltaTime);
            hb.Update(deltaTime);
        }

        public void InteractTool(EntityPlayer player, Area area, World world)
        {
            Item tool = player.GetHeldItem().GetItem();
            if (tool.HasTag(Item.Tag.AXE))
            {
                Color leaf1 = Color.White, leaf2 = Color.White;
                switch (area.GetSeason())
                {
                    case World.Season.SPRING:
                        leaf1 = Util.PARTICLE_GRASS_SPRING_PRIMARY.color;
                        leaf2 = Util.PARTICLE_GRASS_SPRING_SECONDARY.color;
                        break;
                    case World.Season.SUMMER:
                        leaf1 = Util.PARTICLE_GRASS_SUMMER_PRIMARY.color;
                        leaf2 = Util.PARTICLE_GRASS_SUMMER_SECONDARY.color;
                        break;
                    case World.Season.AUTUMN:
                        leaf1 = Util.PARTICLE_GRASS_FALL_PRIMARY.color;
                        leaf2 = Util.PARTICLE_GRASS_FALL_SECONDARY.color;
                        break;
                    case World.Season.WINTER:
                        leaf1 = Util.PARTICLE_GRASS_WINTER_PRIMARY.color;
                        leaf2 = Util.PARTICLE_GRASS_WINTER_SECONDARY.color;
                        break;
                }

                hb.Damage(((DamageDealingItem)tool).GetDamage());
                shakeTimeLeft = SHAKE_DURATION;
                shakeTimer = 0;
                for (int i = 0; i < 7; i++)
                {
                    area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(sprite.GetFrameWidth() / 2, sprite.GetFrameHeight() / 2), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                        leaf1, ParticleFactory.DURATION_MEDIUM));
                    area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(0, 1) + new Vector2(sprite.GetFrameWidth() / 2 + Util.RandInt(-2, 2)),
                        ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                        leaf2, ParticleFactory.DURATION_MEDIUM));
                }

                if (hb.IsDepleted())
                {
                    area.RemoveTileEntity(player, (int)tilePosition.X, (int)tilePosition.Y, world);
                    List<Item> drops = lootTable.RollLoot(player, area, world.GetTimeData());
                    foreach (Item drop in drops)
                    {
                        area.AddEntity(new EntityItem(drop, new Vector2(position.X, position.Y - 4)));
                    }
                    if (fruitAvailable)
                    {
                        this.InteractRight(player, area, world);
                    }

                    for (int i = 0; i < 17; i++)
                    {
                        area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(sprite.GetFrameWidth() / 2, sprite.GetFrameHeight() / 2), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                            leaf1, ParticleFactory.DURATION_MEDIUM));
                        area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(0, 1) + new Vector2(sprite.GetFrameWidth() / 2 + Util.RandInt(-2, 2)),
                            ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                            leaf2, ParticleFactory.DURATION_MEDIUM));
                    }
                }
            }
        }

        private void UpdateSprite(World.Season season)
        {
            string loopNameBase = "";
            switch (season)
            {
                case World.Season.SPRING:
                    loopNameBase = "spring";
                    break;
                case World.Season.SUMMER:
                    loopNameBase = "summer";
                    break;
                case World.Season.AUTUMN:
                    loopNameBase = "fall";
                    break;
                case World.Season.WINTER:
                    loopNameBase = "winter";
                    break;
            }
            string fruitExtension = "";
            if (fruitAvailable)
            {
                fruitExtension += "fruit";
                if (season == World.Season.AUTUMN)
                {
                    if (Util.RandInt(0, 10) >= 6)
                    {
                        fruitExtension += "1";
                    } else
                    {
                        fruitExtension += "2";
                    }
                }
            }

            sprite.SetLoop(loopNameBase + fruitExtension);
        }

        private void UpdateFruitTable()
        {
            if(fruitAvailable)
            {
                switch (sprite.GetCurrentLoop())
                {
                    case "springfruit":
                        fruitTable = LootTables.RASPBERRY_BUSH;
                        break;
                    case "summerfruit":
                        fruitTable = LootTables.BLUEBERRY_BUSH;
                        break;
                    case "fallfruit1":
                        fruitTable = LootTables.BLACKBERRY_BUSH;
                        break;
                    case "fallfruit2":
                        fruitTable = LootTables.ELDERBERRY_BUSH;
                        break;
                    default:
                        fruitTable = null;
                        break;
                }
            } else
            {
                fruitTable = null;
            }
        }

        public void TickDaily(World world, Area area, EntityPlayer player)
        {
            if (area.GetSeason() != World.Season.WINTER && world.GetDay() >= 4)
            {
                fruitAvailable = true;
            }
            else
            {
                fruitAvailable = false;
            }
            UpdateSprite(area.GetSeason());
            UpdateFruitTable();
        }
    }
}
