﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Components;
using Platfarm.Items;
using Platfarm.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Platfarm.Components.LootTables;

namespace Platfarm.Entities
{
    public class TEntityTree : TileEntity, IInteract, IInteractTool, ITickDaily
    {
        private static Dictionary<EntityType, int> GROWTH_DIFFICULTY_BY_TYPE;
        private HealthBar healthBar;
        private AnimatedSprite sprite;
        private EntityType type;
        private int growthStage, drawOffset;
        private LootTable fruitTable, wildFruitTable;
        private World.Season fruitSeason;
        private bool fruitAvailable;
        private LootTables.LootTable lootTable;
        private int stage1Height, stage2Height, stage3Height, stage4Height;
        private string name;
        private bool isWild;
        private int age;

        private static float SHAKE_DURATION = 0.19f;
        private static float SHAKE_INTERVAL = 0.06f;
        private const float SHAKE_AMOUNT = 0.5f;
        private float shakeTimeLeft;
        private float shakeTimer;
        private float shakeModX;

        public TEntityTree(AnimatedSprite sprite, HealthBar hb, Vector2 tilePosition, LootTables.LootTable fruitTable, LootTables.LootTable wildFruitTable, World.Season fruitSeason, LootTables.LootTable lootTable, int drawOffset, int tileHeight, int tileWidth, EntityType type, World.Season season,
            int stage1Height, int stage2Height, int stage3Height, int stage4Height, string name, bool isWild)
        {
            this.wildFruitTable = wildFruitTable;
            this.age = 0;
            this.isWild = isWild;
            this.drawOffset = drawOffset;
            this.healthBar = hb;
            this.sprite = sprite;
            this.type = type;
            this.position = new Vector2(tilePosition.X * 8, tilePosition.Y * 8);
            this.tilePosition = tilePosition;
            this.tileHeight = tileHeight;
            this.tileWidth = tileWidth;
            this.lootTable = lootTable;
            this.drawLayer = DrawLayer.NORMAL;
            this.growthStage = 1;
            this.fruitAvailable = false;
            this.fruitTable = fruitTable;
            this.fruitSeason = fruitSeason;
            this.stage1Height = stage1Height;
            this.stage2Height = stage2Height;
            this.stage3Height = stage3Height;
            this.stage4Height = stage4Height;
            this.name = name;
            this.shakeTimeLeft = 0;
            this.shakeTimer = 0;
            this.shakeModX = 0;
            UpdateSprite(season);

            if(GROWTH_DIFFICULTY_BY_TYPE == null)
            {
                GROWTH_DIFFICULTY_BY_TYPE = new Dictionary<EntityType, int>
                {
                    {EntityType.PINE_TREE, 3},
                    {EntityType.APPLE_TREE, 5},
                    {EntityType.CHERRY_TREE, 4},
                    {EntityType.OLIVE_TREE, 6},
                    {EntityType.LEMON_TREE, 8},
                    {EntityType.COCONUT_PALM, 3},
                    {EntityType.ORANGE_TREE, 5},
                    {EntityType.BANANA_PALM, 5},
                };
            }
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            sprite.Draw(sb, position + new Vector2(-drawOffset, 1) + new Vector2(shakeModX, 0), Color.White, layerDepth);
            healthBar.Draw(sb, this.position + new Vector2(-drawOffset, 1) + new Vector2((sprite.GetFrameWidth() / 2) - (healthBar.GetWidth() / 2), (sprite.GetFrameHeight() - GetStageHeight())-8), 1.0f);
        }

        public override RectangleF GetCollisionRectangle()
        {
            return new RectangleF(position + new Vector2(-drawOffset, 0), new Size2(16, 32));
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
            if(fruitAvailable)
            {
                return "Harvest";
            }
            return "";
        }

