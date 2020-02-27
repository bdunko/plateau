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
    public abstract class TileEntity : Entity, IPersist
    {
        protected Vector2 tilePosition;
        protected int tileWidth, tileHeight;

        public int GetTileWidth()
        {
            if(tileWidth == 0)
            {
                throw new Exception("forgot to give tilewidth in constructor?");
            }
            return tileWidth;
        }
        public int GetTileHeight()
        {
            if (tileHeight == 0)
            {
                throw new Exception("forgot to give tileheight in constructor?");
            }
            return tileHeight;
        }

        public Vector2 GetTilePosition()
        {
            return tilePosition;
        }

        public virtual SaveState GenerateSave()
        {
            SaveState save = new SaveState(SaveState.Identifier.PLACEABLE);
            save.AddData("positionX", position.X.ToString());
            save.AddData("positionY", position.Y.ToString());
            save.AddData("tileX", tilePosition.X.ToString());
            save.AddData("tileY", tilePosition.Y.ToString());
            return save;
        }
        public virtual void LoadSave(SaveState state)
        {
            //do nothing...
        }
    }
}
