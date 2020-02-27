using Microsoft.Xna.Framework;
using Platfarm.Components;
using Platfarm.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Entities
{
    public class MEntityShrine : MapEntity, IInteract, IHaveHoveringInterface
    {
        private GameState.ShrineStatus shrineStatus1, shrineStatus2;
        private string name;

        public MEntityShrine(string id, Vector2 position, AnimatedSprite sprite, string name, GameState.ShrineStatus status1, GameState.ShrineStatus status2) : base(id, position, sprite)
        {
            this.name = name;
            this.position.Y += 1;
            this.drawLayer = DrawLayer.NORMAL;
            this.shrineStatus1 = status1;
            this.shrineStatus2 = status2;
        }

        public override void Update(float deltaTime, Area area)
        {
            sprite.Update(deltaTime);
            base.Update(deltaTime, area);
        }

        public virtual string GetLeftClickAction(EntityPlayer player)
        {
            return "Offer";
        }

        public virtual string GetLeftShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public virtual string GetRightClickAction(EntityPlayer player)
        {
            return "Examine";
        }

        public virtual string GetRightShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public void InteractLeft(EntityPlayer player, Area area, World world)
        {
            ItemStack selected = player.GetHeldItem();
            GameState.ShrineStatus status = null;
            if(!shrineStatus1.IsComplete())
            {
                status = shrineStatus1;
            } else if(!shrineStatus2.IsComplete())
            {
                status = shrineStatus2;
            } 

            if (status != null)
            {
                bool gaveItem = status.TryGiveItem(selected.GetItem());
                if (gaveItem)
                {
                    if (!player.GetHeldItem().GetItem().HasTag(Item.Tag.NO_TRASH))
                    {
                        player.GetHeldItem().Subtract(1);
                    }
                    if (status.IsComplete())
                    {
                        status.Complete(player, area);
                    }
                } else
                {
                    player.AddNotification(new EntityPlayer.Notification("The shrine seems to reject the offering.", Color.Red));
                }
            }
            else
            {
                player.AddNotification(new EntityPlayer.Notification("There is no need to leave more offerings here.", Color.Black));
                return;
            }
        }

        public void InteractLeftShift(EntityPlayer player, Area area, World world)
        {
            //nothing
        }

        public void InteractRight(EntityPlayer player, Area area, World world)
        {
            if (!shrineStatus2.IsComplete())
            {
                GameState.ShrineStatus.RequiredItem[] requiredItems;
                if (!shrineStatus1.IsComplete())
                {
                    requiredItems = shrineStatus1.GetRequiredItems();
                }
                else
                {
                    requiredItems = shrineStatus2.GetRequiredItems();
                }
                DialogueNode root = new DialogueNode("This ancient structure has the words\n \"" + name + "\" engraved on the front.", DialogueNode.PORTRAIT_BAD);
                DialogueNode two = new DialogueNode("It feels like I should leave an offering here...\nMaybe if I do that something good will happen?", DialogueNode.PORTRAIT_BAD);
                string threeString = "I still need to offer:\n";
                bool added = false;
                foreach (GameState.ShrineStatus.RequiredItem ri in requiredItems)
                {
                    int needed = ri.amountNeeded - ri.amountHave;
                    if (needed != 0)
                    {
                        threeString += needed.ToString() + " " + ri.item.GetName() + ", ";
                        added = true;
                    }
                }
                if (added)
                {
                    threeString = threeString.Substring(0, threeString.Length - 2);
                }

                DialogueNode three = new DialogueNode(threeString, DialogueNode.PORTRAIT_BAD);
                root.SetNext(two);
                two.SetNext(three);
                player.SetCurrentDialogue(root);
            } else
            {
                DialogueNode root = new DialogueNode("The shrine has been restored!\nIt emits an aura of natural solemnity.", DialogueNode.PORTRAIT_BAD);
                player.SetCurrentDialogue(root);
            }
        }

        public void InteractRightShift(EntityPlayer player, Area area, World world)
        {
            //nothing
        }

        public HoveringInterface GetHoveringInterface()
        {
            HoveringInterface baseHI = new HoveringInterface();
            HoveringInterface.Row row = new HoveringInterface.Row();
            GameState.ShrineStatus.RequiredItem[] requiredItems;
            if (!shrineStatus1.IsComplete())
            {
                requiredItems = shrineStatus1.GetRequiredItems();
            } else
            {
                requiredItems = shrineStatus2.GetRequiredItems();
            }

            foreach(GameState.ShrineStatus.RequiredItem ri in requiredItems)
            {
                int needed = ri.amountNeeded - ri.amountHave;
                if(needed != 0)
                {
                    row.AddElement(new HoveringInterface.ItemStackElement(new ItemStack(ri.item, needed)));
                }
            }

            baseHI.AddRow(new HoveringInterface.Row(new HoveringInterface.TextElement(name)));
            baseHI.AddRow(row);
            return baseHI;
        }
    }
}
