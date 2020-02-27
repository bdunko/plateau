using Microsoft.Xna.Framework;
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

namespace Platfarm.Entities
{
    public class PEntitySoulchest : PEntityChest, IInteract, IHaveHoveringInterface
    {
        public static int INVENTORY_SIZE = 30;

        private static ItemStack[] soulchestContents = null;
        protected PartialRecolorSprite sprite;

        public PEntitySoulchest(PartialRecolorSprite sprite, Vector2 tilePosition, PlaceableItem sourceItem, DrawLayer drawLayer) : base(sprite, tilePosition, sourceItem, drawLayer)
        {
            sprite.AddLoop("anim", 4, 14, true);
            sprite.AddLoop("placement", 0, 3, false);
            sprite.SetLoop("placement");
            if (soulchestContents == null)
            {
                soulchestContents = new ItemStack[INVENTORY_SIZE];
                for (int i = 0; i < soulchestContents.Length; i++)
                {
                    soulchestContents[i] = new ItemStack(ItemDict.NONE, 0);
                }
            }
        }

        public override ItemStack[] GetContents()
        {
            return soulchestContents;
        }

        public static SaveState GenerateSave()
        {
            if (soulchestContents == null)
            {
                soulchestContents = new ItemStack[INVENTORY_SIZE];
                for (int i = 0; i < soulchestContents.Length; i++)
                {
                    soulchestContents[i] = new ItemStack(ItemDict.NONE, 0);
                }
            }
            SaveState save = new SaveState(SaveState.Identifier.SOULCHEST);
            for (int i = 0; i < INVENTORY_SIZE; i++)
            {
                save.AddData("inventory" + i, soulchestContents[i].GetItem().GetName());
                save.AddData("inventory" + i + "quantity", soulchestContents[i].GetQuantity().ToString());
            }

            return save;
        }

        public static void LoadSave(SaveState state)
        {
            if (soulchestContents == null)
            {
                soulchestContents = new ItemStack[INVENTORY_SIZE];
                for (int i = 0; i < soulchestContents.Length; i++)
                {
                    soulchestContents[i] = new ItemStack(ItemDict.NONE, 0);
                }
            }
            for (int i = 0; i < INVENTORY_SIZE; i++)
            {
                soulchestContents[i] = new ItemStack(ItemDict.GetItemByName(state.TryGetData("inventory" + i, ItemDict.NONE.GetName()))
                    , Int32.Parse(state.TryGetData("inventory" + i + "quantity", "0")));
            }
        }

        public override string GetRightClickAction(EntityPlayer player)
        {
            return "Open Chest";
        }

        public override void OnRemove(EntityPlayer player, Area area, World world)
        {
            base.OnRemove(player, area, world);
        }

        public override void RemoveItemStackAt(int inventorySlot)
        {
            soulchestContents[inventorySlot] = new ItemStack(ItemDict.NONE, 0);
        }

        public override ItemStack GetInventoryItemStack(int inventorySlot)
        {
            return soulchestContents[inventorySlot];
        }

        public override void AddItemStackAt(ItemStack itemStack, int inventorySlot)
        {
            soulchestContents[inventorySlot] = itemStack;
        }

        public override Texture2D GetInventoryItemTexture(int i)
        {
            return soulchestContents[i].GetItem().GetTexture();
        }

        public override bool GetInventoryItemStackable(int i)
        {
            return soulchestContents[i].GetMaxQuantity() > 1;
        }

        public override int GetInventoryItemQuantity(int i)
        {
            return soulchestContents[i].GetQuantity();
        }

        public override HoveringInterface GetHoveringInterface()
        {
            if (soulchestContents[0].GetItem() != ItemDict.NONE)
            {
                if (soulchestContents[1].GetItem() != ItemDict.NONE)
                {
                    if (soulchestContents[2].GetItem() != ItemDict.NONE)
                    {
                        return new HoveringInterface(
                            new HoveringInterface.Row(
                                new HoveringInterface.ItemStackElement(soulchestContents[0]), new HoveringInterface.ItemStackElement(soulchestContents[1]), new HoveringInterface.ItemStackElement(soulchestContents[2])));
                    }
                    return new HoveringInterface(
                        new HoveringInterface.Row(
                            new HoveringInterface.ItemStackElement(soulchestContents[0]), new HoveringInterface.ItemStackElement(soulchestContents[1])));
                }
                return new HoveringInterface(
                    new HoveringInterface.Row(
                        new HoveringInterface.ItemStackElement(soulchestContents[0])));
            }
            return new HoveringInterface();
        }
    }
}
