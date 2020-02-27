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
using Platfarm.Particles;

namespace Platfarm.Entities
{
    class TEntityForage : TileEntity, IInteract
    {
        private Texture2D texture;
        private LootTables.LootTable lootTable;
        private Color particleColor1;
        private Color particleColor2;
        private EntityType type;
        private bool disposable;

        public TEntityForage(Texture2D texture, Vector2 tilePosition, EntityType type, Color particle1, Color particle2, LootTables.LootTable lootTable)
        {
            this.texture = texture;
            this.position = new Vector2(tilePosition.X * 8, tilePosition.Y * 8);
            this.position.Y += 1;
            this.tilePosition = tilePosition;
            this.tileHeight = texture.Height / 8;
            this.tileWidth = texture.Width / 8;
            this.type = type;
            this.lootTable = lootTable;
            this.particleColor1 = particle1;
            this.particleColor2 = particle2;
            this.disposable = false;
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            sb.Draw(texture, position, texture.Bounds, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerDepth);
        }

        public override RectangleF GetCollisionRectangle()
        {
            return new RectangleF(position, new Size2(texture.Width, texture.Height));
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
            return "Gather";
        }

        public string GetRightShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public void InteractLeft(EntityPlayer player, Area area, World world)
        {
            //do nothing
        }

        public void InteractLeftShift(EntityPlayer player, Area area, World world)
        {
            //do nothing
        }

        bool harvested = false;

        public void InteractRight(EntityPlayer player, Area area, World world)
        {
            area.RemoveTileEntity(player, (int)tilePosition.X, (int)tilePosition.Y, world);
            if (!harvested)
            {
                List<Item> drops = lootTable.RollLoot(player, area, world.GetTimeData());
                foreach (Item drop in drops)
                {
                    area.AddEntity(new EntityItem(drop, new Vector2(position.X, position.Y - 10)));
                }
                //player.SetAnimLock
                for (int i = 0; i < 3; i++)
                {
                    area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(0, 3), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.SMALL,
                        particleColor1, ParticleFactory.DURATION_MEDIUM));
                }
                for (int i = 0; i < 2; i++)
                {
                    area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(0, 3), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                        particleColor2, ParticleFactory.DURATION_MEDIUM));
                }
                for (int i = 0; i < 2; i++)
                {
                    area.AddParticle(ParticleFactory.GenerateParticle(this.position + new Vector2(0, 3), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.SMALL,
                        area.GetSecondaryColorForTile((int)player.GetTileStandingOn().X, (int)player.GetTileStandingOn().Y), ParticleFactory.DURATION_MEDIUM));
                }
                harvested = true;
            }
        }

        public override SaveState GenerateSave()
        {
            SaveState save = base.GenerateSave();
            save.AddData("entitytype", type.ToString());
            return save;
        }

        public void InteractRightShift(EntityPlayer player, Area area, World world)
        {
            //do nothing
        }

        public override void Update(float deltaTime, Area area)
        {
            //do nothing...
        }
    }
}
