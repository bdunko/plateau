using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Particles
{
    class StaticVelocityParticle : Particle
    {

        public StaticVelocityParticle(Vector2 source, Texture2D particleShape, Color particleColor, float duration, Vector2 velocity, bool rotates) : base(source, particleShape, particleColor, duration)
        {
            this.velocityX = velocity.X;
            this.velocityY = velocity.Y;
            this.rotationSpeed = rotates ? Util.RandInt(-12, 12) / 100.0f : 0;
        }

        public override void Update(float deltaTime, Area area)
        {
            base.Update(deltaTime, area);

            this.position.Y += velocityY * deltaTime;
            if (CollisionHelper.CheckCollision(new RectangleF(this.position - new Vector2(0, particleShape.Height), new Size2(particleShape.Width, particleShape.Height)), area, velocityY >= 0))
            {
                velocityY = 0;
                timeSinceCreation = 100000;
            }

            this.position.X += velocityX * deltaTime;
            if (CollisionHelper.CheckCollision(new RectangleF(this.position - new Vector2(0, particleShape.Height), new Size2(particleShape.Width, particleShape.Height)), area, velocityY >= 0))
            {
                velocityX = 0;
                timeSinceCreation = 100000;
            }
        }
    }
}
