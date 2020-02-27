﻿using System;
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
    public class MEntityReverseCrystal : MapEntity, IInteract
    {
        private static bool currentlyReversed;

        private bool upsideDown;

        public MEntityReverseCrystal(string id, Vector2 position, AnimatedSprite sprite, bool upsideDown) : base(id, position, sprite)
        {
            this.drawLayer = DrawLayer.NORMAL;
            currentlyReversed = false;
            this.upsideDown = upsideDown;
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            if(currentlyReversed)
            {
                sprite.SetLoopIfNot("reversed");
            } else
            {
                sprite.SetLoopIfNot("normal");
            }
            sprite.Draw(sb, this.position, Color.White, layerDepth, upsideDown ? SpriteEffects.FlipVertically : SpriteEffects.None);
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
            return "Activate";
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
            if (upsideDown)
            {
                player.SetGravityState(EntityPlayer.GravityState.NORMAL);
                currentlyReversed = false;
            }
            else
            {
                player.SetGravityState(EntityPlayer.GravityState.REVERSED);
                currentlyReversed = true;
            }
            
        }

        public void InteractRightShift(EntityPlayer player, Area area, World world)
        {

        }
    }
}
