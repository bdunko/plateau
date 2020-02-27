using Platfarm.Entities;
using Platfarm.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Components
{
    public class LootTables
    {
        public class LootTable {
            protected List<Item> table;
            protected int minRolls, maxRolls;

            public LootTable(int minRolls, int maxRolls, params ItemStack[] entries)
            {
                this.minRolls = minRolls;
                this.maxRolls = maxRolls;
                table = new List<Item>();
                foreach (ItemStack entry in entries)
                {
                    Item item = entry.GetItem();
                    int quantity = entry.GetQuantity();
                    for (int i = 0; i < quantity; i++)
                    {
                        table.Add(item);
                    }
                }
            }

            public void AddEntry(ItemStack entry)
            {
                for (int i = 0; i < entry.GetQuantity(); i++)
                {
                    table.Add(entry.GetItem());
                }
            }

            public virtual List<Item> RollLoot(EntityPlayer player, Area area, World.TimeData timeData)
            {
                List<Item> loot = new List<Item>();
                for (int i = 0; i < Util.RandInt(minRolls, maxRolls); i++)
                {
                    loot.Add(table[Util.RandInt(0, table.Count - 1)]);
                }
                return loot;
            }
        }

        private class ForageLootTable : LootTable {
            public enum ForageType {
                NORMAL, BEACH
            }

            private float insectAmount;
            public static float DEFAULT_INSECT_AMOUNT = 0.5f;
            private ForageType type;

            public ForageLootTable(int minRolls, int maxRolls, ForageType type, float insectAmount, params ItemStack[] entries) : base(minRolls, maxRolls, entries)
            {
                this.insectAmount = insectAmount;
                this.type = type;
            }

            public override List<Item> RollLoot(EntityPlayer player, Area area, World.TimeData timeData)
            {
                List<Item> loot = new List<Item>();

                int bonusRolls = 0;
                if(player.HasEffect(AppliedEffects.FORAGING_IV) ||
                    (player.HasEffect(AppliedEffects.FORAGING_IV_BEACH) && type == ForageType.BEACH) ||
                    (player.HasEffect(AppliedEffects.FORAGING_IV_SUMMER) && area.GetSeason() == World.Season.SUMMER) ||
                    (player.HasEffect(AppliedEffects.FORAGING_IV_WINTER) && area.GetSeason() == World.Season.WINTER))
                {
                    bonusRolls = 2;
                } else if (player.HasEffect(AppliedEffects.FORAGING_III) ||
                    (player.HasEffect(AppliedEffects.FORAGING_III_AUTUMN) && area.GetSeason() == World.Season.AUTUMN) ||
                    (player.HasEffect(AppliedEffects.FORAGING_III_SPRING) && area.GetSeason() == World.Season.SPRING))
                {
                    bonusRolls = Util.RandInt(1, 2);
                } else if (player.HasEffect(AppliedEffects.FORAGING_II) ||
                    (player.HasEffect(AppliedEffects.FORAGING_II_BEACH) && type == ForageType.BEACH) ||
                    (player.HasEffect(AppliedEffects.FORAGING_II_SPRING) && area.GetSeason() == World.Season.SPRING) ||
                    (player.HasEffect(AppliedEffects.FORAGING_II_SUMMER) && area.GetSeason() == World.Season.SUMMER) ||
                    (player.HasEffect(AppliedEffects.FORAGING_II_WINTER) && area.GetSeason() == World.Season.WINTER))
                {
                    bonusRolls = 1;
                } else if (player.HasEffect(AppliedEffects.FORAGING_I))
                {
                    bonusRolls = Util.RandInt(0, 1);
                }

                for (int i = 0; i < Util.RandInt(minRolls, maxRolls) + bonusRolls; i++)
                {
                    Item lootGiven = table[Util.RandInt(0, table.Count - 1)];
                    if(player.HasEffect(AppliedEffects.FORAGING_IV_MUSHROOMS) && lootGiven.HasTag(Item.Tag.MUSHROOM))
                    {
                        loot.Add(lootGiven);
                        loot.Add(lootGiven);
                    } else if (player.HasEffect(AppliedEffects.FORAGING_II_MUSHROOMS) && lootGiven.HasTag(Item.Tag.MUSHROOM))
                    {
                        loot.Add(lootGiven);
                    }
                    if (player.HasEffect(AppliedEffects.FORAGING_IV_FLOWERS) && lootGiven.HasTag(Item.Tag.FLOWER))
                    {
                        loot.Add(lootGiven);
                        loot.Add(lootGiven);
                    } else if (player.HasEffect(AppliedEffects.FORAGING_II_FLOWERS) && lootGiven.HasTag(Item.Tag.FLOWER))
                    {
                        loot.Add(lootGiven);
                    }

                    loot.Add(lootGiven);
                }

                if (area.GetSeason() == World.Season.WINTER) //snow
                {
                    for (int i = 0; i < Util.RandInt(1, 3); i++)
                    {
                        if (Util.RandInt(0, 99) == 99)
                        {
                            loot.Add(ItemDict.ICE_NINE);
                        }
                        else
                        {
                            loot.Add(ItemDict.SNOW_CRYSTAL);
                        }
                    }
                }

                if (area.GetSeason() != World.Season.WINTER && type != ForageType.BEACH) //50/50 to drop an insect
                {
                    //create pool
                    LootTable insectTable = LootTables.GenerateInsectTable(player, area, timeData);
                    float insectAmountLeft = insectAmount;
                    //insect roll
                    while (insectAmountLeft != 0)
                    {
                        if (insectAmountLeft >= 1)
                        {
                            insectAmountLeft--;
                            List<Item> insectLoot = insectTable.RollLoot(player, area, timeData);
                            foreach (Item item in insectLoot)
                            {
                                loot.Add(item);
                            }
                        }
                        else
                        {
                            if (insectAmountLeft >= Util.RandInt(1, 100) / 100.0f)
                            {
                                List<Item> insectLoot = insectTable.RollLoot(player, area, timeData);
                                foreach (Item item in insectLoot)
                                {
                                    loot.Add(item);
                                }
                            }
                            insectAmountLeft = 0;
                        }
                    }
                }

                return loot;
            }
        }

        private class TreeLootTable : LootTable
        {
            public TreeLootTable(int minRolls, int maxRolls, params ItemStack[] entries) : base(minRolls, maxRolls, entries)
            {
                //do nothing
            }

            public override List<Item> RollLoot(EntityPlayer player, Area area, World.TimeData timeData)
            {
                List<Item> loot = new List<Item>();
                int woodBoost = 0;
                if (player.HasEffect(AppliedEffects.CHOPPING_I))
                {
                    woodBoost = 1;
                }
                if (player.HasEffect(AppliedEffects.CHOPPING_II))
                {
                    woodBoost = 2;
                }
                if (player.HasEffect(AppliedEffects.CHOPPING_III))
                {
                    woodBoost = 3;
                }
                if (player.HasEffect(AppliedEffects.CHOPPING_IV))
                {
                    woodBoost = 4;
                }
                for (int i = 0; i < Util.RandInt(minRolls, maxRolls) + woodBoost; i++)
                {
                    loot.Add(table[Util.RandInt(0, table.Count - 1)]);
                }
                return loot;

                if (area.GetSeason() == World.Season.WINTER) //snow
                {
                    for (int i = 0; i < Util.RandInt(2, 5); i++)
                    {
                        loot.Add(ItemDict.SNOW_CRYSTAL);
                    }
                }

                //tree roll
                if (Util.RandInt(0, 2) != 0) {
                    loot.Add(ItemDict.BIRDS_NEST);
                }
                if (Util.RandInt(0, 2) == 0)
                {
                    loot.Add(ItemDict.HONEYCOMB);
                }
                for (int i = 0; i < Util.RandInt(2, 3); i++)
                {
                    if (Util.RandInt(0, 4) != 0)
                    {
                        loot.Add(ItemDict.MOSSY_BARK);
                    }
                }

                int minRoll = 0;

                if(player.HasEffect(AppliedEffects.CHOPPING_IV))
                {
                    minRoll = 4;
                } else if (player.HasEffect(AppliedEffects.CHOPPING_III))
                {
                    minRoll = 3;
                } else if (player.HasEffect(AppliedEffects.CHOPPING_II))
                {
                    minRoll = 2;
                } else if (player.HasEffect(AppliedEffects.CHOPPING_I))
                {
                    minRoll = 1;
                }

                if(Util.RandInt(1, 4) >= minRoll)
                {
                    loot.Add(ItemDict.BIRDS_NEST);
                }
                if (Util.RandInt(1, 4) >= minRoll)
                {
                    loot.Add(ItemDict.MOSSY_BARK);
                }
                if (Util.RandInt(1, 4) >= minRoll)
                {
                    loot.Add(ItemDict.HONEYCOMB);
                }


                //insect roll
                LootTable insectTable = LootTables.GenerateInsectTable(player, area, timeData);
                for (int i = 0; i < Util.RandInt(1, 3); i++)
                {
                    List<Item> insectLoot = insectTable.RollLoot(player, area, timeData);
                    foreach (Item item in insectLoot)
                    {
                        loot.Add(item);
                    }
                }
                return loot;
            }
        }

        private class FishingLootTable : LootTable
        {
            public enum FishType {
                OCEAN, FRESHWATER, LAVA, CAVE, CLOUD
            }

            private FishType fishType;

            public FishingLootTable(int minRolls, int maxRolls, FishType fishType, params ItemStack[] entries) : base(minRolls, maxRolls, entries)
            {
                this.fishType = fishType;
            }

            public override List<Item> RollLoot(EntityPlayer player, Area area, World.TimeData timeData)
            {
                List<Item> loot = new List<Item>();
                bool replacedBySeasonal = false;

                if (fishType == FishType.OCEAN)
                {
                    switch (area.GetSeason())
                    {
                        case World.Season.SPRING:
                            if (Util.RandInt(0, table.Count + 25) < 25)
                            {
                                loot.Add(ItemDict.MACKEREL);
                                replacedBySeasonal = true;
                            }
                            break;
                        case World.Season.SUMMER:
                            int roll = Util.RandInt(0, table.Count + 12 + 7);
                            if (roll < 12)
                            {
                                loot.Add(ItemDict.SHRIMP);
                                replacedBySeasonal = true;
                            }
                            else if (roll < 12 + 7)
                            {
                                loot.Add(ItemDict.PUFFERFISH);
                                replacedBySeasonal = true;
                            }
                            break;
                        case World.Season.AUTUMN:
                            if (Util.RandInt(0, table.Count + 5) < 5)
                            {
                                loot.Add(ItemDict.SEA_TURTLE);
                                replacedBySeasonal = true;
                            }
                            break;
                        case World.Season.WINTER:
                            if (Util.RandInt(0, table.Count + 9) < 9)
                            {
                                loot.Add(ItemDict.TUNA);
                                replacedBySeasonal = true;
                            }
                            break;
                    }
                } else if (fishType == FishType.FRESHWATER)
                {
                    //seasonal freshwater fish...
                }


                if (!replacedBySeasonal) {
                    loot = base.RollLoot(player, area, timeData);
                }


                int rerolls = 0;
                if(player.HasEffect(AppliedEffects.FISHING_IV)
                    || (player.HasEffect(AppliedEffects.FISHING_IV_CAVE) && fishType == FishType.CAVE)
                    || (player.HasEffect(AppliedEffects.FISHING_IV_CLOUD) && fishType == FishType.CLOUD)
                    || (player.HasEffect(AppliedEffects.FISHING_IV_FRESHWATER) && fishType == FishType.FRESHWATER)
                    || (player.HasEffect(AppliedEffects.FISHING_IV_LAVA) && fishType == FishType.LAVA)
                    || (player.HasEffect(AppliedEffects.FISHING_IV_OCEAN) && fishType == FishType.OCEAN))
                {
                    rerolls = 2;
                } else if (player.HasEffect(AppliedEffects.FISHING_III) 
                    || (player.HasEffect(AppliedEffects.FISHING_III_OCEAN) && fishType == FishType.OCEAN)
                    || (player.HasEffect(AppliedEffects.FISHING_III_FRESHWATER) && fishType == FishType.FRESHWATER)
                    || (player.HasEffect(AppliedEffects.FISHING_III_LAVA) && fishType == FishType.LAVA)
                    || (player.HasEffect(AppliedEffects.FISHING_III_CLOUD) && fishType == FishType.CLOUD))
                {
                    rerolls = Util.RandInt(1, 2);
                } else if (player.HasEffect(AppliedEffects.FISHING_II) 
                    || (player.HasEffect(AppliedEffects.FISHING_II_OCEAN) && fishType == FishType.OCEAN))
                {
                    rerolls = 1;
                } else if (player.HasEffect(AppliedEffects.FISHING_I))
                {
                    rerolls = Util.RandInt(0, 1);
                }

                for(int i = 0; i < rerolls; i++)
                {
                    Item rerollItem = table[Util.RandInt(0, table.Count - 1)];
                    if(rerollItem.GetValue() > loot[0].GetValue())
                    {
                        loot.Clear();
                        loot.Add(rerollItem);
                    }
                }

                return loot;
            }
        }

        private class ChoppingLootTable : LootTable
        {
            public ChoppingLootTable(int minRolls, int maxRolls, params ItemStack[] entries) : base(minRolls, maxRolls, entries)
            {
                //do nothing
            }

            public override List<Item> RollLoot(EntityPlayer player, Area area, World.TimeData timeData)
            {
                List<Item> loot = new List<Item>();
                int boost = 0;
                if(player.HasEffect(AppliedEffects.CHOPPING_I))
                {
                    boost = 1;
                }
                if (player.HasEffect(AppliedEffects.CHOPPING_II))
                {
                    boost = 2;
                }
                if (player.HasEffect(AppliedEffects.CHOPPING_III))
                {
                    boost = 3;
                }
                if (player.HasEffect(AppliedEffects.CHOPPING_IV))
                {
                    boost = 4;
                }
                for (int i = 0; i < Util.RandInt(minRolls, maxRolls) + boost; i++)
                {
                    loot.Add(table[Util.RandInt(0, table.Count - 1)]);
                }
                return loot;
            }
        }

        private class MiningLootTable : LootTable
        {
            public MiningLootTable(int minRolls, int maxRolls, params ItemStack[] entries) : base(minRolls, maxRolls, entries)
            {
                //nothing
            }

            public override List<Item> RollLoot(EntityPlayer player, Area area, World.TimeData timeData)
            {
                List<Item> loot = new List<Item>();
                int boost = 0;
                if (player.HasEffect(AppliedEffects.MINING_I))
                {
                    boost = Util.RandInt(0, 1);
                }
                if (player.HasEffect(AppliedEffects.MINING_II))
                {
                    boost = 1;
                }
                if (player.HasEffect(AppliedEffects.MINING_III))
                {
                    boost = Util.RandInt(1, 2);
                }
                if (player.HasEffect(AppliedEffects.MINING_IV))
                {
                    boost = 2;
                }
                for (int i = 0; i < Util.RandInt(minRolls, maxRolls) + boost; i++)
                {
                    loot.Add(table[Util.RandInt(0, table.Count - 1)]);
                }
                return loot;
            }
        }

        private class GatheringLootTable : LootTable
        {
            public enum GatheringType
            {
                COW, CHICKEN, BOAR, SHEEP, PIG
            }

            private GatheringType type;

            public GatheringLootTable(int minRolls, int maxRolls, GatheringType gatheringType, params ItemStack[] entries) : base(minRolls, maxRolls, entries)
            {
                this.type = gatheringType;
            }

            public override List<Item> RollLoot(EntityPlayer player, Area area, World.TimeData timeData)
            {
                List<Item> loot = new List<Item>();
                int boost = 0;
                if (player.HasEffect(AppliedEffects.GATHERING_BOAR) && type == GatheringType.BOAR)
                {
                    boost = Util.RandInt(1, 2);
                }
                else if (player.HasEffect(AppliedEffects.GATHERING_CHICKEN) && type == GatheringType.CHICKEN)
                {
                    boost = Util.RandInt(1, 2);
                }
                else if (player.HasEffect(AppliedEffects.GATHERING_COW) && type == GatheringType.COW)
                {
                    boost = Util.RandInt(1, 2);
                }
                else if (player.HasEffect(AppliedEffects.GATHERING_SHEEP) && type == GatheringType.SHEEP)
                {
                    boost = Util.RandInt(1, 2);
                }
                else if(player.HasEffect(AppliedEffects.GATHERING_PIG) && type == GatheringType.PIG)
                {
                    boost = Util.RandInt(1, 2);
                }
                for (int i = 0; i < Util.RandInt(minRolls, maxRolls) + boost; i++)
                {
                    loot.Add(table[Util.RandInt(0, table.Count - 1)]);
                }
                return loot;
            }
        }

        private class InsectLootTable : LootTable
        {
            public InsectLootTable(int minRolls, int maxRolls, params ItemStack[] entries) : base(minRolls, maxRolls, entries)
            {
                //nothing
            }

            public override List<Item> RollLoot(EntityPlayer player, Area area, World.TimeData timeData)
            {
                List<Item> loot = new List<Item>();
                int boost = 0;
                if (player.HasEffect(AppliedEffects.INSECT_CATCHING_I))
                {
                    boost = Util.RandInt(0, 1);
                }
                if (player.HasEffect(AppliedEffects.INSECT_CATCHING_II))
                {
                    boost = 1;
                }
                if (player.HasEffect(AppliedEffects.INSECT_CATCHING_III))
                {
                    boost = Util.RandInt(1, 2);
                }
                if (player.HasEffect(AppliedEffects.INSECT_CATCHING_IV) ||
                    (player.HasEffect(AppliedEffects.INSECT_CATCHING_IV_MORNING) && timeData.timeOfDay == World.TimeOfDay.MORNING) ||
                    (player.HasEffect(AppliedEffects.INSECT_CATCHING_IV_NIGHT) && timeData.timeOfDay == World.TimeOfDay.NIGHT))
                {
                    boost = 2;
                }
                for (int i = 0; i < Util.RandInt(minRolls, maxRolls) + boost; i++)
                {
                    loot.Add(table[Util.RandInt(0, table.Count - 1)]);
                }
                return loot;
            }
        }

        public static LootTable GenerateInsectTable(EntityPlayer player, Area area, World.TimeData timeData)
        {
            InsectLootTable insectTable = new InsectLootTable(1, 1);
            insectTable.AddEntry(new ItemStack(ItemDict.BANDED_DRAGONFLY, 5));
            insectTable.AddEntry(new ItemStack(ItemDict.YELLOW_BUTTERFLY, 5));
            insectTable.AddEntry(new ItemStack(ItemDict.HONEY_BEE, 3));
            insectTable.AddEntry(new ItemStack(ItemDict.SNAIL, 3));
            insectTable.AddEntry(new ItemStack(ItemDict.STAG_BEETLE, 3));
            if (timeData.timeOfDay == World.TimeOfDay.NIGHT)
            {
                insectTable.AddEntry(new ItemStack(ItemDict.FIREFLY, 15));
                insectTable.AddEntry(new ItemStack(ItemDict.LANTERN_MOTH, 8));
            }
            if (area.GetSeason() == World.Season.SPRING)
            {
                insectTable.AddEntry(new ItemStack(ItemDict.RICE_GRASSHOPPER, 15));
            }
            else if (area.GetSeason() == World.Season.SPRING)
            {
                insectTable.AddEntry(new ItemStack(ItemDict.BROWN_CICADA, 20));
            }
            else if (area.GetSeason() == World.Season.SPRING)
            {
                insectTable.AddEntry(new ItemStack(ItemDict.SOLDIER_ANT, 10));
            }

            if (area.GetWeather() == World.Weather.RAINY)
            {
                insectTable.AddEntry(new ItemStack(ItemDict.EARTHWORM, 25));
            }

            if (area.GetAreaEnum() == Area.AreaEnum.S1)
            {
                insectTable.AddEntry(new ItemStack(ItemDict.PINK_LADYBUG, 10));
            }
            else if (area.GetAreaEnum() == Area.AreaEnum.S2)
            {
                insectTable = new InsectLootTable(1, 2);
                insectTable.AddEntry(new ItemStack(ItemDict.CAVEWORM, 24));
                insectTable.AddEntry(new ItemStack(ItemDict.JEWEL_SPIDER, 1));
            }
            else if (area.GetAreaEnum() == Area.AreaEnum.S3)
            {
                insectTable.AddEntry(new ItemStack(ItemDict.STINGER_HORNET, 15));
                insectTable.AddEntry(new ItemStack(ItemDict.HONEYCOMB, 10));
            }
            else if (area.GetAreaEnum() == Area.AreaEnum.S4)
            {
                insectTable.AddEntry(new ItemStack(ItemDict.EMPRESS_BUTTERFLY, 3));
            }

            return insectTable;
        }

        public static LootTable BEACH_FORAGE; //BeachLootTable
        public static LootTable WEED_FORAGE;
        public static LootTable CHICKWEED_FORAGE, SUNFLOWER_FORAGE, NETTLES_FORAGE, BLUEBELL_FORAGE;
        public static LootTable MARIGOLD_FORAGE, LAVENDER_FORAGE; //ForageLootTable
        public static LootTable FALL_LEAF_PILE_FORAGE;
        public static LootTable WINTER_SNOW_PILE_FORAGE;
        public static LootTable MOREL_FORAGE, MOUNTAIN_WHEAT_FORAGE, SPICY_LEAF_FORAGE;
        public static LootTable VANILLA_BEAN_FORAGE, COCOA_FORAGE, MAIZE_FORAGE, PINEAPPLE_FORAGE;
        public static LootTable CAVE_SOYBEAN_FORAGE, EMERALD_MOSS_FORAGE, CAVE_FUNGI_FORAGE;
        public static LootTable SHIITAKE_FORAGE, SKY_ROSE_FORAGE;
        public static LootTable FARM_ROCK, FARM_BIG_ROCK, FARM_BRANCH, FARM_BIG_BRANCH;
        public static LootTable TREE_PINE, TREE_FRUIT, TREE_THIN, TREE_PALM;
        public static LootTable CHERRY, APPLE, ORANGE, OLIVE, LEMON, BANANA, COCONUT;
        public static LootTable WILD_CHERRY, WILD_APPLE, WILD_ORANGE, WILD_OLIVE, WILD_LEMON, WILD_BANANA, WILD_COCONUT;
        public static LootTable BUSH;
        public static LootTable RASPBERRY_BUSH, ELDERBERRY_BUSH, BLUEBERRY_BUSH, BLACKBERRY_BUSH;
        public static LootTable EMPTY;
        public static LootTable FISH_OCEAN;
        //GatheringLootTable
        public static LootTable COW, PIG, SHEEP, CHICKEN;

        public static void Initialize()
        {
            BEACH_FORAGE = new ForageLootTable(2, 3, ForageLootTable.ForageType.BEACH, 0, new ItemStack(ItemDict.SEAWEED, 30), new ItemStack(ItemDict.SEA_URCHIN, 20), new ItemStack(ItemDict.CLAM, 15), new ItemStack(ItemDict.OYSTER, 10),
                new ItemStack(ItemDict.CRIMSON_CORAL, 15), new ItemStack(ItemDict.FLAWLESS_CONCH, 3), new ItemStack(ItemDict.PEARL, 1));
            WEED_FORAGE = new ForageLootTable(1, 2, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.WEEDS, 1));
            BLUEBELL_FORAGE = new ForageLootTable(1, 1, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.BLUEBELL, 1));
            NETTLES_FORAGE = new ForageLootTable(2, 3, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.NETTLES, 1));
            CHICKWEED_FORAGE = new ForageLootTable(1, 1, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.CHICKWEED, 1));
            SUNFLOWER_FORAGE = new ForageLootTable(1, 1, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.SUNFLOWER, 1));
            MARIGOLD_FORAGE = new ForageLootTable(2, 2, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.MARIGOLD, 1));
            LAVENDER_FORAGE = new ForageLootTable(2, 3, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.LAVENDER, 1));
            FALL_LEAF_PILE_FORAGE = new ForageLootTable(1, 2, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.WILD_RICE, 1), new ItemStack(ItemDict.PERSIMMON, 1), new ItemStack(ItemDict.SASSAFRAS, 1));
            WINTER_SNOW_PILE_FORAGE = new ForageLootTable(1, 2, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.WINTERGREEN, 1), new ItemStack(ItemDict.CHICORY_ROOT, 1), new ItemStack(ItemDict.CHANTERELLE, 1), new ItemStack(ItemDict.SNOWDROP, 1));
            MOREL_FORAGE = new ForageLootTable(1, 1, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.MOREL, 1));
            MOUNTAIN_WHEAT_FORAGE = new ForageLootTable(2, 3, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.MOUNTAIN_WHEAT, 1));
            SPICY_LEAF_FORAGE = new ForageLootTable(1, 1, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.SPICY_LEAF, 1));
            VANILLA_BEAN_FORAGE = new ForageLootTable(2, 5, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.VANILLA_BEAN, 1));
            COCOA_FORAGE = new ForageLootTable(3, 5, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.COCOA_BEAN, 1));
            MAIZE_FORAGE = new ForageLootTable(2, 2, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.MAIZE, 1));
            PINEAPPLE_FORAGE = new ForageLootTable(1, 1, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.PINEAPPLE, 1));
            CAVE_FUNGI_FORAGE = new ForageLootTable(2, 3, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.CAVE_FUNGI, 1));
            CAVE_SOYBEAN_FORAGE = new ForageLootTable(1, 3, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.CAVE_SOYBEAN, 1));
            EMERALD_MOSS_FORAGE = new ForageLootTable(3, 3, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.EMERALD_MOSS, 1));
            SHIITAKE_FORAGE = new ForageLootTable(1, 1, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.SHIITAKE, 1));
            SKY_ROSE_FORAGE = new ForageLootTable(1, 1, ForageLootTable.ForageType.NORMAL, ForageLootTable.DEFAULT_INSECT_AMOUNT, new ItemStack(ItemDict.SKY_ROSE, 1));
            FARM_ROCK = new MiningLootTable(2, 3, new ItemStack(ItemDict.STONE, 20), new ItemStack(ItemDict.SCRAP_IRON, 5), new ItemStack(ItemDict.IRON_ORE, 3), new ItemStack(ItemDict.QUARTZ, 1));
            FARM_BIG_ROCK = new MiningLootTable(4, 6, new ItemStack(ItemDict.STONE, 50), new ItemStack(ItemDict.SCRAP_IRON, 6), new ItemStack(ItemDict.IRON_ORE, 5), new ItemStack(ItemDict.QUARTZ, 2), new ItemStack(ItemDict.AMETHYST, 1), new ItemStack(ItemDict.TOPAZ, 1));
            FARM_BRANCH = new ChoppingLootTable(1, 2, new ItemStack(ItemDict.WOOD, 24), new ItemStack(ItemDict.HARDWOOD, 1));
            FARM_BIG_BRANCH = new ChoppingLootTable(3, 5, new ItemStack(ItemDict.WOOD, 3), new ItemStack(ItemDict.HARDWOOD, 1));
            TREE_PINE = new TreeLootTable(10, 14, new ItemStack(ItemDict.WOOD, 1));
            TREE_FRUIT = new TreeLootTable(8, 11, new ItemStack(ItemDict.WOOD, 2), new ItemStack(ItemDict.HARDWOOD, 1));
            TREE_THIN = new LootTable(4, 6, new ItemStack(ItemDict.WOOD, 80), new ItemStack(ItemDict.HARDWOOD, 20), new ItemStack(ItemDict.GOLDEN_LEAF, 1));
            TREE_PALM = new LootTable(6, 9, new ItemStack(ItemDict.HARDWOOD, 80), new ItemStack(ItemDict.WOOD, 60), new ItemStack(ItemDict.FAIRY_DUST, 1));
            CHERRY = new ForageLootTable(3, 4, ForageLootTable.ForageType.NORMAL, 1, new ItemStack(ItemDict.CHERRY, 1));
            WILD_CHERRY = new ForageLootTable(1, 2, ForageLootTable.ForageType.NORMAL, 1, new ItemStack(ItemDict.CHERRY, 1));
            APPLE = new ForageLootTable(2, 4, ForageLootTable.ForageType.NORMAL, 1.5f, new ItemStack(ItemDict.APPLE, 1));
            WILD_APPLE = new ForageLootTable(1, 2, ForageLootTable.ForageType.NORMAL, 1.5f, new ItemStack(ItemDict.APPLE, 1));
            LEMON = new ForageLootTable(3, 3, ForageLootTable.ForageType.NORMAL, 1.2f, new ItemStack(ItemDict.LEMON, 1));
            WILD_LEMON = new ForageLootTable(1, 2, ForageLootTable.ForageType.NORMAL, 1.2f, new ItemStack(ItemDict.LEMON, 1));
            OLIVE = new ForageLootTable(4, 6, ForageLootTable.ForageType.NORMAL, 3.5f, new ItemStack(ItemDict.OLIVE, 1));
            WILD_OLIVE = new ForageLootTable(2, 3, ForageLootTable.ForageType.NORMAL, 3.5f, new ItemStack(ItemDict.OLIVE, 1));
            ORANGE = new ForageLootTable(2, 3, ForageLootTable.ForageType.NORMAL, 1, new ItemStack(ItemDict.ORANGE, 1));
            WILD_ORANGE = new ForageLootTable(1, 2, ForageLootTable.ForageType.NORMAL, 1, new ItemStack(ItemDict.ORANGE, 1));
            BANANA = new ForageLootTable(3, 3, ForageLootTable.ForageType.NORMAL, 0.8f, new ItemStack(ItemDict.BANANA, 1));
            WILD_BANANA = new ForageLootTable(2, 3, ForageLootTable.ForageType.NORMAL, 0.8f, new ItemStack(ItemDict.BANANA, 1));
            COCONUT = new ForageLootTable(2, 5, ForageLootTable.ForageType.NORMAL, 0.4f, new ItemStack(ItemDict.COCONUT, 1));
            WILD_COCONUT = new ForageLootTable(1, 3, ForageLootTable.ForageType.NORMAL, 0.4f, new ItemStack(ItemDict.COCONUT, 1));
            BUSH = new ForageLootTable(3, 4, ForageLootTable.ForageType.NORMAL, 1.55f, new ItemStack(ItemDict.WOOD, 1));
            BLACKBERRY_BUSH = new ForageLootTable(3, 5, ForageLootTable.ForageType.NORMAL, 0.75f, new ItemStack(ItemDict.BLACKBERRY, 1));
            ELDERBERRY_BUSH = new ForageLootTable(2, 5, ForageLootTable.ForageType.NORMAL, 0.75f, new ItemStack(ItemDict.ELDERBERRY, 1));
            BLUEBERRY_BUSH = new ForageLootTable(4, 6, ForageLootTable.ForageType.NORMAL, 0.65f, new ItemStack(ItemDict.BLUEBERRY, 1));
            RASPBERRY_BUSH = new ForageLootTable(3, 4, ForageLootTable.ForageType.NORMAL, 0.85f, new ItemStack(ItemDict.RASPBERRY, 1));
            EMPTY = new LootTable(1, 1, new ItemStack(ItemDict.NONE, 1));
            FISH_OCEAN = new FishingLootTable(1, 1, FishingLootTable.FishType.OCEAN, new ItemStack(ItemDict.SARDINE, 20), new ItemStack(ItemDict.HERRING, 18), new ItemStack(ItemDict.WOOD, 4), new ItemStack(ItemDict.SEAWEED, 5),
                new ItemStack(ItemDict.STRIPED_BASS, 9), new ItemStack(ItemDict.SARDINE, 7), new ItemStack(ItemDict.INKY_SQUID, 8), new ItemStack(ItemDict.CRIMSON_CORAL, 3),
                new ItemStack(ItemDict.CRAB, 15), new ItemStack(ItemDict.SWORDFISH, 4), new ItemStack(ItemDict.GREAT_WHITE_SHARK, 1));
            COW = new GatheringLootTable(1, 1, GatheringLootTable.GatheringType.COW, new ItemStack(ItemDict.MILK, 1));
            PIG = new GatheringLootTable(1, 1, GatheringLootTable.GatheringType.PIG, new ItemStack(ItemDict.TRUFFLE, 1), new ItemStack(ItemDict.MOREL, 2));
            CHICKEN = new GatheringLootTable(1, 1, GatheringLootTable.GatheringType.CHICKEN, new ItemStack(ItemDict.EGG, 99), new ItemStack(ItemDict.GOLDEN_EGG, 1));
            SHEEP = new GatheringLootTable(1, 1, GatheringLootTable.GatheringType.SHEEP, new ItemStack(ItemDict.WOOL, 99), new ItemStack(ItemDict.GOLDEN_WOOL, 1));
        }
    }
}
