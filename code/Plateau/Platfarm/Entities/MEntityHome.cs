using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Components;

namespace Platfarm.Entities
{
    public class MEntityHome : MapEntity, IInteract, IPersist
    {
        private AnimatedSprite sprite;

        public MEntityHome(string id, Vector2 position, AnimatedSprite sprite) : base(id, position, sprite)
        {
            this.position.Y -= sprite.GetFrameHeight();
        }

        public string GetLeftClickAction(EntityPlayer player)
        {
            return "";
        }

        public string GetRightClickAction(EntityPlayer player)
        {
            return "Enter";
        }

        public SaveState GenerateSave()
        {
            SaveState save = new SaveState(SaveState.Identifier.HOME);
            //add decoration options I guess later
            //...
            //...
            return save;
        }

        public void LoadSave(SaveState state)
        {
            Console.WriteLine("LOAD SAVE FOR HOME");
        }

        public void InteractRight(EntityPlayer player, Area area, World world)
        {
            player.ToggleAttemptTransition();
        }

        public void InteractLeft(EntityPlayer player, Area area, World world)
        {
            
        }

        public string GetLeftShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public string GetRightShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public void InteractRightShift(EntityPlayer player, Area area, World world)
        {
            
        }

        public void InteractLeftShift(EntityPlayer player, Area area, World world)
        {
            
        }
    }
}