        public string GetRightShiftClickAction(EntityPlayer player)
        {
            return "";
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
                        if (fruitTable == LootTables.CHERRY)
                        {
                            leaf1 = Util.PARTICLE_CHERRY_SPRING_PRIMARY.color;
                            leaf2 = Util.PARTICLE_CHERRY_SPRING_SECONDARY.color;
                        }
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
                if (fruitTable == LootTables.COCONUT || fruitTable == LootTables.BANANA)
                {
                    leaf1 = Util.PARTICLE_GRASS_SUMMER_PRIMARY.color;
                    leaf2 = Util.PARTICLE_GRASS_SUMMER_SECONDARY.color;
                }

                int multiplier = 1;
                switch(growthStage)
                {
                    case 1:
                        multiplier = 5;
                        break;
                    case 2:
                        multiplier = 3;
                        break;
                    case 3:
                        multiplier = 2;
                        break;
                }

                healthBar.Damage(((DamageDealingItem)tool).GetDamage() * multiplier);
                shakeTimeLeft = SHAKE_DURATION;
                shakeTimer = 0;
                for (int i = 0; i < 12; i++)
                {
                    area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(-drawOffset, 1) + new Vector2(sprite.GetFrameWidth()/2 + Util.RandInt(-2, 2), sprite.GetFrameHeight() - (GetStageHeight() * 0.15f)), 
                        ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                        Util.PARTICLE_BRANCH_PRIMARY.color, ParticleFactory.DURATION_MEDIUM));
                    area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(-drawOffset, 1) + new Vector2(sprite.GetFrameWidth() / 2 + Util.RandInt(-2, 2), sprite.GetFrameHeight() - (GetStageHeight() * 0.15f)),
                        ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                        Util.PARTICLE_BRANCH_SECONDARY.color, ParticleFactory.DURATION_MEDIUM));
                }
                if (healthBar.IsDepleted())
                {
                    area.RemoveTileEntity(player, (int)tilePosition.X, (int)tilePosition.Y, world);
                    List<Item> drops = lootTable.RollLoot(player, area, world.GetTimeData());
                    int chanceOfLoss = 1;
                    switch (growthStage)
                    {
                        case 1:
                            chanceOfLoss = 8;
                            break;
                        case 2:
                            chanceOfLoss = 5;
                            break;
                        case 3:
                            chanceOfLoss = 2;
                            break;
                    }
                    foreach (Item drop in drops)
                    {
                        if(Util.RandInt(1, chanceOfLoss) == 1) { 
                            area.AddEntity(new EntityItem(drop, new Vector2(position.X, position.Y + (sprite.GetFrameHeight() - GetStageHeight()))));
                        }
                    }
                    if(fruitAvailable)
                    {
                        this.InteractRight(player, area, world);
                    }
                   
                    for(int pHeight = sprite.GetFrameHeight(); pHeight > sprite.GetFrameHeight() - GetStageHeight(); pHeight--)
                    {
                        area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(-drawOffset, 1) + new Vector2(sprite.GetFrameWidth() / 2 + Util.RandInt(-2, 2), pHeight),
                        ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                        Util.PARTICLE_BRANCH_PRIMARY.color, ParticleFactory.DURATION_MEDIUM));
                        area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(-drawOffset, 1) + new Vector2(sprite.GetFrameWidth() / 2 + Util.RandInt(-2, 2), pHeight),
                        ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.CHUNK,
                        Util.PARTICLE_BRANCH_SECONDARY.color, ParticleFactory.DURATION_MEDIUM));

                        if(pHeight <= 0.25 * GetStageHeight()) 
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(-drawOffset, 1) + new Vector2(sprite.GetFrameWidth() / 2 + Util.RandInt(-8, 8), pHeight),
                                ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                                leaf1, ParticleFactory.DURATION_MEDIUM));
                                area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(-drawOffset, 1) + new Vector2(sprite.GetFrameWidth() / 2 + Util.RandInt(-8, 8), pHeight),
                                ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                                leaf2, ParticleFactory.DURATION_MEDIUM));
                            }
                        }
                    }
                }
            }
        }

        public int GetStageHeight()
        {
            switch(growthStage)
            {
                case 1:
                    return stage1Height;
                case 2:
                    return stage2Height;
                case 3:
                    return stage3Height;
                default:
                    return stage4Height;
            }
        }

        public override SaveState GenerateSave()
        {
            SaveState save = base.GenerateSave();
            save.AddData("entitytype", type.ToString());
            save.AddData("sprite", sprite.GetCurrentLoop());
            save.AddData("growth", growthStage.ToString());
            save.AddData("fruitAvailable", fruitAvailable.ToString());
            save.AddData("isWild", isWild.ToString());
            save.AddData("age", age.ToString());
            return save;
        }

        public override void LoadSave(SaveState state)
        {
            base.LoadSave(state);
            sprite.SetLoop(state.TryGetData("sprite", "spring"));
            growthStage = Int32.Parse(state.TryGetData("growth", "1"));
            age = Int32.Parse(state.TryGetData("age", "0"));
            fruitAvailable = state.TryGetData("fruitAvailable", false.ToString()) == true.ToString();
            isWild = state.TryGetData("isWild", false.ToString()) == true.ToString();
        }

        public override void Update(float deltaTime, Area area)
        {
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
            sprite.Update(deltaTime);
            healthBar.Update(deltaTime);
        }

        private void UpdateSprite(World.Season season)
        {
            if (fruitTable != null && fruitAvailable)
            {
                sprite.SetLoop("fruit");
            }
            else
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

                sprite.SetLoop(loopNameBase + growthStage);
            }
        }

        public void TickDaily(World world, Area area, EntityPlayer player)
        {
            age++;
            if(growthStage < 4 && Util.RandInt(0, GROWTH_DIFFICULTY_BY_TYPE[type]) == 0)
            {
                growthStage++;
            }
            if (area.GetSeason() == fruitSeason && growthStage >= 4)
            {
                fruitAvailable = true;
            } else
            {
                fruitAvailable = false;
            }
            UpdateSprite(area.GetSeason());
        }

        public void InteractRight(EntityPlayer player, Area area, World world)
        {
            if(fruitAvailable)
            {
                LootTable toUse = isWild ? wildFruitTable : fruitTable;
                List<Item> drops = toUse.RollLoot(player, area, world.GetTimeData());
                foreach (Item drop in drops)
                {
                    Item toDrop = drop;
                    if (toDrop.HasTag(Item.Tag.CROP))
                    {
                        int silverDifficulty = 100, goldDifficulty = 100;
                        if (!isWild)
                        {
                            if (age >= 100)
                            {
                                silverDifficulty = 1;
                                goldDifficulty = 2;
                            }
                            else if (age >= 56)
                            {
                                silverDifficulty = 3;
                                goldDifficulty = 5;
                            }
                            else if (age >= 28)
                            {
                                silverDifficulty = 8;
                                goldDifficulty = 10;
                            }
                            else if (age >= 14)
                            {
                                silverDifficulty = 15;
                                goldDifficulty = 30;
                            }
                            if (Util.RandInt(1, goldDifficulty) == 1)
                            {
                                toDrop = ItemDict.GetItemByName("Golden " + toDrop.GetName());
                            }
                            else if (Util.RandInt(1, silverDifficulty) == 1)
                            {
                                toDrop = ItemDict.GetItemByName("Silver " + toDrop.GetName());
                            }
                        }
                    }
                    area.AddEntity(new EntityItem(toDrop, new Vector2(position.X, position.Y + 16)));
                }

                fruitAvailable = false;
                UpdateSprite(area.GetSeason());
            }
        }

        public void InteractLeft(EntityPlayer player, Area area, World world)
        {
            //nothing
        }

        public void InteractRightShift(EntityPlayer player, Area area, World world)
        {
            //nothing
        }

        public void InteractLeftShift(EntityPlayer player, Area area, World world)
        {
            //nothing
        }
    }
}
