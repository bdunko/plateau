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
    class MEntityDoor : MapEntity, IInteract
    {
        private bool isVisible;

        public MEntityDoor(string id, Vector2 position, AnimatedSprite sprite) : base(id, position, sprite)
        {
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            /*if (isVisible)
            {
                base.Draw(sb);
            }*/
        }

        public override RectangleF GetCollisionRectangle()
        {
            return new RectangleF(this.position - new Vector2(8, 0), new Size2(sprite.GetFrameWidth() + 16, sprite.GetFrameHeight()));
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
            return "Enter";
        }

        public string GetRightShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public void InteractLeft(EntityPlayer player, Area area, World world)
        {
            
        }

        public void InteractLeftShift(EntityPlayer player, Area area, World world)
        {
            
        }

        public void InteractRight(EntityPlayer player, Area area, World world)
        {
            player.ToggleAttemptTransition();
        }

        public void InteractRightShift(EntityPlayer player, Area area, World world)
        {
            player.ToggleAttemptTransition();
        }
    }
}
