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
    public class MEntityTrampoline : EntityCollidable, IInteractContact
    {
        public enum TrampolineType {
            UP, DOWN, LEFT, RIGHT
        }

        private Vector2 launchVelocity;
        private TrampolineType type;
        private Vector2 position;
        private AnimatedSprite sprite;

        public MEntityTrampoline(AnimatedSprite sprite, Vector2 position, Vector2 launchVelocity, TrampolineType type)
        {
            this.type = type;
            this.position = position;
            this.sprite = sprite;
            this.launchVelocity = launchVelocity;
            this.drawLayer = DrawLayer.PRIORITY;
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            if (type == TrampolineType.DOWN)
            {
                sprite.Draw(sb, position, Color.White, layerDepth, SpriteEffects.FlipVertically);
            }
            else if (type == TrampolineType.LEFT)
            {
                sprite.Draw(sb, position, Color.White, layerDepth, SpriteEffects.FlipHorizontally);
            }
            else
            {
                sprite.Draw(sb, position, Color.White, layerDepth);
            }
        }

        public override RectangleF GetCollisionRectangle()
        {
            switch(type)
            {
                case TrampolineType.UP:
                    return new RectangleF(position.X + 2, position.Y + sprite.GetFrameHeight() - 4, sprite.GetFrameWidth() - 4, 4);
                case TrampolineType.DOWN:
                    return new RectangleF(position.X + 2, position.Y, sprite.GetFrameWidth() - 4, 4);
                case TrampolineType.RIGHT:
                    return new RectangleF(position.X, position.Y+2, 4, sprite.GetFrameHeight()-4);
                case TrampolineType.LEFT:
                    return new RectangleF(position.X + sprite.GetFrameWidth() - 4, position.Y + 2, 4, sprite.GetFrameHeight() - 4);
            }
            throw new Exception("Error in Trampoline GetCollisionRectangle...");
        }

        public void OnContact(EntityPlayer player, Area area)
        {
            if (type == TrampolineType.UP && (player.IsGroundPound() || player.IsGroundPoundLock()))
            {
                player.SetExternalVelocityX(launchVelocity.X);
                player.SetVelocityY(launchVelocity.Y - 0.8f);
            }
            else
            {
                player.SetExternalVelocityX(launchVelocity.X);
                player.SetVelocityY(launchVelocity.Y);
            }
            sprite.SetLoop("anim");
            player.CancelGlide();
            player.CancelGroundPound();
            if (type == TrampolineType.LEFT || type == TrampolineType.RIGHT)
            {
                player.ResetInputVelocityX();
            }
        }

        public override void Update(float deltaTime, Area area)
        {
            if(sprite.GetCurrentLoop() == "anim" && sprite.IsCurrentLoopFinished())
            {
                sprite.SetLoopIfNot("idle");
            }
            sprite.Update(deltaTime);
        }

        public override bool IsRiding(RectangleF solidRect)
        {
            RectangleF ridingHitbox = GetCollisionRectangle();
            ridingHitbox.X -= 1;
            ridingHitbox.Y -= 1;
            ridingHitbox.Width += 2;
            ridingHitbox.Height += 2;
            return solidRect.Intersects(ridingHitbox);
        }

        protected override void OnXCollision()
        {
            //do nothing
        }

        protected override void OnYCollision()
        {
            //do nothing
        }

        public override bool PushX(float pushX, Area area)
        {
            this.position.X += pushX;
            return true;
        }

        public override bool PushY(float pushY, Area area)
        {
            this.position.Y += pushY;
            return true;
        }
    }
}
