using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Components;

namespace Platfarm.Entities
{
    public class MEntityMovingPlatform : MEntitySolid
    {
        public MEntityMovingPlatform(string id, Vector2 position, AnimatedSprite sprite, EntityPlayer player, PlatformType type, Vector2 initialVelocity, bool collideWithTerrain) : base(id, position, sprite, player, type, collideWithTerrain)
        {
            this.velocity = initialVelocity;
        }

        public override void Update(float deltaTime, Area area)
        {
            base.Update(deltaTime, area);

            Area.MovingPlatformDirectorZone directorZone = area.GetDirectorZoneAt(GetCollisionRectangle());
            if(directorZone != null)
            {
                this.velocity = directorZone.newVelocity;
            }
        }
    }
}
