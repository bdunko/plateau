using Microsoft.Xna.Framework;
using Platfarm.Entities;
using Platfarm.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Components
{
    public class DebugConsole
    {
        private World world;
        private SaveManager saveManager;
        private EntityPlayer player;
        private bool didLastSucceed;

        public DebugConsole(World world, SaveManager saveManager, EntityPlayer player)
        {
            this.world = world;
            this.saveManager = saveManager;
            this.player = player;
            this.didLastSucceed = true;
        }

        public bool DidLastSucceed()
        {
            return didLastSucceed;
        }

        public void RunCommand(string command)
        {
            didLastSucceed = false;
            command = command.ToLower();
            string[] pieces = command.Split(' ');
            Console.WriteLine("Running command: " + command);

            if (pieces[0].Equals("info"))
            {
                Console.WriteLine("Current Area:         " + world.GetCurrentArea().GetAreaEnum().ToString()); 
                Console.WriteLine("Player Position:      " + player.GetAdjustedPosition());
                Console.WriteLine("Weather:              " + world.GetWeather().ToString());
                didLastSucceed = true;
            }
            else if (pieces[0].Equals("save"))
            {
                saveManager.SaveFile(player, world);
                didLastSucceed = true;
            }
            else if (pieces[0].Equals("timeset"))
            {
                if(pieces.Count() == 2)
                {
                    int hour = 0;
                    if (Int32.TryParse(pieces[1], out hour))
                    {
                        world.SetTime(hour, 0);
                        didLastSucceed = true;
                    }
                } else if (pieces.Count() == 3)
                {
                    int hour = 0, minute;
                    if (Int32.TryParse(pieces[1], out hour))
                    {
                        if (Int32.TryParse(pieces[2], out minute))
                        {
                            world.SetTime(hour, minute);
                            didLastSucceed = true;
                        }
                    }
                }
            }
            else if (pieces[0].Equals("warp"))
            {
                if(pieces.Count() != 1)
                {
                    string areaName = pieces[1].ToUpper();
                    foreach (Area.AreaEnum areaEnum in world.GetAreaDict().Keys)
                    {
                        if (areaEnum.ToString().Equals(areaName))
                        {
                            world.SetCurrentArea(world.GetAreaDict()[areaEnum]);
                            didLastSucceed = true;
                            break;
                        }
                    }
                }
            }
            else if (pieces[0].Equals("move"))
            {
                int x, y;
                if (pieces[1].Equals("x"))
                {
                    if (Int32.TryParse(pieces[2], out y))
                    {
                        player.SetPosition(new Vector2(player.GetAdjustedPosition().X, y));
                        didLastSucceed = true;
                    }
                } else if (pieces[2].Equals("y"))
                {
                    if (Int32.TryParse(pieces[1], out x))
                    {
                        player.SetPosition(new Vector2(x, player.GetAdjustedPosition().Y - 10));
                        didLastSucceed = true;
                    }
                }
                else if (Int32.TryParse(pieces[1], out x))
                {
                    if (Int32.TryParse(pieces[2], out y))
                    {
                        player.SetPosition(new Vector2(x, y));
                        didLastSucceed = true;
                    }
                }
            }
            else if (pieces[0].Equals("give"))
            {
                string itemName = "";
                for(int i = 1; i < pieces.Length; i++)
                {
                    itemName += pieces[i];
                    itemName += " ";
                }
                Item toGive = ItemDict.GetItemByName(itemName.Substring(0, itemName.Length-1));
                if (toGive != ItemDict.NONE)
                {
                    for (int i = 0; i < toGive.GetStackCapacity(); i++)
                    {
                        player.AddItemToInventory(toGive, false, true);
                    }
                    didLastSucceed = true;
                }
            }
            else if (pieces[0].Equals("nextday"))
            {
                if (pieces.Count() == 1)
                {
                    world.AdvanceDay(player);
                    saveManager.SaveFile(player, world);
                    didLastSucceed = true;
                } else if (pieces.Count() == 2)
                {
                    int num;
                    if (Int32.TryParse(pieces[1], out num))
                    {
                        for(int i = 0; i < num; i++)
                        {
                            world.AdvanceDay(player);
                        }
                        saveManager.SaveFile(player, world);
                        didLastSucceed = true;
                    }
                }
            }
            else if (pieces[0].Equals("launch"))
            {
                player.SetPosition(new Vector2(player.GetAdjustedPosition().X, 0));
                didLastSucceed = true;
            }
            else if (pieces[0].Equals("commands") || pieces[0].Equals("help"))
            {
                Console.WriteLine("info - outputs information");
                Console.WriteLine("save - saves data");
                Console.WriteLine("give <item> - gives player item");
                Console.WriteLine("clearinv - removes all items from inventory");
                Console.WriteLine("resetinv - clears inventory; then gives tools");
                Console.WriteLine("dyes - gives player a stack of all dyes");
                Console.WriteLine("gold <x> - gives x gold");
                Console.WriteLine("bankrupt - sets gold to 0");
                Console.WriteLine("hair <item> - sets the player hair");
                Console.WriteLine("skin <item> - sets the player skin");
                Console.WriteLine("eyes <item> - sets the player eyes");
                Console.WriteLine("timeset <hour> <minute> - sets current time");
                Console.WriteLine("nextday - advances day");
                Console.WriteLine("nextday <x> - advances x days");
                Console.WriteLine("warp <area> - changes current area");
                Console.WriteLine("move <x> <y> - sets player xy position");
                Console.WriteLine("move x <y> - sets player y position, x stays same");
                Console.WriteLine("move <x> y - sets player x position, y stays same");
                Console.WriteLine("launch - sets player y to top of screen");

                didLastSucceed = true;
            }
            else if (pieces[0].Equals("gold"))
            {
                if(pieces.Count() == 2)
                {
                    int goldAmount = 0;
                    if (Int32.TryParse(pieces[1], out goldAmount))
                    {
                        player.GainGold(goldAmount);
                        didLastSucceed = true;
                    }
                }
            }
            else if (pieces[0].Equals("bankrupt"))
            {
                player.SpendGold(player.GetGold());
                didLastSucceed = true;
            }
            else if (pieces[0].Equals("hair"))
            {
                string itemName = "";
                for (int i = 1; i < pieces.Length; i++)
                {
                    itemName += pieces[i];
                    itemName += " ";
                }
                Item toGive = ItemDict.GetItemByName(itemName.Substring(0, itemName.Length - 1));
                if (toGive != ItemDict.NONE && toGive.HasTag(Item.Tag.HAIR))
                {
                    player.SetHair(new ItemStack(toGive, 1));
                    didLastSucceed = true;
                }
            }
            else if (pieces[0].Equals("skin"))
            {
                string itemName = "";
                for (int i = 1; i < pieces.Length; i++)
                {
                    itemName += pieces[i];
                    itemName += " ";
                }
                Item toGive = ItemDict.GetItemByName(itemName.Substring(0, itemName.Length - 1));
                if (toGive != ItemDict.NONE && toGive.HasTag(Item.Tag.SKIN))
                {
                    player.SetSkin(new ItemStack(toGive, 1));
                    didLastSucceed = true;
                }
            }
            else if (pieces[0].Equals("eyes"))
            {
                string itemName = "";
                for (int i = 1; i < pieces.Length; i++)
                {
                    itemName += pieces[i];
                    itemName += " ";
                }
                Item toGive = ItemDict.GetItemByName(itemName.Substring(0, itemName.Length - 1));
                if (toGive != ItemDict.NONE && toGive.HasTag(Item.Tag.EYES))
                {
                    player.SetEyes(new ItemStack(toGive, 1));
                    didLastSucceed = true;
                }
            }
            else if (pieces[0].Equals("clearinv"))
            {
                for(int i = 0; i < EntityPlayer.INVENTORY_SIZE; i++)
                {
                    player.RemoveItemStackAt(i);
                }
                didLastSucceed = true;
            }
            else if (pieces[0].Equals("dyes"))
            {
                RunCommand("give un-dye");
                RunCommand("give white dye");
                RunCommand("give light grey dye");
                RunCommand("give dark grey dye");
                RunCommand("give black dye");
                RunCommand("give navy dye");
                RunCommand("give blue dye");
                RunCommand("give red dye");
                RunCommand("give pink dye");
                RunCommand("give orange dye");
                RunCommand("give yellow dye");
                RunCommand("give brown dye");
                RunCommand("give green dye");
                RunCommand("give olive dye");
                RunCommand("give purple dye");
                didLastSucceed = true;
            }
            else if (pieces[0].Equals("resetinv"))
            {
                RunCommand("clearinv");
                RunCommand("give hoe");
                RunCommand("give watering can");
                RunCommand("give pickaxe");
                RunCommand("give axe");
                RunCommand("give fishing rod");
                didLastSucceed = true;
            } else if (pieces[0].Equals("haircolor"))
            {
                string hairName = ItemDict.GetColoredItemBaseForm(player.GetHair().GetItem());
                if (pieces.Length != 1)
                {
                    hairName += " (Hair ";
                    for (int i = 1; i < pieces.Length; i++)
                    {
                        hairName += pieces[i];
                        hairName += " ";
                    }
                    hairName = hairName.Substring(0, hairName.Length - 1);
                    hairName += ")";
                }
                Item toGive = ItemDict.GetItemByName(hairName);
                if (toGive != ItemDict.NONE && toGive.HasTag(Item.Tag.HAIR))
                {
                    player.SetHair(new ItemStack(toGive, 1));
                    didLastSucceed = true;
                }
            } else if (pieces[0].Equals("weather"))
            {
                string weatherName = pieces[1].ToUpper();
                World.Weather weather;
                bool success = Enum.TryParse(weatherName, out weather);
                if(success)
                    world.SetWeather(weather);
                didLastSucceed = success;
            } else if (pieces[0].Equals("unlockallrecipes"))
            {
                GameState.UnlockAllRecipes();
                didLastSucceed = true;
            } else if (pieces[0].Equals("unlockrecipe"))
            {
                string itemName = "";
                if(pieces.Length != 1) {
                    for (int i = 1; i < pieces.Length; i++)
                    {
                        itemName += pieces[i];
                        itemName += " ";
                    }
                    Item toUnlock = ItemDict.GetItemByName(itemName.Substring(0, itemName.Length - 1));
                    didLastSucceed = true;
                } else
                {
                    didLastSucceed = false;
                }
            }
            else
            {
                Console.WriteLine("Illegal command - see \"commands\"");
            }
        }
    }
}
