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

namespace Platfarm.Entities
{
    public class PEntityBeehive : PlacedEntity, IInteract, ITick
    {
        private static Area.AreaEnum[] INCLUDED_AREAS = { Area.AreaEnum.BEACH, Area.AreaEnum.FARM, Area.AreaEnum.TOWN, Area.AreaEnum.S0, Area.AreaEnum.S1, Area.AreaEnum.S3};

        private static int MAX_CAPACITY = 8;
        private static int PROCESSING_TIME = 12 * 60;
        private PartialRecolorSprite sprite;
        private int timeRemaining;
        private ResultHoverBox resultHoverBox;
        private ItemStack producedItem;
        private static float TIME_BETWEEN_ANIMATION = 3.0f;
        private float animationTimer;

        public PEntityBeehive(PartialRecolorSprite sprite, Vector2 tilePosition, PlaceableItem sourceItem, DrawLayer drawLayer) : base(tilePosition, sourceItem, drawLayer)
        {
            this.producedItem = new ItemStack(ItemDict.NONE, 0);
            this.sprite = sprite;
            sprite.AddLoop("idle", 0, 0, true);
            sprite.AddLoop("placement", 0, 3, false);
            sprite.AddLoop("animation", 4, 9, false);
            sprite.SetLoop("placement");
            this.tileHeight = sprite.GetFrameHeight() / 8;
            this.tileWidth = sprite.GetFrameWidth() / 8;
            this.timeRemaining = PROCESSING_TIME;
            this.resultHoverBox = new ResultHoverBox();
            this.animationTimer = TIME_BETWEEN_ANIMATION;
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            sprite.Draw(sb, new Vector2(position.X, position.Y + 1), Color.Wheat, layerDepth);
            resultHoverBox.Draw(sb, new Vector2(position.X + (sprite.GetFrameWidth() / 2), position.Y), layerDepth);
        }

        public override SaveState GenerateSave()
        {
            SaveState save = base.GenerateSave();
            save.AddData("item", producedItem.GetItem().GetName());
            save.AddData("quantity", producedItem.GetQuantity().ToString());
            save.AddData("timeRemaining", timeRemaining.ToString());
            return save;
        }

        public override void LoadSave(SaveState saveState)
        {
            producedItem = new ItemStack(ItemDict.GetItemByName(saveState.TryGetData("item", ItemDict.NONE.GetName())),
                Int32.Parse(saveState.TryGetData("quantity", "0")));
            timeRemaining = Int32.Parse(saveState.TryGetData("timeRemaining", "0"));
        }

        public override void Update(float deltaTime, Area area)
        {
            sprite.Update(deltaTime);
            if(sprite.IsCurrentLoopFinished())
            {
                sprite.SetLoop("idle");
            }
            if(!sprite.IsCurrentLoop("placement"))
            {
                animationTimer -= deltaTime;
                if(animationTimer <= 0)
                {
                    animationTimer = TIME_BETWEEN_ANIMATION;
                    sprite.SetLoop("animation");
                }
            }

            if(producedItem.GetItem() != ItemDict.NONE)
            {
                resultHoverBox.AssignItemStack(producedItem);
            } else
            {
                resultHoverBox.RemoveItemStack();
            }
            resultHoverBox.Update(deltaTime);
        }

        public override void OnRemove(EntityPlayer player, Area area, World world)
        {
            if (producedItem.GetItem() != ItemDict.NONE)
            {
                for (int i = 0; i < producedItem.GetQuantity(); i++)
                {
                    area.AddEntity(new EntityItem(producedItem.GetItem(), new Vector2(position.X, position.Y - 10)));
                }
            }
            base.OnRemove(player, area, world);
        }

        public override RectangleF GetCollisionRectangle()
        {
            return new RectangleF(position, new Size2(sprite.GetFrameWidth(), sprite.GetFrameHeight()));
        }

        public string GetLeftShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public string GetRightShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public string GetLeftClickAction(EntityPlayer player)
        {
            return "";
        }

        public string GetRightClickAction(EntityPlayer player)
        {
            if(producedItem.GetItem() != ItemDict.NONE)
            {
                return "Gather";
            }
            return "";
        }

        public void InteractRight(EntityPlayer player, Area area, World world)
        {
            if (producedItem.GetItem() != ItemDict.NONE)
            {
                List<Item> result = new List<Item>();
                for(int j = 0; j < producedItem.GetQuantity(); j++)
                {
                    result.Add(producedItem.GetItem());
                }
                    
                switch(area.GetAreaEnum())
                {
                    case Area.AreaEnum.FARM:
                    case Area.AreaEnum.TOWN:
                    case Area.AreaEnum.S0:
                        for(int j = 0; j < producedItem.GetQuantity(); j++)
                        {
                            if (Util.RandInt(1, 100) <= 40) {result.Add(ItemDict.BEESWAX); }
                            if (Util.RandInt(1, 100) <= 10) { result.Add(ItemDict.HONEYCOMB); }
                        }
                        break;
                    case Area.AreaEnum.BEACH:
                        for(int j = 0; j < producedItem.GetQuantity(); j++)
                        {
                            if (Util.RandInt(1, 100) <= 40) { result.Add(ItemDict.HONEYCOMB); }
                        }
                        break;
                    case Area.AreaEnum.S1:
                        for (int j = 0; j < producedItem.GetQuantity(); j++)
                        {
                            if (Util.RandInt(1, 100) <= 40) { result.Add(ItemDict.ROYAL_JELLY); }
                            if (Util.RandInt(1, 100) <= 10) { result.Add(ItemDict.BEESWAX); }
                        }
                        break;
                    case Area.AreaEnum.S3:
                        for (int j = 0; j < producedItem.GetQuantity(); j++)
                        {
                            if (Util.RandInt(1, 100) <= 40) { result.Add(ItemDict.POLLEN_PUFF); }
                            if (Util.RandInt(1, 100) <= 10) { result.Add(ItemDict.BEESWAX); }
                        }
                        break;
                }
                if(Util.RandInt(1, 100) == 1) { result.Add(ItemDict.QUEENS_STINGER);  }

                foreach(Item resultItem in result)
                {
                    area.AddEntity(new EntityItem(resultItem, new Vector2(position.X, position.Y - 10)));
                }
                sprite.SetLoop("placement");
                producedItem = new ItemStack(ItemDict.NONE, 0);
            }
        }

        public void InteractLeft(EntityPlayer player, Area area, World world)
        {
            
        }

        public void InteractRightShift(EntityPlayer player, Area area, World world)
        {
            
        }

        public void InteractLeftShift(EntityPlayer player, Area area, World world)
        {
            
        }

        public void Tick(int minutesTicked, EntityPlayer player, Area area, World world)
        {
            timeRemaining -= minutesTicked;
            if(timeRemaining <= 0)
            {
                if (INCLUDED_AREAS.Contains(area.GetAreaEnum()))
                {
                    if (producedItem.GetItem() == ItemDict.NONE)
                    {
                        producedItem = new ItemStack(ItemDict.WILD_HONEY, 1);
                    }
                    else
                    {
                        if (producedItem.GetQuantity() < MAX_CAPACITY)
                        {
                            producedItem.Add(1);
                        }
                    }
                }
                timeRemaining = PROCESSING_TIME;
            }
        }
    }
}
