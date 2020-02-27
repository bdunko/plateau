using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Components;
using Platfarm.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Entities
{
    public class PEntityTotem : PlacedEntity, ITickDaily
    {
        private EntityAnimal linkedAnimal;
        private static float ANIMATION_DELAY = 5.0f;
        private float timeSinceAnimation;
        protected PartialRecolorSprite sprite;
        private EntityAnimal.Type animalType;
        private bool animalCanSpawn;

        public PEntityTotem(PartialRecolorSprite sprite, Vector2 tilePosition, PlaceableItem sourceItem, DrawLayer drawLayer, EntityAnimal.Type animalType) : base(tilePosition, sourceItem, drawLayer)
        {
            this.sprite = sprite;
            this.tileHeight = sprite.GetFrameHeight() / 8;
            this.tileWidth = sprite.GetFrameWidth() / 8;
            this.itemForm = sourceItem;
            this.animalType = animalType;
            this.linkedAnimal = null;
            this.animalCanSpawn = false;
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            sprite.Draw(sb, position, Color.White, layerDepth);
        }

        public override SaveState GenerateSave()
        {
            SaveState state = base.GenerateSave();
            state.AddData("animalExists", (linkedAnimal != null || animalCanSpawn).ToString());
            //state.AddData("animalHarvestable", (linkedAnimal == null ? false.ToString() : linkedAnimal.IsHarvestable().ToString()));
            return state;
        }

        public override void LoadSave(SaveState state)
        {
            base.LoadSave(state);
            bool animalExists = state.TryGetData("animalExists", false.ToString()) == true.ToString();
            //bool animalHarvestable = state.TryGetData("animalHarvestable", false.ToString()) == true.ToString();
            if(animalExists)
            {
                animalCanSpawn = true;
            }
        }

        public override void Update(float deltaTime, Area area)
        {
            if(animalCanSpawn)
            {
                SpawnAnimal(area);
            }

            timeSinceAnimation += deltaTime;
            sprite.Update(deltaTime);
            if (sprite.IsCurrentLoopFinished())
            {
                sprite.SetLoopIfNot("idle");
                if(animalType == EntityAnimal.Type.PIG)
                {
                    sprite.SetLoop("anim");
                }
            }
            if (timeSinceAnimation >= ANIMATION_DELAY && sprite.IsCurrentLoop("idle"))
            {
                sprite.SetLoopIfNot("anim");
                timeSinceAnimation = 0;
            }
        }

        public override void OnRemove(EntityPlayer player, Area area, World world)
        {
            area.RemoveEntity(linkedAnimal);
            base.OnRemove(player, area, world);
        }

        private Vector2 FindSpawnPosition(Area area)
        {
            int animalWidth = 0, animalHeight = 0;

            switch(animalType)
            {
                case EntityAnimal.Type.CHICKEN:
                    animalWidth = 2;
                    animalHeight = 2;
                    break;
                case EntityAnimal.Type.COW:
                    animalWidth = 3;
                    animalHeight = 2;
                    break;
                case EntityAnimal.Type.SHEEP:
                    animalWidth = 2;
                    animalHeight = 2;
                    break;
                case EntityAnimal.Type.PIG:
                    animalWidth = 3;
                    animalHeight = 2;
                    break;
            }

            List<Area.XYTile> checkSpots = new List<Area.XYTile>();
            for(int x = -6; x < 6; x++)
            {
                for(int y = -6; y < 6; y++)
                {
                    Vector2 spotToCheck = new Vector2(tilePosition.X + x, tilePosition.Y + y);
                    checkSpots.Add(new Area.XYTile((int)spotToCheck.X, (int)spotToCheck.Y));
                }
            }
            checkSpots = Util.ShuffleList<Area.XYTile>(checkSpots);

            bool spotFound = false;
            int currentTile = 0;

            while(!spotFound && currentTile < checkSpots.Count - 1)
            {
                bool noSolidInHitbox = true; 
                Area.XYTile toCheck = checkSpots[currentTile];
                
                //make sure none of spawn is covered by solid tiles
                for(int width = 0; width < animalWidth; width++)
                {
                    for(int height = 0; height < animalHeight; height++)
                    {
                        if(area.GetCollisionTypeAt(toCheck.tileX+width, toCheck.tileY + height) == Area.CollisionTypeEnum.SOLID ||
                            area.GetCollisionTypeAt(toCheck.tileX + width, toCheck.tileY + height) == Area.CollisionTypeEnum.WATER ||
                            area.GetCollisionTypeAt(toCheck.tileX + width, toCheck.tileY + height) == Area.CollisionTypeEnum.BOUNDARY ||
                            area.GetCollisionTypeAt(toCheck.tileX + width, toCheck.tileY + height) == Area.CollisionTypeEnum.SCAFFOLDING_BLOCK ||
                            area.GetCollisionTypeAt(toCheck.tileX + width, toCheck.tileY + height) == Area.CollisionTypeEnum.BRIDGE)
                        {
                            noSolidInHitbox = false;
                        }
                    }
                }

                if (noSolidInHitbox)
                {
                    //check to see if ground is on tiles below
                    for (int width = 0; width < animalWidth; width++) {
                        if (area.GetCollisionTypeAt(toCheck.tileX + width, toCheck.tileY + animalHeight) == Area.CollisionTypeEnum.SOLID ||
                            area.GetCollisionTypeAt(toCheck.tileX + width, toCheck.tileY + animalHeight) == Area.CollisionTypeEnum.BRIDGE ||
                            area.GetCollisionTypeAt(toCheck.tileX + width, toCheck.tileY + animalHeight) == Area.CollisionTypeEnum.SCAFFOLDING_BLOCK ||
                            area.GetCollisionTypeAt(toCheck.tileX + width, toCheck.tileY + animalHeight) == Area.CollisionTypeEnum.SCAFFOLDING_BRIDGE)
                        {
                            spotFound = true;
                        }
                    }
                }

                if(!spotFound)
                {
                    currentTile++;
                }
            }

            if(spotFound)
            {
                return new Vector2(checkSpots[currentTile].tileX, checkSpots[currentTile].tileY);
            } 

            return new Vector2(-100000, -100000);
        }

        private void SpawnAnimal(Area area)
        {
            Vector2 spawnPosition = FindSpawnPosition(area);
            if(spawnPosition.X == -100000 && spawnPosition.Y == -100000)
            {
                return;
            }

            switch (animalType)
            {
                case EntityAnimal.Type.CHICKEN:
                    linkedAnimal = (EntityAnimal)EntityFactory.GetEntity(EntityType.CHICKEN, null, spawnPosition, area);
                    break;
                case EntityAnimal.Type.COW:
                    linkedAnimal = (EntityAnimal)EntityFactory.GetEntity(EntityType.COW, null, spawnPosition, area);
                    break;
                case EntityAnimal.Type.PIG:
                    linkedAnimal = (EntityAnimal)EntityFactory.GetEntity(EntityType.PIG, null, spawnPosition, area);
                    break;
                case EntityAnimal.Type.SHEEP:
                    linkedAnimal = (EntityAnimal)EntityFactory.GetEntity(EntityType.SHEEP, null, spawnPosition, area);
                    break;
            }
            
            animalCanSpawn = false;
            area.AddEntity(linkedAnimal);
        }

        public override RectangleF GetCollisionRectangle()
        {
            return new RectangleF(position, new Size2(sprite.GetFrameWidth(), sprite.GetFrameHeight()));
        }

        public void TickDaily(World world, Area area, EntityPlayer player)
        {
            if(linkedAnimal == null)
            {
                animalCanSpawn = true;
            }
        }
    }
}
