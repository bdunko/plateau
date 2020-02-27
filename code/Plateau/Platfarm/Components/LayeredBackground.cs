using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Components
{
    public class LayeredBackground
    {
        //TODO:
        //static background elements such as sun, moon, sky - don't display correctly
        //doesn't account for parallax, so stuff still "pops" in visibly

        public class Element
        {
            public Texture2D texture;
            public Vector2 position, speed;
            protected bool isStatic;

            public Element(Texture2D texture, Vector2 position, Vector2 speed)
            {
                this.texture = texture;
                this.speed = speed;
                this.position = new Vector2((Plateau.SCREEN_DIMENSIONS.Width * position.X)/Plateau.SCALE, (Plateau.SCREEN_DIMENSIONS.Height * position.Y)/Plateau.SCALE);
                this.isStatic = true;
            }

            public Element(Texture2D texture, RectangleF inflatedBounds, Vector2 speed)
            {
                this.texture = texture;
                this.position = new Vector2(0, 0);
                RandomizePosition(inflatedBounds);
                this.speed = speed;
            }

            public void RandomizePosition(RectangleF bounds)
            {
                if (!isStatic)
                {
                    position.X = Util.RandInt((int)(bounds.Left - texture.Width), (int)(bounds.Right + texture.Width));
                    position.Y = Util.RandInt((int)(bounds.Top - texture.Height), (int)(bounds.Bottom + texture.Height));
                }
            }

            public virtual void Update(float deltaTime, RectangleF boundingRect, RectangleF cameraBoundingBox, Vector2 cameraMovementSinceLastFrame, float layerMultiplier)
            {
                if (!isStatic)
                {
                    RectangleF elementRect = new RectangleF(position, new Size2(texture.Bounds.X, texture.Bounds.Y));
                    if (!elementRect.Intersects(boundingRect.ToRectangle()))
                    {
                        do
                        {
                            RandomizePosition(boundingRect);
                            elementRect = new RectangleF(position, new Size2(texture.Width, texture.Height));
                        } while (elementRect.Intersects(cameraBoundingBox.ToRectangle()));
                    }
                    this.position += new Vector2(speed.X * deltaTime, speed.Y * deltaTime);
                    this.position += new Vector2(cameraMovementSinceLastFrame.X * layerMultiplier * deltaTime, cameraMovementSinceLastFrame.Y * layerMultiplier * deltaTime);
                }
            }

            public virtual void Draw(SpriteBatch sb, RectangleF cameraBoundingBox, float layerMultiplier, Color colorTint, float layerDepth)
            {
                if (isStatic)
                {
                    sb.Draw(texture, cameraBoundingBox.TopLeft + position, texture.Bounds, colorTint, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerDepth);
                } else
                {
                    if (new RectangleF(position.X, position.Y, texture.Width, texture.Height).Intersects(cameraBoundingBox));
                    {
                        sb.Draw(texture, position, texture.Bounds, colorTint, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerDepth);
                    }
                }
            }
        }

        public class RotatingElement : Element
        {
            private float rotationSpeed = 0;
            private float angle;

            public RotatingElement(Texture2D texture, RectangleF inflatedBounds, Vector2 speed, float rotationSpeed) : base(texture, inflatedBounds, speed)
            {
                this.rotationSpeed = rotationSpeed;
                this.angle = 3.14f / 2;
            }

            public override void Update(float deltaTime, RectangleF boundingRect, RectangleF cameraBoundingBox, Vector2 cameraMovementSinceLastFrame, float layerMultiplier)
            {
                base.Update(deltaTime, boundingRect, cameraBoundingBox, cameraMovementSinceLastFrame, layerMultiplier);
                angle += rotationSpeed;
            }

            public override void Draw(SpriteBatch sb, RectangleF cameraBoundingBox, float layerMultiplier, Color colorTint, float layerDepth)
            {
                if (isStatic)
                {
                    sb.Draw(texture, cameraBoundingBox.TopLeft + position, texture.Bounds, colorTint, angle, new Vector2(texture.Width / 2, texture.Height / 2), 1.0f, SpriteEffects.None, layerDepth);
                }
                else
                {
                    if (new RectangleF(position.X, position.Y, texture.Width, texture.Height).Intersects(cameraBoundingBox))
                    {
                        sb.Draw(texture, position, texture.Bounds, colorTint, angle, new Vector2(texture.Width / 2, texture.Height / 2), 1.0f, SpriteEffects.None, layerDepth);
                    }
                }
            }
        }

        public class Layer
        {
            private string name;
            private List<Element> elements;
            private Color colorTint;
            //higher values = more resistant = less movement;
            //far away = high value (1 means it never moves)
            //close = lower value (negative values means it is in FRONT of the player)
            private float layerMultipier;
            //private bool absolutePosition;
            private bool disable;
            private float transparency;

            public Layer(string name, Color colorTint, float layerMultiplier)
            {
                this.name = name;
                this.colorTint = colorTint;
                //this.absolutePosition = absolutePosition;
                elements = new List<Element>();
                this.layerMultipier = -layerMultiplier;
                this.disable = false;
                this.transparency = 1.0f;
            }

            public void Randomize(RectangleF bounds)
            {
                foreach(Element e in elements)
                {
                    e.RandomizePosition(bounds);
                }
            }

            public void Disable()
            {
                this.disable = true;
            }

            public void Enable()
            {
                this.disable = false;
            }

            public Color GetColorTint()
            {
                return colorTint;
            }

            public bool IsDisabled()
            {
                return disable;
            }

            public string GetName()
            {
                return name;
            }

            public void AddElement(Element element)
            {
                elements.Add(element);
            }

            public void ChangeColorTint(Color newTint)
            {
                this.colorTint = newTint;
            }

            public void Update(float deltaTime, RectangleF inflatedBounds, RectangleF cameraBoundingBox, Vector2 cameraMovementSinceLastFrame)
            {
                foreach(Element element in elements)
                {
                    element.Update(deltaTime, inflatedBounds, cameraBoundingBox, cameraMovementSinceLastFrame, layerMultipier);
                }
            }

            public void SetTransparency(float transparency)
            {
                this.transparency = transparency;
            }

            public void Draw(SpriteBatch sb, RectangleF cameraBoundingBox, float layerDepth) {
                foreach(Element element in elements)
                {
                    element.Draw(sb, cameraBoundingBox, layerMultipier, colorTint * transparency, layerDepth);
                }
            }
        }

        private Dictionary<string, Layer> layers;
        private RectangleF lastFrameCameraBox;
        private bool firstFrame;

        public LayeredBackground()
        {
            this.firstFrame = true;

            layers = new Dictionary<string, Layer>();
        }

        public void AddLayer(Layer layer)
        {
            layers[layer.GetName()] = layer;
        }

        public void TrySetTransparency(string name, float transparency)
        {
            if(layers.ContainsKey(name))
            {
                layers[name].SetTransparency(transparency);
            }
        }

        public Color TryGetTint(string name)
        {
            if(layers.ContainsKey(name))
            {
                return layers[name].GetColorTint();
            }
            return Color.White;
        }

        public void TrySetTint(string name, Color tint)
        {
            if (layers.ContainsKey(name))
            {
                layers[name].ChangeColorTint(tint);
            }
        }

        public void TryDisableLayer(string name)
        {
            if (layers.ContainsKey(name))
            {
                layers[name].Disable();
            }
        }

        public void TryEnableLayer(string name)
        {
            if (layers.ContainsKey(name))
            {
                layers[name].Enable();
            }
        }

        public Layer GetLayer(string name)
        {
            if(!layers.ContainsKey(name))
            {
                return null;
            }
            return layers[name];
        }

        private static float X_INFLATION = Plateau.RESOLUTION_X*2;
        private static float Y_INFLATION = Plateau.RESOLUTION_Y*2;

        public static RectangleF CalculateInflatedBoundsRect(RectangleF cameraBoundingBox)
        {
            return new RectangleF(cameraBoundingBox.X - X_INFLATION, cameraBoundingBox.Y - Y_INFLATION, cameraBoundingBox.Width + (2 * X_INFLATION), cameraBoundingBox.Height + (2 * Y_INFLATION));
        }

        public void Update(float deltaTime, RectangleF cameraBoundingBox)
        {
            if(firstFrame)
            {
                firstFrame = !firstFrame;
                lastFrameCameraBox = cameraBoundingBox;
            }

            Vector2 cameraMovementSinceLastFrame = new Vector2(cameraBoundingBox.X - lastFrameCameraBox.X, cameraBoundingBox.Y - lastFrameCameraBox.Y);
            lastFrameCameraBox = cameraBoundingBox;

            RectangleF inflatedBounds = CalculateInflatedBoundsRect(cameraBoundingBox);
            foreach (string layerName in layers.Keys)
            {
                if (!layers[layerName].IsDisabled())
                {
                    layers[layerName].Update(deltaTime, inflatedBounds, cameraBoundingBox, cameraMovementSinceLastFrame);
                }
            }
        }

        public void Draw(SpriteBatch sb, RectangleF cameraBoundingBox, float layerDepth)
        {
            //Vector2 cameraPosition = cameraBoundingBox.Center;
            foreach(string layerName in layers.Keys)
            {
                if (!layers[layerName].IsDisabled())
                {
                    layers[layerName].Draw(sb, cameraBoundingBox, layerDepth);
                }
            }
        }

        public void Randomize(RectangleF cameraBoundingBox)
        {
            RectangleF bounds = CalculateInflatedBoundsRect(cameraBoundingBox);
            foreach(string layerName in layers.Keys)
            {
                layers[layerName].Randomize(bounds);
            }
        }
    }
}
