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
    public class MEntityShippingBin : MapEntity, IInteract, IPersist, ITickDaily, IHaveHoveringInterface
    {
        private List<Item> shippedItems;
        private int totalValue;

        public MEntityShippingBin(string id, Vector2 position, AnimatedSprite sprite) : base(id, position, sprite)
        {
            this.position.Y += 1;
            this.drawLayer = DrawLayer.NORMAL;
            this.totalValue = 0;
            shippedItems = new List<Item>();
            sprite.AddLoop("still", 0, 0, true);
            sprite.AddLoop("anim", 1, 2, false);
            sprite.SetLoop("still");
        }

        public SaveState GenerateSave()
        {
            SaveState save = new SaveState(SaveState.Identifier.SHIPPING_BIN);
            save.AddData("id", GetID());
            save.AddData("numItems", shippedItems.Count.ToString());
            for(int i = 0; i < shippedItems.Count; i++)
            {
                save.AddData("item" + i, shippedItems[i].GetName());
            }
            return save;
        }

        public override void Update(float deltaTime, Area area)
        {
            sprite.Update(deltaTime);
            if(sprite.IsCurrentLoopFinished())
            {
                sprite.SetLoop("still");
            }
            base.Update(deltaTime, area);
        }

        public virtual void LoadSave(SaveState state)
        {
            int numItems = Int32.Parse(state.TryGetData("numItems", "0"));
            for(int i = 0; i < numItems; i++)
            {
                shippedItems.Add(ItemDict.GetItemByName(state.TryGetData("item" + i, "ERROR")));
            }
        }

        public virtual string GetLeftClickAction(EntityPlayer player)
        {
            return "Ship";
        }

        public virtual string GetLeftShiftClickAction(EntityPlayer player)
        {
            return "Ship Stack";
        }

        public virtual string GetRightClickAction(EntityPlayer player)
        {
            return "Empty";
        }

        public virtual string GetRightShiftClickAction(EntityPlayer player)
        {
            return "Ship All";
        }

        public void InteractLeft(EntityPlayer player, Area area, World world)
        {
            ItemStack selected = player.GetHeldItem();
            if(!selected.GetItem().HasTag(Item.Tag.NO_TRASH))
            {
                shippedItems.Add(selected.GetItem());
                totalValue += selected.GetItem().GetValue();
                player.GetHeldItem().Subtract(1);
                sprite.SetLoop("anim");
            } else if (selected.GetItem() != ItemDict.NONE)
            {
                player.AddNotification(new EntityPlayer.Notification("I can't sell that.", Color.Red));
            }
        }

        public void InteractLeftShift(EntityPlayer player, Area area, World world)
        {
            ItemStack selected = player.GetHeldItem();
            if (!selected.GetItem().HasTag(Item.Tag.NO_TRASH))
            {
                while(selected.GetQuantity() != 0) { 
                    shippedItems.Add(selected.GetItem());
                    totalValue += selected.GetItem().GetValue();
                    player.GetHeldItem().Subtract(1);
                    sprite.SetLoop("anim");
                }
            }
            else if (selected.GetItem() != ItemDict.NONE)
            {
                player.AddNotification(new EntityPlayer.Notification("I can't sell that.", Color.Red));
            }
        }

        public void InteractRight(EntityPlayer player, Area area, World world)
        {
            if (shippedItems.Count != 0)
            {
                foreach (Item item in shippedItems)
                {
                    area.AddEntity(new EntityItem(item, new Vector2(position.X, position.Y - 10)));
                    sprite.SetLoop("anim");
                }
                shippedItems.Clear();
                totalValue = 0;
            } else
            {
                player.AddNotification(new EntityPlayer.Notification("The bin is empty.", Color.Red));
            }
        }

        public void InteractRightShift(EntityPlayer player, Area area, World world)
        {
            ItemStack selected = player.GetHeldItem();
            if (!selected.GetItem().HasTag(Item.Tag.NO_TRASH))
            {
                Item shipAllOf = selected.GetItem();
                while (player.RemoveItemFromInventory(shipAllOf))
                {
                    shippedItems.Add(shipAllOf);
                    totalValue += shipAllOf.GetValue();
                    sprite.SetLoop("anim");
                }
            }
            else if (selected.GetItem() != ItemDict.NONE)
            {
                player.AddNotification(new EntityPlayer.Notification("I can't sell that.", Color.Red));
            }
        }

        public void TickDaily(World world, Area area, EntityPlayer player)
        {
            if (totalValue != 0)
            {
                player.GainGold(totalValue);
                player.AddNotification(new EntityPlayer.Notification("Overnight, the contents of your shipping bin were sold for " + totalValue + "G", Color.DarkGreen, EntityPlayer.Notification.Length.LONG));
            }
            shippedItems.Clear();
            totalValue = 0;
        }

        public HoveringInterface GetHoveringInterface()
        {
            ItemStack recent = new ItemStack(ItemDict.NONE, 0);
            if(shippedItems.Count > 0)
            {
                recent = new ItemStack(shippedItems[shippedItems.Count - 1], 1);
            } 
            return new HoveringInterface(
                new HoveringInterface.Row(
                    new HoveringInterface.TextElement("Shipment Bin")),
                new HoveringInterface.Row(
                    new HoveringInterface.TextElement("Total Value: " + totalValue)),
                new HoveringInterface.Row(
                    new HoveringInterface.ItemStackElement(recent)));
        }
    }
}
