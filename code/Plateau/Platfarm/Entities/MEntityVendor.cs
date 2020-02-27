using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Components;
using Platfarm.Entities;
using Platfarm.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Entities
{
    public abstract class MEntityVendor : MapEntity, IInteract, IPersist, IHaveHoveringInterface
    {
        protected class SaleItem
        {
            public static SaleItem NONE = new SaleItem(ItemDict.NONE, 0, 1, null);

            public int quantity;
            public DialogueNode examineDialogue;
            public Item item;
            public int cost;

            public SaleItem(Item item, int quantity, int cost, DialogueNode examineDialogue)
            {
                this.item = item;
                this.quantity = quantity;
                this.examineDialogue = examineDialogue;
                this.cost = cost;
            }
        }

        protected SaleItem currentSaleItem;
        protected List<SaleItem> saleItems;
        protected int quantityRemaining;

        public MEntityVendor(string id, Vector2 position, AnimatedSprite sprite) : base(id, position, sprite)
        {
            this.currentSaleItem = SaleItem.NONE;
            quantityRemaining = 0;
            saleItems = new List<SaleItem>();
        }

        public SaveState GenerateSave()
        {
            SaveState save = new SaveState(SaveState.Identifier.VENDORENTITY);
            save.AddData("id", GetID());
            save.AddData("currentItem", currentSaleItem.item.GetName());
            save.AddData("quantityRemaining", quantityRemaining.ToString());
            return save;
        }

        public virtual void LoadSave(SaveState state)
        {
            Item currentItem = ItemDict.GetItemByName(state.TryGetData("currentItem", "NONE"));
            foreach(SaleItem si in saleItems)
            {
                if(si.item == currentItem)
                {
                    currentSaleItem = si;
                    break;
                }
            }
            quantityRemaining = Int32.Parse(state.TryGetData("quantityRemaining", "0"));
        }

        public virtual string GetLeftClickAction(EntityPlayer player)
        {
            return "";
            
        }

        public virtual string GetLeftShiftClickAction(EntityPlayer player)
        {
            if (currentSaleItem != SaleItem.NONE)
            {
                return "Buy 1";
            }
            return "";
        }

        public virtual string GetRightClickAction(EntityPlayer player)
        {
            if (currentSaleItem != SaleItem.NONE)
            {
                return "Examine";
            }
            return "";
            
        }

        public virtual string GetRightShiftClickAction(EntityPlayer player)
        {
            if (currentSaleItem != SaleItem.NONE)
            {
                return "Buy 5";
            }
            return "";
        }

        public void InteractLeft(EntityPlayer player, Area area, World world)
        {
            //do nothing
        }

        public void InteractLeftShift(EntityPlayer player, Area area, World world)
        {
            if (currentSaleItem != SaleItem.NONE && quantityRemaining > 0)
            {
                if (player.GetGold() > currentSaleItem.cost)
                {
                    player.SpendGold(currentSaleItem.cost);
                    area.AddEntity(new EntityItem(currentSaleItem.item, new Vector2(position.X, position.Y - 10)));
                    quantityRemaining--;
                    if (quantityRemaining == 0)
                    {
                        currentSaleItem = SaleItem.NONE;
                    }
                } else
                {
                    player.AddNotification(new EntityPlayer.Notification("Not enough gold!", Color.Red, EntityPlayer.Notification.Length.SHORT));
                }
            }
        }

        public void InteractRight(EntityPlayer player, Area area, World world)
        {
            if (currentSaleItem != SaleItem.NONE)
            {
                player.SetCurrentDialogue(currentSaleItem.examineDialogue);
            }
        }

        public void InteractRightShift(EntityPlayer player, Area area, World world)
        {
            for(int i = 0; i < 5; i++)
            {
                InteractLeftShift(player, area, world);
            }
        }

        public HoveringInterface GetHoveringInterface()
        {
            if(currentSaleItem != SaleItem.NONE)
            {
                return new HoveringInterface(
                    new HoveringInterface.Row(
                        new HoveringInterface.TextElement(currentSaleItem.item.GetName())),
                    new HoveringInterface.Row(
                        new HoveringInterface.TextElement("Price: " + currentSaleItem.cost.ToString() + "G")));
            }
            return new HoveringInterface(
                new HoveringInterface.Row(
                    new HoveringInterface.TextElement("Sold Out!")));
        }
    }
}
