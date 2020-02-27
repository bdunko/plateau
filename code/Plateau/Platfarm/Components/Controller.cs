﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Components
{
    public class Controller
    {
        private static int QUEUE_SIZE = 8;
        protected Queue<KeyboardState> kStates;
        protected Queue<MouseState> mStates;
        protected int previousScrollValue;
        protected TextInputHandler textInputHandler;

        public Controller()
        {
            textInputHandler = new TextInputHandler();
            kStates = new Queue<KeyboardState>();
            mStates = new Queue<MouseState>();

            for(int i = 0; i < QUEUE_SIZE; i++)
            {
                kStates.Enqueue(Keyboard.GetState());
                mStates.Enqueue(Mouse.GetState());
            }

            previousScrollValue = 0;
        }

        public void ClearStringInput()
        {
            textInputHandler.ClearInputString();
        }

        public bool IsAcceptingInput()
        {
            return textInputHandler.IsActive();
        }

        public void ActivateStringInput()
        {
            textInputHandler.Activate();
        }

        public void DeactivateStringInput()
        {
            textInputHandler.Deactivate();
        }

        public string GetStringInput()
        {
            return textInputHandler.GetInputString();
        }

        public virtual void Update()
        {
            textInputHandler.Update();
            kStates.Dequeue();
            mStates.Dequeue();
            kStates.Enqueue(Keyboard.GetState());
            mStates.Enqueue(Mouse.GetState());
        }

        //Is Key down this frame
        public bool IsKeyDown(Keys key)
        {
            return kStates.ElementAt(QUEUE_SIZE-1).IsKeyDown(key);
        }

        //Key released this frame, was down last frame
        public bool IsKeyPressed(Keys key)
        {
            return (kStates.ElementAt(QUEUE_SIZE-2).IsKeyUp(key) && kStates.ElementAt(QUEUE_SIZE-1).IsKeyDown(key));
        }

        public bool IsKeyUp(Keys key)
        {
            return kStates.ElementAt(QUEUE_SIZE-1).IsKeyUp(key);
        }

        public bool IsKeyDoublePressed(Keys key)
        {
            int flags = 0;
            for(int i = 0; i < QUEUE_SIZE; i++)
            {
                if(flags == 0)
                {
                    if(kStates.ElementAt(i).IsKeyDown(key))
                    {
                        flags++;
                    }
                } else if (flags == 1)
                {
                    if (kStates.ElementAt(i).IsKeyUp(key))
                    {
                        flags++;
                    }
                } else if (flags == 2)
                {
                    if (kStates.ElementAt(i).IsKeyDown(key))
                    {
                        flags++;
                    }
                }
            }
            
            return flags == 3;
        }

        public bool GetMouseLeftPress()
        {
            return (mStates.ElementAt(QUEUE_SIZE - 2).LeftButton == ButtonState.Pressed && mStates.ElementAt(QUEUE_SIZE - 1).LeftButton == ButtonState.Released);
        }

        public bool GetMouseLeftDown()
        {
            return mStates.ElementAt(QUEUE_SIZE - 1).LeftButton == ButtonState.Pressed;
        }

        public bool GetMouseRightPress()
        {
            return (mStates.ElementAt(QUEUE_SIZE - 2).RightButton == ButtonState.Pressed && mStates.ElementAt(QUEUE_SIZE - 1).RightButton == ButtonState.Released);
        }

        public bool GetMouseRightDown()
        {
            return mStates.ElementAt(QUEUE_SIZE - 1).RightButton == ButtonState.Pressed;
        }

        public Vector2 GetMousePos()
        {
            Vector2 mousePos = new Vector2(mStates.ElementAt(QUEUE_SIZE - 1).X / Plateau.SCALE, mStates.ElementAt(QUEUE_SIZE - 1).Y / Plateau.SCALE);
            return mousePos;
        }

        public int GetChangeInMouseWheel()
        {
            int change = 0;
            MouseState recent = mStates.ElementAt(QUEUE_SIZE - 1);
            int newScrollValue = recent.ScrollWheelValue;

            if(newScrollValue > previousScrollValue)
            {
                change = -1;
            } else if (newScrollValue < previousScrollValue)
            {
                change = 1;
            }

            previousScrollValue = newScrollValue;
            return change;
        }
    }
}
