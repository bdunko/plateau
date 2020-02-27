using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Components
{
    public class MainMenuInterface
    {
        public enum MainMenuState {
            NONE, CLICKED_SAVE_1, CLICKED_SAVE_2, CLICKED_SAVE_3
        }

        private Vector2 logoCurrentLocation;
        private static float LOGO_SCROLL_SPEED = 0.5f;
        private static Vector2 LOGO_LOCATION = new Vector2(88, 12);
        private static Rectangle[] buttons = { new Rectangle(56, 160, 54, 35), new Rectangle(133, 160, 54, 35), new Rectangle(210, 160, 54, 35), new Rectangle(299, 179, 16, 16) };
        private static int NEW_SAVE_X_OFFSET = 3;
        private static int NEW_SAVE_Y_OFFSET = 11;
        bool file1Exists, file2Exists, file3Exists;

        private Texture2D background;
        private Texture2D logo, saveSlot, settingsIcon, newSaveIcon;
        private Texture2D mouseCursor;
        private Controller controller;
        private MainMenuState state;

        public MainMenuInterface(Controller controller, bool file1Exists, bool file2Exists, bool file3Exists)
        {
            logoCurrentLocation = new Vector2(LOGO_LOCATION.X, LOGO_LOCATION.Y - 80);
            this.controller = controller;
            this.state = MainMenuState.NONE;
            this.file1Exists = file1Exists;
            this.file2Exists = file2Exists;
            this.file3Exists = file3Exists;
        }

        public void Update(RectangleF cameraBoundingBox)
        {
            logoCurrentLocation.Y = Util.AdjustTowards(logoCurrentLocation.Y, LOGO_LOCATION.Y, LOGO_SCROLL_SPEED);
            
            if (controller.GetMouseLeftPress())
            {
                Vector2 mousePosition = controller.GetMousePos();
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i].Contains(mousePosition))
                    {
                        switch(i)
                        {
                            case 0:
                                state = MainMenuState.CLICKED_SAVE_1;
                                break;
                            case 1:
                                state = MainMenuState.CLICKED_SAVE_2;
                                break;
                            case 2:
                                state = MainMenuState.CLICKED_SAVE_3;
                                break;
                            case 3:
                                //settings
                                break;
                        }
                    }
                }
            }
        }

        public void LoadContent(ContentManager content)
        {
            background = content.Load<Texture2D>(Paths.INTERFACE_MAINMENU_BACKGROUND);
            newSaveIcon = content.Load<Texture2D>(Paths.INTERFACE_MAINMENU_NEWGAME);
            logo = content.Load<Texture2D>(Paths.INTERFACE_MAINMENU_LOGO);
            saveSlot = content.Load<Texture2D>(Paths.INTERFACE_MAINMENU_SAVESLOT);
            settingsIcon = content.Load<Texture2D>(Paths.INTERFACE_MAINMENU_SETTINGS);
        }

        public void Draw(SpriteBatch sb, RectangleF cameraBoundingBox)
        {
            sb.Draw(background, cameraBoundingBox.TopLeft, Color.White);
            sb.Draw(logo, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, logoCurrentLocation), Color.White);
            sb.Draw(saveSlot, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, new Vector2(buttons[0].X, buttons[0].Y)), Color.White);
            sb.Draw(saveSlot, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, new Vector2(buttons[1].X, buttons[1].Y)), Color.White);
            sb.Draw(saveSlot, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, new Vector2(buttons[2].X, buttons[2].Y)), Color.White);
            if(!file1Exists)
            {
                sb.Draw(newSaveIcon, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, new Vector2(buttons[0].X + NEW_SAVE_X_OFFSET, buttons[0].Y + NEW_SAVE_Y_OFFSET)), Color.White);
            }
            if (!file2Exists)
            {
                sb.Draw(newSaveIcon, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, new Vector2(buttons[1].X + NEW_SAVE_X_OFFSET, buttons[1].Y + NEW_SAVE_Y_OFFSET)), Color.White);
            }
            if (!file3Exists)
            {
                sb.Draw(newSaveIcon, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, new Vector2(buttons[2].X + NEW_SAVE_X_OFFSET, buttons[2].Y + NEW_SAVE_Y_OFFSET)), Color.White);
            }
            sb.Draw(settingsIcon, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, new Vector2(buttons[3].X, buttons[3].Y)), Color.White);
            //sb.Draw(mouseCursor, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, controller.GetMousePos()), Color.White);
        }

        public MainMenuState GetState()
        {
            return state;
        }

    }
}
