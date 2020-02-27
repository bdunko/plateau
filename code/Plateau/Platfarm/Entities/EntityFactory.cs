using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platfarm.Components;
using Platfarm.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Entities
{
    public enum EntityType
    {
        //add 8f
        PIG, COW, SHEEP, CHICKEN,
        CHEST, COMPOST_BIN, DAIRY_CHURN, MAYONNAISE_MAKER, LOOM, CHEFS_TABLE, CLAY_OVEN, PERFUMERY,
        BEEHIVE, BIRDHOUSE, SEED_MAKER, POTTERY_WHEEL, PAINTERS_PRESS, FURNACE, GEMSTONE_REPLICATOR, COMPRESSOR, MUSH_BOX, SOULCHEST,
        FLOWERBED, GLASSBLOWER, AQUARIUM, ALCHEMY_CAULDRON, ANVIL, KEG, SKY_STATUE, DRACONIC_PILLAR, WORKBENCH, VIVARIUM, SYNTHESIZER, JEWELERS_BENCH,
        LIGHT_DECOR, LIGHT_DECOR_ANIM, WALLPAPER, FLOOR_DECOR, WALL_DECOR, DECOR, ANIMATED_DECOR_4F, ANIMATED_DECOR_6F, ANIMATED_DECOR_8F, FENCE,
        TOTEM_COW, TOTEM_CHICKEN, TOTEM_PIG, TOTEM_SHEEP,
        ROCK, ROCK_LARGE, BRANCH, BRANCH_LARGE, WEEDS, GRASS, BUSH,
        PINE_TREE, APPLE_TREE, ORANGE_TREE, CHERRY_TREE, LEMON_TREE, OLIVE_TREE, COCONUT_PALM, BANANA_PALM,
        WILD_APPLE_TREE, WILD_ORANGE_TREE, WILD_CHERRY_TREE, WILD_LEMON_TREE, WILD_OLIVE_TREE, WILD_COCONUT_PALM, WILD_BANANA_PALM,
        BLUEBELL, NETTLES, CHICKWEED, SUNFLOWER, MOREL, MOUNTAIN_WHEAT, SPICY_LEAF,
        MARIGOLD, LAVENDER, VANILLA_BEAN, CACOA, MAIZE, PINEAPPLE,
        FALL_LEAF_PILE, CAVE_SOYBEAN, EMERALD_MOSS, CAVE_FUNGI,
        WINTER_SNOW_PILE, SHIITAKE, SKY_ROSE,
        BEACH_FORAGE,
        SPRING_SHRINE,
        FARMABLE, USE_ITEM, NONE
    }

    public class EntityFactory
    {
        public static EntityType StringToEntityType(string str)
        {
            foreach(EntityType type in Enum.GetValues(typeof(EntityType)))
            {
                if(type.ToString() == str)
                {
                    return type;
                }
            }
            return EntityType.NONE;
        }

        public static Entity GetEntity(EntityType type, Item item, Vector2 tilePlacement, Area area)
        {
            if (type != EntityType.USE_ITEM)
            {
                if(type == EntityType.COW)
                {
                    float[] frameLengths = Util.CreateAndFillArray(10, 0.25f);
                    AnimatedSprite sprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_COW), 10, 1, 10, frameLengths);
                    sprite.AddLoop("idleL", 0, 0, true);
                    sprite.AddLoop("walkL", 1, 4, true);
                    sprite.AddLoop("idleR", 5, 5, true);
                    sprite.AddLoop("walkR", 6, 9, true);
                    return new EntityAnimal(sprite, tilePlacement*8, LootTables.COW, ItemDict.MILKING_PAIL, EntityAnimal.Type.COW);
                } else if (type == EntityType.CHICKEN)
                {
                    float[] frameLengths = Util.CreateAndFillArray(10, 0.25f);
                    AnimatedSprite sprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_CHICKEN), 10, 1, 10, frameLengths);
                    sprite.AddLoop("idleL", 0, 0, true);
                    sprite.AddLoop("walkL", 1, 4, true);
                    sprite.AddLoop("idleR", 5, 5, true);
                    sprite.AddLoop("walkR", 6, 9, true);
                    return new EntityAnimal(sprite, tilePlacement*8, LootTables.CHICKEN, ItemDict.BASKET, EntityAnimal.Type.CHICKEN);
                } else if (type == EntityType.PIG)
                {
                    float[] frameLengths = Util.CreateAndFillArray(10, 0.25f);
                    AnimatedSprite sprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_PIG), 10, 1, 10, frameLengths);
                    sprite.AddLoop("idleL", 0, 0, true);
                    sprite.AddLoop("walkL", 1, 4, true);
                    sprite.AddLoop("idleR", 5, 5, true);
                    sprite.AddLoop("walkR", 6, 9, true);
                    return new EntityAnimal(sprite, tilePlacement*8, LootTables.PIG, ItemDict.NONE, EntityAnimal.Type.PIG);
                } else if (type == EntityType.SHEEP)
                {
                    float[] frameLengths = Util.CreateAndFillArray(20, 0.25f);
                    AnimatedSprite sprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_SHEEP), 20, 2, 10, frameLengths);
                    sprite.AddLoop("idleL", 0, 0, true);
                    sprite.AddLoop("walkL", 1, 4, true);
                    sprite.AddLoop("idle2L", 5, 5, true);
                    sprite.AddLoop("walk2L", 6, 9, true);
                    sprite.AddLoop("idleR", 10, 10, true);
                    sprite.AddLoop("walkR", 11, 14, true);
                    sprite.AddLoop("idle2R", 15, 15, true);
                    sprite.AddLoop("walk2R", 16, 19, true);
                    return new EntityAnimal(sprite, tilePlacement*8, LootTables.SHEEP, ItemDict.SHEARS, EntityAnimal.Type.SHEEP);
                }
                else if (type == EntityType.FARMABLE)
                {
                    float[] frameLengths = Util.CreateAndFillArray(18, 100f);
                    AnimatedSprite sprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_FARMABLE), 18, 1, 18, frameLengths);
                    float[] frameLengths2 = Util.CreateAndFillArray(37, 100f);
                    AnimatedSprite cropSprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_CROPS), 36, 4, 10, frameLengths2);
                    return new TEntityFarmable(sprite, tilePlacement, cropSprite);
                }
                else if (type == EntityType.ROCK)
                {
                    HealthBar hb = new HealthBar(Util.RandInt(12, 20), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR_SEGMENT));
                    Texture2D tex;
                    switch (Util.RandInt(1, 5))
                    {
                        case 1:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_ROCK1);
                            break;
                        case 2:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_ROCK2);
                            break;
                        case 3:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_ROCK3);
                            break;
                        case 4:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_ROCK4);
                            break;
                        case 5:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_ROCK5);
                            break;
                        default:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_ROCK1);
                            break;
                    }
                    return new TEntityToolable(tex, tilePlacement, type, Item.Tag.PICKAXE, hb, LootTables.FARM_ROCK, Util.PARTICLE_STONE_PRIMARY.color, Util.PARTICLE_STONE_SECONDARY.color);
                }
                else if (type == EntityType.ROCK_LARGE)
                {
                    HealthBar hb = new HealthBar(Util.RandInt(30, 45), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR_SEGMENT));
                    Texture2D tex;
                    switch (Util.RandInt(1, 3))
                    {
                        case 1:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_ROCK6);
                            break;
                        case 2:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_ROCK7);
                            break;
                        case 3:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_ROCK8);
                            break;
                        default:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_ROCK1);
                            break;
                    }
                    return new TEntityToolable(tex, tilePlacement, type, Item.Tag.PICKAXE, hb, LootTables.FARM_BIG_ROCK, Util.PARTICLE_STONE_PRIMARY.color, Util.PARTICLE_STONE_SECONDARY.color);
                }
                else if (type == EntityType.WEEDS)
                {
                    Texture2D tex;
                    switch (Util.RandInt(1, 3))
                    {
                        case 1:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_WEEDS1);
                            break;
                        case 2:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_WEEDS2);
                            break;
                        case 3:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_WEEDS3);
                            break;
                        default:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HOME);
                            break;
                    }
                    return new TEntityForage(tex, tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, LootTables.WEED_FORAGE);
                }
                else if (type == EntityType.BRANCH_LARGE)
                {
                    HealthBar hb = new HealthBar(Util.RandInt(20, 30), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR_SEGMENT));
                    Texture2D tex;
                    switch (Util.RandInt(1, 2))
                    {
                        case 1:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_BRANCH3);
                            break;
                        case 2:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_BRANCH4);
                            break;
                        default:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HOME);
                            break;
                    }
                    return new TEntityToolable(tex, tilePlacement, type, Item.Tag.AXE, hb, LootTables.FARM_BIG_BRANCH, Util.PARTICLE_BRANCH_PRIMARY.color, Util.PARTICLE_BRANCH_PRIMARY.color);
                }
                else if (type == EntityType.BRANCH)
                {
                    HealthBar hb = new HealthBar(Util.RandInt(8, 15), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR_SEGMENT));
                    Texture2D tex;
                    switch (Util.RandInt(1, 2))
                    {
                        case 1:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_BRANCH1);
                            break;
                        case 2:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_BRANCH2);
                            break;
                        default:
                            tex = Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HOME);
                            break;
                    }
                    return new TEntityToolable(tex, tilePlacement, type, Item.Tag.AXE, hb, LootTables.FARM_BRANCH, Util.PARTICLE_BRANCH_PRIMARY.color, Util.PARTICLE_BRANCH_PRIMARY.color);
                }
                else if (type == EntityType.BEACH_FORAGE)
                {
                    return new TEntityForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_BEACH_FORAGE), tilePlacement, type, Util.SAND_PRIMARY.color, Util.SAND_SECONDARY.color, LootTables.BEACH_FORAGE);
                } else if (type == EntityType.BLUEBELL)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_BLUEBELL), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.SPRING, LootTables.BLUEBELL_FORAGE);
                } else if (type == EntityType.NETTLES)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_NETTLES), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.SPRING, LootTables.NETTLES_FORAGE);
                } else if (type == EntityType.CHICKWEED)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_CHICKWEED), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.SPRING, LootTables.CHICKWEED_FORAGE);
                } else if (type == EntityType.SUNFLOWER)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_SUNFLOWER), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.SPRING, LootTables.SUNFLOWER_FORAGE);
                }
                else if (type == EntityType.MOREL)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_MOREL), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.SPRING, LootTables.MOREL_FORAGE);
                }
                else if (type == EntityType.MOUNTAIN_WHEAT)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_MOUNTAIN_WHEAT), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.SPRING, LootTables.MOUNTAIN_WHEAT_FORAGE);
                }
                else if (type == EntityType.SPICY_LEAF)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_SPICY_LEAF), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.SPRING, LootTables.SPICY_LEAF_FORAGE);
                }
                else if (type == EntityType.MARIGOLD)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_MARIGOLD), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.SUMMER, LootTables.MARIGOLD_FORAGE);
                }
                else if (type == EntityType.LAVENDER)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_LAVENDER), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.SUMMER, LootTables.LAVENDER_FORAGE);
                }
                else if (type == EntityType.VANILLA_BEAN)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_VANILLA_BEAN), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.SUMMER, LootTables.VANILLA_BEAN_FORAGE);
                }
                else if (type == EntityType.CACOA)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_CACOA_BEAN), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.SUMMER, LootTables.COCOA_FORAGE);
                }
                else if (type == EntityType.MAIZE)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_MAIZE), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.SUMMER, LootTables.MAIZE_FORAGE);
                }
                else if (type == EntityType.PINEAPPLE)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_PINEAPPLE), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.SUMMER, LootTables.PINEAPPLE_FORAGE);
                }
                else if (type == EntityType.CAVE_FUNGI)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_CAVE_FUNGI), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.AUTUMN, LootTables.CAVE_FUNGI_FORAGE);
                }
                else if (type == EntityType.CAVE_SOYBEAN)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_CAVE_SOYBEAN), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.AUTUMN, LootTables.CAVE_SOYBEAN_FORAGE);
                }
                else if (type == EntityType.EMERALD_MOSS)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_EMERALD_MOSS), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.AUTUMN, LootTables.EMERALD_MOSS_FORAGE);
                }
                else if (type == EntityType.SHIITAKE)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_SHIITAKE), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.WINTER, LootTables.SHIITAKE_FORAGE);
                }
                else if (type == EntityType.SKY_ROSE)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_SKY_ROSE), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.WINTER, LootTables.SKY_ROSE_FORAGE);
                }
                else if (type == EntityType.FALL_LEAF_PILE)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_AUTUMN_LEAF_PILE), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.AUTUMN, LootTables.FALL_LEAF_PILE_FORAGE);
                }
                else if (type == EntityType.WINTER_SNOW_PILE)
                {
                    return new TEntitySeasonalForage(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_WINTER_SNOW_PILE), tilePlacement, type, Util.PARTICLE_WEEDS_PRIMARY.color, Util.PARTICLE_WEEDS_SECONDARY.color, World.Season.WINTER, LootTables.WINTER_SNOW_PILE_FORAGE);
                }
                else if (type == EntityType.GRASS)
                {
                    AnimatedSprite grassSprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_GRASS_SPRITESHEET), 16, 2, 10, Util.CreateAndFillArray(16, 1000f));
                    return new TEntityGrass(grassSprite, tilePlacement, type);
                } else if (type == EntityType.PINE_TREE)
                {
                    HealthBar hb = new HealthBar(Util.RandInt(65, 75), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR_SEGMENT));
                    AnimatedSprite pineSprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_PINE_TREE), 16, 2, 10, Util.CreateAndFillArray(16, 1000f));
                    pineSprite.AddLoop("spring1", 0, 0, false);
                    pineSprite.AddLoop("spring2", 1, 1, false);
                    pineSprite.AddLoop("spring3", 2, 2, false);
                    pineSprite.AddLoop("spring4", 3, 3, false);
                    pineSprite.AddLoop("summer1", 4, 4, false);
                    pineSprite.AddLoop("summer2", 5, 5, false);
                    pineSprite.AddLoop("summer3", 6, 6, false);
                    pineSprite.AddLoop("summer4", 7, 7, false);
                    pineSprite.AddLoop("fall1", 8, 8, false);
                    pineSprite.AddLoop("fall2", 9, 9, false);
                    pineSprite.AddLoop("fall3", 10, 10, false);
                    pineSprite.AddLoop("fall4", 11, 11, false);
                    pineSprite.AddLoop("winter1", 12, 12, false);
                    pineSprite.AddLoop("winter2", 13, 13, false);
                    pineSprite.AddLoop("winter3", 14, 14, false);
                    pineSprite.AddLoop("winter4", 15, 15, false);
                    return new TEntityTree(pineSprite, hb, tilePlacement, null, null, World.Season.NONE, LootTables.TREE_PINE, 16, 9, 2, type, area.GetSeason(), 14, 29, 44, 71, "Pine Tree", true);
                }
                else if (type == EntityType.APPLE_TREE || type == EntityType.WILD_APPLE_TREE)
                {
                    HealthBar hb = new HealthBar(Util.RandInt(95, 105), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR_SEGMENT));
                    AnimatedSprite appleSprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_FRUIT_TREE), 23, 3, 10, Util.CreateAndFillArray(23, 1000f));
                    appleSprite.AddLoop("spring1", 0, 0, false);
                    appleSprite.AddLoop("spring2", 1, 1, false);
                    appleSprite.AddLoop("spring3", 2, 2, false);
                    appleSprite.AddLoop("spring4", 3, 3, false);
                    appleSprite.AddLoop("summer1", 4, 4, false);
                    appleSprite.AddLoop("summer2", 5, 5, false);
                    appleSprite.AddLoop("summer3", 6, 6, false);
                    appleSprite.AddLoop("summer4", 7, 7, false);
                    appleSprite.AddLoop("fall1", 8, 8, false);
                    appleSprite.AddLoop("fall2", 9, 9, false);
                    appleSprite.AddLoop("fall3", 10, 10, false);
                    appleSprite.AddLoop("fall4", 11, 11, false);
                    appleSprite.AddLoop("winter1", 12, 12, false);
                    appleSprite.AddLoop("winter2", 13, 13, false);
                    appleSprite.AddLoop("winter3", 14, 14, false);
                    appleSprite.AddLoop("winter4", 15, 15, false);
                    appleSprite.AddLoop("fruit", 22, 22, false);
                    return new TEntityTree(appleSprite, hb, tilePlacement, LootTables.APPLE, LootTables.WILD_APPLE, World.Season.AUTUMN, LootTables.TREE_FRUIT, 8, 9, 2, type, area.GetSeason(), 16, 29, 43, 65, "Apple Tree", type == EntityType.WILD_APPLE_TREE);
                }
                else if (type == EntityType.CHERRY_TREE || type == EntityType.WILD_CHERRY_TREE)
                {
                    HealthBar hb = new HealthBar(Util.RandInt(65, 80), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR_SEGMENT));
                    AnimatedSprite cherrySprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_FRUIT_TREE), 23, 3, 10, Util.CreateAndFillArray(23, 1000f));
                    cherrySprite.AddLoop("spring1", 16, 16, false);
                    cherrySprite.AddLoop("spring2", 17, 17, false);
                    cherrySprite.AddLoop("spring3", 18, 18, false);
                    cherrySprite.AddLoop("spring4", 19, 19, false);
                    cherrySprite.AddLoop("summer1", 4, 4, false);
                    cherrySprite.AddLoop("summer2", 5, 5, false);
                    cherrySprite.AddLoop("summer3", 6, 6, false);
                    cherrySprite.AddLoop("summer4", 7, 7, false);
                    cherrySprite.AddLoop("fall1", 8, 8, false);
                    cherrySprite.AddLoop("fall2", 9, 9, false);
                    cherrySprite.AddLoop("fall3", 10, 10, false);
                    cherrySprite.AddLoop("fall4", 11, 11, false);
                    cherrySprite.AddLoop("winter1", 12, 12, false);
                    cherrySprite.AddLoop("winter2", 13, 13, false);
                    cherrySprite.AddLoop("winter3", 14, 14, false);
                    cherrySprite.AddLoop("winter4", 15, 15, false);
                    cherrySprite.AddLoop("fruit", 20, 20, false);
                    return new TEntityTree(cherrySprite, hb, tilePlacement, LootTables.CHERRY, LootTables.WILD_CHERRY, World.Season.SPRING, LootTables.TREE_FRUIT, 8, 9, 2, type, area.GetSeason(), 16, 29, 43, 65, "Cherry Tree", type == EntityType.WILD_CHERRY_TREE);
                }
                else if (type == EntityType.ORANGE_TREE || type == EntityType.WILD_ORANGE_TREE)
                {
                    HealthBar hb = new HealthBar(Util.RandInt(90, 100), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR_SEGMENT));
                    AnimatedSprite orangeSprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_FRUIT_TREE), 23, 3, 10, Util.CreateAndFillArray(23, 1000f));
                    orangeSprite.AddLoop("spring1", 0, 0, false);
                    orangeSprite.AddLoop("spring2", 1, 1, false);
                    orangeSprite.AddLoop("spring3", 2, 2, false);
                    orangeSprite.AddLoop("spring4", 3, 3, false);
                    orangeSprite.AddLoop("summer1", 4, 4, false);
                    orangeSprite.AddLoop("summer2", 5, 5, false);
                    orangeSprite.AddLoop("summer3", 6, 6, false);
                    orangeSprite.AddLoop("summer4", 7, 7, false);
                    orangeSprite.AddLoop("fall1", 8, 8, false);
                    orangeSprite.AddLoop("fall2", 9, 9, false);
                    orangeSprite.AddLoop("fall3", 10, 10, false);
                    orangeSprite.AddLoop("fall4", 11, 11, false);
                    orangeSprite.AddLoop("winter1", 12, 12, false);
                    orangeSprite.AddLoop("winter2", 13, 13, false);
                    orangeSprite.AddLoop("winter3", 14, 14, false);
                    orangeSprite.AddLoop("winter4", 15, 15, false);
                    orangeSprite.AddLoop("fruit", 21, 21, false);
                    return new TEntityTree(orangeSprite, hb, tilePlacement, LootTables.ORANGE, LootTables.WILD_ORANGE, World.Season.SUMMER, LootTables.TREE_FRUIT, 8, 9, 2, type, area.GetSeason(), 16, 29, 43, 65, "Orange Tree", type == EntityType.WILD_ORANGE_TREE);
                } else if (type == EntityType.OLIVE_TREE || type == EntityType.WILD_OLIVE_TREE)
                {
                    HealthBar hb = new HealthBar(Util.RandInt(40, 55), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR_SEGMENT));
                    AnimatedSprite oliveSprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_THIN_TREE), 18, 2, 10, Util.CreateAndFillArray(18, 1000f));
                    oliveSprite.AddLoop("spring1", 0, 0, false);
                    oliveSprite.AddLoop("spring2", 1, 1, false);
                    oliveSprite.AddLoop("spring3", 2, 2, false);
                    oliveSprite.AddLoop("spring4", 3, 3, false);
                    oliveSprite.AddLoop("summer1", 4, 4, false);
                    oliveSprite.AddLoop("summer2", 5, 5, false);
                    oliveSprite.AddLoop("summer3", 6, 6, false);
                    oliveSprite.AddLoop("summer4", 7, 7, false);
                    oliveSprite.AddLoop("fall1", 8, 8, false);
                    oliveSprite.AddLoop("fall2", 9, 9, false);
                    oliveSprite.AddLoop("fall3", 10, 10, false);
                    oliveSprite.AddLoop("fall4", 11, 11, false);
                    oliveSprite.AddLoop("winter1", 12, 12, false);
                    oliveSprite.AddLoop("winter2", 13, 13, false);
                    oliveSprite.AddLoop("winter3", 14, 14, false);
                    oliveSprite.AddLoop("winter4", 15, 15, false);
                    oliveSprite.AddLoop("fruit", 16, 16, false);
                    return new TEntityTree(oliveSprite, hb, tilePlacement, LootTables.OLIVE, LootTables.WILD_OLIVE, World.Season.SPRING, LootTables.TREE_THIN, 0, 5, 2, type, area.GetSeason(), 10, 15, 23, 34, "Olive Tree", type == EntityType.WILD_OLIVE_TREE);
                }
                else if (type == EntityType.LEMON_TREE || type == EntityType.WILD_LEMON_TREE)
                {
                    HealthBar hb = new HealthBar(Util.RandInt(46, 66), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR_SEGMENT));
                    AnimatedSprite lemonSprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_THIN_TREE), 18, 2, 10, Util.CreateAndFillArray(18, 1000f));
                    lemonSprite.AddLoop("spring1", 0, 0, false);
                    lemonSprite.AddLoop("spring2", 1, 1, false);
                    lemonSprite.AddLoop("spring3", 2, 2, false);
                    lemonSprite.AddLoop("spring4", 3, 3, false);
                    lemonSprite.AddLoop("summer1", 4, 4, false);
                    lemonSprite.AddLoop("summer2", 5, 5, false);
                    lemonSprite.AddLoop("summer3", 6, 6, false);
                    lemonSprite.AddLoop("summer4", 7, 7, false);
                    lemonSprite.AddLoop("fall1", 8, 8, false);
                    lemonSprite.AddLoop("fall2", 9, 9, false);
                    lemonSprite.AddLoop("fall3", 10, 10, false);
                    lemonSprite.AddLoop("fall4", 11, 11, false);
                    lemonSprite.AddLoop("winter1", 12, 12, false);
                    lemonSprite.AddLoop("winter2", 13, 13, false);
                    lemonSprite.AddLoop("winter3", 14, 14, false);
                    lemonSprite.AddLoop("winter4", 15, 15, false);
                    lemonSprite.AddLoop("fruit", 17, 17, false);
                    return new TEntityTree(lemonSprite, hb, tilePlacement, LootTables.LEMON, LootTables.WILD_LEMON, World.Season.AUTUMN, LootTables.TREE_THIN, 0, 5, 2, type, area.GetSeason(), 10, 15, 23, 34, "Lemon Tree", type == EntityType.WILD_LEMON_TREE);
                }
                else if (type == EntityType.BANANA_PALM || type == EntityType.WILD_BANANA_PALM)
                {
                    HealthBar hb = new HealthBar(Util.RandInt(85, 95), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR_SEGMENT));
                    AnimatedSprite bananaSprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_PALM_TREE), 6, 1, 6, Util.CreateAndFillArray(6, 1000f));
                    bananaSprite.AddLoop("spring1", 0, 0, false);
                    bananaSprite.AddLoop("spring2", 1, 1, false);
                    bananaSprite.AddLoop("spring3", 2, 2, false);
                    bananaSprite.AddLoop("spring4", 3, 3, false);
                    bananaSprite.AddLoop("summer1", 0, 0, false);
                    bananaSprite.AddLoop("summer2", 1, 1, false);
                    bananaSprite.AddLoop("summer3", 2, 2, false);
                    bananaSprite.AddLoop("summer4", 3, 3, false);
                    bananaSprite.AddLoop("fall1", 0, 0, false);
                    bananaSprite.AddLoop("fall2", 1, 1, false);
                    bananaSprite.AddLoop("fall3", 2, 2, false);
                    bananaSprite.AddLoop("fall4", 3, 3, false);
                    bananaSprite.AddLoop("winter1", 0, 0, false);
                    bananaSprite.AddLoop("winter2", 1, 1, false);
                    bananaSprite.AddLoop("winter3", 2, 2, false);
                    bananaSprite.AddLoop("winter4", 3, 3, false);
                    bananaSprite.AddLoop("fruit", 5, 5, false);
                    return new TEntityTree(bananaSprite, hb, tilePlacement, LootTables.BANANA, LootTables.WILD_BANANA, World.Season.SUMMER, LootTables.TREE_PALM, 16, 8, 2, type, area.GetSeason(), 20, 28, 43, 57, "Banana Palm", type == EntityType.WILD_BANANA_PALM);
                }
                else if (type == EntityType.COCONUT_PALM || type == EntityType.WILD_COCONUT_PALM)
                {
                    HealthBar hb = new HealthBar(Util.RandInt(80, 88), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR_SEGMENT));
                    AnimatedSprite coconutSprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_PALM_TREE), 6, 1, 6, Util.CreateAndFillArray(6, 1000f));
                    coconutSprite.AddLoop("spring1", 0, 0, false);
                    coconutSprite.AddLoop("spring2", 1, 1, false);
                    coconutSprite.AddLoop("spring3", 2, 2, false);
                    coconutSprite.AddLoop("spring4", 3, 3, false);
                    coconutSprite.AddLoop("summer1", 0, 0, false);
                    coconutSprite.AddLoop("summer2", 1, 1, false);
                    coconutSprite.AddLoop("summer3", 2, 2, false);
                    coconutSprite.AddLoop("summer4", 3, 3, false);
                    coconutSprite.AddLoop("fall1", 0, 0, false);
                    coconutSprite.AddLoop("fall2", 1, 1, false);
                    coconutSprite.AddLoop("fall3", 2, 2, false);
                    coconutSprite.AddLoop("fall4", 3, 3, false);
                    coconutSprite.AddLoop("winter1", 0, 0, false);
                    coconutSprite.AddLoop("winter2", 1, 1, false);
                    coconutSprite.AddLoop("winter3", 2, 2, false);
                    coconutSprite.AddLoop("winter4", 3, 3, false);
                    coconutSprite.AddLoop("fruit", 4, 4, false);
                    return new TEntityTree(coconutSprite, hb, tilePlacement, LootTables.COCONUT, LootTables.WILD_COCONUT, World.Season.SUMMER, LootTables.TREE_PALM, 16, 8, 2, type, area.GetSeason(), 20, 28, 43, 57, "Coconut Palm", type == EntityType.WILD_COCONUT_PALM);
                } else if (type == EntityType.BUSH)
                {
                    HealthBar hb = new HealthBar(Util.RandInt(35, 45), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR), Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_HEALTH_BAR_SEGMENT));
                    AnimatedSprite bushSprite = new AnimatedSprite(Plateau.CONTENT.Load<Texture2D>(Paths.SPRITE_BUSH_SPRITESHEET), 8, 1, 8, Util.CreateAndFillArray(8, 1000f));
                    bushSprite.AddLoop("spring", 0, 0, false);
                    bushSprite.AddLoop("springfruit", 1, 1, false);
                    bushSprite.AddLoop("summer", 2, 2, false);
                    bushSprite.AddLoop("summerfruit", 3, 3, false);
                    bushSprite.AddLoop("fall", 4, 4, false);
                    bushSprite.AddLoop("fallfruit1", 5, 5, false);
                    bushSprite.AddLoop("fallfruit2", 6, 6, false);
                    bushSprite.AddLoop("winter", 7, 7, false);
                    return new TEntityBush(bushSprite, tilePlacement, hb, type, LootTables.BUSH);
                }
            }
            else if (item is PlaceableItem)
            {
                EntityType created = ((PlaceableItem)item).GetPlacedEntityType();
                if (created == EntityType.CHEST)
                {
                    float[] frameLengths = Util.CreateAndFillArray(7, 0.075f);
                    frameLengths[0] = 1.5f;
                    frameLengths[4] = 0.1f;
                    frameLengths[5] = 0.1f;
                    frameLengths[6] = 0.1f;
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 7, 1, 7, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 7, 1, 7, frameLengths);
                    return new PEntityChest(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.LIGHT_DECOR)
                {
                    float[] frameLengths = Util.CreateAndFillArray(4, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 4, 1, 4, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 4, 1, 4, frameLengths);
                    PartialRecolorSprite prs = new PartialRecolorSprite(recolor, nonrecolor);
                    prs.AddLoop("placement", 0, 3, false);
                    prs.AddLoop("anim", 0, 0, true);
                    prs.SetLoop("placement");
                    return new PEntityLightSource(prs, tilePlacement, Area.LightSource.Strength.LARGE, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.LIGHT_DECOR_ANIM)
                {
                    float[] frameLengths = Util.CreateAndFillArray(8, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 8, 1, 8, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 8, 1, 8, frameLengths);
                    PartialRecolorSprite prs = new PartialRecolorSprite(recolor, nonrecolor);
                    prs.AddLoop("placement", 0, 3, false);
                    prs.AddLoop("anim", 4, 7, true);
                    prs.SetLoop("placement");
                    return new PEntityLightSource(prs, tilePlacement, Area.LightSource.Strength.MEDIUM, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.COMPOST_BIN)
                {
                    float[] frameLengths = Util.CreateAndFillArray(8, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 8, 1, 8, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 8, 1, 8, frameLengths);
                    return new PEntityCompostBin(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.DAIRY_CHURN)
                {
                    float[] frameLengths = Util.CreateAndFillArray(9, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 9, 1, 9, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 9, 1, 9, frameLengths);
                    return new PEntityDairyChurn(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.LOOM)
                {
                    float[] frameLengths = Util.CreateAndFillArray(8, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 8, 1, 8, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 8, 1, 8, frameLengths);
                    return new PEntityLoom(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.CHEFS_TABLE)
                {
                    float[] frameLengths = Util.CreateAndFillArray(7, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 7, 1, 7, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 7, 1, 7, frameLengths);
                    return new PEntityChefsTable(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.CLAY_OVEN)
                {
                    float[] frameLengths = Util.CreateAndFillArray(8, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 8, 1, 8, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 8, 1, 8, frameLengths);
                    return new PEntityClayOven(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.PERFUMERY)
                {
                    float[] frameLengths = Util.CreateAndFillArray(12, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 12, 1, 12, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 12, 1, 12, frameLengths);
                    return new PEntityPerfumery(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.BEEHIVE)
                {
                    float[] frameLengths = Util.CreateAndFillArray(10, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 10, 1, 10, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 10, 1, 10, frameLengths);
                    return new PEntityBeehive(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.BIRDHOUSE)
                {
                    float[] frameLengths = Util.CreateAndFillArray(4, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 4, 1, 4, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 4, 1, 4, frameLengths);
                    return new PEntityBirdhouse(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.SEED_MAKER)
                {
                    float[] frameLengths = Util.CreateAndFillArray(9, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 9, 1, 9, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 9, 1, 9, frameLengths);
                    return new PEntitySeedMaker(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.POTTERY_WHEEL)
                {
                    float[] frameLengths = Util.CreateAndFillArray(8, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 8, 1, 8, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 8, 1, 8, frameLengths);
                    return new PEntityPotteryWheel(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.PAINTERS_PRESS)
                {
                    float[] frameLengths = Util.CreateAndFillArray(8, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 8, 1, 8, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 8, 1, 8, frameLengths);
                    return new PEntityPaintersPress(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.FURNACE)
                {
                    float[] frameLengths = Util.CreateAndFillArray(8, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 8, 1, 8, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 8, 1, 8, frameLengths);
                    return new PEntityFurance(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.GEMSTONE_REPLICATOR)
                {
                    float[] frameLengths = Util.CreateAndFillArray(8, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 8, 1, 8, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 8, 1, 8, frameLengths);
                    return new PEntityGemstoneReplicator(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.COMPRESSOR)
                {
                    float[] frameLengths = Util.CreateAndFillArray(11, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 11, 1, 11, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 11, 1, 11, frameLengths);
                    return new PEntityCompressor(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.MUSH_BOX)
                {
                    float[] frameLengths = Util.CreateAndFillArray(14, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 14, 1, 14, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 14, 1, 14, frameLengths);
                    return new PEntityMushbox(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.SOULCHEST)
                {
                    float[] frameLengths = Util.CreateAndFillArray(15, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 15, 1, 15, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 15, 1, 15, frameLengths);
                    return new PEntitySoulchest(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.FLOWERBED)
                {
                    float[] frameLengths = Util.CreateAndFillArray(12, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 11, 2, 10, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 11, 2, 10, frameLengths);
                    return new PEntityFlowerbed (new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.GLASSBLOWER)
                {
                    float[] frameLengths = Util.CreateAndFillArray(10, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 10, 1, 10, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 10, 1, 10, frameLengths);
                    return new PEntityGlassblower(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.AQUARIUM)
                {
                    float[] frameLengths = Util.CreateAndFillArray(12, 0.1f);
                    for(int i = 4; i < 12; i++)
                    {
                        frameLengths[i] = 0.25f;
                    }
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 12, 1, 12, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 12, 1, 12, frameLengths);
                    return new PEntityAquarium(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.ALCHEMY_CAULDRON)
                {
                    float[] frameLengths = Util.CreateAndFillArray(12, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 11, 1, 11, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 11, 1, 11, frameLengths);
                    return new PEntityAlchemyCauldron(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.ANVIL)
                {
                    float[] frameLengths = Util.CreateAndFillArray(16, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 16, 1, 16, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 16, 1, 16, frameLengths);
                    return new PEntityAnvil(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.KEG)
                {
                    float[] frameLengths = Util.CreateAndFillArray(12, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 12, 1, 12, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 12, 1, 12, frameLengths);
                    return new PEntityPerfumery(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL); //EDIT
                }
                else if (created == EntityType.SKY_STATUE)
                {
                    float[] frameLengths = Util.CreateAndFillArray(12, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 12, 1, 12, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 12, 1, 12, frameLengths);
                    return new PEntityPerfumery(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL); //EDIT
                }
                else if (created == EntityType.DRACONIC_PILLAR)
                {
                    float[] frameLengths = Util.CreateAndFillArray(12, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 12, 1, 12, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 12, 1, 12, frameLengths);
                    return new PEntityPerfumery(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL); //EDIT
                }
                else if (created == EntityType.WORKBENCH)
                {
                    float[] frameLengths = Util.CreateAndFillArray(11, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 11, 2, 10, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 11, 2, 10, frameLengths);
                    return new PEntityWorkbench(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL); //EDIT
                }
                else if (created == EntityType.VIVARIUM)
                {
                    float[] frameLengths = Util.CreateAndFillArray(12, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 12, 1, 12, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 12, 1, 12, frameLengths);
                    return new PEntityPerfumery(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL); //EDIT
                }
                else if (created == EntityType.SYNTHESIZER)
                {
                    float[] frameLengths = Util.CreateAndFillArray(12, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 12, 1, 12, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 12, 1, 12, frameLengths);
                    return new PEntityPerfumery(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL); //EDIT
                }
                else if (created == EntityType.JEWELERS_BENCH)
                {
                    float[] frameLengths = Util.CreateAndFillArray(9, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 9, 1, 9, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 9, 1, 9, frameLengths);
                    return new PEntityJewelersBench(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.MAYONNAISE_MAKER)
                {
                    float[] frameLengths = Util.CreateAndFillArray(7, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 7, 1, 7, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 7, 1, 7, frameLengths);
                    return new PEntityMayonaiseMaker(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.WALLPAPER)
                {
                    float[] frameLengths = Util.CreateAndFillArray(1, 1000f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 1, 1, 1, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 1, 1, 1, frameLengths);
                    if (area.GetCollisionTypeAt((int)tilePlacement.X, (int)tilePlacement.Y - 1) == Area.CollisionTypeEnum.SOLID)
                    {
                        recolor = new AnimatedSprite(((WallpaperItem)item).GetPlacedTextureRecolorTop(), 1, 1, 1, frameLengths);
                        nonrecolor = new AnimatedSprite(((WallpaperItem)item).GetPlacedTextureBottom(), 1, 1, 1, frameLengths);
                    }
                    else if (area.GetCollisionTypeAt((int)tilePlacement.X, (int)tilePlacement.Y + 1) == Area.CollisionTypeEnum.SOLID)
                    {
                        recolor = new AnimatedSprite(((WallpaperItem)item).GetPlacedTextureRecolorBottom(), 1, 1, 1, frameLengths);
                        nonrecolor = new AnimatedSprite(((WallpaperItem)item).GetPlacedTextureBottom(), 1, 1, 1, frameLengths);
                    }

                    return new PEntityWallpaper(new PartialRecolorSprite(recolor, nonrecolor), tilePlacement, (PlaceableItem)item, DrawLayer.BACKGROUND_WALLPAPER);
                }
                else if (created == EntityType.ANIMATED_DECOR_4F)
                {
                    float[] frameLengths = Util.CreateAndFillArray(8, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 8, 1, 8, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 8, 1, 8, frameLengths);
                    PartialRecolorSprite prs = new PartialRecolorSprite(recolor, nonrecolor);
                    prs.AddLoop("anim", 4, 7, true);
                    prs.AddLoop("placement", 0, 3, false, false);
                    prs.SetLoop("placement");
                    return new PEntityDecoration(prs, tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.ANIMATED_DECOR_6F)
                {
                    float[] frameLengths = Util.CreateAndFillArray(10, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 10, 1, 10, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 10, 1, 10, frameLengths);
                    PartialRecolorSprite prs = new PartialRecolorSprite(recolor, nonrecolor);
                    prs.AddLoop("anim", 4, 9, true);
                    prs.AddLoop("placement", 0, 3, false, false);
                    prs.SetLoop("placement");
                    return new PEntityDecoration(prs, tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.ANIMATED_DECOR_8F)
                {
                    float[] frameLengths = Util.CreateAndFillArray(12, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 12, 1, 12, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 12, 1, 12, frameLengths);
                    PartialRecolorSprite prs = new PartialRecolorSprite(recolor, nonrecolor);
                    prs.AddLoop("anim", 4, 11, true);
                    prs.AddLoop("placement", 0, 3, false, false);
                    prs.SetLoop("placement");
                    return new PEntityDecoration(prs, tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.DECOR)
                {
                    float[] frameLengths = Util.CreateAndFillArray(4, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 4, 1, 4, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 4, 1, 4, frameLengths);
                    PartialRecolorSprite prs = new PartialRecolorSprite(recolor, nonrecolor);
                    prs.AddLoop("anim", 0, 0, false);
                    prs.AddLoop("placement", 0, 3, false, false);
                    prs.SetLoop("placement");
                    return new PEntityDecoration(prs, tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL);
                }
                else if (created == EntityType.FLOOR_DECOR)
                {
                    float[] frameLengths = Util.CreateAndFillArray(1, 1000f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 1, 1, 1, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 1, 1, 1, frameLengths);
                    PartialRecolorSprite prs = new PartialRecolorSprite(recolor, nonrecolor);
                    prs.AddLoop("anim", 0, 0, false);
                    prs.SetLoop("anim");
                    return new PEntityDecoration(prs, tilePlacement, (PlaceableItem)item, DrawLayer.FOREGROUND_CARPET);
                }
                else if (created == EntityType.WALL_DECOR)
                {
                    float[] frameLengths = Util.CreateAndFillArray(4, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 4, 1, 4, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 4, 1, 4, frameLengths);
                    PartialRecolorSprite prs = new PartialRecolorSprite(recolor, nonrecolor);
                    prs.AddLoop("anim", 0, 0, false);
                    prs.AddLoop("placement", 0, 3, false, false);
                    prs.SetLoop("placement");
                    return new PEntityDecoration(prs, tilePlacement, (PlaceableItem)item, DrawLayer.BACKGROUND_WALL);
                }
                else if (created == EntityType.FENCE)
                {
                    float[] frameLengths = Util.CreateAndFillArray(4, 1000f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 4, 1, 4, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 4, 1, 4, frameLengths);
                    PartialRecolorSprite prs = new PartialRecolorSprite(recolor, nonrecolor);
                    prs.AddLoop("middle", 0, 0, false);
                    prs.AddLoop("left", 1, 1, false);
                    prs.AddLoop("right", 2, 2, false);
                    prs.AddLoop("single", 3, 3, false);
                    prs.SetLoop("single");
                    return new PEntityFence(prs, tilePlacement, (PlaceableItem)item, DrawLayer.FOREGROUND);
                }
                else if (created == EntityType.TOTEM_CHICKEN)
                {
                    float[] frameLengths = Util.CreateAndFillArray(8, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 8, 1, 8, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 8, 1, 8, frameLengths);
                    PartialRecolorSprite prs = new PartialRecolorSprite(recolor, nonrecolor);
                    prs.AddLoop("idle", 0, 0, true);
                    prs.AddLoop("anim", 4, 7, false);
                    prs.AddLoop("placement", 0, 3, false, false);
                    prs.SetLoop("placement");
                    return new PEntityTotem(prs, tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL, EntityAnimal.Type.CHICKEN);
                }
                else if (created == EntityType.TOTEM_COW)
                {
                    float[] frameLengths = Util.CreateAndFillArray(8, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 8, 1, 8, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 8, 1, 8, frameLengths);
                    PartialRecolorSprite prs = new PartialRecolorSprite(recolor, nonrecolor);
                    prs.AddLoop("idle", 0, 0, true);
                    prs.AddLoop("anim", 4, 7, false);
                    prs.AddLoop("placement", 0, 3, false, false);
                    prs.SetLoop("placement");
                    return new PEntityTotem(prs, tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL, EntityAnimal.Type.COW);
                }
                else if (created == EntityType.TOTEM_SHEEP)
                {
                    float[] frameLengths = Util.CreateAndFillArray(8, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 8, 1, 8, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 8, 1, 8, frameLengths);
                    PartialRecolorSprite prs = new PartialRecolorSprite(recolor, nonrecolor);
                    prs.AddLoop("idle", 0, 0, true);
                    prs.AddLoop("anim", 4, 7, false);
                    prs.AddLoop("placement", 0, 3, false, false);
                    prs.SetLoop("placement");
                    return new PEntityTotem(prs, tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL, EntityAnimal.Type.SHEEP);
                }
                else if (created == EntityType.TOTEM_PIG)
                {
                    float[] frameLengths = Util.CreateAndFillArray(12, 0.1f);
                    AnimatedSprite recolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTextureRecolor(), 12, 1, 12, frameLengths);
                    AnimatedSprite nonrecolor = new AnimatedSprite(((PlaceableItem)item).GetPlacedTexture(), 12, 1, 12, frameLengths);
                    PartialRecolorSprite prs = new PartialRecolorSprite(recolor, nonrecolor);
                    prs.AddLoop("idle", 0, 0, true);
                    prs.AddLoop("anim", 4, 11, false);
                    prs.AddLoop("placement", 0, 3, false, false);
                    prs.SetLoop("placement");
                    return new PEntityTotem(prs, tilePlacement, (PlaceableItem)item, DrawLayer.NORMAL, EntityAnimal.Type.PIG);
                }
            }
            throw new Exception("No PlaceableItem found?");
        }
    }
}
