﻿using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Platfarm.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Components
{
    public class CollisionHelper
    {
        public static bool CheckForCollisionType(RectangleF hitbox, Area area, Area.CollisionTypeEnum type)
        {
            int indexXLeft = (int)(hitbox.X / 8);
            int indexYTop = (int)(hitbox.Y / 8);
            int indexXRight = (int)((hitbox.X + hitbox.Width) / 8);
            int indexYBottom = (int)((hitbox.Y - 1 + hitbox.Height) / 8);

            //Console.WriteLine(indexXLeft + " " + indexXRight + " " + indexYTop + " " + indexYBottom + " ");

            for (int indexXCurrent = indexXLeft; indexXCurrent <= indexXRight; indexXCurrent++)
            {
                for (int indexYCurrent = indexYTop; indexYCurrent <= indexYBottom; indexYCurrent++)
                {
                    Area.CollisionTypeEnum cType = area.GetCollisionTypeAt(indexXCurrent, indexYCurrent);
                    if (cType == type)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool CheckCollision(RectangleF hitbox, Area area, bool falling)
        {
            return CheckCollision(hitbox, area, falling, null);
        }

        public static bool CheckCollision(RectangleF hitbox, Area area, bool falling, MEntitySolid self)
        {
            int indexXLeft = (int)(hitbox.X / 8);
            int indexYTop = (int)(hitbox.Y / 8);
            int indexXRight = (int)((hitbox.X + hitbox.Width) / 8);
            int indexYBottom = (int)((hitbox.Y-1 + hitbox.Height) / 8);

            //Console.WriteLine(indexXLeft + " " + indexXRight + " " + indexYTop + " " + indexYBottom + " ");

            for (int indexXCurrent = indexXLeft; indexXCurrent <= indexXRight; indexXCurrent++)
            {
                for (int indexYCurrent = indexYTop; indexYCurrent <= indexYBottom; indexYCurrent++)
                {
                    Area.CollisionTypeEnum cType = area.GetCollisionTypeAt(indexXCurrent, indexYCurrent);
                    if (cType == Area.CollisionTypeEnum.SOLID || cType == Area.CollisionTypeEnum.SCAFFOLDING_BLOCK)
                    {
                        return true;
                    }
                    else if ((cType == Area.CollisionTypeEnum.BRIDGE || cType == Area.CollisionTypeEnum.SCAFFOLDING_BRIDGE) && falling &&
                        hitbox.Y + hitbox.Height >= area.GetPositionOfTile(indexXCurrent, indexYCurrent).Y)
                    {
                        return true;
                    }
                }
            }

            //EntitySolids
            List<MEntitySolid> solids = area.GetSolidEntities();
            foreach (MEntitySolid solid in solids)
            {
                if (solid != self) //prevent collision on itself
                {
                    RectangleF solidHitbox = solid.GetCollisionRectangle();
                    if (solidHitbox.Intersects(hitbox) && solid.GetPlatformType() != MEntitySolid.PlatformType.AIR)
                    {
                        if (solid.GetPlatformType() == MEntitySolid.PlatformType.BRIDGE && falling && hitbox.Y < solidHitbox.Top)
                        {
                            return true;
                        }
                        else if (solid.GetPlatformType() == MEntitySolid.PlatformType.SOLID)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

    }
}
