using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Entities;
using Platfarm.Items;
using Platfarm.Sound;
using System;
using System.Collections.Generic;

namespace Platfarm.Components
{
    public enum InterfaceState
    {
        NONE, INVENTORY, CHEST, SCRAPBOOK, EXIT, EXIT_CONFIRMED, SETTINGS, CRAFTING,
        TRANSITION_TO_UP, TRANSITION_TO_DOWN, TRANSITION_TO_LEFT, TRANSITION_TO_RIGHT, TRANSITION_FADE_TO_BLACK, TRANSITION_FADE_IN
    }

    public class NineSlice
    {
        private Texture2D slicetopleft, slicetopright, slicetopcenter, slicebottomleft, slicebottomright, slicebottomcenter, slicemiddleleft, slicemiddleright, slicemiddlecenter;

        public NineSlice(Texture2D topleft, Texture2D topright, Texture2D topcenter,
            Texture2D middleleft, Texture2D middleright, Texture2D middlecenter,
            Texture2D bottomleft, Texture2D bottomright, Texture2D bottomcenter)
        {
            slicetopleft = topleft;
            slicetopright = topright;
            slicetopcenter = topcenter;
            slicebottomright = bottomright;
            slicebottomleft = bottomleft;
            slicebottomcenter = bottomcenter;
            slicemiddleleft = middleleft;
            slicemiddleright = middleright;
            slicemiddlecenter = middlecenter;
        }

        public void Draw(SpriteBatch sb, RectangleF rectangle)
        {
            int sliceWidth = slicemiddlecenter.Width;
            int sliceHeight = slicemiddlecenter.Height;

            rectangle.X -= sliceWidth + 2;
            rectangle.Y -= sliceHeight - 1;
            rectangle.Width += sliceWidth + 4;
            rectangle.Height += sliceHeight - 2;
            for (int x = 0; x < rectangle.Width; x += sliceWidth)
            {
                for (int y = 0; y < rectangle.Height; y += sliceHeight)
                {
                    Texture2D drawnPiece = slicemiddlecenter;
                    if (y == 0)
                    {
                        if (x == 0)
                        {
                            drawnPiece = slicetopleft;
                        }
                        else if (x + sliceWidth >= rectangle.Width)
                        {
                            drawnPiece = slicetopright;
                        }
                        else
                        {
                            drawnPiece = slicetopcenter;
                        }
                    }
                    else if (x == 0)
                    {
                        if (y + sliceHeight >= rectangle.Height)
                        {
                            drawnPiece = slicebottomleft;
                        }
                        else
                        {
                            drawnPiece = slicemiddleleft;
                        }
                    }
                    else if (y + sliceHeight >= rectangle.Height)
                    {
                        if (x + sliceWidth >= rectangle.Width)
                        {
                            drawnPiece = slicebottomright;
                        }
                        else
                        {
                            drawnPiece = slicebottomcenter;
                        }
                    }
                    else if (x + sliceWidth >= rectangle.Width)
                    {
                        drawnPiece = slicemiddleright;
                    }
                    sb.Draw(drawnPiece, new Vector2(x + rectangle.X, y + rectangle.Y), Color.White);
                }
            }
        }

        //adjusts position a bit too..
        public void DrawString(SpriteBatch sb, string str, Vector2 position, RectangleF cameraBoundingBox, Color color)
        {
            Vector2 textSize = Plateau.FONT.MeasureString(str) * Plateau.FONT_SCALE;
            bool runsOffRight = position.X + textSize.X + 5 > cameraBoundingBox.Right;
            bool runsOffBottom = position.Y + textSize.Y + 10 > cameraBoundingBox.Bottom;

            if (runsOffRight && runsOffBottom)
            {
                position.X = cameraBoundingBox.Right - (textSize.X + 8);
                position.Y -= textSize.Y;
            }
            else if (runsOffRight)
            {
                position.X = cameraBoundingBox.Right - (textSize.X + 8);
                position.Y -= textSize.Y;
            }
            else if (runsOffBottom)
            {
                position.Y = cameraBoundingBox.Bottom - (textSize.Y + 8);
            }

            if(position.Y - 4 < cameraBoundingBox.Top)
            {
                position.Y = cameraBoundingBox.Top + 4;
            }

            Draw(sb, new RectangleF(position, textSize));
            sb.DrawString(Plateau.FONT, str, position, color, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
        }

    }

    public class GameplayInterface
    {
        public static NineSlice TOOLTIP_9SLICE;
        public static NineSlice INTERFACE_9SLICE;
        public static NineSlice DIALOGUE_9SLICE;
        public static NineSlice THOUGHTS_9SLICE;

        private class CollectedTooltip
        {
            public static int FAST_THRESHOLD = 200;

            public static float LENGTH = 0.8f;
            private static float DELTA_Y = 47;
            private static float DELTA_Y_FAST = 140;
            private static float LENGTH_FAST = 0.6f;

            public float timeElapsed;
            public string name;
            public bool fast;

            public CollectedTooltip(string name, bool fast)
            {
                this.name = name;
                this.timeElapsed = 0;
                this.fast = fast;
            }

            public void Update(float deltaTime)
            {
                timeElapsed += deltaTime;
            }

            public float GetYAdjustment()
            {
                return (fast ? DELTA_Y_FAST : DELTA_Y) * (timeElapsed / (fast?LENGTH_FAST : LENGTH));
            }

            public bool IsFinished()
            {
                return timeElapsed >= (fast ? LENGTH_FAST : LENGTH);
            }
        }

        public static int HOTBAR_LENGTH = 10;

        private static float BLACK_BACKGROUND_OPACITY = 0.7f;
        private static float PLACEMENT_OPACITY = 0.9f;
        private static float GRID_OPACITY = 0.15f;

        private static int NUM_SCRAPBOOK_TABS = 11;
        private static int NUM_SCRAPBOOK_TITLES_PER_TAB = 10;
        private static int NUM_SCRAPBOOK_PAGES = NUM_SCRAPBOOK_TABS * NUM_SCRAPBOOK_TITLES_PER_TAB;

        private Controller controller;
        private Texture2D hotbar, hotbar_selected, inventory, chest_inventory, chest_inventory_greyscale, inventory_selected;
        private Texture2D craftingButtonSolo;
        private string tooltipName, tooltipDescription;
        private ItemStack[] inventoryItems;
        public static Texture2D[] numbers, numbersNoBorder;
        public static Texture2D itemBox, hoveringItemBox;
        private Texture2D black_background, black_box, grid;
        private Texture2D placeableTexture;
        private Texture2D[] seasonText;
        private Texture2D[] dayText;
        private Texture2D dateTimePanel;
        private Texture2D mouseControl, keyControl, menuControl, shiftOnUnpressed, shiftOnPressed, shiftOffUnpressed, shiftOffPressed, escPressed, escUnpressed;
        private Texture2D keyControlWDown, keyControlADown, keyControlSDown, keyControlDDown;
        private bool isWDown, isADown, isSDown, isDDown;
        private bool isHoldingPlaceable = false;
        private bool isPlaceableLocationValid = false;
        private bool showPlaceableTexture = true;
        private Vector2 placeableLocation;
        private Vector2 gridLocation;
        private List<CollectedTooltip> collectedTooltips;
        private Vector2 lastPlacedTile;

        private ClothingItem hat, shirt, outerwear, pants, socks, shoes, gloves, earrings, scarf, glasses, back, sailcloth, hair;
        private Item accessory1, accessory2, accessory3;
        private ClothedSprite playerSprite;

        private ItemStack[] chestInventory;
        private Color chestColor;
        private int seasonIndex, dayIndex, hourTensIndex, hourOnesIndex, minuteTensIndex, minuteOnesIndex;
        private Vector2 inventorySelectedPosition;
        private bool paused;

        private InterfaceState interfaceState;

        private RectangleF[] itemRectangles;
        private RectangleF[] chestRectangles;
        private Vector2 TOOLTIP_OFFSET = new Vector2(18, -8);

        private static Vector2 INVENTORY_POSITION = new Vector2(58.5f, 10); //if modified, have to manually edit garbage can location & clothing rectangles...
        private static RectangleF GLASSES_INVENTORY_RECT = new RectangleF(131.5f, 12, 18, 18);
        private static RectangleF BACK_INVENTORY_RECT = new RectangleF(131.5f, 31, 18, 18);
        private static RectangleF SAILCLOTH_INVENTORY_RECT = new RectangleF(131.5f, 50, 18, 18);
        private static RectangleF SCARF_INVENTORY_RECT = new RectangleF(150.5f, 12, 18, 18);
        private static RectangleF OUTERWEAR_INVENTORY_RECT = new RectangleF(150.5f, 31, 18, 18);
        private static RectangleF SOCKS_INVENTORY_RECT = new RectangleF(150.5f, 50, 18, 18);
        private static RectangleF HAT_INVENTORY_RECT = new RectangleF(169.5f, 12, 18, 18);
        private static RectangleF SHIRT_INVENTORY_RECT = new RectangleF(169.5f, 31, 18, 18);
        private static RectangleF PANTS_INVENTORY_RECT = new RectangleF(169.5f, 50, 18, 18);
        private static RectangleF EARRINGS_INVENTORY_RECT = new RectangleF(188.5f, 12, 18, 18);
        private static RectangleF GLOVES_INVENTORY_RECT = new RectangleF(188.5f, 31, 18, 18);
        private static RectangleF SHOES_INVENTORY_RECT = new RectangleF(188.5f, 50, 18, 18);
        private static RectangleF ACCESSORY1_INVENTORY_RECT = new RectangleF(207.5f, 12, 18, 18);
        private static RectangleF ACCESSORY2_INVENTORY_RECT = new RectangleF(207.5f, 31, 18, 18);
        private static RectangleF ACCESSORY3_INVENTORY_RECT = new RectangleF(207.5f, 50, 18, 18);
        private static Color CLOTHING_INDICATOR_COLOR = Color.Green * 0.5f;

        private static Vector2 HOTBAR_POSITION = new Vector2(58.5f, 157); //m
        private static Vector2 HOTBAR_SELECTED_POSITION_0 = new Vector2(64.5f, 158); //m
        private static Vector2 CHEST_INVENTORY_POSITION = INVENTORY_POSITION - new Vector2(0, 9);
        private static Vector2 INVENTORY_PLAYER_PREVIEW = new Vector2(48.5f, 0); //m
        private static Vector2 DATETIME_PANEL_POSITION = new Vector2(271, 2);
        private static Vector2 DATETIME_PANEL_DAYTEXT_OFFSET = new Vector2(1, 15);
        private static Vector2 DATETIME_PANEL_SEASONTEXT_OFFSET = new Vector2(2, 1);
        private static Vector2 DATETIME_PANEL_TIME_OFFSET = new Vector2(26, 16.5f);
        private static Vector2 DATETIME_PANEL_GOLD_OFFSET = new Vector2(35, 29);

        private string mouseLeftAction, mouseRightAction, mouseLeftShiftAction, mouseRightShiftAction;
        private static Vector2 MOUSE_CONTROL_POSITION = new Vector2(272, 155); //m
        private static Vector2 MOUSE_LEFT_TEXT_POSITION = new Vector2(273, 170); //m
        private static Vector2 MOUSE_RIGHT_TEXT_POSITION = new Vector2(302, 170); //m

        private string upAction, leftAction, rightAction, downAction;
        private static Vector2 KEY_CONTROL_POSITION = new Vector2(15, 152); //m
        private static Vector2 KEY_RIGHT_TEXT_POSITION = new Vector2(50, 154); //m
        private static Vector2 KEY_LEFT_TEXT_POSITION = new Vector2(12, 154); //m
        private static Vector2 KEY_UP_TEXT_POSITION = new Vector2(30, 148); //m
        private static Vector2 KEY_DOWN_TEXT_POSITION = new Vector2(30, 176); //m

        private static Vector2 KEY_CONTROL_POSITION_DIALOGUE = new Vector2(160, 60); //m
        private static Vector2 KEY_RIGHT_TEXT_POSITION_DIALOGUE = new Vector2(212, 69); //m
        private static Vector2 KEY_LEFT_TEXT_POSITION_DIALOGUE = new Vector2(135, 69); //m
        private static Vector2 KEY_UP_TEXT_POSITION_DIALOGUE = new Vector2(175, 52); //m
        private static Vector2 KEY_DOWN_TEXT_POSITION_DIALOGUE = new Vector2(175, 88); //m

        private static Vector2 MENU_CONTROL_POSITION = new Vector2(1, 59); //m
        private static Vector2 MENU_BAG_HOTKEY_POSITION = new Vector2(15, 63); //m
        private static Vector2 MENU_SCRAPBOOK_HOTKEY_POSITION = new Vector2(15, 76); //m
        private static Vector2 MENU_CRAFTING_HOTKEY_POSITION = new Vector2(15, 89); //m
        private static Vector2 MENU_SETTINGS_HOTKEY_POSITION = new Vector2(15, 102); //m
        private Texture2D menuControlsInventoryEnlarge, menuControlsInventoryDepressed;
        private Texture2D menuControlsScrapbookEnlarge, menuControlsScrapbookDepressed;
        private Texture2D menuControlsSettingsEnlarge, menuControlsSettingsDepressed;
        private Texture2D menuControlsCraftingEnlarge, menuControlsCraftingDepressed;
        private bool isMouseOverInventoryMC, isMouseOverScrapbookMC, isMouseOverCraftingMC, isMouseOverSettingsMC;
        private static Vector2 BACKGROUND_BLACK_OFFSET = new Vector2(-9, -9);

        private static Vector2 SHIFT_CONTROL_POSITION = new Vector2(295, 144); //m
        private static Vector2 SHIFT_TEXT_POSITION = new Vector2(251, 139); //m

        private static Vector2 ESC_CONTROL_POSITION = new Vector2(295, 129.5f); //m
        private static Vector2 ESC_TEXT_POSITION = new Vector2(281, 131.5f); //m

        private static Vector2 MENU_BUTTON_SIZE = new Vector2(11, 11);
        private static int MENU_DELTA_Y = 13;
        private RectangleF[] menuButtons;

        private static int HOTBAR_SELECTED_DELTA_X = 19;
        private static int INVENTORY_INITIAL_DELTA_X = 8;
        private static int INVENTORY_INITIAL_DELTA_Y = 65;
        private static int INVENTORY_ITEM_DELTA_X = 19;
        private static int INVENTORY_ITEM_DELTA_Y = 19;
        private ItemStack inventoryHeldItem;

        private ItemStack heldItem;

        private Vector2 playerPosition;
        private int displayGold;
        private static int ITEM_COLLECTED_TOOLTIP_Y_ADDITION = 8;
        private static float ITEM_COLLECTED_TOOLTIP_DELAY = 0.1f;
        private static float ITEM_COLLECTED_TOOLTIP_DELAY_FAST = 0.02f;
        private float timeSinceItemCollectedTooltipAdded = 0.0f;

        private Texture2D scrapbookBase;
        private Texture2D scrapbookTab1Active, scrapbookTab2Active, scrapbookTab3Active, scrapbookTab4Active, scrapbookTab5Active, scrapbookTab6Active, scrapbookTab7Active, scrapbookTab8Active, scrapbookTab9Active, scrapbookTab10Active, scrapbookTab11Active;
        private Texture2D scrapbookTab1Hover, scrapbookTab2Hover, scrapbookTab3Hover, scrapbookTab4Hover, scrapbookTab5Hover, scrapbookTab6Hover, scrapbookTab7Hover, scrapbookTab8Hover, scrapbookTab9Hover, scrapbookTab10Hover, scrapbookTab11Hover;
        private Texture2D scrapbookTab1ActiveHover, scrapbookTab2ActiveHover, scrapbookTab3ActiveHover, scrapbookTab4ActiveHover, scrapbookTab5ActiveHover, scrapbookTab6ActiveHover, scrapbookTab7ActiveHover, scrapbookTab8ActiveHover, scrapbookTab9ActiveHover, scrapbookTab10ActiveHover, scrapbookTab11ActiveHover;
        private Texture2D scrapbookTitleActive, scrapbookTitleHover, scrapbookTitleActiveHover;
        public static Vector2 SCRAPBOOK_POSITION = new Vector2(17, 6); //m
        private static Vector2 SCRAPBOOK_TAB1_POSITION = SCRAPBOOK_POSITION + new Vector2(-1, 24);
        private static Vector2 SCRAPBOOK_TAB2_POSITION = SCRAPBOOK_TAB1_POSITION + new Vector2(0, 10);
        private static Vector2 SCRAPBOOK_TAB3_POSITION = SCRAPBOOK_TAB2_POSITION + new Vector2(0, 10);
        private static Vector2 SCRAPBOOK_TAB4_POSITION = SCRAPBOOK_TAB3_POSITION + new Vector2(0, 10);
        private static Vector2 SCRAPBOOK_TAB5_POSITION = SCRAPBOOK_TAB4_POSITION + new Vector2(0, 10);
        private static Vector2 SCRAPBOOK_TAB6_POSITION = SCRAPBOOK_TAB5_POSITION + new Vector2(0, 10);
        private static Vector2 SCRAPBOOK_TAB7_POSITION = SCRAPBOOK_TAB6_POSITION + new Vector2(0, 10);
        private static Vector2 SCRAPBOOK_TAB8_POSITION = SCRAPBOOK_TAB7_POSITION + new Vector2(0, 10);
        private static Vector2 SCRAPBOOK_TAB9_POSITION = SCRAPBOOK_TAB8_POSITION + new Vector2(0, 10);
        private static Vector2 SCRAPBOOK_TAB10_POSITION = SCRAPBOOK_TAB9_POSITION + new Vector2(0, 10);
        private static Vector2 SCRAPBOOK_TAB11_POSITION = SCRAPBOOK_TAB10_POSITION + new Vector2(0, 10);
        private static Vector2 SCRAPBOOK_TITLE1_POSITION = SCRAPBOOK_POSITION + new Vector2(20, 26);
        private static Vector2 SCRAPBOOK_TITLE2_POSITION = SCRAPBOOK_TITLE1_POSITION + new Vector2(0, 11);
        private static Vector2 SCRAPBOOK_TITLE3_POSITION = SCRAPBOOK_TITLE2_POSITION + new Vector2(0, 11);
        private static Vector2 SCRAPBOOK_TITLE4_POSITION = SCRAPBOOK_TITLE3_POSITION + new Vector2(0, 11);
        private static Vector2 SCRAPBOOK_TITLE5_POSITION = SCRAPBOOK_TITLE4_POSITION + new Vector2(0, 11);
        private static Vector2 SCRAPBOOK_TITLE6_POSITION = SCRAPBOOK_TITLE5_POSITION + new Vector2(0, 11);
        private static Vector2 SCRAPBOOK_TITLE7_POSITION = SCRAPBOOK_TITLE6_POSITION + new Vector2(0, 11);
        private static Vector2 SCRAPBOOK_TITLE8_POSITION = SCRAPBOOK_TITLE7_POSITION + new Vector2(0, 11);
        private static Vector2 SCRAPBOOK_TITLE9_POSITION = SCRAPBOOK_TITLE8_POSITION + new Vector2(0, 11);
        private static Vector2 SCRAPBOOK_TITLE10_POSITION = SCRAPBOOK_TITLE9_POSITION + new Vector2(0, 11);
        private int scrapbookCurrentTab = 0;
        private int scrapbookCurrentPage = 0;
        private int scrapbookHoverTab = 0;
        private int scrapbookHoverPage = 0;
        private Dictionary<string, ScrapbookPage.Component> scrapbookDynamicComponents;
        private ScrapbookPage[] scrapbookPages;
        private RectangleF[] scrapbookTabs;
        private RectangleF[] scrapbookTitles;

        private static string SCRAPBOOK_CALENDAR_CURRENT_DAY = "calendarCurrentDay";

        private int selectedHotbarPosition;

        private DialogueNode currentDialogue = null;

        private Texture2D exitPrompt;
        private static Vector2 EXIT_PROMPT_POSITION = new Vector2(130, 25); //m
        private RectangleF exitPromptButton;

        private Texture2D settings, checkmark;
        private static Vector2 SETTINGS_POSITION = new Vector2(130, 15); //m
        private RectangleF[] settingsRectangles;

        private Vector2 targetTile;
        private Entity targetEntity;
        private Entity targetEntityLastFrame;
        private Texture2D reticle;
        private bool drawReticle;

        private Texture2D garbageCanOpen, garbageCanClosed;
        private static Vector2 GARBAGE_CAN_LOCATION = new Vector2(44, 118); //m 
        private RectangleF garbageCanRectangle;

        private static string selectedHotbarItemName;
        private static Vector2 SELECTED_HOTBAR_ITEM_NAME = new Vector2(160, 158); //m

        private AnimatedSprite dialogueBox, bounceArrow;
        private int currentDialogueNumChars;
        private static Vector2 DIALOGUE_BOX_LOCATION = new Vector2(65.5f, 15);
        private static Vector2 DIALOGUE_PORTRAIT_LOCATION = DIALOGUE_BOX_LOCATION + new Vector2(7, 8);
        private static Vector2 DIALOGUE_TEXT_LOCATION = DIALOGUE_BOX_LOCATION + new Vector2(50, 8);
        private static Vector2 DIALOGUE_BOUNCE_ARROW_LOCATION = DIALOGUE_BOX_LOCATION + new Vector2(187, 35);

        private static int TRANSITION_DELTA_Y = 500;
        private static int TRANSITION_DELTA_X = 725;
        private static float TRANSITION_ALPHA_SPEED = 2.7f;
        private Vector2 transitionPosition;
        private float transitionAlpha;

        private bool isMouseRightDown, isMouseLeftDown;
        private Texture2D mouseLeftDown, mouseRightDown;

        private static Texture2D workbench;
        private static Texture2D workbenchClothingTab, workbenchFloorWallTab, workbenchScaffoldingTab, workbenchFurnitureTab, workbenchMachineTab;
        private static Texture2D workbenchArrowLeft, workbenchArrowRight, workbenchCraftButton, workbenchCraftButtonEnlarged, workbenchQuestionMark, workbenchBlueprintDepression;

        private static Vector2 WORKBENCH_POSITION = new Vector2(53, 14); //m
        private static Vector2 WORKBENCH_MACHINE_TAB_POSITION = WORKBENCH_POSITION + new Vector2(0, 28);
        private static Vector2 WORKBENCH_SCAFFOLDING_TAB_POSITION = WORKBENCH_MACHINE_TAB_POSITION + new Vector2(0, 11);
        private static Vector2 WORKBENCH_FURNITURE_TAB_POSITION = WORKBENCH_SCAFFOLDING_TAB_POSITION + new Vector2(0, 11);
        private static Vector2 WORKBENCH_HOUSE_TAB_POSITION = WORKBENCH_FURNITURE_TAB_POSITION + new Vector2(0, 11);
        private static Vector2 WORKBENCH_CLOTHING_TAB_POSITION = WORKBENCH_HOUSE_TAB_POSITION + new Vector2(0, 11);
        private static Vector2 WORKBENCH_LEFT_ARROW_POSITION = WORKBENCH_POSITION + new Vector2(16, 92);
        private static Vector2 WORKBENCH_RIGHT_ARROW_POSITION = WORKBENCH_LEFT_ARROW_POSITION + new Vector2(91, 0);
        private static Vector2 WORKBENCH_CRAFT_BUTTON = WORKBENCH_POSITION + new Vector2(156, 98);
        private static Vector2 WORKBENCH_BLUEPRINT_POSITION = WORKBENCH_POSITION + new Vector2(19, 26);
        private static Vector2 WORKBENCH_PAGE_NAME_POSITION = WORKBENCH_POSITION + new Vector2(67, 103);
        private static Vector2 WORKBENCH_SELECTED_RECIPE_POSITION = WORKBENCH_POSITION + new Vector2(162, 18);
        private static Vector2 WORKBENCH_SELECTED_RECIPE_COMPONENT_1 = WORKBENCH_POSITION + new Vector2(132, 48);
        private static Vector2 WORKBENCH_SELECTED_RECIPE_COMPONENT_2 = WORKBENCH_SELECTED_RECIPE_COMPONENT_1 + new Vector2(20, 0);
        private static Vector2 WORKBENCH_SELECTED_RECIPE_COMPONENT_3 = WORKBENCH_SELECTED_RECIPE_COMPONENT_2 + new Vector2(20, 0);
        private static Vector2 WORKBENCH_SELECTED_RECIPE_COMPONENT_4 = WORKBENCH_SELECTED_RECIPE_COMPONENT_3 + new Vector2(20, 0);
        private static int blueprintDeltaX = 20, blueprintDeltaY = 20, haveBoxesDeltaY = 28;
        private bool hoveringLeftArrow, hoveringRightArrow, hoveringCraftButton;
        private int workbenchCurrentTab, workbenchCurrentPage;
        private RectangleF[] workbenchTabRectangles;
        private RectangleF[] workbenchBlueprintRectangles;
        private RectangleF workbenchLeftArrowRectangle, workbenchRightArrowRectangle, workbenchCraftButtonRectangle;
        private Vector2 workbenchInventorySelectedPosition;
        private List<Vector2> workbenchCraftablePosition;
        private GameState.CraftingRecipe[] currentRecipes;
        private int selectedRecipeSlot;
        private GameState.CraftingRecipe selectedRecipe;
        private int[] numMaterialsOfRecipe;

        private static Vector2 NOTIFICATION_POSITION = new Vector2(160, 18);
        private EntityPlayer.Notification currentNotification;

        private static Vector2 APPLIED_EFFECT_ANCHOR = new Vector2(2, 16);
        private static float APPLIED_EFFECT_DELTA_X = 13.0f;
        private List<RectangleF> effectRects;
        private List<EntityPlayer.TimedEffect> appliedEffects;

        private static Vector2 AREA_NAME_POSITION = new Vector2(8, 5);
        private string areaName, zoneName;

        private bool isHidden;

        private float HOVERING_INTERFACE_MAX_OPACITY = 0.8f;
        private float HOVERING_INTERFACE_OPACITY_SPEED = 4.8f;
        private float hoveringInterfaceOpacity = 0.0f;
        private HoveringInterface currentHoveringInterface;
        private Vector2 hoveringInterfacePosition = new Vector2(-100, -100);

        public GameplayInterface(Controller controller)
        {
            this.currentHoveringInterface = null;
            this.areaName = "";
            this.zoneName = "";
            this.isHidden = false;
            this.effectRects = new List<RectangleF>();
            this.currentNotification = null;
            this.selectedRecipeSlot = -1;
            this.numMaterialsOfRecipe = new int[4];
            for(int i = 0; i < 4; i++)
            {
                this.numMaterialsOfRecipe[i] = 0;
            }
            this.selectedRecipe = null;
            this.currentRecipes = new GameState.CraftingRecipe[15];
            this.workbenchInventorySelectedPosition = new Vector2(-1000, -1000);
            this.hoveringLeftArrow = false;
            this.hoveringRightArrow = false;
            this.workbenchCurrentPage = 0;
            this.workbenchCurrentTab = 0;
            this.transitionPosition = new Vector2(0, 0);
            this.transitionAlpha = 1.0f;
            this.displayGold = -1;
            this.workbenchCraftablePosition = new List<Vector2>();
            this.currentDialogueNumChars = 0;
            this.paused = false;
            this.controller = controller;
            this.selectedHotbarPosition = 0;
            this.inventorySelectedPosition = new Vector2(-100, -100);
            this.inventoryItems = new ItemStack[EntityPlayer.INVENTORY_SIZE];
            this.chestInventory = new ItemStack[PEntityChest.INVENTORY_SIZE];
            numbers = new Texture2D[10];
            numbersNoBorder = new Texture2D[10];
            this.seasonText = new Texture2D[4];
            this.dayText = new Texture2D[7];
            this.tooltipName = "";
            this.tooltipDescription = "";
            this.collectedTooltips = new List<CollectedTooltip>();
            this.menuButtons = new RectangleF[4];
            this.targetTile = new Vector2(-100, -100);
            this.drawReticle = true;
            this.lastPlacedTile = new Vector2(-100, -100);
            this.isMouseLeftDown = false;
            this.isMouseRightDown = false;
            this.isWDown = false;
            this.isADown = false;
            this.isDDown = false;
            this.isSDown = false;
            this.isMouseOverCraftingMC = false;
            this.isMouseOverInventoryMC = false;
            this.isMouseOverScrapbookMC = false;
            this.isMouseOverSettingsMC = false;
        }

        public bool IsItemHeld()
        {
            return inventoryHeldItem.GetItem() != ItemDict.NONE;
        }

        public void LoadContent(ContentManager content)
        {
            craftingButtonSolo = content.Load<Texture2D>(Paths.INTERFACE_CRAFTING_BUTTON_SOLO);
            inventoryHeldItem = new ItemStack(ItemDict.NONE, 0);
            inventory_selected = content.Load<Texture2D>(Paths.INTERFACE_INVENTORY_SELECTED);

            TOOLTIP_9SLICE = new NineSlice(content.Load<Texture2D>(Paths.INTERFACE_9SLICE_TOPLEFT_TOOLTIP), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_TOPRIGHT_TOOLTIP), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_TOPCENTER_TOOLTIP),
                content.Load<Texture2D>(Paths.INTERFACE_9SLICE_MIDDLELEFT_TOOLTIP), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_MIDDLERIGHT_TOOLTIP), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_MIDDLECENTER_TOOLTIP),
                content.Load<Texture2D>(Paths.INTERFACE_9SLICE_BOTTOMLEFT_TOOLTIP), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_BOTTOMRIGHT_TOOLTIP), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_BOTTOMCENTER_TOOLTIP));
            DIALOGUE_9SLICE = new NineSlice(content.Load<Texture2D>(Paths.INTERFACE_9SLICE_TOPLEFT_DIALOGUE), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_TOPRIGHT_DIALOGUE), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_TOPCENTER_DIALOGUE),
                content.Load<Texture2D>(Paths.INTERFACE_9SLICE_MIDDLELEFT_DIALOGUE), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_MIDDLERIGHT_DIALOGUE), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_MIDDLECENTER_DIALOGUE),
                content.Load<Texture2D>(Paths.INTERFACE_9SLICE_BOTTOMLEFT_DIALOGUE), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_BOTTOMRIGHT_DIALOGUE), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_BOTTOMCENTER_DIALOGUE));
            THOUGHTS_9SLICE = new NineSlice(content.Load<Texture2D>(Paths.INTERFACE_9SLICE_TOPLEFT_THOUGHTS), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_TOPRIGHT_THOUGHTS), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_TOPCENTER_THOUGHTS),
                            content.Load<Texture2D>(Paths.INTERFACE_9SLICE_MIDDLELEFT_THOUGHTS), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_MIDDLERIGHT_THOUGHTS), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_MIDDLECENTER_THOUGHTS),
                            content.Load<Texture2D>(Paths.INTERFACE_9SLICE_BOTTOMLEFT_THOUGHTS), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_BOTTOMRIGHT_THOUGHTS), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_BOTTOMCENTER_THOUGHTS));
            INTERFACE_9SLICE = new NineSlice(content.Load<Texture2D>(Paths.INTERFACE_9SLICE_TOPLEFT_INTERFACE), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_TOPRIGHT_INTERFACE), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_TOPCENTER_INTERFACE),
                            content.Load<Texture2D>(Paths.INTERFACE_9SLICE_MIDDLELEFT_INTERFACE), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_MIDDLERIGHT_INTERFACE), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_MIDDLECENTER_INTERFACE),
                            content.Load<Texture2D>(Paths.INTERFACE_9SLICE_BOTTOMLEFT_INTERFACE), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_BOTTOMRIGHT_INTERFACE), content.Load<Texture2D>(Paths.INTERFACE_9SLICE_BOTTOMCENTER_INTERFACE));

            hotbar = content.Load<Texture2D>(Paths.INTERFACE_HOTBAR);
            hotbar_selected = content.Load<Texture2D>(Paths.INTERFACE_HOTBAR_SELECTED);
            numbers[0] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_ZERO);
            numbers[1] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_ONE);
            numbers[2] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_TWO);
            numbers[3] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_THREE);
            numbers[4] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_FOUR);
            numbers[5] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_FIVE);
            numbers[6] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_SIX);
            numbers[7] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_SEVEN);
            numbers[8] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_EIGHT);
            numbers[9] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_NINE);
            numbersNoBorder[0] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_ZERO_NO_BORDER);
            numbersNoBorder[1] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_ONE_NO_BORDER);
            numbersNoBorder[2] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_TWO_NO_BORDER);
            numbersNoBorder[3] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_THREE_NO_BORDER);
            numbersNoBorder[4] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_FOUR_NO_BORDER);
            numbersNoBorder[5] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_FIVE_NO_BORDER);
            numbersNoBorder[6] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_SIX_NO_BORDER);
            numbersNoBorder[7] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_SEVEN_NO_BORDER);
            numbersNoBorder[8] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_EIGHT_NO_BORDER);
            numbersNoBorder[9] = content.Load<Texture2D>(Paths.INTERFACE_NUMBER_NINE_NO_BORDER);
            itemBox = content.Load<Texture2D>(Paths.INTERFACE_ITEM_BOX);
            hoveringItemBox = content.Load<Texture2D>(Paths.INTERFACE_HOVERING_ITEM_BOX);
            inventory = content.Load<Texture2D>(Paths.INTERFACE_INVENTORY);
            black_background = content.Load<Texture2D>(Paths.INTERFACE_BACKGROUND_BLACK);
            black_box = content.Load<Texture2D>(Paths.INTERFACE_BLACK_BOX);
            grid = content.Load<Texture2D>(Paths.INTERFACE_GRID);

            dateTimePanel = content.Load<Texture2D>(Paths.INTERFACE_DATETIME_PANEL);
            seasonText[0] = content.Load<Texture2D>(Paths.INTERFACE_TEXT_SPRING);
            seasonText[1] = content.Load<Texture2D>(Paths.INTERFACE_TEXT_SUMMER);
            seasonText[2] = content.Load<Texture2D>(Paths.INTERFACE_TEXT_AUTUMN);
            seasonText[3] = content.Load<Texture2D>(Paths.INTERFACE_TEXT_WINTER);
            dayText[0] = content.Load<Texture2D>(Paths.INTERFACE_TEXT_MON);
            dayText[1] = content.Load<Texture2D>(Paths.INTERFACE_TEXT_TUE);
            dayText[2] = content.Load<Texture2D>(Paths.INTERFACE_TEXT_WED);
            dayText[3] = content.Load<Texture2D>(Paths.INTERFACE_TEXT_THU);
            dayText[4] = content.Load<Texture2D>(Paths.INTERFACE_TEXT_FRI);
            dayText[5] = content.Load<Texture2D>(Paths.INTERFACE_TEXT_SAT);
            dayText[6] = content.Load<Texture2D>(Paths.INTERFACE_TEXT_SUN);

            mouseControl = content.Load<Texture2D>(Paths.INTERFACE_MOUSE_CONTROLS);
            keyControl = content.Load<Texture2D>(Paths.INTERFACE_KEY_CONTROLS);
            keyControlWDown = content.Load<Texture2D>(Paths.INTERFACE_KEY_CONTROLS_W_DOWN);
            keyControlADown = content.Load<Texture2D>(Paths.INTERFACE_KEY_CONTROLS_A_DOWN);
            keyControlSDown = content.Load<Texture2D>(Paths.INTERFACE_KEY_CONTROLS_S_DOWN);
            keyControlDDown = content.Load<Texture2D>(Paths.INTERFACE_KEY_CONTROLS_D_DOWN);
            menuControl = content.Load<Texture2D>(Paths.INTERFACE_MENU_CONTROLS);

            shiftOnPressed = content.Load<Texture2D>(Paths.INTERFACE_SHIFT_ON_PRESSED);
            shiftOnUnpressed = content.Load<Texture2D>(Paths.INTERFACE_SHIFT_ON_UNPRESSED);
            shiftOffPressed = content.Load<Texture2D>(Paths.INTERFACE_SHIFT_OFF_PRESSED);
            shiftOffUnpressed = content.Load<Texture2D>(Paths.INTERFACE_SHIFT_OFF_UNPRESSED);

            escPressed = content.Load<Texture2D>(Paths.INTERFACE_ESC_PRESSED);
            escUnpressed = content.Load<Texture2D>(Paths.INTERFACE_ESC_UNPRESSED);

            chest_inventory = content.Load<Texture2D>(Paths.INTERFACE_CHEST);
            chest_inventory_greyscale = content.Load<Texture2D>(Paths.INTERFACE_CHEST_GREYSCALE);

            //math out itemRectangles...
            itemRectangles = new RectangleF[EntityPlayer.INVENTORY_SIZE];

            for (int i = 0; i < GameplayInterface.HOTBAR_LENGTH; i++)
            {
                Vector2 itemPosition = new Vector2(HOTBAR_POSITION.X + 7 + (i * 19), HOTBAR_POSITION.Y + 2);
                itemRectangles[i] = new RectangleF(itemPosition.X, itemPosition.Y, 18, 18);
            }

            float startingX = INVENTORY_POSITION.X + INVENTORY_INITIAL_DELTA_X - 1;
            Vector2 location = new Vector2(startingX, INVENTORY_POSITION.Y + INVENTORY_INITIAL_DELTA_Y - 1);

            for (int i = 10; i < EntityPlayer.INVENTORY_SIZE; i++)
            {
                itemRectangles[i] = new RectangleF(location.X, location.Y, 18, 18);

                location.X += INVENTORY_ITEM_DELTA_X;
                if (i != 10 && (i + 1) % 10 == 0)
                {
                    location.Y += INVENTORY_ITEM_DELTA_Y;
                    location.X = startingX;
                }
            }

            //math out chest rectangles
            chestRectangles = new RectangleF[PEntityChest.INVENTORY_SIZE];
            startingX = CHEST_INVENTORY_POSITION.X + 7;
            location = new Vector2(startingX, CHEST_INVENTORY_POSITION.Y + 11);
            for(int i = 0; i < PEntityChest.INVENTORY_SIZE; i++)
            {
                chestRectangles[i] = new RectangleF(location.X, location.Y, 18, 18);
                location.X += INVENTORY_ITEM_DELTA_X;
                if(i != 0 && (i+1) % 10 == 0)
                {
                    location.Y += INVENTORY_ITEM_DELTA_Y;
                    location.X = startingX;
                }
            }

            InitializeScrapbook(content);

            exitPrompt = content.Load<Texture2D>(Paths.INTERFACE_EXIT_PROMPT);
            exitPromptButton = new RectangleF(EXIT_PROMPT_POSITION + new Vector2(30, 19), new Vector2(22, 10));

            settings = content.Load<Texture2D>(Paths.INTERFACE_SETTINGS);
            checkmark = content.Load<Texture2D>(Paths.INTERFACE_CHECKMARK);
            settingsRectangles = new RectangleF[3];
            settingsRectangles[0] = new RectangleF(SETTINGS_POSITION + new Vector2(6, 16), new Vector2(8, 8));
            settingsRectangles[1] = new RectangleF(new Vector2(settingsRectangles[0].X, settingsRectangles[0].Y + 10), new Vector2(8, 8));
            settingsRectangles[2] = new RectangleF(new Vector2(settingsRectangles[1].X, settingsRectangles[1].Y + 10), new Vector2(8, 8));

            reticle = content.Load<Texture2D>(Paths.INTERFACE_RETICLE);

            garbageCanClosed = content.Load<Texture2D>(Paths.INTERFACE_GARBAGE_CAN_CLOSED);
            garbageCanOpen = content.Load<Texture2D>(Paths.INTERFACE_GARBAGE_CAN_OPEN);
            garbageCanRectangle = new RectangleF(GARBAGE_CAN_LOCATION, new Vector2(garbageCanClosed.Width, garbageCanClosed.Height));

            float[] frameLengths = Util.CreateAndFillArray(4, 0.05f);
            dialogueBox = new AnimatedSprite(content.Load<Texture2D>(Paths.INTERFACE_DIALOGUE_BOX), 4, 1, 4, frameLengths);
            dialogueBox.AddLoop("anim", 0, 3, false);
            dialogueBox.SetLoop("anim");

            frameLengths = Util.CreateAndFillArray(5, 0.1f);
            frameLengths[0] = 0.5f;
            bounceArrow = new AnimatedSprite(content.Load<Texture2D>(Paths.INTERFACE_BOUNCE_ARROW), 5, 1, 5, frameLengths);
            bounceArrow.AddLoop("anim", 0, 4);
            bounceArrow.SetLoop("anim");

            mouseLeftDown = content.Load<Texture2D>(Paths.INTERFACE_MOUSE_LEFT_DOWN);
            mouseRightDown = content.Load<Texture2D>(Paths.INTERFACE_MOUSE_RIGHT_DOWN);

            menuControlsCraftingDepressed = content.Load<Texture2D>(Paths.INTERFACE_MENU_CONTROLS_CRAFTING_DEPRESSED);
            menuControlsCraftingEnlarge = content.Load<Texture2D>(Paths.INTERFACE_MENU_CONTROLS_CRAFTING_ENLARGE);
            menuControlsSettingsDepressed = content.Load<Texture2D>(Paths.INTERFACE_MENU_CONTROLS_SETTINGS_DEPRESSED);
            menuControlsSettingsEnlarge = content.Load<Texture2D>(Paths.INTERFACE_MENU_CONTROLS_SETTINGS_ENLARGE);
            menuControlsInventoryDepressed = content.Load<Texture2D>(Paths.INTERFACE_MENU_CONTROLS_INVENTORY_DEPRESSED);
            menuControlsInventoryEnlarge = content.Load<Texture2D>(Paths.INTERFACE_MENU_CONTROLS_INVENTORY_ENLARGE);
            menuControlsScrapbookDepressed = content.Load<Texture2D>(Paths.INTERFACE_MENU_CONTROLS_SCRAPBOOK_DEPRESSED);
            menuControlsScrapbookEnlarge = content.Load<Texture2D>(Paths.INTERFACE_MENU_CONTROLS_SCRAPBOOK_ENLARGE);

            /*
             * 
             *         private static Texture2D workbench;
        private static Texture2D workbenchClothingTab, workbenchClothingTab2, workbenchHouseTab, workbenchScaffoldingTab, workbenchFurnitureTab;
        private static Texture2D workbenchArrowLeft, workbenchArrowRight, workbenchCraftButton;
             */
            workbench = content.Load<Texture2D>(Paths.INTERFACE_CRAFTING);
            workbenchClothingTab = content.Load<Texture2D>(Paths.INTERFACE_CRAFTING_TAB_CLOTHING);
            workbenchFloorWallTab = content.Load<Texture2D>(Paths.INTERFACE_CRAFTING_TAB_FLOOR_WALL);
            workbenchScaffoldingTab = content.Load<Texture2D>(Paths.INTERFACE_CRAFTING_TAB_SCAFFOLDING);
            workbenchFurnitureTab = content.Load<Texture2D>(Paths.INTERFACE_CRAFTING_TAB_FURNITURE);
            workbenchMachineTab = content.Load<Texture2D>(Paths.INTERFACE_CRAFTING_TAB_MACHINE);
            workbenchQuestionMark = content.Load<Texture2D>(Paths.INTERFACE_CRAFTING_QUESTION_MARK);
            workbenchBlueprintDepression = content.Load<Texture2D>(Paths.INTERFACE_CRAFTING_BLUEPRINT_DEPRESSION);

            workbenchArrowLeft = content.Load<Texture2D>(Paths.INTERFACE_CRAFTING_ARROW_LEFT);
            workbenchArrowRight = content.Load<Texture2D>(Paths.INTERFACE_CRAFTING_ARROW_RIGHT);
            workbenchCraftButton = content.Load<Texture2D>(Paths.INTERFACE_CRAFTING_CRAFT_BUTTON);
            workbenchCraftButtonEnlarged = content.Load<Texture2D>(Paths.INTERFACE_CRAFTING_CRAFT_BUTTON_ENLARGED);

            workbenchTabRectangles = new RectangleF[5];
            workbenchTabRectangles[0] = new RectangleF(WORKBENCH_MACHINE_TAB_POSITION, new Size2(10, 9));
            workbenchTabRectangles[1] = new RectangleF(WORKBENCH_SCAFFOLDING_TAB_POSITION, new Size2(10, 9));
            workbenchTabRectangles[2] = new RectangleF(WORKBENCH_FURNITURE_TAB_POSITION, new Size2(10, 9));
            workbenchTabRectangles[3] = new RectangleF(WORKBENCH_HOUSE_TAB_POSITION, new Size2(10, 9));
            workbenchTabRectangles[4] = new RectangleF(WORKBENCH_CLOTHING_TAB_POSITION, new Size2(10, 9));

            workbenchBlueprintRectangles = new RectangleF[15];
            workbenchBlueprintRectangles[0] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION, new Size2(18, 18));
            workbenchBlueprintRectangles[1] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(blueprintDeltaX, 0), new Size2(18, 18));
            workbenchBlueprintRectangles[2] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(blueprintDeltaX*2, 0), new Size2(18, 18));
            workbenchBlueprintRectangles[3] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(blueprintDeltaX*3, 0), new Size2(18, 18));
            workbenchBlueprintRectangles[4] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(blueprintDeltaX*4, 0), new Size2(18, 18));
            workbenchBlueprintRectangles[5] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(0, blueprintDeltaY), new Size2(18, 18));
            workbenchBlueprintRectangles[6] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(blueprintDeltaX, blueprintDeltaY), new Size2(18, 18));
            workbenchBlueprintRectangles[7] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(blueprintDeltaX * 2, blueprintDeltaY), new Size2(18, 18));
            workbenchBlueprintRectangles[8] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(blueprintDeltaX * 3, blueprintDeltaY), new Size2(18, 18));
            workbenchBlueprintRectangles[9] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(blueprintDeltaX * 4, blueprintDeltaY), new Size2(18, 18));
            workbenchBlueprintRectangles[10] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(0, blueprintDeltaY * 2), new Size2(18, 18));
            workbenchBlueprintRectangles[11] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(blueprintDeltaX, blueprintDeltaY * 2), new Size2(18, 18));
            workbenchBlueprintRectangles[12] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(blueprintDeltaX * 2, blueprintDeltaY * 2), new Size2(18, 18));
            workbenchBlueprintRectangles[13] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(blueprintDeltaX * 3, blueprintDeltaY * 2), new Size2(18, 18));
            workbenchBlueprintRectangles[14] = new RectangleF(WORKBENCH_BLUEPRINT_POSITION + new Vector2(blueprintDeltaX * 4, blueprintDeltaY * 2), new Size2(18, 18));

            workbenchLeftArrowRectangle = new RectangleF(WORKBENCH_LEFT_ARROW_POSITION, new Size2(13, 11));
            workbenchRightArrowRectangle = new RectangleF(WORKBENCH_RIGHT_ARROW_POSITION, new Size2(13, 11));

            workbenchCraftButtonRectangle = new RectangleF(WORKBENCH_CRAFT_BUTTON, new Size2(28, 12));

        }

        public bool IsTransitionReady()
        {
            if(interfaceState == InterfaceState.TRANSITION_TO_DOWN)
            {
                return transitionPosition.Y >= 0;
            }
            else if (interfaceState == InterfaceState.TRANSITION_TO_UP)
            {
                return transitionPosition.Y <= 0;
            }
            else if (interfaceState == InterfaceState.TRANSITION_TO_LEFT)
            {
                return transitionPosition.X <= 0;
            }
            else if (interfaceState == InterfaceState.TRANSITION_TO_RIGHT)
            {
                return transitionPosition.X >= 0;
            } else if (interfaceState == InterfaceState.TRANSITION_FADE_TO_BLACK)
            {
                return transitionAlpha >= 1.0f;
            } else if (interfaceState == InterfaceState.TRANSITION_FADE_IN)
            {
                return transitionAlpha <= 0.0f;
            }
            return true;
        }

        private void InitializeScrapbook(ContentManager content)
        {
            //scrapbook stuff
            scrapbookBase = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_BASE);
            scrapbookTitleActive = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACTIVE);
            scrapbookTitleHover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_HOVER);
            scrapbookTitleActiveHover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACTIVE_HOVER);
            scrapbookTab1Active = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB1_ACTIVE);
            scrapbookTab2Active = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB2_ACTIVE);
            scrapbookTab3Active = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB3_ACTIVE);
            scrapbookTab4Active = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB4_ACTIVE);
            scrapbookTab5Active = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB5_ACTIVE);
            scrapbookTab6Active = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB6_ACTIVE);
            scrapbookTab7Active = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB7_ACTIVE);
            scrapbookTab8Active = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB8_ACTIVE);
            scrapbookTab9Active = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB9_ACTIVE);
            scrapbookTab10Active = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB10_ACTIVE);
            scrapbookTab11Active = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB11_ACTIVE);
            scrapbookTab1Hover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB1_HOVER);
            scrapbookTab2Hover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB2_HOVER);
            scrapbookTab3Hover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB3_HOVER);
            scrapbookTab4Hover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB4_HOVER);
            scrapbookTab5Hover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB5_HOVER);
            scrapbookTab6Hover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB6_HOVER);
            scrapbookTab7Hover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB7_HOVER);
            scrapbookTab8Hover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB8_HOVER);
            scrapbookTab9Hover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB9_HOVER);
            scrapbookTab10Hover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB10_HOVER);
            scrapbookTab11Hover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB11_HOVER);
            scrapbookTab1ActiveHover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB1_HOVERACTIVE);
            scrapbookTab2ActiveHover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB2_HOVERACTIVE);
            scrapbookTab3ActiveHover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB3_HOVERACTIVE);
            scrapbookTab4ActiveHover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB4_HOVERACTIVE);
            scrapbookTab5ActiveHover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB5_HOVERACTIVE);
            scrapbookTab6ActiveHover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB6_HOVERACTIVE);
            scrapbookTab7ActiveHover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB7_HOVERACTIVE);
            scrapbookTab8ActiveHover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB8_HOVERACTIVE);
            scrapbookTab9ActiveHover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB9_HOVERACTIVE);
            scrapbookTab10ActiveHover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB10_HOVERACTIVE);
            scrapbookTab11ActiveHover = content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_TAB11_HOVERACTIVE);

            //scrapbook tab rectangles
            scrapbookTabs = new RectangleF[NUM_SCRAPBOOK_TABS];
            scrapbookTabs[0] = new RectangleF(SCRAPBOOK_TAB1_POSITION, new Vector2(scrapbookTab1Active.Width, scrapbookTab1Active.Height));
            scrapbookTabs[1] = new RectangleF(SCRAPBOOK_TAB2_POSITION, new Vector2(scrapbookTab1Active.Width, scrapbookTab1Active.Height));
            scrapbookTabs[2] = new RectangleF(SCRAPBOOK_TAB3_POSITION, new Vector2(scrapbookTab1Active.Width, scrapbookTab1Active.Height));
            scrapbookTabs[3] = new RectangleF(SCRAPBOOK_TAB4_POSITION, new Vector2(scrapbookTab1Active.Width, scrapbookTab1Active.Height));
            scrapbookTabs[4] = new RectangleF(SCRAPBOOK_TAB5_POSITION, new Vector2(scrapbookTab1Active.Width, scrapbookTab1Active.Height));
            scrapbookTabs[5] = new RectangleF(SCRAPBOOK_TAB6_POSITION, new Vector2(scrapbookTab1Active.Width, scrapbookTab1Active.Height));
            scrapbookTabs[6] = new RectangleF(SCRAPBOOK_TAB7_POSITION, new Vector2(scrapbookTab1Active.Width, scrapbookTab1Active.Height));
            scrapbookTabs[7] = new RectangleF(SCRAPBOOK_TAB8_POSITION, new Vector2(scrapbookTab1Active.Width, scrapbookTab1Active.Height));
            scrapbookTabs[8] = new RectangleF(SCRAPBOOK_TAB9_POSITION, new Vector2(scrapbookTab1Active.Width, scrapbookTab1Active.Height));
            scrapbookTabs[9] = new RectangleF(SCRAPBOOK_TAB10_POSITION, new Vector2(scrapbookTab1Active.Width, scrapbookTab1Active.Height));
            scrapbookTabs[10] = new RectangleF(SCRAPBOOK_TAB11_POSITION, new Vector2(scrapbookTab1Active.Width, scrapbookTab1Active.Height));

            //scrapbook content rectangles
            scrapbookTitles = new RectangleF[NUM_SCRAPBOOK_PAGES];
            scrapbookTitles[0] = new RectangleF(SCRAPBOOK_TITLE1_POSITION, new Vector2(scrapbookTitleActive.Width, scrapbookTitleActive.Height));
            scrapbookTitles[1] = new RectangleF(SCRAPBOOK_TITLE2_POSITION, new Vector2(scrapbookTitleActive.Width, scrapbookTitleActive.Height));
            scrapbookTitles[2] = new RectangleF(SCRAPBOOK_TITLE3_POSITION, new Vector2(scrapbookTitleActive.Width, scrapbookTitleActive.Height));
            scrapbookTitles[3] = new RectangleF(SCRAPBOOK_TITLE4_POSITION, new Vector2(scrapbookTitleActive.Width, scrapbookTitleActive.Height));
            scrapbookTitles[4] = new RectangleF(SCRAPBOOK_TITLE5_POSITION, new Vector2(scrapbookTitleActive.Width, scrapbookTitleActive.Height));
            scrapbookTitles[5] = new RectangleF(SCRAPBOOK_TITLE6_POSITION, new Vector2(scrapbookTitleActive.Width, scrapbookTitleActive.Height));
            scrapbookTitles[6] = new RectangleF(SCRAPBOOK_TITLE7_POSITION, new Vector2(scrapbookTitleActive.Width, scrapbookTitleActive.Height));
            scrapbookTitles[7] = new RectangleF(SCRAPBOOK_TITLE8_POSITION, new Vector2(scrapbookTitleActive.Width, scrapbookTitleActive.Height));
            scrapbookTitles[8] = new RectangleF(SCRAPBOOK_TITLE9_POSITION, new Vector2(scrapbookTitleActive.Width, scrapbookTitleActive.Height));
            scrapbookTitles[9] = new RectangleF(SCRAPBOOK_TITLE10_POSITION, new Vector2(scrapbookTitleActive.Width, scrapbookTitleActive.Height));

            //TODODO
            scrapbookDynamicComponents = new Dictionary<string, ScrapbookPage.Component>();
            scrapbookPages = new ScrapbookPage[NUM_SCRAPBOOK_PAGES];

            Vector2 cookingRecipe1Pos = new Vector2(14, 17);
            Vector2 cookingRecipe2Pos = cookingRecipe1Pos + new Vector2(0, 19);
            Vector2 cookingRecipe3Pos = cookingRecipe2Pos + new Vector2(0, 19);
            Vector2 cookingRecipe4Pos = cookingRecipe3Pos + new Vector2(0, 19);
            Vector2 cookingRecipe5Pos = cookingRecipe4Pos + new Vector2(0, 19);
            Vector2 cookingRecipe6Pos = cookingRecipe5Pos + new Vector2(0, 19);
            Vector2 craftingRecipe1Pos = new Vector2(10, 17);
            Vector2 craftingRecipe2Pos = craftingRecipe1Pos + new Vector2(0, 19);
            Vector2 craftingRecipe3Pos = craftingRecipe2Pos + new Vector2(0, 19);
            Vector2 craftingRecipe4Pos = craftingRecipe3Pos + new Vector2(0, 19);
            Vector2 craftingRecipe5Pos = craftingRecipe4Pos + new Vector2(0, 19);
            Vector2 craftingRecipe6Pos = craftingRecipe5Pos + new Vector2(0, 19);

            ScrapbookPage.ImageComponent currentDay = new ScrapbookPage.ImageComponent(new Vector2(100, 100), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_CALENDAR_CURRENT_DAY), Color.White);
            scrapbookDynamicComponents[SCRAPBOOK_CALENDAR_CURRENT_DAY] = currentDay;
            scrapbookPages[0] = new ScrapbookPage("Calendar", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_CALENDAR), Color.White), currentDay);
            scrapbookPages[1] = new ScrapbookPage("Maps");
            scrapbookPages[2] = new ScrapbookPage("Relationships", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_RELATIONSHIPS1), Color.White));
            scrapbookPages[3] = new ScrapbookPage("Relationships 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_RELATIONSHIPS2), Color.White));
            scrapbookPages[4] = new ScrapbookPage("Farming Intro", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_FARMING_INTRO), Color.White)); 
            scrapbookPages[5] = new ScrapbookPage("Farming Animals", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_FARMING_ANIMALS), Color.White));
            scrapbookPages[6] = new ScrapbookPage("Farming Compost", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_FARMING_COMPOST), Color.White));
            scrapbookPages[7] = new ScrapbookPage("Farming Seeds", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_FARMING_SEEDS), Color.White));
            scrapbookPages[8] = new ScrapbookPage("Farming Trees", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_FARMING_TREES), Color.White));
            scrapbookPages[9] = new ScrapbookPage("Farming Mastery");
            scrapbookPages[10] = new ScrapbookPage("Farming Legends", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_FARMING_LEGENDS), Color.White));
            scrapbookPages[11] = new ScrapbookPage("Workbench", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_WORKBENCH), Color.White));
            scrapbookPages[12] = new ScrapbookPage("Trading");
            scrapbookPages[13] = new ScrapbookPage("Dating");
            scrapbookPages[14] = new ScrapbookPage("Furnace");
            scrapbookPages[15] = new ScrapbookPage("Anvil");
            scrapbookPages[16] = new ScrapbookPage("Compressor");
            scrapbookPages[17] = new ScrapbookPage("Painter's Press");
            scrapbookPages[18] = new ScrapbookPage("Glassblower");
            scrapbookPages[19] = new ScrapbookPage("Pottery Wheel");
            scrapbookPages[20] = new ScrapbookPage("Perfumery");
            scrapbookPages[21] = new ScrapbookPage("Kegs");
            scrapbookPages[22] = new ScrapbookPage("Beehive 1");
            scrapbookPages[23] = new ScrapbookPage("Beehive 2");
            scrapbookPages[24] = new ScrapbookPage("Birdhouse 1");
            scrapbookPages[25] = new ScrapbookPage("Birdhouse 2");
            scrapbookPages[26] = new ScrapbookPage("Fishing Seasons");
            scrapbookPages[27] = new ScrapbookPage("Fishing Legends");
            scrapbookPages[28] = new ScrapbookPage("Fishing Beyond");
            scrapbookPages[29] = new ScrapbookPage("Fashion");
            scrapbookPages[30] = new ScrapbookPage("Decor");
            scrapbookPages[31] = new ScrapbookPage("Cookbook Forage 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_FORAGE1), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.MOUNTAIN_BREAD), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.CRISPY_GRASSHOPPER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.REJUVENATION_TEA), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.CHICKWEED_BLEND), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetCookingRecipeForResult(ItemDict.HOMESTYLE_JELLY), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetCookingRecipeForResult(ItemDict.SEAFOOD_BASKET), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING));
            scrapbookPages[32] = new ScrapbookPage("Cookbook Forage 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_FORAGE2), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.ELDERBERRY_TART), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.AUTUMN_MASH), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.WILD_POPCORN), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.CAMPFIRE_COFFEE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetCookingRecipeForResult(ItemDict.DARK_TEA), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetCookingRecipeForResult(ItemDict.LICHEN_JUICE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING));
            scrapbookPages[33] = new ScrapbookPage("Cookbook Forage 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_FORAGE3), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.FRIED_FISH), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.FRIED_OYSTERS), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.BLIND_DINNER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.SWEET_COCO_TREAT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetCookingRecipeForResult(ItemDict.SARDINE_SNACK), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetCookingRecipeForResult(ItemDict.SURVIVORS_SURPRISE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING));
            scrapbookPages[34] = new ScrapbookPage("Cookbook Spring", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_SPRING), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.SPRING_PIZZA), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.BOAR_STEW), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.VEGGIE_CHIPS), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.BAKED_POTATO), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetCookingRecipeForResult(ItemDict.STRAWBERRY_SALAD), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING));
            scrapbookPages[35] = new ScrapbookPage("Cookbook Summer", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_SUMMER), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.FRESH_SALAD), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.MEATY_PIZZA), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.STEWED_VEGGIES), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.WATERMELON_ICE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING));
            scrapbookPages[36] = new ScrapbookPage("Cookbook Autumn", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_AUTUMN), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.SEAFOOD_PAELLA), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.DWARVEN_STEW), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.ROASTED_PUMPKIN), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.WRAPPED_CABBAGE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetCookingRecipeForResult(ItemDict.VEGGIE_SIDE_ROAST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING));
            scrapbookPages[37] = new ScrapbookPage("Cookbook Four", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_FOUR), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.SEASONAL_PIPERADE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.COCONUT_BOAR), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.POTATO_AND_BEET_FRIES), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.PICKLED_BEET_EGGS), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetCookingRecipeForResult(ItemDict.SUPER_JUICE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING));
            scrapbookPages[38] = new ScrapbookPage("Cookbook Ice", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_ICE), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.MINTY_MELT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.VANILLA_ICECREAM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.BERRY_MILKSHAKE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.BANANA_SUNDAE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetCookingRecipeForResult(ItemDict.MINT_CHOCO_BAR), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING));
            scrapbookPages[39] = new ScrapbookPage("Cookbook Morning", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_BREAKFAST), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.FRIED_EGG), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.EGG_SCRAMBLE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.SPICY_BACON), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.BLUEBERRY_PANCAKES), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetCookingRecipeForResult(ItemDict.APPLE_MUFFIN), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetCookingRecipeForResult(ItemDict.BREAKFAST_POTATOES), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING));
            scrapbookPages[40] = new ScrapbookPage("Cookbook Supper 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_SUPPER1), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.STUFFED_FLOUNDER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.FRIED_CATFISH), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.GRILLED_SALMON), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.BAKED_SNAPPER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetCookingRecipeForResult(ItemDict.SWORDFISH_POT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetCookingRecipeForResult(ItemDict.CLAM_LINGUINI), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING));
            scrapbookPages[41] = new ScrapbookPage("Cookbook Supper 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_SUPPER2), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.HONEY_STIR_FRY), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.BUTTERED_ROLLS), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.COLESLAW), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.SAVORY_ROAST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetCookingRecipeForResult(ItemDict.CHERRY_CHEESECAKE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetCookingRecipeForResult(ItemDict.LEMON_SHORTCAKE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING));
            scrapbookPages[42] = new ScrapbookPage("Cookbook Eastern", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_EASTERN), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.MOUNTAIN_TERIYAKI), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.SEARED_TUNA), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.SUSHI_ROLL), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.SEAWEED_SNACK), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetCookingRecipeForResult(ItemDict.MUSHROOM_STIR_FRY), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetCookingRecipeForResult(ItemDict.LETHAL_SASHIMI), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING));
            scrapbookPages[43] = new ScrapbookPage("Cookbook Soups", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_SOUP), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.FRENCH_ONION_SOUP), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.CREAM_OF_MUSHROOM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.CREAMY_SPINACH), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.SHRIMP_GUMBO), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetCookingRecipeForResult(ItemDict.FARMERS_STEW), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetCookingRecipeForResult(ItemDict.TOMATO_SOUP), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING));
            scrapbookPages[44] = new ScrapbookPage("Cookbook Unique", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_COOKBOOK_UNIQUE), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetCookingRecipeForResult(ItemDict.STORMFISH), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetCookingRecipeForResult(ItemDict.LUMINOUS_STEW), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetCookingRecipeForResult(ItemDict.ESCARGOT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetCookingRecipeForResult(ItemDict.CREAMY_SQUID), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetCookingRecipeForResult(ItemDict.RAW_CALAMARI), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetCookingRecipeForResult(ItemDict.EEL_ROLL), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.COOKING)); 
            scrapbookPages[45] = new ScrapbookPage("Blueprint Wood 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_WOOD1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.BOX), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.BRAZIER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.CRATE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.GARDEN_ARCH), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.LATTICE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.TORCH), numbers, craftingButtonSolo));
            scrapbookPages[46] = new ScrapbookPage("Blueprint Wood 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_WOOD2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.WOODEN_BENCH), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.WOODEN_CHAIR), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.WOODEN_COLUMN), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.WOODEN_LONGTABLE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.WOODEN_ROUNDTABLE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.WOODEN_SQUARETABLE), numbers, craftingButtonSolo));
            scrapbookPages[47] = new ScrapbookPage("Blueprint Wood 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_WOOD3), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.WOODEN_STOOL), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.WOODEN_POST), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.WOODEN_FENCE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.HORIZONTAL_WALLPAPER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.VERTICAL_WALLPAPER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.SOLID_WALLPAPER), numbers, craftingButtonSolo));
            scrapbookPages[48] = new ScrapbookPage("Blueprint Farm 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_FARM1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.CART), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.CLOTHESLINE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.FLAGPOLE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.HAYBALE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.MAILBOX), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.MARKET_STALL), numbers, craftingButtonSolo));
            scrapbookPages[49] = new ScrapbookPage("Blueprint Farm 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_FARM2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.MILK_JUG), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.PET_BOWL), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.PILE_OF_BRICKS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.SCARECROW), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.SIGNPOST), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.TOOLBOX), numbers, craftingButtonSolo));
            scrapbookPages[50] = new ScrapbookPage("Blueprint Farm 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_FARM3), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.TOOLRACK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.WAGON), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.WATER_PUMP), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.WATERTOWER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.WHEELBARROW), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.TALL_FENCE), numbers, craftingButtonSolo));
            scrapbookPages[51] = new ScrapbookPage("Blueprint Farm 4", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_FARM4), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.CARPET_FLOOR), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.STEPPING_STONE_FLOOR), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.POLKA_WALLPAPER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.DOT_WALLPAPER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.SCAFFOLDING_WOOD), numbers, craftingButtonSolo));
            scrapbookPages[52] = new ScrapbookPage("Blueprint Stone 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_STONE1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.CUBE_STATUE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.PYRAMID_STATUE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.SPHERE_STATUE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.STATUE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.STONE_COLUMN), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.WELL), numbers, craftingButtonSolo));
            scrapbookPages[53] = new ScrapbookPage("Blueprint Stone 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_STONE2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.FIREPLACE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.CONCRETE_FLOOR), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.STONE_FENCE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.SCAFFOLDING_STONE), numbers, craftingButtonSolo));
            scrapbookPages[54] = new ScrapbookPage("Blueprint Nature 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_NATURE1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.BAMBOO_POT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.BUOY), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.CAMPFIRE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.DECORATIVE_BOULDER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.DECORATIVE_LOG), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.FIREPIT), numbers, craftingButtonSolo));
            scrapbookPages[55] = new ScrapbookPage("Blueprint Nature 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_NATURE2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.HAMMOCK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.LIFEBUOY_SIGN), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.MINECART), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.SANDCASTLE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.SURFBOARD), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.TARGET), numbers, craftingButtonSolo));
            scrapbookPages[56] = new ScrapbookPage("Blueprint Nature 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_NATURE3), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.UMBRELLA), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.UMBRELLA_TABLE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.BAMBOO_FENCE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.STAR_WALLPAPER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.BUBBLE_WALLPAPER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.WAVE_WALLPAPER), numbers, craftingButtonSolo));
            scrapbookPages[57] = new ScrapbookPage("Blueprint Reflect 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_REFLECT1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.HORIZONTAL_MIRROR), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.WALL_MIRROR), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.ORNATE_MIRROR), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.TRIPLE_MIRRORS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.BANNER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.CANVAS), numbers, craftingButtonSolo));
            scrapbookPages[58] = new ScrapbookPage("Blueprint Reflect 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_REFLECT2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.HELIX_POSTER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.ANATOMICAL_POSTER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.RAINBOW_GRAFFITI), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.GLASS_FENCE), numbers, craftingButtonSolo));
            scrapbookPages[59] = new ScrapbookPage("Blueprint Play 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_PLAYGROUND1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.BLACKBOARD), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.WHITEBOARD), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.SANDBOX), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.SEESAW), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.SLIDE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.SWINGS), numbers, craftingButtonSolo));
            scrapbookPages[60] = new ScrapbookPage("Blueprint Play 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_PLAYGROUND2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.METAL_FENCE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.BOARDWALK_FLOOR), numbers, craftingButtonSolo));
            scrapbookPages[61] = new ScrapbookPage("Blueprint Musical", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_MUSIC), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.BELL), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.CYMBAL), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.DRUM), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.GUITAR_PLACEABLE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.PIANO), numbers, craftingButtonSolo));
            scrapbookPages[62] = new ScrapbookPage("Blueprint Tech 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_ENGINEERING1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.CLOCK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.GRANDFATHER_CLOCK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.STREETLAMP), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.STREETLIGHT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.LAMP), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.TELEVISION), numbers, craftingButtonSolo));
            scrapbookPages[63] = new ScrapbookPage("Blueprint Tech 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_ENGINEERING2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.LANTERN), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.LIGHTNING_ROD), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.SOLAR_PANEL), numbers, craftingButtonSolo));
            scrapbookPages[64] = new ScrapbookPage("Blueprint Urban 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_URBAN1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.BOOMBOX), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.FIRE_HYDRANT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.GYM_BENCH), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.POSTBOX), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.RECYCLING_BIN), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.SOFA), numbers, craftingButtonSolo));
            scrapbookPages[65] = new ScrapbookPage("Blueprint Urban 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_URBAN2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.TRAFFIC_CONE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.TRAFFIC_LIGHT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.TRASHCAN), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.FULL_THROTTLE_GRAFFITI), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.HEARTBREAK_GRAFFITI), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.HEROINE_GRAFFITI), numbers, craftingButtonSolo));
            scrapbookPages[66] = new ScrapbookPage("Blueprint Urban 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_URBAN3), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.LEFTWARD_GRAFFITI), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.RIGHT_ARROW_GRAFFITI), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.NOIZEBOYZ_GRAFFITI), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.RETRO_GRAFFITI), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.SMILE_GRAFFITI), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.SOURCE_UNKNOWN_GRAFFITI), numbers, craftingButtonSolo));
            scrapbookPages[67] = new ScrapbookPage("Blueprint Urban 4", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_URBAN4), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.MYTHRIL_FENCE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.GOLDEN_FENCE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.STREET_FLOOR), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.SCAFFOLDING_METAL), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.SCAFFOLDING_GOLDEN), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.SCAFFOLDING_MYTHRIL), numbers, craftingButtonSolo));
            scrapbookPages[68] = new ScrapbookPage("Blueprint Urban 5", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_URBAN5), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.ODD_WALLPAPER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.INVADER_WALLPAPER), numbers, craftingButtonSolo));
            scrapbookPages[69] = new ScrapbookPage("Blueprint Ice", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_BLUEPRINTS_ICE), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.FROST_SCULPTURE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.ICE_BLOCK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.IGLOO), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.SNOWMAN), numbers, craftingButtonSolo));
            scrapbookPages[70] = new ScrapbookPage("Pattern S/S 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_SS1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.BASEBALL_CAP), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.HEADBAND), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.BUTTERFLY_CLIP), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.ALL_SEASON_JACKET), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.SHORT_SLEEVE_TEE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.STRIPED_SHIRT), numbers, craftingButtonSolo));
            scrapbookPages[71] = new ScrapbookPage("Pattern S/S 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_SS2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.TANKER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.JEANS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.CHINO_SHORTS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.SHORT_SKIRT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.PUFF_SKIRT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.SHORT_SOCKS), numbers, craftingButtonSolo));
            scrapbookPages[72] = new ScrapbookPage("Pattern S/S 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_SS3), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.SNEAKERS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.GLASSES), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.GOGGLES), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.RUCKSACK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.GUITAR), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.SAILCLOTH), numbers, craftingButtonSolo));
            scrapbookPages[73] = new ScrapbookPage("Pattern F/W 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_FW1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.BOWLER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.CAMEL_HAT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.SQUARE_HAT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.FLAT_CAP), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.HOODED_SWEATSHIRT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.OVERCOAT), numbers, craftingButtonSolo));
            scrapbookPages[74] = new ScrapbookPage("Pattern F/W 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_FW2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.LONG_SLEEVED_TEE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.BUTTON_DOWN), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.PLAID_BUTTON), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.TURTLENECK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.CHINOS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.LONG_SKIRT), numbers, craftingButtonSolo));
            scrapbookPages[75] = new ScrapbookPage("Pattern F/W 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_FW3), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.SWEATER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.SCARF), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.NECKWARMER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.WOOL_MITTENS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.LONG_SOCKS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.STRIPED_SOCKS), numbers, craftingButtonSolo));
            scrapbookPages[76] = new ScrapbookPage("Pattern F/W 4", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_FW4), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.FESTIVE_SOCKS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.HIGH_TOPS), numbers, craftingButtonSolo));
            scrapbookPages[77] = new ScrapbookPage("Pattern Country 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_COUNTRY1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.CONICAL_FARMER), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.STRAW_HAT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.TEN_GALLON), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.BANDANA), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.FACEMASK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.APRON), numbers, craftingButtonSolo));
            scrapbookPages[78] = new ScrapbookPage("Pattern Country 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_COUNTRY2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.OVERALLS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.WORK_GLOVES), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.PROTECTIVE_VISOR), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.BACKPACK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.SASH), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.TALL_BOOTS), numbers, craftingButtonSolo));
            scrapbookPages[79] = new ScrapbookPage("Pattern Tropical 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_TROPICAL1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.WHISKERS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.BUNNY_EARS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.CAT_EARS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.TRACE_TATTOO), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.NOMAD_VEST), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.BATHROBE), numbers, craftingButtonSolo));
            scrapbookPages[80] = new ScrapbookPage("Pattern Tropical 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_TROPICAL2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.ISLANDER_TATTOO), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.LINEN_BUTTON), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.SUPER_SHORTS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.TIGHTIES), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.CAT_TAIL), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.SNORKEL), numbers, craftingButtonSolo));
            scrapbookPages[81] = new ScrapbookPage("Pattern Tropical 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_TROPICAL3), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.EYEPATCH), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.WING_SANDLES), numbers, craftingButtonSolo));
            scrapbookPages[82] = new ScrapbookPage("Pattern Costume 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_COSTUME1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.DINO_MASK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.DOG_MASK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.NIGHTMARE_MASK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.NIGHTCAP), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.CHEFS_HAT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.MEDAL), numbers, craftingButtonSolo));
            scrapbookPages[83] = new ScrapbookPage("Pattern Costume 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_COSTUME2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.SPORTBALL_UNIFORM), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.BOXING_MITTS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.DANGLE_EARRING), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.MISMATTCHED), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.CAPE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.WOLF_TAIL), numbers, craftingButtonSolo));
            scrapbookPages[84] = new ScrapbookPage("Pattern Costume 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_COSTUME3), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.CLOCKWORK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.ROBO_ARMS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.FOX_TAIL), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.BLINDFOLD), numbers, craftingButtonSolo));
            scrapbookPages[85] = new ScrapbookPage("Pattern Urban 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_URBAN1), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.TOP_HAT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.SNAPBACK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.RAINCOAT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.PUNK_JACKET), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.SUIT_JACKET), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.WEDDING_DRESS), numbers, craftingButtonSolo));
            scrapbookPages[86] = new ScrapbookPage("Pattern Urban 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_URBAN2), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.ONESIE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.TORN_JEANS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.JEAN_SHORTS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.FLASH_HEELS), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.EARRING_STUD), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe6Pos, GameState.GetCraftingRecipeForResult(ItemDict.PIERCING), numbers, craftingButtonSolo));
            scrapbookPages[87] = new ScrapbookPage("Pattern Urban 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_PATTERNS_URBAN3), Color.White),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe1Pos, GameState.GetCraftingRecipeForResult(ItemDict.SUNGLASSES), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe2Pos, GameState.GetCraftingRecipeForResult(ItemDict.QUERADE_MASK), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe3Pos, GameState.GetCraftingRecipeForResult(ItemDict.ASCOT), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe4Pos, GameState.GetCraftingRecipeForResult(ItemDict.NECKLACE), numbers, craftingButtonSolo),
                new ScrapbookPage.CraftingRecipeComponent(craftingRecipe5Pos, GameState.GetCraftingRecipeForResult(ItemDict.TIE), numbers, craftingButtonSolo));
            scrapbookPages[88] = new ScrapbookPage("Acces. Home 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACCESSORIES_HOMEMADE1), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAccessoryRecipeForResult(ItemDict.BUTTERFLY_CHARM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAccessoryRecipeForResult(ItemDict.DROPLET_CHARM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAccessoryRecipeForResult(ItemDict.SNOUT_CHARM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAccessoryRecipeForResult(ItemDict.SUNFLOWER_CHARM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetAccessoryRecipeForResult(ItemDict.SALTY_CHARM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetAccessoryRecipeForResult(ItemDict.SPINED_CHARM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY));
            scrapbookPages[89] = new ScrapbookPage("Acces. Home 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACCESSORIES_HOMEMADE2), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAccessoryRecipeForResult(ItemDict.MANTLE_CHARM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAccessoryRecipeForResult(ItemDict.DANDYLION_CHARM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAccessoryRecipeForResult(ItemDict.ACID_BRACER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAccessoryRecipeForResult(ItemDict.URCHIN_BRACER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetAccessoryRecipeForResult(ItemDict.FLUFFY_BRACER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetAccessoryRecipeForResult(ItemDict.DRUID_BRACER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY));
            scrapbookPages[90] = new ScrapbookPage("Acces. Home 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACCESSORIES_HOMEMADE3), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAccessoryRecipeForResult(ItemDict.TRADITION_BRACER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAccessoryRecipeForResult(ItemDict.SANDSTORM_BRACER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAccessoryRecipeForResult(ItemDict.DWARVEN_CHILDS_BRACER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAccessoryRecipeForResult(ItemDict.CARNIVORE_BRACER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetAccessoryRecipeForResult(ItemDict.PURIFICATION_BRACER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetAccessoryRecipeForResult(ItemDict.SCRAP_BRACER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY));
            scrapbookPages[91] = new ScrapbookPage("Acces. Home 4", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACCESSORIES_HOMEMADE4), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAccessoryRecipeForResult(ItemDict.ESSENCE_BRACER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY));
            scrapbookPages[92] = new ScrapbookPage("Acces. Nature 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACCESSORIES_NATURAL1), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAccessoryRecipeForResult(ItemDict.CHURN_CHARM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAccessoryRecipeForResult(ItemDict.PRIMAL_CHARM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAccessoryRecipeForResult(ItemDict.SUNRISE_CHARM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAccessoryRecipeForResult(ItemDict.VOLCANIC_CHARM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetAccessoryRecipeForResult(ItemDict.MUSHY_CHARM), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetAccessoryRecipeForResult(ItemDict.LUMINOUS_RING), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY));
            scrapbookPages[93] = new ScrapbookPage("Acces. Nature 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACCESSORIES_NATURAL2), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAccessoryRecipeForResult(ItemDict.FLORAL_RING), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAccessoryRecipeForResult(ItemDict.MUSICBOX_RING), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAccessoryRecipeForResult(ItemDict.STRIPED_BRACER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAccessoryRecipeForResult(ItemDict.PIN_BRACER), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetAccessoryRecipeForResult(ItemDict.DISSECTION_PENDANT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetAccessoryRecipeForResult(ItemDict.GAIA_PENDANT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY));
            scrapbookPages[94] = new ScrapbookPage("Acces. Nature 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACCESSORIES_NATURAL3), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAccessoryRecipeForResult(ItemDict.CONTRACT_PENDANT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAccessoryRecipeForResult(ItemDict.DYNAMITE_PENDANT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAccessoryRecipeForResult(ItemDict.OILY_PENDANT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAccessoryRecipeForResult(ItemDict.NEUTRALIZED_PENDANT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY));
            scrapbookPages[95] = new ScrapbookPage("Acces. Jewelry 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACCESSORIES_JEWELERY1), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAccessoryRecipeForResult(ItemDict.PHILOSOPHERS_CREST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAccessoryRecipeForResult(ItemDict.BLIND_RING), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAccessoryRecipeForResult(ItemDict.FLIGHT_RING), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAccessoryRecipeForResult(ItemDict.GLIMMER_RING), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetAccessoryRecipeForResult(ItemDict.MONOCULTURE_RING), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetAccessoryRecipeForResult(ItemDict.LUMBER_RING), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY));
            scrapbookPages[96] = new ScrapbookPage("Acces. Jewelry 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACCESSORIES_JEWELERY2), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAccessoryRecipeForResult(ItemDict.BAKERY_RING), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAccessoryRecipeForResult(ItemDict.ROSE_RING), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAccessoryRecipeForResult(ItemDict.SHELL_RING), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAccessoryRecipeForResult(ItemDict.FURNACE_RING), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetAccessoryRecipeForResult(ItemDict.SOUND_PENDANT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetAccessoryRecipeForResult(ItemDict.EROSION_PENDANT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY));
            scrapbookPages[97] = new ScrapbookPage("Acces. Jewelry 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACCESSORIES_JEWELERY3), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAccessoryRecipeForResult(ItemDict.POLYCULTURE_PENDANT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAccessoryRecipeForResult(ItemDict.LADYBUG_PENDANT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAccessoryRecipeForResult(ItemDict.STREAMLINE_PENDANT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAccessoryRecipeForResult(ItemDict.TORNADO_PENDANT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY));
            scrapbookPages[98] = new ScrapbookPage("Acces. Shaman 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACCESSORIES_SHAMAN1), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAccessoryRecipeForResult(ItemDict.ROYAL_CREST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAccessoryRecipeForResult(ItemDict.MIDIAN_SYMBOL), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAccessoryRecipeForResult(ItemDict.UNITY_CREST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAccessoryRecipeForResult(ItemDict.COMPRESSION_CREST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetAccessoryRecipeForResult(ItemDict.POLYMORPH_CREST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetAccessoryRecipeForResult(ItemDict.DASHING_CREST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY));
            scrapbookPages[99] = new ScrapbookPage("Acces. Shaman 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACCESSORIES_SHAMAN2), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAccessoryRecipeForResult(ItemDict.FROZEN_CREST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAccessoryRecipeForResult(ItemDict.MUTATING_CREST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAccessoryRecipeForResult(ItemDict.MYTHICAL_CREST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAccessoryRecipeForResult(ItemDict.VAMPYRIC_CREST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetAccessoryRecipeForResult(ItemDict.BREWERY_CREST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetAccessoryRecipeForResult(ItemDict.CLOUD_CREST), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY));
            scrapbookPages[100] = new ScrapbookPage("Acces. Shaman 3", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ACCESSORIES_SHAMAN3), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAccessoryRecipeForResult(ItemDict.OCEANIC_RING), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAccessoryRecipeForResult(ItemDict.CYCLE_PENDANT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ACCESSORY));
            scrapbookPages[101] = new ScrapbookPage("Alchemy Dusty", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ALCHEMY_DUSTY), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAlchemyRecipeForResult(ItemDict.BLACK_CANDLE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAlchemyRecipeForResult(ItemDict.SALTED_CANDLE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAlchemyRecipeForResult(ItemDict.SPICED_CANDLE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAlchemyRecipeForResult(ItemDict.MOSS_BOTTLE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetAlchemyRecipeForResult(ItemDict.SHIMMERING_SALVE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetAlchemyRecipeForResult(ItemDict.VOODOO_STEW), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY));
            scrapbookPages[102] = new ScrapbookPage("Alchemy Musty", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ALCHEMY_MUSTY), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAlchemyRecipeForResult(ItemDict.SUGAR_CANDLE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAlchemyRecipeForResult(ItemDict.SKY_BOTTLE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAlchemyRecipeForResult(ItemDict.HEART_VESSEL), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAlchemyRecipeForResult(ItemDict.INVINCIROID), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetAlchemyRecipeForResult(ItemDict.ADAMANTITE_BAR), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY));
            scrapbookPages[103] = new ScrapbookPage("Alchemy Mystica 1", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ALCHEMY_MYSTICA1), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAlchemyRecipeForResult(ItemDict.SOOTHE_CANDLE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAlchemyRecipeForResult(ItemDict.BURST_STONE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAlchemyRecipeForResult(ItemDict.UNSTABLE_LIQUID), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAlchemyRecipeForResult(ItemDict.PHILOSOPHERS_STONE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetAlchemyRecipeForResult(ItemDict.GOLD_BAR), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe6Pos, GameState.GetAlchemyRecipeForResult(ItemDict.MYTHRIL_BAR), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY));
            scrapbookPages[104] = new ScrapbookPage("Alchemy Mystica 2", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ALCHEMY_MYSTICA2), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAlchemyRecipeForResult(ItemDict.TROPICAL_BOTTLE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY));
            scrapbookPages[105] = new ScrapbookPage("Alchemy Incense", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ALCHEMY_INCENSE), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAlchemyRecipeForResult(ItemDict.IMPERIAL_INCENSE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAlchemyRecipeForResult(ItemDict.SWEET_INCENSE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAlchemyRecipeForResult(ItemDict.LAVENDER_INCENSE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAlchemyRecipeForResult(ItemDict.COLD_INCENSE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe5Pos, GameState.GetAlchemyRecipeForResult(ItemDict.FRESH_INCENSE), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY));
            scrapbookPages[106] = new ScrapbookPage("Alchemy Elements", new ScrapbookPage.ImageComponent(new Vector2(0, 0), content.Load<Texture2D>(Paths.INTERFACE_SCRAPBOOK_PAGE_ALCHEMY_ELEMENTS), Color.White),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe1Pos, GameState.GetAlchemyRecipeForResult(ItemDict.SKY_ELEMENT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe2Pos, GameState.GetAlchemyRecipeForResult(ItemDict.SEA_ELEMENT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe3Pos, GameState.GetAlchemyRecipeForResult(ItemDict.LAND_ELEMENT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY),
                new ScrapbookPage.CookingRecipeComponent(cookingRecipe4Pos, GameState.GetAlchemyRecipeForResult(ItemDict.PRIMORDIAL_ELEMENT), numbers, ScrapbookPage.CookingRecipeComponent.RecipeType.ALCHEMY));
            scrapbookPages[107] = new ScrapbookPage("");
            scrapbookPages[108] = new ScrapbookPage("");
            scrapbookPages[109] = new ScrapbookPage("");
        }

        private void SetKeyActionTexts(EntityPlayer player)
        {
            if (currentDialogue != null)
            {
                rightAction = currentDialogue.decisionRightText;
                leftAction = currentDialogue.decisionLeftText;
                upAction = currentDialogue.decisionUpText;
                downAction = currentDialogue.decisionDownText;
            }
            else
            {
                if (player.GetInterfaceState() != InterfaceState.NONE)
                {
                    rightAction = "";
                    leftAction = "";
                    upAction = "";
                    downAction = "";
                }
                else
                {
                    rightAction = "Right";
                    leftAction = "Left";
                    if (player.IsGroundPound() || player.IsGroundPoundLock())
                    {
                        rightAction = "";
                        leftAction = "";
                    }
                    else if (player.IsWallGrab() || player.IsWallCling())
                    {
                        rightAction = player.GetDirection() == DirectionEnum.LEFT ? "Release" : "Hold";
                        leftAction = player.GetDirection() == DirectionEnum.LEFT ? "Hold" : "Release";
                    }
                    if (player.IsRolling() && player.IsGrounded())
                    {
                        upAction = "Stand";
                    }
                    else if (player.IsGrounded() || (!player.IsGrounded() && player.IsRolling()) || player.IsWallGrab() || player.IsWallCling())
                    {
                        upAction = "Jump";
                    }
                    else if (player.IsGliding() || player.IsGroundPound() || player.IsGroundPoundLock())
                    {
                        upAction = "";
                    }
                    else
                    {
                        if (player.GetSailcloth().GetItem() != ItemDict.CLOTHING_NONE)
                        {
                            upAction = "Glide";
                        } else
                        {
                            upAction = "";
                        }
                    }
                    if (player.IsGroundPoundLock())
                    {
                        downAction = "Quickroll";
                    }
                    else if (player.IsRolling() || player.IsWallGrab() || player.IsWallCling())
                    {
                        downAction = "";
                    }
                    else if (player.IsGrounded())
                    {
                        downAction = "Roll";
                    }
                    else if (!player.IsGrounded() && !player.IsGliding())
                    {
                        downAction = "Groundpound";
                    }
                    else if (player.IsGliding())
                    {
                        downAction = "Exit Glide";
                    }
                    else
                    {
                        downAction = "";
                    }
                }
            }
        }

        private void SetMouseActionTexts(EntityPlayer player, bool hoveringPlaceable)
        {
            if (currentDialogue != null)
            {
                if (!currentDialogue.Splits())
                {
                    mouseLeftAction = "Next";
                    mouseRightAction = "";
                    if(currentDialogueNumChars < currentDialogue.dialogueText.Length)
                    {
                        mouseRightAction = "Speed-Up";
                    }
                    mouseRightShiftAction = "";
                    mouseLeftShiftAction = "";
                } else
                {
                    mouseLeftAction = "";
                    mouseLeftShiftAction = "";
                }
            }
            else
            {
                mouseLeftAction = player.GetLeftClickAction();
                mouseRightAction = player.GetRightClickAction();
                mouseLeftShiftAction = player.GetLeftShiftClickAction();
                mouseRightShiftAction = player.GetRightShiftClickAction();
                if (player.GetInterfaceState() == InterfaceState.INVENTORY || player.GetInterfaceState() == InterfaceState.CHEST)
                {
                    mouseLeftShiftAction = "Hot-swap";
                    mouseRightShiftAction = "";
                    if (inventoryHeldItem.GetItem() != ItemDict.NONE)
                    {
                        mouseLeftAction = "Place All";
                        mouseRightAction = "Place One";
                        if (inventoryHeldItem.GetItem() is DyeItem)
                        {
                            mouseRightAction = "Apply Dye";
                            mouseRightShiftAction = "x10 Dye";
                        }
                    }
                    else
                    {
                        mouseLeftAction = "Grab All";
                        mouseRightAction = "Grab Half";
                    }
                }
                else if (player.GetHeldItem().GetItem() is PlaceableItem || player.GetHeldItem().GetItem() is BuildingBlockItem)
                {
                    mouseLeftAction = player.GetLeftClickAction().Equals("") ? "Place" : player.GetLeftClickAction();
                    if (hoveringPlaceable) {
                        mouseRightAction = "Remove";
                    }
                } else if (player.GetInterfaceState() == InterfaceState.SCRAPBOOK || player.GetInterfaceState() == InterfaceState.SETTINGS)
                {
                    mouseLeftAction = "Select";
                    mouseRightAction = "Select";
                    mouseRightShiftAction = "";
                    mouseLeftShiftAction = "";
                } else if (player.GetInterfaceState() == InterfaceState.EXIT)
                {
                    mouseLeftAction = "Confirm";
                    mouseRightAction = "";
                    mouseRightShiftAction = "";
                    mouseLeftShiftAction = "";
                } else if (player.GetInterfaceState() == InterfaceState.CRAFTING)
                {
                    mouseLeftAction = "Select";
                    mouseRightAction = "";
                    mouseRightShiftAction = "";
                    mouseLeftShiftAction = "";
                }

                if(player.IsRolling())
                {
                    mouseLeftAction = "";
                    mouseRightAction = "";
                    mouseRightShiftAction = "";
                    mouseLeftShiftAction = "";
                }
            }
        }

        public void Pause()
        {
            this.paused = true;
        }

        public void Unpause()
        {
            this.paused = false;
        }

        private bool CheckClothingTooltips(EntityPlayer player, Vector2 mouse)
        {
            if (HAT_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = HAT_INVENTORY_RECT.TopLeft;
                if (hat == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Hat";
                    tooltipDescription = "You can place a hat\nhere to put it on.";
                }
                else
                {
                    tooltipName = player.GetHat().GetItem().GetName();
                    tooltipDescription = player.GetHat().GetItem().GetDescription() + (player.GetHat().GetItem().GetValue() > 0 ? "\nValue: " + player.GetHat().GetItem().GetValue() : "");
                }
                return true;
            }
            else if (SHIRT_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = SHIRT_INVENTORY_RECT.TopLeft;
                if (shirt == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Shirt";
                    tooltipDescription = "You can place a shirt\nhere to put it on.";
                }
                else
                {
                    tooltipName = player.GetShirt().GetItem().GetName();
                    tooltipDescription = player.GetShirt().GetItem().GetDescription() + (player.GetShirt().GetItem().GetValue() > 0 ? "\nValue: " + player.GetShirt().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            else if (OUTERWEAR_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = OUTERWEAR_INVENTORY_RECT.TopLeft;
                if (outerwear == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Outerwear";
                    tooltipDescription = "You can place outerwear\nhere to put it on.";
                }
                else
                {
                    tooltipName = player.GetOuterwear().GetItem().GetName();
                    tooltipDescription = player.GetOuterwear().GetItem().GetDescription() + (player.GetOuterwear().GetItem().GetValue() > 0 ? "\nValue: " + player.GetOuterwear().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            else if (PANTS_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = PANTS_INVENTORY_RECT.TopLeft;
                if (pants == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Pants";
                    tooltipDescription = "You can place pants\nhere to put them on.";
                }
                else
                {
                    tooltipName = player.GetPants().GetItem().GetName();
                    tooltipDescription = player.GetPants().GetItem().GetDescription() + (player.GetPants().GetItem().GetValue() > 0 ? "\nValue: " + player.GetPants().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            else if (SOCKS_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = SOCKS_INVENTORY_RECT.TopLeft;
                if (socks == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Socks";
                    tooltipDescription = "You can place socks\nhere to put them on.";
                }
                else
                {
                    tooltipName = player.GetSocks().GetItem().GetName();
                    tooltipDescription = player.GetSocks().GetItem().GetDescription() + (player.GetSocks().GetItem().GetValue() > 0 ? "\nValue: " + player.GetSocks().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            else if (SHOES_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = SHOES_INVENTORY_RECT.TopLeft;
                if (shoes == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Shoes";
                    tooltipDescription = "You can place shoes\nhere to put them on.";
                }
                else
                {
                    tooltipName = player.GetShoes().GetItem().GetName();
                    tooltipDescription = player.GetShoes().GetItem().GetDescription() + (player.GetShoes().GetItem().GetValue() > 0 ? "\nValue: " + player.GetShoes().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            else if (GLOVES_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = GLOVES_INVENTORY_RECT.TopLeft;
                if (gloves == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Gloves";
                    tooltipDescription = "You can place gloves\nhere to put them on.";
                }
                else
                {
                    tooltipName = player.GetGloves().GetItem().GetName();
                    tooltipDescription = player.GetGloves().GetItem().GetDescription() + (player.GetGloves().GetItem().GetValue() > 0 ? "\nValue: " + player.GetGloves().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            else if (EARRINGS_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = EARRINGS_INVENTORY_RECT.TopLeft;
                if (earrings == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Earrings";
                    tooltipDescription = "You can place earrings\nhere to put them on.";
                }
                else
                {
                    tooltipName = player.GetEarrings().GetItem().GetName();
                    tooltipDescription = player.GetEarrings().GetItem().GetDescription() + (player.GetEarrings().GetItem().GetValue() > 0 ? "\nValue: " + player.GetEarrings().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            else if (SCARF_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = SCARF_INVENTORY_RECT.TopLeft;
                if (scarf == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Scarf";
                    tooltipDescription = "You can place a scarf\nhere to put it on.";
                }
                else
                {
                    tooltipName = player.GetScarf().GetItem().GetName();
                    tooltipDescription = player.GetScarf().GetItem().GetDescription() + (player.GetScarf().GetItem().GetValue() > 0 ? "\nValue: " + player.GetScarf().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            else if (GLASSES_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = GLASSES_INVENTORY_RECT.TopLeft;
                if (glasses == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Glasses";
                    tooltipDescription = "You can place glasses\nhere to put them on.";
                }
                else
                {
                    tooltipName = player.GetGlasses().GetItem().GetName();
                    tooltipDescription = player.GetGlasses().GetItem().GetDescription() + (player.GetGlasses().GetItem().GetValue() > 0 ? "\nValue: " + player.GetGlasses().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            else if (BACK_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = BACK_INVENTORY_RECT.TopLeft;
                if (back == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Back";
                    tooltipDescription = "You can place a back item\nhere to put it on.";
                }
                else
                {
                    tooltipName = player.GetBack().GetItem().GetName();
                    tooltipDescription = player.GetBack().GetItem().GetDescription() + (player.GetBack().GetItem().GetValue() > 0 ? "\nValue: " + player.GetBack().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            else if (SAILCLOTH_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = SAILCLOTH_INVENTORY_RECT.TopLeft;
                if (sailcloth == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Sailcloth";
                    tooltipDescription = "You can place a sailcloth\nhere to put it on.";
                }
                else
                {
                    tooltipName = player.GetSailcloth().GetItem().GetName();
                    tooltipDescription = player.GetSailcloth().GetItem().GetDescription() + (player.GetSailcloth().GetItem().GetValue() > 0 ? "\nValue: " + player.GetSailcloth().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            else if (ACCESSORY1_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = ACCESSORY1_INVENTORY_RECT.TopLeft;
                if (accessory1 == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Accessory 1";
                    tooltipDescription = "You can place an accessory\nhere to equip it.";
                }
                else
                {
                    tooltipName = player.GetAccessory1().GetItem().GetName();
                    tooltipDescription = player.GetAccessory1().GetItem().GetDescription() + (player.GetAccessory1().GetItem().GetValue() > 0 ? "\nValue: " + player.GetAccessory1().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            else if (ACCESSORY2_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = ACCESSORY2_INVENTORY_RECT.TopLeft;
                if (accessory2 == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Accessory 2";
                    tooltipDescription = "You can place an accessory\nhere to equip it.";
                }
                else
                {
                    tooltipName = player.GetAccessory2().GetItem().GetName();
                    tooltipDescription = player.GetAccessory2().GetItem().GetDescription() + (player.GetAccessory2().GetItem().GetValue() > 0 ? "\nValue: " + player.GetAccessory2().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            else if (ACCESSORY3_INVENTORY_RECT.Contains(mouse))
            {
                inventorySelectedPosition = ACCESSORY3_INVENTORY_RECT.TopLeft;
                if (accessory3 == ItemDict.CLOTHING_NONE)
                {
                    tooltipName = "Accessory 3";
                    tooltipDescription = "You can place an accessory\nhere to equip it.";
                }
                else
                {
                    tooltipName = player.GetAccessory3().GetItem().GetName();
                    tooltipDescription = player.GetAccessory3().GetItem().GetDescription() + (player.GetAccessory3().GetItem().GetValue() > 0 ? "\nValue: " + player.GetAccessory3().GetItem().GetValue() : ""); ;
                }
                return true;
            }
            return false;
        }

        private void CheckClothingClick(EntityPlayer player, Vector2 mousePos, bool shift)
        {
            if (BACK_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() is DyeItem && player.GetBack().GetItem() != ItemDict.NONE)
                {
                    TryApplyDye(player.GetBack(), player);
                }
                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetBack().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetBack().GetItem(), false, false))
                            {
                                player.SetBack(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetBack();
                            player.SetBack(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.BACK))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetBack().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetBack();
                    }
                    player.SetBack(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (GLASSES_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() is DyeItem && player.GetGlasses().GetItem() != ItemDict.NONE)
                {
                    TryApplyDye(player.GetGlasses(), player);
                }
                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetGlasses().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetGlasses().GetItem(), false, false))
                            {
                                player.SetGlasses(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetGlasses();
                            player.SetGlasses(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.GLASSES))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetGlasses().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetGlasses();
                    }
                    player.SetGlasses(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (SAILCLOTH_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() is DyeItem && player.GetSailcloth().GetItem() != ItemDict.NONE)
                {
                    TryApplyDye(player.GetSailcloth(), player);
                }
                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetSailcloth().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetSailcloth().GetItem(), false, false))
                            {
                                player.SetSailcloth(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetSailcloth();
                            player.SetSailcloth(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.SAILCLOTH))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetSailcloth().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetSailcloth();
                    }
                    player.SetSailcloth(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (SCARF_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() is DyeItem && player.GetScarf().GetItem() != ItemDict.NONE)
                {
                    TryApplyDye(player.GetScarf(), player);
                }
                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetScarf().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetScarf().GetItem(), false, false))
                            {
                                player.SetScarf(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetScarf();
                            player.SetScarf(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.SCARF))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetScarf().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetScarf();
                    }
                    player.SetScarf(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (OUTERWEAR_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() is DyeItem && player.GetOuterwear().GetItem() != ItemDict.NONE)
                {
                    TryApplyDye(player.GetOuterwear(), player);
                }
                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetOuterwear().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetOuterwear().GetItem(), false, false))
                            {
                                player.SetOuterwear(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetOuterwear();
                            player.SetOuterwear(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.OUTERWEAR))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetOuterwear().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetOuterwear();
                    }
                    player.SetOuterwear(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (SOCKS_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() is DyeItem && player.GetSocks().GetItem() != ItemDict.NONE)
                {
                    TryApplyDye(player.GetSocks(), player);
                }
                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetSocks().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetSocks().GetItem(), false, false))
                            {
                                player.SetSocks(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetSocks();
                            player.SetSocks(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.SOCKS))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetSocks().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetSocks();
                    }
                    player.SetSocks(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (HAT_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() is DyeItem && player.GetHat().GetItem() != ItemDict.NONE)
                {
                    TryApplyDye(player.GetHat(), player);
                }
                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetHat().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetHat().GetItem(), false, false))
                            {
                                player.SetHat(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetHat();
                            player.SetHat(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.HAT))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetHat().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetHat();
                    }
                    player.SetHat(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (SHIRT_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() is DyeItem && player.GetShirt().GetItem() != ItemDict.NONE)
                {
                    TryApplyDye(player.GetShirt(), player);
                }
                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetShirt().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetShirt().GetItem(), false, false))
                            {
                                player.SetShirt(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetShirt();
                            player.SetShirt(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.SHIRT))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetShirt().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetShirt();
                    }
                    player.SetShirt(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (PANTS_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() is DyeItem && player.GetPants().GetItem() != ItemDict.NONE)
                {
                    TryApplyDye(player.GetPants(), player);
                }
                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetPants().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetPants().GetItem(), false, false))
                            {
                                player.SetPants(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetPants();
                            player.SetPants(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.PANTS))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetPants().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetPants();
                    }
                    player.SetPants(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (EARRINGS_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() is DyeItem && player.GetEarrings().GetItem() != ItemDict.NONE)
                {
                    TryApplyDye(player.GetEarrings(), player);
                }
                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetEarrings().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetEarrings().GetItem(), false, false))
                            {
                                player.SetEarrings(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetEarrings();
                            player.SetEarrings(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.EARRINGS))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetEarrings().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetEarrings();
                    }
                    player.SetEarrings(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (GLOVES_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() is DyeItem && player.GetGloves().GetItem() != ItemDict.NONE)
                {
                    TryApplyDye(player.GetGloves(), player);
                }
                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetGloves().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetGloves().GetItem(), false, false))
                            {
                                player.SetGloves(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetGloves();
                            player.SetGloves(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.GLOVES))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetGloves().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetGloves();
                    }
                    player.SetGloves(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (SHOES_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() is DyeItem && player.GetShoes().GetItem() != ItemDict.NONE)
                {
                    TryApplyDye(player.GetShoes(), player);
                }
                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetShoes().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetShoes().GetItem(), false, false))
                            {
                                player.SetShoes(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetShoes();
                            player.SetShoes(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.SHOES))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetShoes().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetShoes();
                    }
                    player.SetShoes(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (ACCESSORY1_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetAccessory1().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetAccessory1().GetItem(), false, false))
                            {
                                player.SetAccessory1(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetAccessory1();
                            player.SetAccessory1(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.ACCESSORY))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetAccessory1().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetAccessory1();
                    }
                    player.SetAccessory1(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (ACCESSORY2_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetAccessory2().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetAccessory2().GetItem(), false, false))
                            {
                                player.SetAccessory2(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetAccessory2();
                            player.SetAccessory2(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.ACCESSORY))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetAccessory2().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetAccessory2();
                    }
                    player.SetAccessory2(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
            else if (ACCESSORY3_INVENTORY_RECT.Contains(mousePos))
            {
                if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (player.GetAccessory3().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        if (shift)
                        {
                            if (player.AddItemToInventory(player.GetAccessory3().GetItem(), false, false))
                            {
                                player.SetAccessory3(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                            }
                        }
                        else
                        {
                            inventoryHeldItem = player.GetAccessory3();
                            player.SetAccessory3(new ItemStack(ItemDict.CLOTHING_NONE, 0));
                        }
                    }
                }
                else if (inventoryHeldItem.GetItem().HasTag(Item.Tag.ACCESSORY))
                {
                    ItemStack oldItem = new ItemStack(ItemDict.NONE, 0);
                    if (player.GetAccessory3().GetItem() != ItemDict.CLOTHING_NONE)
                    {
                        oldItem = player.GetAccessory3();
                    }
                    player.SetAccessory3(inventoryHeldItem);
                    inventoryHeldItem = oldItem;
                }
            }
        }

        private void DrawClothingHeldIndicator(SpriteBatch sb, RectangleF cameraBoundingBox)
        {
            if (inventoryHeldItem.GetItem().HasTag(Item.Tag.ACCESSORY))
            {
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, ACCESSORY1_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, ACCESSORY2_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, ACCESSORY3_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
            }
            if(inventoryHeldItem.GetItem().HasTag(Item.Tag.BACK))
            {
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, BACK_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
            }
            if(inventoryHeldItem.GetItem().HasTag(Item.Tag.EARRINGS))
            {
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, EARRINGS_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
            }
            if (inventoryHeldItem.GetItem().HasTag(Item.Tag.GLASSES))
            {
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, GLASSES_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
            }
            if (inventoryHeldItem.GetItem().HasTag(Item.Tag.GLOVES))
            {
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, GLOVES_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
            }
            if (inventoryHeldItem.GetItem().HasTag(Item.Tag.HAT))
            {
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, HAT_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
            }
            if (inventoryHeldItem.GetItem().HasTag(Item.Tag.OUTERWEAR))
            {
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, OUTERWEAR_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
            }
            if (inventoryHeldItem.GetItem().HasTag(Item.Tag.PANTS))
            {
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, PANTS_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
            }
            if (inventoryHeldItem.GetItem().HasTag(Item.Tag.SAILCLOTH))
            {
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SAILCLOTH_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
            }
            if (inventoryHeldItem.GetItem().HasTag(Item.Tag.SCARF))
            {
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCARF_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
            }
            if (inventoryHeldItem.GetItem().HasTag(Item.Tag.SHIRT))
            {
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SHIRT_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
            }
            if (inventoryHeldItem.GetItem().HasTag(Item.Tag.SHOES))
            {
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SHOES_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
            }
            if (inventoryHeldItem.GetItem().HasTag(Item.Tag.SOCKS))
            {
                sb.DrawRectangle(Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SOCKS_INVENTORY_RECT), CLOTHING_INDICATOR_COLOR);
            }
        }

        private void ShiftSwapClothingItem(EntityPlayer player, int i)
        {
            ItemStack item = player.GetInventoryItemStack(i);
            if(item.GetItem().HasTag(Item.Tag.ACCESSORY))
            {
                if(player.GetAccessory1().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetAccessory1(item);
                    player.RemoveItemStackAt(i);
                } else if (player.GetAccessory2().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetAccessory2(item);
                    player.RemoveItemStackAt(i);
                } else if (player.GetAccessory3().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetAccessory3(item);
                    player.RemoveItemStackAt(i);
                } else
                {
                    ItemStack oldAcc = player.GetAccessory1();
                    player.SetAccessory1(item);
                    player.AddItemStackAt(oldAcc, i);
                }
            } else if (item.GetItem().HasTag(Item.Tag.BACK))
            {
                if(player.GetBack().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetBack(item);
                    player.RemoveItemStackAt(i);
                } else
                {
                    ItemStack old = player.GetBack();
                    player.SetBack(item);
                    player.AddItemStackAt(old, i);
                }
            }
            else if (item.GetItem().HasTag(Item.Tag.EARRINGS))
            {
                if (player.GetEarrings().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetEarrings(item);
                    player.RemoveItemStackAt(i);
                }
                else
                {
                    ItemStack old = player.GetEarrings();
                    player.SetEarrings(item);
                    player.AddItemStackAt(old, i);
                }
            }
            else if (item.GetItem().HasTag(Item.Tag.GLASSES))
            {
                if (player.GetGlasses().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetGlasses(item);
                    player.RemoveItemStackAt(i);
                }
                else
                {
                    ItemStack old = player.GetGlasses();
                    player.SetGlasses(item);
                    player.AddItemStackAt(old, i);
                }
            }
            else if (item.GetItem().HasTag(Item.Tag.GLOVES))
            {
                if (player.GetGloves().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetGloves(item);
                    player.RemoveItemStackAt(i);
                }
                else
                {
                    ItemStack old = player.GetGloves();
                    player.SetGloves(item);
                    player.AddItemStackAt(old, i);
                }
            }
            else if (item.GetItem().HasTag(Item.Tag.HAT))
            {
                if (player.GetHat().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetHat(item);
                    player.RemoveItemStackAt(i);
                }
                else
                {
                    ItemStack old = player.GetHat();
                    player.SetHat(item);
                    player.AddItemStackAt(old, i);
                }
            }
            else if (item.GetItem().HasTag(Item.Tag.OUTERWEAR))
            {
                if (player.GetOuterwear().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetOuterwear(item);
                    player.RemoveItemStackAt(i);
                }
                else
                {
                    ItemStack old = player.GetOuterwear();
                    player.SetOuterwear(item);
                    player.AddItemStackAt(old, i);
                }
            }
            else if (item.GetItem().HasTag(Item.Tag.PANTS))
            {
                if (player.GetPants().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetPants(item);
                    player.RemoveItemStackAt(i);
                }
                else
                {
                    ItemStack old = player.GetPants();
                    player.SetPants(item);
                    player.AddItemStackAt(old, i);
                }
            }
            else if (item.GetItem().HasTag(Item.Tag.SAILCLOTH))
            {
                if (player.GetSailcloth().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetSailcloth(item);
                    player.RemoveItemStackAt(i);
                }
                else
                {
                    ItemStack old = player.GetSailcloth();
                    player.SetSailcloth(item);
                    player.AddItemStackAt(old, i);
                }
            }
            else if (item.GetItem().HasTag(Item.Tag.SCARF))
            {
                if (player.GetScarf().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetScarf(item);
                    player.RemoveItemStackAt(i);
                }
                else
                {
                    ItemStack old = player.GetScarf();
                    player.SetScarf(item);
                    player.AddItemStackAt(old, i);
                }
            }
            else if (item.GetItem().HasTag(Item.Tag.SHIRT))
            {
                if (player.GetShirt().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetShirt(item);
                    player.RemoveItemStackAt(i);
                }
                else
                {
                    ItemStack old = player.GetShirt();
                    player.SetShirt(item);
                    player.AddItemStackAt(old, i);
                }
            }
            else if (item.GetItem().HasTag(Item.Tag.SHOES))
            {
                if (player.GetShoes().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetShoes(item);
                    player.RemoveItemStackAt(i);
                }
                else
                {
                    ItemStack old = player.GetShoes();
                    player.SetShoes(item);
                    player.AddItemStackAt(old, i);
                }
            }
            else if (item.GetItem().HasTag(Item.Tag.SOCKS))
            {
                if (player.GetSocks().GetItem() == ItemDict.CLOTHING_NONE)
                {
                    player.SetSocks(item);
                    player.RemoveItemStackAt(i);
                }
                else
                {
                    ItemStack old = player.GetSocks();
                    player.SetSocks(item);
                    player.AddItemStackAt(old, i);
                }
            }
        }

        private void OpenScrapbook(EntityPlayer player, World.TimeData timeData, World world)
        {
            int y = 0;
            int x = 0;
            switch(timeData.season)
            {
                case World.Season.SPRING:
                    y = 45;
                    break;
                case World.Season.SUMMER:
                    y = 45 + 18;
                    break;
                case World.Season.AUTUMN:
                    y = 45 + 18 + 18;
                    break;
                case World.Season.WINTER:
                    y = 45 + 18 + 18 + 18;
                    break;
            }
            switch(timeData.day)
            {
                case 0:
                    x = 33;
                    break;
                case 1:
                    x = 48;
                    break;
                case 2:
                    x = 62;
                    break;
                case 3:
                    x = 77;
                    break;
                case 4:
                    x = 89;
                    break;
                case 5:
                    x = 105;
                    break;
                case 6:
                    x = 123;
                    break;
            }
            scrapbookDynamicComponents[SCRAPBOOK_CALENDAR_CURRENT_DAY].SetPosition(new Vector2(x, y));

            for(int i = 0; i < scrapbookPages.Length; i++)
            {
                scrapbookPages[i].IsUnlocked(true);
            }
            scrapbookPages[0].IsUnlocked(true); //calendar
            scrapbookPages[1].IsUnlocked(true); //maps
            scrapbookPages[2].IsUnlocked(true); //relationships
            scrapbookPages[3].IsUnlocked(true); //relationships ii
            scrapbookPages[4].IsUnlocked(true); //farming intro
            /*scrapbookPages[5].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_THE_FARMERS_HANDBOOK_ANIMALS));
            scrapbookPages[6].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_THE_FARMERS_HANDBOOK_COMPOST));
            scrapbookPages[7].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_THE_FARMERS_HANDBOOK_SEED_MAKERS));
            scrapbookPages[8].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_THE_FARMERS_HANDBOOK_ADVANCED));
            scrapbookPages[9].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_THE_FARMERS_HANDBOOK_MASTERY));
            scrapbookPages[10].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_THE_FARMERS_HANDBOOK_MYTHS_AND_LEGENDS));
            scrapbookPages[11].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_WORKING_WITH_YOUR_WORKBENCH));
            scrapbookPages[12].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_THE_TRADERS_ATLAS));
            scrapbookPages[13].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_DATING_FOR_DUMMIES));
            scrapbookPages[14].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_STRAIGHTFORWARD_SMELTING));
            scrapbookPages[15].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_MASTERFUL_METALWORKING));
            scrapbookPages[16].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_COMPRESSOR_USER_MANUAL));
            scrapbookPages[17].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_A_STUDY_OF_COLOR_IN_NATURE));
            scrapbookPages[18].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_GREAT_GLASSBLOWING));
            scrapbookPages[19].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_POTTERY_FOR_FUN_AND_PROFIT));
            scrapbookPages[20].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_ARTISTIC_SCENT));
            scrapbookPages[21].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_JUICY_JAMS_PRECIOUS_PRESERVES_AND_WORTHWHILE_WINES));
            scrapbookPages[22].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_BEEKEEPERS_MANUAL));
            scrapbookPages[23].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_BEEMASTERS_MANUAL));
            scrapbookPages[24].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_BIRDWATCHERS_ISSUE_I));
            scrapbookPages[25].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_BIRDWATCHERS_ISSUE_II));
            scrapbookPages[26].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_FISHING_THROUGH_THE_SEASONS));
            scrapbookPages[27].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_ANCIENT_MARINERS_SCROLL));
            scrapbookPages[28].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_FISH_BEYOND));
            scrapbookPages[29].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_FASHION_PRIMER));
            scrapbookPages[30].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_DECOR_PRIMER));
            scrapbookPages[31].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_THE_FORAGERS_COOKBOOK_VOL_1));
            scrapbookPages[32].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_THE_FORAGERS_COOKBOOK_VOL_2));
            scrapbookPages[33].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_THE_FORAGERS_COOKBOOK_VOL_3));
            scrapbookPages[34].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_COOKBOOK_SPRINGS_GIFT));
            scrapbookPages[35].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_COOKBOOK_SUMMERS_BOUNTY));
            scrapbookPages[36].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_COOKBOOK_AUTUMNS_HARVEST));
            scrapbookPages[37].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_COOKBOOK_FOUR_SEASONS));
            scrapbookPages[38].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_CHILLING_CONFECTIONS));
            scrapbookPages[39].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_BREAKFAST_WITH_GRANDMA_NINE));
            scrapbookPages[40].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_SUPPER_WITH_GRANDMA_NINE));
            scrapbookPages[41].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_SUPPER_WITH_GRANDMA_NINE));
            scrapbookPages[42].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_SOUPER_SOUPS));
            scrapbookPages[43].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_EASTERN_CUISINE));
            scrapbookPages[44].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_UNIQUE_FLAVORS)); //GOOD
            scrapbookPages[45].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_SIMPLY_WOODWORKING));
            scrapbookPages[46].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_SIMPLY_WOODWORKING));
            scrapbookPages[47].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_SIMPLY_WOODWORKING));
            scrapbookPages[48].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_FABULOUS_FARMSTEADS));
            scrapbookPages[49].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_FABULOUS_FARMSTEADS));
            scrapbookPages[50].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_FABULOUS_FARMSTEADS));
            scrapbookPages[51].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_FABULOUS_FARMSTEADS));
            scrapbookPages[52].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_CRAVING_STONECARVING));
            scrapbookPages[53].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_CRAVING_STONECARVING));
            scrapbookPages[54].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_A_TOUCH_OF_NATURE));
            scrapbookPages[55].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_A_TOUCH_OF_NATURE));
            scrapbookPages[56].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_A_TOUCH_OF_NATURE));
            scrapbookPages[57].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_AN_ARTISTS_REFLECTION));
            scrapbookPages[58].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_AN_ARTISTS_REFLECTION));
            scrapbookPages[59].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_PLAYGROUND_PREP));
            scrapbookPages[60].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_PLAYGROUND_PREP));
            scrapbookPages[61].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_MUSIC_AT_HOME));
            scrapbookPages[62].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_ESSENTIAL_ENGINEERING));
            scrapbookPages[63].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_ESSENTIAL_ENGINEERING));
            scrapbookPages[64].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_URBAN_DESIGN_BIBLE));
            scrapbookPages[65].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_URBAN_DESIGN_BIBLE));
            scrapbookPages[66].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_URBAN_DESIGN_BIBLE));
            scrapbookPages[67].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_URBAN_DESIGN_BIBLE));
            scrapbookPages[68].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_URBAN_DESIGN_BIBLE));
            scrapbookPages[69].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_ICE_A_TREATISE));
            scrapbookPages[70].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_BASIC_PATTERNS_SS));
            scrapbookPages[71].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_BASIC_PATTERNS_SS));
            scrapbookPages[72].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_BASIC_PATTERNS_SS));
            scrapbookPages[73].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_BASIC_PATTERNS_FW));
            scrapbookPages[74].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_BASIC_PATTERNS_FW));
            scrapbookPages[75].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_BASIC_PATTERNS_FW));
            scrapbookPages[76].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_BASIC_PATTERNS_FW));
            scrapbookPages[77].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_COUNTRY_PATTERNS));
            scrapbookPages[78].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_COUNTRY_PATTERNS));
            scrapbookPages[79].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_TROPICAL_PATTERNS));
            scrapbookPages[80].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_TROPICAL_PATTERNS));
            scrapbookPages[81].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_TROPICAL_PATTERNS));
            scrapbookPages[82].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_COSTUME_PATTERNS));
            scrapbookPages[83].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_COSTUME_PATTERNS));
            scrapbookPages[84].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_COSTUME_PATTERNS));
            scrapbookPages[85].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_URBAN_PATTERNS));
            scrapbookPages[86].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_URBAN_PATTERNS));
            scrapbookPages[87].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_URBAN_PATTERNS));
            scrapbookPages[88].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_HOMEMADE_ACCESSORIES));
            scrapbookPages[89].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_HOMEMADE_ACCESSORIES));
            scrapbookPages[90].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_HOMEMADE_ACCESSORIES));
            scrapbookPages[91].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_HOMEMADE_ACCESSORIES));
            scrapbookPages[92].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_NATURAL_CRAFTS));
            scrapbookPages[93].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_NATURAL_CRAFTS));
            scrapbookPages[94].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_NATURAL_CRAFTS));
            scrapbookPages[95].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_JEWELERS_HANDBOOK));
            scrapbookPages[96].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_JEWELERS_HANDBOOK));
            scrapbookPages[97].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_JEWELERS_HANDBOOK));
            scrapbookPages[98].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_FOCI_OF_THE_SHAMAN));
            scrapbookPages[99].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_FOCI_OF_THE_SHAMAN));
            scrapbookPages[100].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_FOCI_OF_THE_SHAMAN));
            scrapbookPages[101].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_DUSTY_TOME));
            scrapbookPages[102].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_MUSTY_TOME));
            scrapbookPages[103].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_ELEMENTAL_MYSTICA));
            scrapbookPages[104].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_ELEMENTAL_MYSTICA));
            scrapbookPages[105].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_INTENSE_INCENSE));
            scrapbookPages[106].IsUnlocked(GameState.CheckFlag(GameState.FLAG_BOOK_CHANNELING_THE_ELEMENTS));*/
            scrapbookPages[107].IsUnlocked(true);
            scrapbookPages[108].IsUnlocked(true);
            scrapbookPages[109].IsUnlocked(true);

            player.SetInterfaceState(InterfaceState.SCRAPBOOK);
            player.Pause();
            world.Pause();
        }

        private void TryApplyDye(ItemStack toDye, EntityPlayer player)
        {
            if (inventoryHeldItem.GetItem() is DyeItem)
            {
                if (toDye.GetItem().HasTag(Item.Tag.DYEABLE))
                {
                    string name = ItemDict.GetColoredItemBaseForm(toDye.GetItem());
                    if (inventoryHeldItem.GetItem() != ItemDict.UN_DYE)
                    {
                        name += ((DyeItem)inventoryHeldItem.GetItem()).GetDyedNameAdjustment();
                    }
                    if (!name.Equals(toDye.GetItem().GetName()))
                    {
                        if (toDye.GetMaxQuantity() == 1)
                        {
                            toDye.SetItem(ItemDict.GetItemByName(name));
                            inventoryHeldItem.Subtract(1);
                        }
                        else
                        {
                            if (player.AddItemToInventory(ItemDict.GetItemByName(name), false, false))
                            {
                                inventoryHeldItem.Subtract(1);
                                toDye.Subtract(1);
                            }
                        }
                    }
                }
            }
        }

        public void TransitionUp()
        {
            interfaceState = InterfaceState.TRANSITION_TO_UP;
            transitionPosition = new Vector2(0, Plateau.RESOLUTION_Y);
            transitionAlpha = 1.0f;
        }

        public void TransitionDown()
        {
            interfaceState = InterfaceState.TRANSITION_TO_DOWN;
            transitionPosition = new Vector2(0, -Plateau.RESOLUTION_Y);
            transitionAlpha = 1.0f;
        }

        public void TransitionLeft()
        {
            interfaceState = InterfaceState.TRANSITION_TO_LEFT;
            transitionPosition = new Vector2(Plateau.RESOLUTION_X, 0);
            transitionAlpha = 1.0f;
        }

        public void TransitionRight()
        {
            interfaceState = InterfaceState.TRANSITION_TO_RIGHT;
            transitionPosition = new Vector2(-Plateau.RESOLUTION_X, 0);
            transitionAlpha = 1.0f;
        }

        public void TransitionFadeToBlack()
        {
            interfaceState = InterfaceState.TRANSITION_FADE_TO_BLACK;
            transitionAlpha = 0.0f;
            transitionPosition = new Vector2(0, 0);
        }

        public void TransitionFadeIn()
        {
            interfaceState = InterfaceState.TRANSITION_FADE_IN;
            transitionAlpha = 1.0f;
            transitionPosition = new Vector2(0, 0);
        }

        public void Update(float deltaTime, EntityPlayer player, RectangleF cameraBoundingBox, Area currentArea, World.TimeData timeData, World world)
        {
            heldItem = player.GetHeldItem();
            workbenchCraftablePosition.Clear();
            tooltipName = "";
            tooltipDescription = "";
            areaName = currentArea.GetName();
            zoneName = currentArea.GetZoneName(player.GetAdjustedPosition());
            if(displayGold == -1)
            {
                displayGold = player.GetGold();
            }
            int actualGold = player.GetGold();
            if (Math.Abs(actualGold - displayGold) <= 10)
            {
                displayGold = actualGold;
            }
            else
            {
                displayGold = Util.AdjustTowards(displayGold, actualGold, (Math.Abs(actualGold - displayGold) / 25) + 1);
            }


            appliedEffects = player.GetEffects();
            effectRects.Clear();
            float effectX = APPLIED_EFFECT_ANCHOR.X;
            float effectY = APPLIED_EFFECT_ANCHOR.Y;
            foreach(EntityPlayer.TimedEffect effect in appliedEffects)
            {
                RectangleF rect = new RectangleF(effectX, effectY, 10, 10);
                effectRects.Add(rect);
                if(rect.Contains(controller.GetMousePos()))
                {
                    tooltipName = effect.effect.name;
                    string hoursLeft = ((int)effect.timeRemaining / 60).ToString();
                    if (hoursLeft.Length == 1)
                    {
                        hoursLeft = "0" + hoursLeft;
                    }
                    string minutesLeft = ((int)effect.timeRemaining % 60).ToString();
                    if(minutesLeft.Length == 1)
                    {
                        minutesLeft = "0" + minutesLeft;
                    }
                    tooltipDescription = effect.effect.description + "\nActive for:  " + hoursLeft + ":" + minutesLeft;
                }
                effectX += APPLIED_EFFECT_DELTA_X;
            }

            isMouseLeftDown = controller.GetMouseLeftDown();
            isMouseRightDown = controller.GetMouseRightDown();
            isADown = controller.IsKeyDown(KeyBinds.LEFT);
            isDDown = controller.IsKeyDown(KeyBinds.RIGHT);
            isWDown = controller.IsKeyDown(KeyBinds.UP);
            isSDown = controller.IsKeyDown(KeyBinds.DOWN);
            currentNotification = player.GetCurrentNotification();
            isMouseOverCraftingMC = false;
            isMouseOverInventoryMC = false;
            isMouseOverScrapbookMC = false;
            isMouseOverSettingsMC = false;
            if (menuButtons[0].Contains(controller.GetMousePos()))
            {
                isMouseOverInventoryMC = true;
                tooltipName = "Inventory";
                player.IgnoreMouseInputThisFrame();
            } else if (menuButtons[1].Contains(controller.GetMousePos()))
            {
                isMouseOverScrapbookMC = true;
                tooltipName = "Scrapbook";
                player.IgnoreMouseInputThisFrame();
            } else if (menuButtons[2].Contains(controller.GetMousePos()))
            {
                isMouseOverCraftingMC = true;
                tooltipName = "Crafting";
                player.IgnoreMouseInputThisFrame();
            } else if (menuButtons[3].Contains(controller.GetMousePos()))
            {
                isMouseOverSettingsMC = true;
                tooltipName = "Settings";
                player.IgnoreMouseInputThisFrame();
            }
            
            

            if (interfaceState == InterfaceState.TRANSITION_TO_DOWN)
            {
                transitionPosition += new Vector2(0, TRANSITION_DELTA_Y * deltaTime);
                if (transitionPosition.Y > Plateau.RESOLUTION_Y)
                {
                    player.Unpause();
                    interfaceState = InterfaceState.NONE;
                }
            } else if (interfaceState == InterfaceState.TRANSITION_TO_UP)
            {
                transitionPosition += new Vector2(0, -TRANSITION_DELTA_Y * deltaTime);
                if (transitionPosition.Y < -Plateau.RESOLUTION_Y)
                {
                    player.Unpause();
                    interfaceState = InterfaceState.NONE;
                }
            } else if (interfaceState == InterfaceState.TRANSITION_TO_LEFT)
            {
                transitionPosition += new Vector2(-TRANSITION_DELTA_X * deltaTime, 0);
                if (transitionPosition.X < -Plateau.RESOLUTION_X)
                {
                    player.Unpause();
                    interfaceState = InterfaceState.NONE;
                }
            } else if (interfaceState == InterfaceState.TRANSITION_TO_RIGHT)
            {
                transitionPosition += new Vector2(TRANSITION_DELTA_X * deltaTime, 0);
                if(transitionPosition.X > Plateau.RESOLUTION_X)
                {
                    player.Unpause();
                    interfaceState = InterfaceState.NONE;
                }
            } else if (interfaceState == InterfaceState.TRANSITION_FADE_TO_BLACK)
            {
                transitionAlpha += TRANSITION_ALPHA_SPEED * deltaTime;
                //a bit diff than other transitions... used for cutscnee stuff - intended to do FadeToBlack THEN immediately afterwards FadeIn
            } else if (interfaceState == InterfaceState.TRANSITION_FADE_IN)
            {
                transitionAlpha -= TRANSITION_ALPHA_SPEED * deltaTime;
                if (transitionAlpha <= 0.0f)
                {
                    player.Unpause();
                    interfaceState = InterfaceState.NONE;
                }
            }
            else
            {
                dialogueBox.Update(deltaTime);
                if (currentDialogue != null && currentDialogueNumChars >= currentDialogue.dialogueText.Length)
                {
                    bounceArrow.Update(deltaTime);
                }

                interfaceState = player.GetInterfaceState();
                if(interfaceState == InterfaceState.CHEST)
                {
                    world.Pause();
                }
                targetTile = player.GetTargettedTile();
                targetEntityLastFrame = targetEntity;
                targetEntity = player.GetTargettedEntity();
                if(targetEntity == null)
                {
                    targetEntity = player.GetTargettedTileEntity();
                }

                if (targetEntity is IHaveHoveringInterface && targetEntity == targetEntityLastFrame)
                {
                    hoveringInterfaceOpacity += HOVERING_INTERFACE_OPACITY_SPEED * deltaTime;
                    hoveringInterfaceOpacity = Math.Min(HOVERING_INTERFACE_MAX_OPACITY, hoveringInterfaceOpacity);
                } else if (targetEntity is IHaveHoveringInterface && targetEntity != null && targetEntity != targetEntityLastFrame)
                {
                    hoveringInterfaceOpacity = 0;
                } else
                {
                    hoveringInterfaceOpacity -= HOVERING_INTERFACE_OPACITY_SPEED * deltaTime;
                    hoveringInterfaceOpacity = Math.Max(0, hoveringInterfaceOpacity);
                }

                selectedHotbarItemName = player.GetHeldItem().GetItem().GetName();
                inventorySelectedPosition = new Vector2(-100, -100);

                if (currentDialogue == null)
                {
                    currentDialogue = player.GetCurrentDialogue();
                    if (currentDialogue != null)
                    {
                        currentDialogueNumChars = 0;
                        player.SetToDefaultPose();
                        dialogueBox.SetLoop("anim");
                        bounceArrow.SetLoop("anim");
                    }
                }
                else
                {
                    currentDialogueNumChars++;
                    if (controller.GetMouseLeftPress() && !currentDialogue.Splits())
                    {
                        if (currentDialogueNumChars < currentDialogue.dialogueText.Length)
                        {
                            currentDialogueNumChars = currentDialogue.dialogueText.Length;
                        }
                        else
                        {
                            currentDialogueNumChars = 0;
                            bounceArrow.SetLoop("anim");
                            currentDialogue = currentDialogue.GetNext();
                            if (currentDialogue == null)
                            {
                                player.ClearDialogueNode();
                                player.IgnoreMouseInputThisFrame();
                            } else
                            {
                                currentDialogue.OnActivation(player);
                            }
                        }
                    }
                    else if (controller.GetMouseRightPress())
                    {
                        if (currentDialogueNumChars < currentDialogue.dialogueText.Length)
                        {
                            currentDialogueNumChars = currentDialogue.dialogueText.Length;
                        }
                    }
                    else if (controller.IsKeyPressed(KeyBinds.LEFT))
                    {
                        if (!currentDialogue.decisionLeftText.Equals(""))
                        {
                            currentDialogue = currentDialogue.decisionLeftNode;
                            currentDialogueNumChars = 0;
                            if (currentDialogue == null)
                            {
                                player.ClearDialogueNode();
                            }
                            else
                            {
                                currentDialogue.OnActivation(player);
                            }
                            bounceArrow.SetLoop("anim");
                        } 
                    }
                    else if (controller.IsKeyPressed(KeyBinds.RIGHT))
                    {
                        if (!currentDialogue.decisionRightText.Equals(""))
                        {
                            currentDialogue = currentDialogue.decisionRightNode;
                            currentDialogueNumChars = 0;
                            if (currentDialogue == null)
                            {
                                player.ClearDialogueNode();
                            }
                            else
                            {
                                currentDialogue.OnActivation(player);
                            }
                            bounceArrow.SetLoop("anim");
                        }

                    }
                    else if (controller.IsKeyPressed(KeyBinds.UP))
                    {
                        if (!currentDialogue.decisionUpText.Equals(""))
                        {
                            currentDialogue = currentDialogue.decisionUpNode;
                            currentDialogueNumChars = 0;
                            if (currentDialogue == null)
                            {
                                player.ClearDialogueNode();
                            }
                            else
                            {
                                currentDialogue.OnActivation(player);
                            }
                            bounceArrow.SetLoop("anim");
                        }
                    }
                    else if (controller.IsKeyPressed(KeyBinds.DOWN))
                    {
                        if (!currentDialogue.decisionDownText.Equals(""))
                        {
                            currentDialogue = currentDialogue.decisionDownNode;
                            currentDialogueNumChars = 0;
                            if (currentDialogue == null)
                            {
                                player.ClearDialogueNode();
                            }
                            else
                            {
                                currentDialogue.OnActivation(player);
                            }
                            bounceArrow.SetLoop("anim");
                        }
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    menuButtons[i] = new RectangleF(MENU_CONTROL_POSITION + new Vector2(0, i * MENU_DELTA_Y), MENU_BUTTON_SIZE);
                }

                Vector2 mousePosition = controller.GetMousePos();
                if ((controller.GetMouseLeftPress() || controller.GetMouseRightPress()) && currentDialogue == null && inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (menuButtons[0].Contains(mousePosition))
                    {
                        if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                        {
                            if (player.GetInterfaceState() == InterfaceState.INVENTORY)
                            {
                                player.SetInterfaceState(InterfaceState.NONE);
                                player.Unpause();
                                world.Unpause();
                            }
                            else
                            {
                                player.SetInterfaceState(InterfaceState.INVENTORY);
                                player.Pause();
                                world.Pause();
                            }
                            player.SetToDefaultPose();
                            player.IgnoreMouseInputThisFrame();
                            SoundSystem.PlayFX(SoundSystem.Sound.FX_TEST);
                        }
                    }
                    else if (menuButtons[1].Contains(mousePosition))
                    {
                        if (interfaceState == InterfaceState.SCRAPBOOK)
                        {
                            player.SetInterfaceState(InterfaceState.NONE);
                            player.Unpause();
                            world.Unpause();
                        }
                        else
                        {
                            OpenScrapbook(player, timeData, world);
                        }
                        player.IgnoreMouseInputThisFrame();
                    }
                    else if (menuButtons[2].Contains(mousePosition))
                    {
                        if (interfaceState == InterfaceState.CRAFTING)
                        {
                            player.SetInterfaceState(InterfaceState.NONE);
                            player.Unpause();
                            world.Unpause();
                        }
                        else
                        {
                            player.SetInterfaceState(InterfaceState.CRAFTING);
                            player.Pause();
                            world.Pause();
                        }
                        player.IgnoreMouseInputThisFrame();
                    }
                    else if (menuButtons[3].Contains(mousePosition))
                    {
                        if (interfaceState == InterfaceState.SETTINGS)
                        {
                            player.SetInterfaceState(InterfaceState.NONE);
                            player.Unpause();
                            world.Unpause();
                        }
                        else
                        {
                            player.SetInterfaceState(InterfaceState.SETTINGS);
                            player.Pause();
                            world.Pause();
                        }
                        player.IgnoreMouseInputThisFrame();
                    }
                }

                //place down a plaeable
                if (!player.DidInteractThisFrame())
                {
                    if ((player.GetHeldItem().GetItem() is PlaceableItem || player.GetHeldItem().GetItem() is BuildingBlockItem) && interfaceState == InterfaceState.NONE)
                    {
                        if (controller.GetMouseLeftPress() && player.GetHeldItem().GetItem() is PlaceableItem)
                        {
                            PlaceableItem item = (PlaceableItem)player.GetHeldItem().GetItem();
                            Vector2 mouseLocation = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, controller.GetMousePos());
                            Vector2 tile = new Vector2((int)(mouseLocation.X / 8), (int)(mouseLocation.Y / 8) - (item.GetPlaceableHeight() - 1));

                            if (item.GetPlacementType() == PlaceableItem.PlacementType.NORMAL)
                            {
                                bool isPlaceableLocationValid = currentArea.IsTileEntityPlacementValid((int)tile.X, (int)tile.Y, item.GetPlaceableWidth(), item.GetPlaceableHeight());

                                if (isPlaceableLocationValid)
                                {
                                    Vector2 placementLocation = new Vector2(tile.X * 8, tile.Y * 8);
                                    TileEntity toPlace = (TileEntity)EntityFactory.GetEntity(EntityType.USE_ITEM, item, tile, currentArea);
                                    currentArea.AddTileEntity(toPlace);
                                    player.GetHeldItem().Subtract(1);
                                    player.IgnoreMouseInputThisFrame();
                                    showPlaceableTexture = false;
                                    lastPlacedTile = tile;
                                }
                                else
                                {
                                    if (showPlaceableTexture)
                                    {
                                        player.AddNotification(new EntityPlayer.Notification("This can\'t be placed here.", Color.Red));
                                    }
                                }
                            }
                            else if (item.GetPlacementType() == PlaceableItem.PlacementType.WALL)
                            {
                                bool isWallLocationValid = currentArea.IsWallEntityPlacementValid((int)tile.X, (int)tile.Y, item.GetPlaceableWidth(), item.GetPlaceableHeight());

                                if (isWallLocationValid)
                                {
                                    Vector2 placementLocation = new Vector2(tile.X * 8, tile.Y * 8);
                                    TileEntity toPlace = (TileEntity)EntityFactory.GetEntity(EntityType.USE_ITEM, item, tile, currentArea);
                                    currentArea.AddWallEntity(toPlace);
                                    player.GetHeldItem().Subtract(1);
                                    player.IgnoreMouseInputThisFrame();
                                    showPlaceableTexture = false;
                                    lastPlacedTile = tile;
                                }
                                else
                                {
                                    if (showPlaceableTexture)
                                    {
                                        player.AddNotification(new EntityPlayer.Notification("This can\'t be placed here.", Color.Red));
                                    }
                                }
                            }
                            else if (item.GetPlacementType() == PlaceableItem.PlacementType.CEILING)
                            {

                            }
                            else if (item.GetPlacementType() == PlaceableItem.PlacementType.WALLPAPER)
                            {
                                bool isWallpaperLocationValid = currentArea.IsWallpaperPlacementValid((int)tile.X, (int)tile.Y, item.GetPlaceableWidth(), item.GetPlaceableHeight());
                                if (isWallpaperLocationValid)
                                {
                                    Vector2 placementLocation = new Vector2(tile.X * 8, tile.Y * 8);
                                    PEntityWallpaper toPlace = (PEntityWallpaper)EntityFactory.GetEntity(EntityType.USE_ITEM, item, tile, currentArea);
                                    currentArea.AddWallpaperEntity(toPlace);
                                    player.GetHeldItem().Subtract(1);
                                    player.IgnoreMouseInputThisFrame();
                                    showPlaceableTexture = false;
                                    lastPlacedTile = tile;
                                }
                                else
                                {
                                    if (showPlaceableTexture)
                                    {
                                        player.AddNotification(new EntityPlayer.Notification("This can\'t be placed here.", Color.Red));
                                    }
                                }
                            }
                            else if (item.GetPlacementType() == PlaceableItem.PlacementType.FLOOR)
                            {
                                bool isFloorPlacementValid = currentArea.IsFloorEntityPlacementValid((int)tile.X, (int)tile.Y, item.GetPlaceableWidth());
                                if (isFloorPlacementValid)
                                {
                                    Vector2 placementLocation = new Vector2(tile.X * 8, tile.Y * 8);
                                    TileEntity toPlace = (TileEntity)EntityFactory.GetEntity(EntityType.USE_ITEM, item, tile, currentArea);
                                    currentArea.AddTileEntity(toPlace);
                                    player.GetHeldItem().Subtract(1);
                                    player.IgnoreMouseInputThisFrame();
                                    showPlaceableTexture = false;
                                    lastPlacedTile = tile;
                                }
                                else
                                {
                                    if (showPlaceableTexture)
                                    {
                                        player.AddNotification(new EntityPlayer.Notification("This can\'t be placed here.", Color.Red));
                                    }
                                }
                            }
                        }
                        else if (controller.GetMouseLeftPress() && player.GetHeldItem().GetItem() is BuildingBlockItem)
                        {

                            BuildingBlockItem item = (BuildingBlockItem)player.GetHeldItem().GetItem();
                            Vector2 mouseLocation = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, controller.GetMousePos());
                            Vector2 tile = new Vector2((int)(mouseLocation.X / 8), (int)(mouseLocation.Y / 8));
                            bool isBBLocationValid = currentArea.IsBuildingBlockPlacementValid((int)tile.X, (int)tile.Y, item.GetBlockType() == BlockType.BLOCK);
                            if (item.GetBlockType() == BlockType.BLOCK && player.GetCollisionRectangle().Intersects(new RectangleF(tile * new Vector2(8, 8), new Vector2(8, 4))))
                            {
                                isBBLocationValid = false;
                            }

                            if (isBBLocationValid)
                            {
                                BuildingBlock toPlace = new BuildingBlock(item, tile, item.GetPlacedTexture(), item.GetBlockType());
                                currentArea.AddBuildingBlock(toPlace);
                                player.GetHeldItem().Subtract(1);
                                player.IgnoreMouseInputThisFrame();
                                showPlaceableTexture = false;
                                lastPlacedTile = tile;
                            }
                            else
                            {
                                if (showPlaceableTexture)
                                {
                                    player.AddNotification(new EntityPlayer.Notification("There\'s something in the way.", Color.Red));
                                }
                            }
                        }
                        else if (controller.GetMouseLeftPress()) //remove placeable
                        {
                            Vector2 location = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, controller.GetMousePos());
                            Vector2 tile = new Vector2((int)(location.X / 8), (int)(location.Y / 8));
                            Item itemForm = currentArea.GetTileEntityItemForm((int)tile.X, (int)tile.Y);
                            if (itemForm != ItemDict.NONE)
                            {
                                currentArea.RemoveTileEntity(player, (int)tile.X, (int)tile.Y, world);
                                player.IgnoreMouseInputThisFrame();
                            }
                            else
                            {
                                itemForm = currentArea.GetBuildingBlockItemForm((int)tile.X, (int)tile.Y);
                                if (itemForm != ItemDict.NONE)
                                {
                                    currentArea.RemoveBuildingBlock((int)tile.X, (int)tile.Y, player, world);
                                    player.IgnoreMouseInputThisFrame();
                                }
                                else
                                {
                                    itemForm = currentArea.GetWallEntityItemForm((int)tile.X, (int)tile.Y);
                                    if (itemForm != ItemDict.NONE)
                                    {
                                        currentArea.RemoveWallEntity(player, (int)tile.X, (int)tile.Y, world);
                                        player.IgnoreMouseInputThisFrame();
                                    }
                                    else
                                    {
                                        itemForm = currentArea.GetWallpaperItemForm((int)tile.X, (int)tile.Y);
                                        if (itemForm != ItemDict.NONE)
                                        {
                                            currentArea.RemoveWallpaperEntity(player, (int)tile.X, (int)tile.Y, world);
                                            player.IgnoreMouseInputThisFrame();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (controller.IsKeyPressed(KeyBinds.CRAFTING) && currentDialogue == null && inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (interfaceState == InterfaceState.CRAFTING)
                    {
                        player.SetInterfaceState(InterfaceState.NONE);
                        player.Unpause();
                        world.Unpause();
                    }
                    else
                    {
                        player.SetInterfaceState(InterfaceState.CRAFTING);
                        player.Pause();
                        world.Pause();
                    }
                }

                if (controller.IsKeyPressed(KeyBinds.OPEN_SCRAPBOOK) && currentDialogue == null && inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (interfaceState == InterfaceState.SCRAPBOOK)
                    {
                        player.SetInterfaceState(InterfaceState.NONE);
                        player.Unpause();
                        world.Unpause();
                    }
                    else
                    {
                        OpenScrapbook(player, timeData, world);
                    }
                }

                if (controller.IsKeyPressed(KeyBinds.SETTINGS) && currentDialogue == null && inventoryHeldItem.GetItem() == ItemDict.NONE)
                {
                    if (interfaceState == InterfaceState.SETTINGS)
                    {
                        player.SetInterfaceState(InterfaceState.NONE);
                        player.Unpause();
                        world.Unpause();
                    }
                    else
                    {
                        player.SetInterfaceState(InterfaceState.SETTINGS);
                        player.Pause();
                        world.Pause();
                    }
                }

                Vector2 locationT = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, controller.GetMousePos());
                Vector2 tileT = new Vector2((int)(locationT.X / 8), (int)(locationT.Y / 8));
                Item itemFormT = currentArea.GetTileEntityItemForm((int)tileT.X, (int)tileT.Y);

                SetKeyActionTexts(player);
                SetMouseActionTexts(player, itemFormT != ItemDict.NONE);

                timeSinceItemCollectedTooltipAdded += deltaTime;
                playerPosition = player.GetAdjustedPosition();
                switch (timeData.season)
                {
                    case (World.Season.SPRING):
                        seasonIndex = 0;
                        break;
                    case (World.Season.SUMMER):
                        seasonIndex = 1;
                        break;
                    case (World.Season.AUTUMN):
                        seasonIndex = 2;
                        break;
                    case (World.Season.WINTER):
                        seasonIndex = 3;
                        break;
                }
                dayIndex = timeData.day;
                hourOnesIndex = timeData.hour % 10;
                hourTensIndex = timeData.hour / 10;
                minuteOnesIndex = timeData.minute % 10;
                minuteTensIndex = timeData.minute / 10;

                isHoldingPlaceable = false;
                gridLocation = currentArea.GetPositionOfTile((int)(cameraBoundingBox.Left / 8), (int)(cameraBoundingBox.Top / 8));
                gridLocation.X -= 0.5f;
                gridLocation.Y -= 0.5f;

                if (controller.IsKeyPressed(KeyBinds.OPEN_INVENTORY) && inventoryHeldItem.GetItem() == ItemDict.NONE && currentDialogue == null)
                {
                    if (player.GetInterfaceState() == InterfaceState.INVENTORY || player.GetInterfaceState() == InterfaceState.CHEST)
                    {
                        player.SetInterfaceState(InterfaceState.NONE);
                        player.Unpause();
                        world.Unpause();
                    }
                    else
                    {
                        player.SetInterfaceState(InterfaceState.INVENTORY);
                        player.Pause();
                        world.Pause();
                    }
                    player.SetToDefaultPose();
                }

                if (player.GetInterfaceState() == InterfaceState.INVENTORY || player.GetInterfaceState() == InterfaceState.CHEST)
                {
                    player.UpdateSprite(0);
                    playerSprite = player.GetSprite();
                    hair = (ClothingItem)player.GetHair().GetItem();
                    hat = (ClothingItem)player.GetHat().GetItem();
                    shirt = (ClothingItem)player.GetShirt().GetItem();
                    outerwear = (ClothingItem)player.GetOuterwear().GetItem();
                    pants = (ClothingItem)player.GetPants().GetItem();
                    socks = (ClothingItem)player.GetSocks().GetItem();
                    shoes = (ClothingItem)player.GetShoes().GetItem();
                    gloves = (ClothingItem)player.GetGloves().GetItem();
                    earrings = (ClothingItem)player.GetEarrings().GetItem();
                    scarf = (ClothingItem)player.GetScarf().GetItem();
                    glasses = (ClothingItem)player.GetGlasses().GetItem();
                    back = (ClothingItem)player.GetBack().GetItem();
                    sailcloth = (ClothingItem)player.GetSailcloth().GetItem();
                    accessory1 = player.GetAccessory1().GetItem();
                    accessory2 = player.GetAccessory2().GetItem();
                    accessory3 = player.GetAccessory3().GetItem();

                    bool hovering = false;
                    Vector2 mouse = controller.GetMousePos();
                    for (int i = 0; i < itemRectangles.Length; i++)
                    {
                        if (itemRectangles[i].Contains(mouse))
                        {
                            inventorySelectedPosition = itemRectangles[i].TopLeft;
                            ItemStack hovered = player.GetInventoryItemStack(i);
                            if (hovered.GetItem() == ItemDict.NONE)
                            {
                                break;
                            }
                            hovering = true;
                            tooltipDescription = hovered.GetItem().GetDescription() + (hovered.GetItem().GetValue() > 0 ? "\nValue: " + hovered.GetItem().GetValue() : "");
                            tooltipName = hovered.GetItem().GetName() + (hovered.GetMaxQuantity() == 1 ? "" : " x" + hovered.GetQuantity());
                        }
                    }

                    if (interfaceState == InterfaceState.INVENTORY)
                    {
                        if (!hovering)
                        {
                            hovering = CheckClothingTooltips(player, mouse);
                        }
                    }
                    else if (interfaceState == InterfaceState.CHEST)
                    {
                        //TODO
                        PEntityChest chest = (PEntityChest)player.GetTargettedTileEntity();
                        for (int i = 0; i < chestRectangles.Length; i++)
                        {
                            if (chestRectangles[i].Contains(mouse))
                            {
                                inventorySelectedPosition = chestRectangles[i].TopLeft;
                                ItemStack hovered = chest.GetInventoryItemStack(i);
                                if (hovered.GetItem() == ItemDict.NONE)
                                {
                                    break;
                                }
                                hovering = true;
                                tooltipDescription = hovered.GetItem().GetDescription() + (hovered.GetItem().GetValue() > 0 ? "\nValue: " + hovered.GetItem().GetValue() : "");
                                tooltipName = hovered.GetItem().GetName() + (hovered.GetMaxQuantity() == 1 ? "" : " x" + hovered.GetQuantity());
                            }
                        }
                    }

                    if (garbageCanRectangle.Contains(mousePosition))
                    {
                        hovering = true;
                        tooltipName = "Garbage Can";
                        tooltipDescription = "WARNING: disposal\nis permanent.";
                    }

                    //this could be better but whatever
                    if (controller.GetMouseLeftPress())
                    {
                        Vector2 mousePos = controller.GetMousePos();
                        if (garbageCanRectangle.Contains(mousePos))
                        {
                            if (!inventoryHeldItem.GetItem().HasTag(Item.Tag.NO_TRASH))
                            {
                                inventoryHeldItem = new ItemStack(ItemDict.NONE, 0);
                            }
                        }

                        for (int i = 0; i < itemRectangles.Length; i++)
                        {
                            if (itemRectangles[i].Contains(mousePos))
                            {
                                ItemStack selected = player.GetInventoryItemStack(i);
                                if (controller.IsKeyDown(KeyBinds.SHIFT))
                                {
                                    if ((selected.GetItem() is ClothingItem || selected.GetItem().HasTag(Item.Tag.ACCESSORY)) && interfaceState == InterfaceState.INVENTORY)
                                    {
                                        ShiftSwapClothingItem(player, i);
                                    }
                                    else if (interfaceState == InterfaceState.INVENTORY)
                                    {
                                        if (i >= 10)
                                        {
                                            for (int j = 0; j < GameplayInterface.HOTBAR_LENGTH; j++)
                                            {
                                                ItemStack hotbarStack = player.GetInventoryItemStack(j);
                                                if (hotbarStack.GetItem() == selected.GetItem() && !hotbarStack.IsFull())
                                                {
                                                    int overflow = hotbarStack.Add(selected.GetQuantity());
                                                    selected.SetQuantity(overflow);
                                                    if (selected.GetQuantity() == 0)
                                                    {
                                                        player.RemoveItemStackAt(i);
                                                    }
                                                }
                                            }
                                            if (selected.GetQuantity() > 0)
                                            {
                                                for (int j = 0; j < GameplayInterface.HOTBAR_LENGTH; j++)
                                                {
                                                    ItemStack hotbarStack = player.GetInventoryItemStack(j);
                                                    if (hotbarStack.GetItem() == ItemDict.NONE)
                                                    {
                                                        player.AddItemStackAt(selected, j);
                                                        player.RemoveItemStackAt(i);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            for (int j = 10; j < EntityPlayer.INVENTORY_SIZE; j++)
                                            {
                                                ItemStack inventoryStack = player.GetInventoryItemStack(j);
                                                if (inventoryStack.GetItem() == selected.GetItem() && !inventoryStack.IsFull())
                                                {
                                                    int overflow = inventoryStack.Add(selected.GetQuantity());
                                                    selected.SetQuantity(overflow);
                                                    if (selected.GetQuantity() == 0)
                                                    {
                                                        player.RemoveItemStackAt(i);
                                                    }
                                                }
                                            }
                                            if (selected.GetQuantity() > 0)
                                            {
                                                for (int j = 10; j < EntityPlayer.INVENTORY_SIZE; j++)
                                                {
                                                    ItemStack inventoryStack = player.GetInventoryItemStack(j);
                                                    if (inventoryStack.GetItem() == ItemDict.NONE)
                                                    {
                                                        player.AddItemStackAt(selected, j);
                                                        player.RemoveItemStackAt(i);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (interfaceState == InterfaceState.CHEST)
                                    {
                                        PEntityChest chest = (PEntityChest)player.GetTargettedTileEntity();
                                        for (int j = 0; j < PEntityChest.INVENTORY_SIZE; j++)
                                        {
                                            ItemStack inventoryStack = chest.GetInventoryItemStack(j);
                                            if (inventoryStack.GetItem() == selected.GetItem() && !inventoryStack.IsFull())
                                            {
                                                int overflow = inventoryStack.Add(selected.GetQuantity());
                                                selected.SetQuantity(overflow);
                                                if (selected.GetQuantity() == 0)
                                                {
                                                    player.RemoveItemStackAt(i);
                                                }
                                            }
                                        }
                                        if (selected.GetQuantity() > 0)
                                        {
                                            for (int j = 0; j < PEntityChest.INVENTORY_SIZE; j++)
                                            {
                                                ItemStack inventoryStack = chest.GetInventoryItemStack(j);
                                                if (inventoryStack.GetItem() == ItemDict.NONE)
                                                {
                                                    chest.AddItemStackAt(selected, j);
                                                    player.RemoveItemStackAt(i);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                                {
                                    inventoryHeldItem = selected;
                                    player.RemoveItemStackAt(i);
                                }
                                else if (selected.GetItem() == inventoryHeldItem.GetItem())
                                {
                                    inventoryHeldItem.SetQuantity(selected.Add(inventoryHeldItem.GetQuantity()));
                                    if (inventoryHeldItem.GetQuantity() == 0)
                                    {
                                        inventoryHeldItem = new ItemStack(ItemDict.NONE, 0);
                                    }
                                }
                                else
                                {
                                    player.AddItemStackAt(inventoryHeldItem, i);
                                    inventoryHeldItem = selected;
                                }
                            }
                        }

                        if (player.GetInterfaceState() == InterfaceState.INVENTORY)
                        {
                            CheckClothingClick(player, mousePos, controller.IsKeyDown(KeyBinds.SHIFT));
                        }
                        else if (player.GetInterfaceState() == InterfaceState.CHEST) //chest left click
                        {
                            PEntityChest chest = (PEntityChest)player.GetTargettedTileEntity();
                            for (int i = 0; i < chestRectangles.Length; i++)
                            {
                                if (chestRectangles[i].Contains(mousePos))
                                {
                                    ItemStack selected = chest.GetInventoryItemStack(i);
                                    if (controller.IsKeyDown(KeyBinds.SHIFT)) //shift click item from chest to inventory
                                    {
                                        bool placedInInventory = false;
                                        //attempt to add to inventory
                                        for (int j = 10; j < EntityPlayer.INVENTORY_SIZE; j++)
                                        {
                                            ItemStack inventoryStack = player.GetInventoryItemStack(j);
                                            if (inventoryStack.GetItem() == selected.GetItem() && !inventoryStack.IsFull())
                                            {
                                                int overflow = inventoryStack.Add(selected.GetQuantity());
                                                selected.SetQuantity(overflow);
                                                if (selected.GetQuantity() == 0)
                                                {
                                                    chest.RemoveItemStackAt(i);
                                                    placedInInventory = true;
                                                }
                                            }
                                        }
                                        if (selected.GetQuantity() > 0) //if stack not found to combine; or extra after combining, place in a NONE slot
                                        {
                                            for (int j = 10; j < EntityPlayer.INVENTORY_SIZE; j++)
                                            {
                                                ItemStack inventoryStack = player.GetInventoryItemStack(j);
                                                if (inventoryStack.GetItem() == ItemDict.NONE)
                                                {
                                                    player.AddItemStackAt(selected, j);
                                                    chest.RemoveItemStackAt(i);
                                                    placedInInventory = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (!placedInInventory)
                                        {
                                            //attempt to add to hotbar
                                            for (int j = 0; j < HOTBAR_LENGTH; j++)
                                            {
                                                ItemStack inventoryStack = player.GetInventoryItemStack(j);
                                                if (inventoryStack.GetItem() == selected.GetItem() && !inventoryStack.IsFull())
                                                {
                                                    int overflow = inventoryStack.Add(selected.GetQuantity());
                                                    selected.SetQuantity(overflow);
                                                    if (selected.GetQuantity() == 0)
                                                    {
                                                        chest.RemoveItemStackAt(i);
                                                    }
                                                }
                                            }
                                            if (selected.GetQuantity() > 0) //if stack not found to combine; or extra after combining, place in a NONE slot
                                            {
                                                for (int j = 0; j < HOTBAR_LENGTH; j++)
                                                {
                                                    ItemStack inventoryStack = player.GetInventoryItemStack(j);
                                                    if (inventoryStack.GetItem() == ItemDict.NONE)
                                                    {
                                                        player.AddItemStackAt(selected, j);
                                                        chest.RemoveItemStackAt(i);
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                    }
                                    else if (inventoryHeldItem.GetItem() == ItemDict.NONE) //if not shifting, picking item up
                                    {
                                        inventoryHeldItem = selected;
                                        chest.RemoveItemStackAt(i);
                                    }
                                    else if (selected.GetItem() == inventoryHeldItem.GetItem()) //adding held item into existing stack
                                    {
                                        inventoryHeldItem.SetQuantity(selected.Add(inventoryHeldItem.GetQuantity()));
                                        if (inventoryHeldItem.GetQuantity() == 0)
                                        {
                                            inventoryHeldItem = new ItemStack(ItemDict.NONE, 0);
                                        }
                                    }
                                    else //if not shifting, placing/swapping an item down
                                    {
                                        chest.AddItemStackAt(inventoryHeldItem, i);
                                        inventoryHeldItem = selected;
                                    }
                                }
                            }
                        }
                    }
                    else if (controller.GetMouseRightPress()) //right click...
                    {
                        Vector2 mousePos = controller.GetMousePos();
                        for (int i = 0; i < itemRectangles.Length; i++)
                        {
                            if (itemRectangles[i].Contains(mousePos))
                            {
                                ItemStack selected = player.GetInventoryItemStack(i);
                                if (inventoryHeldItem != null && inventoryHeldItem.GetItem() is DyeItem && selected.GetItem() != ItemDict.NONE)
                                {
                                    if (controller.IsKeyDown(KeyBinds.SHIFT))
                                    {
                                        for (int j = 0; j < 9; j++)
                                        {
                                            TryApplyDye(selected, player);
                                        }
                                    }
                                    TryApplyDye(selected, player);
                                }
                                else if (selected.GetItem() is ClothingItem && controller.IsKeyDown(KeyBinds.SHIFT) && interfaceState == InterfaceState.INVENTORY)
                                {
                                    ShiftSwapClothingItem(player, i);
                                }
                                else if (inventoryHeldItem.GetItem() == ItemDict.NONE)
                                {
                                    if (selected.GetQuantity() > 1)
                                    {
                                        inventoryHeldItem = new ItemStack(selected.GetItem(), (int)Math.Floor((double)selected.GetQuantity() / 2));
                                        selected.SetQuantity((int)Math.Ceiling((double)selected.GetQuantity() / 2));
                                    }
                                }
                                else
                                {
                                    if (selected.GetItem() == ItemDict.NONE)
                                    {
                                        player.AddItemStackAt(new ItemStack(inventoryHeldItem.GetItem(), 1), i);
                                        inventoryHeldItem.Subtract(1);
                                        if (inventoryHeldItem.GetQuantity() == 0)
                                        {
                                            inventoryHeldItem = new ItemStack(ItemDict.NONE, 0);
                                        }
                                    }
                                    else if (selected.GetItem() == inventoryHeldItem.GetItem())
                                    {
                                        if (selected.Add(1) != 1)
                                        {
                                            inventoryHeldItem.Subtract(1);
                                            if (inventoryHeldItem.GetQuantity() == 0)
                                            {
                                                inventoryHeldItem = new ItemStack(ItemDict.NONE, 0);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (player.GetInterfaceState() == InterfaceState.INVENTORY)
                        {
                            CheckClothingClick(player, mousePos, controller.IsKeyDown(KeyBinds.SHIFT));
                        }
                        else if (player.GetInterfaceState() == InterfaceState.CHEST) //chest right click
                        {
                            PEntityChest chest = (PEntityChest)player.GetTargettedTileEntity();
                            for (int i = 0; i < chestRectangles.Length; i++)
                            {
                                if (chestRectangles[i].Contains(mousePos))
                                {
                                    ItemStack selected = chest.GetInventoryItemStack(i);
                                    if (inventoryHeldItem.GetItem() == ItemDict.NONE) //picking up half a stack
                                    {
                                        if (selected.GetQuantity() > 1)
                                        {
                                            inventoryHeldItem = new ItemStack(selected.GetItem(), (int)Math.Floor((double)selected.GetQuantity() / 2));
                                            selected.SetQuantity((int)Math.Ceiling((double)selected.GetQuantity() / 2));
                                        }
                                    }
                                    else //placing down a single of a held item into chest
                                    {
                                        if (selected.GetItem() == ItemDict.NONE)
                                        {
                                            chest.AddItemStackAt(new ItemStack(inventoryHeldItem.GetItem(), 1), i);
                                            inventoryHeldItem.Subtract(1);
                                            if (inventoryHeldItem.GetQuantity() == 0)
                                            {
                                                inventoryHeldItem = new ItemStack(ItemDict.NONE, 0);
                                            }
                                        }
                                        else if (selected.GetItem() == inventoryHeldItem.GetItem()) //combining stacks...
                                        {
                                            if (selected.Add(1) != 1)
                                            {
                                                inventoryHeldItem.Subtract(1);
                                                if (inventoryHeldItem.GetQuantity() == 0)
                                                {
                                                    inventoryHeldItem = new ItemStack(ItemDict.NONE, 0);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (interfaceState == InterfaceState.SCRAPBOOK)
                { //if scrapbook is open 
                    scrapbookHoverTab = -1;
                    scrapbookHoverPage = -1;
                    for (int i = 0; i < scrapbookTabs.Length; i++)
                    {
                        if (scrapbookTabs[i].Contains(mousePosition))
                        {
                            scrapbookHoverTab = i;
                            if (controller.GetMouseLeftDown() || controller.GetMouseRightDown())
                            {
                                scrapbookCurrentTab = i;
                            }
                        }
                    }

                    for (int i = 0; i < scrapbookTitles.Length; i++)
                    {
                        if (scrapbookTitles[i].Contains(mousePosition))
                        {
                            scrapbookHoverPage = (scrapbookCurrentTab * 10) + i;
                            if (controller.GetMouseLeftDown() || controller.GetMouseRightDown())
                            {
                                scrapbookCurrentPage = (scrapbookCurrentTab * 10) + i;
                            }
                        }
                    }

                }
                else if (interfaceState == InterfaceState.EXIT)
                {
                    if (controller.GetMouseLeftPress() || controller.GetMouseRightPress())
                    {
                        if (exitPromptButton.Contains(mousePosition))
                        {
                            player.SetInterfaceState(InterfaceState.EXIT_CONFIRMED);
                        }
                    }
                }
                else if (interfaceState == InterfaceState.SETTINGS)
                {
                    if (controller.GetMouseLeftPress() || controller.GetMouseRightPress())
                    {
                        if (settingsRectangles[0].Contains(mousePosition))
                        {
                            GameState.FlipFlag(GameState.FLAG_SETTINGS_HIDE_CONTROLS);
                        }
                        else if (settingsRectangles[1].Contains(mousePosition))
                        {
                            GameState.FlipFlag(GameState.FLAG_SETTINGS_HIDE_GRID);
                        }
                        else if (settingsRectangles[2].Contains(mousePosition))
                        {
                            GameState.FlipFlag(GameState.FLAG_SETTINGS_HIDE_RETICLE);
                        }
                    }
                } else if (interfaceState == InterfaceState.CRAFTING) //workbench
                {
                    if (selectedRecipe != null)
                    {
                        for (int j = 0; j < numMaterialsOfRecipe.Length; j++)
                        {
                            numMaterialsOfRecipe[j] = 0;
                        }
                        for (int k = 0; k < selectedRecipe.components.Length; k++)
                        {
                            numMaterialsOfRecipe[k] = player.GetNumberOfItemInInventory(selectedRecipe.components[k].GetItem());
                        }
                    }

                    if (controller.GetMouseLeftDown()) //change tab
                    {
                        for (int i = 0; i < workbenchTabRectangles.Length; i++)
                        {
                            if (workbenchTabRectangles[i].Contains(mousePosition))
                            {
                                workbenchCurrentTab = i;
                                workbenchCurrentPage = 0;
                                selectedRecipeSlot = -1;
                                break;
                            }
                        }
                    }

                    if (workbenchLeftArrowRectangle.Contains(mousePosition)) //left arrow
                    {
                        hoveringLeftArrow = true;
                        if(controller.GetMouseLeftPress())
                        {
                            int numPagesInCurrentTab = 0;
                            switch (workbenchCurrentTab)
                            {
                                case 0:
                                    numPagesInCurrentTab = (GameState.NumMachineRecipes()-1) / 15;
                                    break;
                                case 1:
                                    numPagesInCurrentTab = (GameState.NumScaffoldingRecipes()-1) / 15;
                                    break;
                                case 2:
                                    numPagesInCurrentTab = (GameState.NumFurnitureRecipes()-1) / 15;
                                    break;
                                case 3:
                                    numPagesInCurrentTab = (GameState.NumWallFloorRecipes()-1) / 15;
                                    break;
                                case 4:
                                    numPagesInCurrentTab = (GameState.NumClothingRecipes()-1) / 15;
                                    break;
                            }

                            if(numPagesInCurrentTab != 0)
                            {
                                selectedRecipeSlot = -1;
                                workbenchCurrentPage--;
                                if(workbenchCurrentPage == -1)
                                {
                                    workbenchCurrentPage = numPagesInCurrentTab;
                                }
                            }
                        }
                    }
                    else if (workbenchRightArrowRectangle.Contains(mousePosition)) //right arrow
                    {
                        hoveringRightArrow = true;
                        if(controller.GetMouseLeftPress())
                        {
                            int numPagesInCurrentTab = 0;
                            switch(workbenchCurrentTab)
                            {
                                case 0:
                                    numPagesInCurrentTab = (GameState.NumMachineRecipes()-1) / 15;
                                    break;
                                case 1:
                                    numPagesInCurrentTab = (GameState.NumScaffoldingRecipes()-1) / 15;
                                    break;
                                case 2:
                                    numPagesInCurrentTab = (GameState.NumFurnitureRecipes()-1) / 15;
                                    break;
                                case 3:
                                    numPagesInCurrentTab = (GameState.NumWallFloorRecipes()-1) / 15;
                                    break;
                                case 4:
                                    numPagesInCurrentTab = (GameState.NumClothingRecipes()-1) / 15;
                                    break;
                            }

                            if (numPagesInCurrentTab > workbenchCurrentPage)
                            {
                                workbenchCurrentPage++;
                                selectedRecipeSlot = -1;
                            } else if (numPagesInCurrentTab == workbenchCurrentPage)
                            {
                                workbenchCurrentPage = 0;
                            }
                        }
                    }
                    else
                    {
                        hoveringLeftArrow = false;
                        hoveringRightArrow = false;
                    }

                    if (workbenchCraftButtonRectangle.Contains(mousePosition)) //craft button
                    {
                        hoveringCraftButton = true;
                        if (controller.GetMouseLeftPress())
                        {
                            for (int j = 0; j < (controller.IsKeyDown(KeyBinds.SHIFT) ? 5 : 1); j++) { 
                                //check if player has all needed to craft...
                                bool possible = true;
                                foreach(ItemStack stack in selectedRecipe.components)
                                {
                                    if (!player.HasItemStack(stack))
                                        possible = false;
                                }

                                //if so, attempt to remove those items and add crafted stuff to inv
                                if (possible)
                                {
                                    //remove components
                                    foreach (ItemStack stack in selectedRecipe.components)
                                    {
                                        player.RemoveItemStackFromInventory(stack);
                                    }

                                    //add crafted stuff
                                    bool addedAll = true;
                                    for (int i = 0; i < selectedRecipe.result.GetQuantity(); i++)
                                    {
                                        if (!player.AddItemToInventory(selectedRecipe.result.GetItem())) //if it doesn't fit...
                                        {
                                            //take back added items so far and abort
                                            player.RemoveItemStackFromInventory(new ItemStack(selectedRecipe.result.GetItem(), i));
                                            addedAll = false;
                                            break;
                                        }
                                    }

                                    //if unsuccessful refund the items used
                                    if (!addedAll)
                                    {
                                        foreach (ItemStack stack in selectedRecipe.components)
                                        {
                                            for (int i = 0; i < stack.GetQuantity(); i++)
                                            {
                                                player.AddItemToInventory(stack.GetItem());
                                            }
                                        }
                                        player.AddNotification(new EntityPlayer.Notification("I don/'t have enough space in my bag to craft this.", Color.Red));
                                    }
                                }
                            }
                        }
                    } else
                    {
                        hoveringCraftButton = false;
                    }

                    for(int i = 0; i < 15; i++)
                    {
                        switch (workbenchCurrentTab)
                        {
                            case 0:
                                currentRecipes[i] = GameState.GetMachineRecipe((workbenchCurrentPage * 15) + i);
                                break;
                            case 1:
                                currentRecipes[i] = GameState.GetScaffoldingRecipe((workbenchCurrentPage * 15) + i);
                                break;
                            case 2:
                                currentRecipes[i] = GameState.GetFurnitureRecipe((workbenchCurrentPage * 15) + i);
                                break;
                            case 3:
                                currentRecipes[i] = GameState.GetWallFloorRecipe((workbenchCurrentPage * 15) + i);
                                break;
                            case 4:
                                currentRecipes[i] = GameState.GetClothingRecipe((workbenchCurrentPage * 15) + i);
                                break;
                        } 
                    }

                    workbenchInventorySelectedPosition = new Vector2(-1000, -1000);
                    for (int i = 0; i < workbenchBlueprintRectangles.Length; i++) //blueprint rects
                    {
                        if (workbenchBlueprintRectangles[i].Contains(mousePosition))
                        {
                            workbenchInventorySelectedPosition = workbenchBlueprintRectangles[i].TopLeft;
                            if(currentRecipes[i] != null)
                            {
                                if (currentRecipes[i].haveBlueprint)
                                {
                                    Item result = currentRecipes[i].result.GetItem();
                                    tooltipName = result.GetName();
                                } else
                                {
                                    tooltipName = "???";
                                }
                            }
                            if (controller.GetMouseLeftDown() && currentRecipes[i] != null && currentRecipes[i].haveBlueprint)
                            {
                                selectedRecipeSlot = i;
                                selectedRecipe = currentRecipes[i];
                                for (int j = 0; j < numMaterialsOfRecipe.Length; j++)
                                {
                                    numMaterialsOfRecipe[j] = 0;
                                }
                                for (int k = 0; k < selectedRecipe.components.Length; k++)
                                {
                                    numMaterialsOfRecipe[k] = player.GetNumberOfItemInInventory(selectedRecipe.components[k].GetItem());
                                }
                            }
                        }
                        if (currentRecipes[i] != null && currentRecipes[i].haveBlueprint)
                        {
                            bool possible = true;
                            foreach (ItemStack stack in currentRecipes[i].components)
                            {
                                if (!player.HasItemStack(stack))
                                    possible = false;
                            }
                            if (possible)
                            {
                                workbenchCraftablePosition.Add(workbenchBlueprintRectangles[i].TopLeft);
                            }
                        }
                    }

                    //set tooltip
                    if(selectedRecipe != null)
                    {
                        if(new RectangleF(WORKBENCH_SELECTED_RECIPE_POSITION, new Vector2(16, 16)).Contains(mousePosition)) {
                            tooltipName = selectedRecipe.result.GetItem().GetName();
                        } else if (new RectangleF(WORKBENCH_SELECTED_RECIPE_COMPONENT_1, new Vector2(16, 16)).Contains(mousePosition) || new RectangleF(WORKBENCH_SELECTED_RECIPE_COMPONENT_1+new Vector2(0, haveBoxesDeltaY), new Vector2(16, 16)).Contains(mousePosition))
                        {
                            if (selectedRecipe.components.Length >= 1)
                            {
                                tooltipName = selectedRecipe.components[0].GetItem().GetName();
                            }
                        } else if (new RectangleF(WORKBENCH_SELECTED_RECIPE_COMPONENT_2, new Vector2(16, 16)).Contains(mousePosition) || new RectangleF(WORKBENCH_SELECTED_RECIPE_COMPONENT_2 + new Vector2(0, haveBoxesDeltaY), new Vector2(16, 16)).Contains(mousePosition))
                        {
                            if (selectedRecipe.components.Length >= 2)
                            {
                                tooltipName = selectedRecipe.components[1].GetItem().GetName();
                            }
                        } else if (new RectangleF(WORKBENCH_SELECTED_RECIPE_COMPONENT_3, new Vector2(16, 16)).Contains(mousePosition) || new RectangleF(WORKBENCH_SELECTED_RECIPE_COMPONENT_3 + new Vector2(0, haveBoxesDeltaY), new Vector2(16, 16)).Contains(mousePosition))
                        {
                            if (selectedRecipe.components.Length >= 3)
                            {
                                tooltipName = selectedRecipe.components[2].GetItem().GetName();
                            }
                        } else if (new RectangleF(WORKBENCH_SELECTED_RECIPE_COMPONENT_4, new Vector2(16, 16)).Contains(mousePosition) || new RectangleF(WORKBENCH_SELECTED_RECIPE_COMPONENT_4 + new Vector2(0, haveBoxesDeltaY), new Vector2(16, 16)).Contains(mousePosition))
                        {
                            if (selectedRecipe.components.Length >= 4)
                            {
                                tooltipName = selectedRecipe.components[3].GetItem().GetName();
                            }
                        }
                    }
                }
                else //if inventory/scrapbook is not open
                {
                    if (timeSinceItemCollectedTooltipAdded >= ITEM_COLLECTED_TOOLTIP_DELAY || (player.GetItemsCollectedRecently().Count >= CollectedTooltip.FAST_THRESHOLD && timeSinceItemCollectedTooltipAdded >= ITEM_COLLECTED_TOOLTIP_DELAY_FAST))
                    {
                        List<Item> itemsCollectedRecently = player.GetItemsCollectedRecently();
                        if (itemsCollectedRecently.Count != 0)
                        {
                            Item newItem = itemsCollectedRecently[0];
                            collectedTooltips.Add(new CollectedTooltip(newItem.GetName(), itemsCollectedRecently.Count >= CollectedTooltip.FAST_THRESHOLD));
                            itemsCollectedRecently.Remove(newItem);
                            timeSinceItemCollectedTooltipAdded = 0.0f;
                        }
                    }

                    List<CollectedTooltip> finished = new List<CollectedTooltip>();
                    foreach (CollectedTooltip ct in collectedTooltips)
                    {
                        ct.Update(deltaTime);
                        if (ct.IsFinished())
                        {
                            finished.Add(ct);
                        }
                    }
                    foreach (CollectedTooltip finishedCT in finished)
                    {
                        collectedTooltips.Remove(finishedCT);
                    }

                    if (player.GetHeldItem().GetItem() is PlaceableItem && interfaceState == InterfaceState.NONE)
                    {
                        PlaceableItem item = (PlaceableItem)player.GetHeldItem().GetItem();
                        isHoldingPlaceable = true;
                        placeableTexture = item.GetPreviewTexture();
                        Vector2 location = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, controller.GetMousePos());
                        Vector2 tile = new Vector2((int)(location.X / 8), (int)(location.Y / 8) - (item.GetPlaceableHeight() - 1));
                        placeableLocation = currentArea.GetPositionOfTile((int)tile.X, (int)tile.Y);
                        isPlaceableLocationValid = false;
                        if (item.GetPlacementType() == PlaceableItem.PlacementType.NORMAL)
                        {
                            placeableLocation.Y += 1;
                            isPlaceableLocationValid = currentArea.IsTileEntityPlacementValid((int)tile.X, (int)tile.Y, item.GetPlaceableWidth(), item.GetPlaceableHeight());
                        } else if (item.GetPlacementType() == PlaceableItem.PlacementType.WALL)
                        {
                            isPlaceableLocationValid = currentArea.IsWallEntityPlacementValid((int)tile.X, (int)tile.Y, item.GetPlaceableWidth(), item.GetPlaceableHeight());
                        } else if (item.GetPlacementType() == PlaceableItem.PlacementType.WALLPAPER)
                        {
                            isPlaceableLocationValid = currentArea.IsWallpaperPlacementValid((int)tile.X, (int)tile.Y, item.GetPlaceableWidth(), item.GetPlaceableHeight());
                        }
                        else if (item.GetPlacementType() == PlaceableItem.PlacementType.CEILING)
                        {
                            //ceiling todo
                        } else if (item.GetPlacementType() == PlaceableItem.PlacementType.FLOOR)
                        {
                            isPlaceableLocationValid = currentArea.IsFloorEntityPlacementValid((int)tile.X, (int)tile.Y, item.GetPlaceableWidth());
                        }

                        if (!showPlaceableTexture)
                        {
                            if (lastPlacedTile.X != tile.X || lastPlacedTile.Y != tile.Y)
                            {
                                showPlaceableTexture = true;
                                lastPlacedTile = new Vector2(-1000, -1000);
                            }
                        }
                    }
                    else if (player.GetHeldItem().GetItem() is BuildingBlockItem && interfaceState == InterfaceState.NONE)
                    {
                        //todo fix
                        BuildingBlockItem item = (BuildingBlockItem)player.GetHeldItem().GetItem();
                        isHoldingPlaceable = true;
                        placeableTexture = item.GetPlacedTexture();
                        Vector2 location = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, controller.GetMousePos());
                        Vector2 tile = new Vector2((int)(location.X / 8), (int)(location.Y / 8));
                        placeableLocation = currentArea.GetPositionOfTile((int)tile.X, (int)tile.Y);
                        isPlaceableLocationValid = currentArea.IsBuildingBlockPlacementValid((int)tile.X, (int)tile.Y, item.GetBlockType() == BlockType.BLOCK);
                        if (!showPlaceableTexture)
                        {
                            if (lastPlacedTile.X != tile.X || lastPlacedTile.Y != tile.Y)
                            {
                                showPlaceableTexture = true;
                                lastPlacedTile = new Vector2(-1000, -1000);
                            }
                        }
                    }
                }

                selectedHotbarPosition = player.GetSelectedHotbarPosition();
                for (int i = 0; i < EntityPlayer.INVENTORY_SIZE; i++)
                {
                    inventoryItems[i] = player.GetInventoryItemStack(i);
                }

                if (player.GetInterfaceState() == InterfaceState.CHEST)
                {
                    PEntityChest chest = (PEntityChest)player.GetTargettedTileEntity(); ;
                    for (int i = 0; i < PEntityChest.INVENTORY_SIZE; i++)
                    {
                        chestInventory[i] = chest.GetInventoryItemStack(i);
                    }
                }

                drawReticle = player.IsGrounded() && !player.IsRolling();
            }
        }

        public void Draw(SpriteBatch sb, RectangleF cameraBoundingBox, float layerDepth)
        {
            if((heldItem.GetItem() is PlaceableItem || heldItem.GetItem() is BuildingBlockItem) && !GameState.CheckFlag(GameState.FLAG_SETTINGS_HIDE_GRID))
            {
                sb.Draw(grid, gridLocation, Color.White * GRID_OPACITY);
            }

            //draw reticle
            if (!GameState.CheckFlag(GameState.FLAG_SETTINGS_HIDE_RETICLE) && drawReticle)
            {
                sb.Draw(reticle, targetTile * new Vector2(8, 8), Color.White);
            }

            if(targetEntity != null && targetEntity is IHaveHoveringInterface)
            {
                currentHoveringInterface = ((IHaveHoveringInterface)targetEntity).GetHoveringInterface();
                Vector2 hoveringSize = currentHoveringInterface.GetSize();
                hoveringInterfacePosition = targetEntity.GetPosition();
                hoveringInterfacePosition.Y -= 12;
                RectangleF targetSize = targetEntity.GetCollisionRectangle();
                hoveringInterfacePosition.X += targetSize.Width / 2;
                hoveringInterfacePosition.X -= hoveringSize.X / 2;
                hoveringInterfacePosition.Y -= hoveringSize.Y;
            }
            if (currentHoveringInterface != null)
            {
                currentHoveringInterface.Draw(sb, hoveringInterfacePosition, hoveringInterfaceOpacity);
            }

            foreach (CollectedTooltip ct in collectedTooltips)
            {
                string text = " + " + ct.name; 
                Vector2 nameSize = Plateau.FONT.MeasureString(text) * Plateau.FONT_SCALE;
                Vector2 position = new Vector2(playerPosition.X - (0.5f * nameSize.X), playerPosition.Y - ITEM_COLLECTED_TOOLTIP_Y_ADDITION - ct.GetYAdjustment());
                float opacity = 1.0f;
                if(ct.timeElapsed >= CollectedTooltip.LENGTH - 0.2f)
                {
                    opacity = 0.7f;
                    if (ct.timeElapsed >= CollectedTooltip.LENGTH - 0.1f)
                    {
                        opacity = 0.4f;
                    }
                }
                //collectedtooltip color
                sb.DrawString(Plateau.FONT, text, position, Color.DarkGreen * opacity, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
            }

            //draw scrapbook
            if (interfaceState == InterfaceState.SCRAPBOOK)
            {
                sb.Draw(black_background, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, BACKGROUND_BLACK_OFFSET), Color.White * BLACK_BACKGROUND_OPACITY);
                sb.Draw(scrapbookBase, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_POSITION), Color.White);
                switch (scrapbookCurrentTab)
                {
                    case 0:
                        sb.Draw(scrapbookHoverTab == 0 ? scrapbookTab1ActiveHover : scrapbookTab1Active, 
                            Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB1_POSITION + (scrapbookHoverTab == 0 ? new Vector2(-1, 0) : new Vector2(1, 1))), Color.White);
                        break;
                    case 1:
                        sb.Draw(scrapbookHoverTab == 1 ? scrapbookTab2ActiveHover : scrapbookTab2Active, 
                            Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB2_POSITION + (scrapbookHoverTab == 1 ? new Vector2(-1, 0) : new Vector2(1, 1))), Color.White);
                        break;
                    case 2:
                        sb.Draw(scrapbookHoverTab == 2 ? scrapbookTab3ActiveHover : scrapbookTab3Active, 
                            Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB3_POSITION + (scrapbookHoverTab == 2 ? new Vector2(-1, 0) : new Vector2(1, 1))), Color.White);
                        break;
                    case 3:
                        sb.Draw(scrapbookHoverTab == 3 ? scrapbookTab4ActiveHover : scrapbookTab4Active, 
                            Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB4_POSITION + (scrapbookHoverTab == 3 ? new Vector2(-1, 0) : new Vector2(1, 1))), Color.White);
                        break;
                    case 4:
                        sb.Draw(scrapbookHoverTab == 4 ? scrapbookTab5ActiveHover : scrapbookTab5Active, 
                            Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB5_POSITION + (scrapbookHoverTab == 4 ? new Vector2(-1, 0) : new Vector2(1, 1))), Color.White);
                        break;
                    case 5:
                        sb.Draw(scrapbookHoverTab == 5 ? scrapbookTab6ActiveHover : scrapbookTab6Active, 
                            Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB6_POSITION + (scrapbookHoverTab == 5 ? new Vector2(-1, 0) : new Vector2(1, 1))), Color.White);
                        break;
                    case 6:
                        sb.Draw(scrapbookHoverTab == 6 ? scrapbookTab7ActiveHover : scrapbookTab7Active, 
                            Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB7_POSITION + (scrapbookHoverTab == 6 ? new Vector2(-1, 0) : new Vector2(1, 1))), Color.White);
                        break;
                    case 7:
                        sb.Draw(scrapbookHoverTab == 7 ? scrapbookTab8ActiveHover : scrapbookTab8Active,
                            Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB8_POSITION + (scrapbookHoverTab == 7 ? new Vector2(-1, 0) : new Vector2(1, 1))), Color.White);
                        break;
                    case 8:
                        sb.Draw(scrapbookHoverTab == 8 ? scrapbookTab9ActiveHover : scrapbookTab9Active,
                            Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB9_POSITION + (scrapbookHoverTab == 8 ? new Vector2(-1, 0) : new Vector2(1, 1))), Color.White);
                        break;
                    case 9:
                        sb.Draw(scrapbookHoverTab == 9 ? scrapbookTab10ActiveHover : scrapbookTab10Active,
                            Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB10_POSITION + (scrapbookHoverTab == 9 ? new Vector2(-1, 0) : new Vector2(1, 1))), Color.White);
                        break;
                    case 10:
                        sb.Draw(scrapbookHoverTab == 10 ? scrapbookTab11ActiveHover : scrapbookTab11Active,
                            Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB11_POSITION + (scrapbookHoverTab == 10 ? new Vector2(-1, 0) : new Vector2(1, 1))), Color.White);
                        break;
                }
                if (scrapbookHoverTab != scrapbookCurrentTab)
                {
                    switch (scrapbookHoverTab)
                    {
                        case 0:
                            sb.Draw(scrapbookTab1Hover, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB1_POSITION + new Vector2(-1, 0)), Color.White);
                            break;
                        case 1:
                            sb.Draw(scrapbookTab2Hover, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB2_POSITION + new Vector2(-1, 0)), Color.White);
                            break;
                        case 2:
                            sb.Draw(scrapbookTab3Hover, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB3_POSITION + new Vector2(-1, 0)), Color.White);
                            break;
                        case 3:
                            sb.Draw(scrapbookTab4Hover, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB4_POSITION + new Vector2(-1, 0)), Color.White);
                            break;
                        case 4:
                            sb.Draw(scrapbookTab5Hover, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB5_POSITION + new Vector2(-1, 0)), Color.White);
                            break;
                        case 5:
                            sb.Draw(scrapbookTab6Hover, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB6_POSITION + new Vector2(-1, 0)), Color.White);
                            break;
                        case 6:
                            sb.Draw(scrapbookTab7Hover, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB7_POSITION + new Vector2(-1, 0)), Color.White);
                            break;
                        case 7:
                            sb.Draw(scrapbookTab8Hover, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB8_POSITION + new Vector2(-1, 0)), Color.White);
                            break;
                        case 8:
                            sb.Draw(scrapbookTab9Hover, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB9_POSITION + new Vector2(-1, 0)), Color.White);
                            break;
                        case 9:
                            sb.Draw(scrapbookTab10Hover, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB10_POSITION + new Vector2(-1, 0)), Color.White);
                            break;
                        case 10:
                            sb.Draw(scrapbookTab11Hover, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCRAPBOOK_TAB11_POSITION + new Vector2(-1, 0)), Color.White);
                            break;
                    }
                }

                //draw selected title
                if (((int)scrapbookCurrentPage / 10) == scrapbookCurrentTab)
                {
                    sb.Draw(scrapbookHoverPage ==  scrapbookCurrentPage ? scrapbookTitleActiveHover : scrapbookTitleActive, 
                        Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, scrapbookTitles[scrapbookCurrentPage % 10].Position) + (scrapbookHoverPage == scrapbookCurrentPage ? new Vector2(1, 0) : new Vector2(2, 1)), Color.White);
                }
                if(scrapbookHoverPage != -1 && scrapbookHoverPage != scrapbookCurrentPage && ((int)scrapbookHoverPage / 10) == scrapbookCurrentTab)
                {
                    sb.Draw(scrapbookTitleHover, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, scrapbookTitles[scrapbookHoverPage % 10].Position) + new Vector2(1, 0), Color.White);
                }

                //draw titles...
                for (int i = scrapbookCurrentTab * 10; i < (scrapbookCurrentTab * 10) + 10; i++)
                {
                    sb.DrawString(Plateau.FONT, scrapbookPages[i].GetName(), Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, scrapbookTitles[i % 10].Position) + new Vector2(4, 3), Color.Black, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                }

                scrapbookPages[scrapbookCurrentPage].Draw(sb, cameraBoundingBox);
            }
            else if (interfaceState == InterfaceState.INVENTORY || interfaceState == InterfaceState.CHEST)
            {
                sb.Draw(black_background, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, BACKGROUND_BLACK_OFFSET), Color.White * BLACK_BACKGROUND_OPACITY);

                //draw inventory
                sb.Draw(inventory, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, INVENTORY_POSITION), Color.White);
                //sb.Draw(playerSprite, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, INVENTORY_PLAYER_PREVIEW), null, Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f);
                playerSprite.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, INVENTORY_PLAYER_PREVIEW), layerDepth, 2.0f);
                //draw clothing
                glasses.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, GLASSES_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                back.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, BACK_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                sailcloth.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SAILCLOTH_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                scarf.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SCARF_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                outerwear.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, OUTERWEAR_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                socks.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SOCKS_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                hat.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, HAT_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                shirt.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SHIRT_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                pants.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, PANTS_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                earrings.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, EARRINGS_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                gloves.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, GLOVES_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                shoes.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SHOES_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                accessory1.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, ACCESSORY1_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                accessory2.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, ACCESSORY2_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                accessory3.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, ACCESSORY3_INVENTORY_RECT.TopLeft) + new Vector2(1, 1), Color.White, layerDepth);
                DrawClothingHeldIndicator(sb, cameraBoundingBox);

                //draw garbage can
                if (garbageCanRectangle.Contains(controller.GetMousePos()) && inventoryHeldItem.GetItem() != ItemDict.NONE && !inventoryHeldItem.GetItem().HasTag(Item.Tag.NO_TRASH))
                {
                    sb.Draw(garbageCanOpen, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, GARBAGE_CAN_LOCATION), Color.White);
                }
                else
                {
                    sb.Draw(garbageCanClosed, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, GARBAGE_CAN_LOCATION), Color.White);
                }


                for (int i = 10; i < EntityPlayer.INVENTORY_SIZE; i++)
                {
                    Item item = inventoryItems[i].GetItem();
                    Vector2 position = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, new Vector2(itemRectangles[i].X, itemRectangles[i].Y)) + new Vector2(1, 1);
                    item.Draw(sb, position, Color.White, layerDepth);
                    if (item.GetStackCapacity() != 1 && inventoryItems[i].GetQuantity() != 0)
                    {
                        Vector2 itemQuantityPosition = new Vector2(itemRectangles[i].X + 12, itemRectangles[i].Y + 10);
                        sb.Draw(numbers[inventoryItems[i].GetQuantity() % 10], Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, itemQuantityPosition), Color.White);
                        if (inventoryItems[i].GetQuantity() >= 10)
                        {
                            itemQuantityPosition.X -= 4;
                            sb.Draw(numbers[inventoryItems[i].GetQuantity() / 10], Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, itemQuantityPosition), Color.White);
                        }
                    }
                }

                if (interfaceState == InterfaceState.CHEST)
                {
                    sb.Draw(chest_inventory, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, CHEST_INVENTORY_POSITION), Util.DEFAULT_COLOR.color);

                    for (int i = 0; i < PEntityChest.INVENTORY_SIZE; i++)
                    {
                        Item item = chestInventory[i].GetItem();
                        Vector2 position = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, new Vector2(chestRectangles[i].X, chestRectangles[i].Y)) + new Vector2(1, 1);
                        item.Draw(sb, position, Color.White, layerDepth);
                        if (item.GetStackCapacity() != 1 && chestInventory[i].GetQuantity() != 0)
                        {
                            Vector2 chestQuantityPosition = new Vector2(chestRectangles[i].X + 12, chestRectangles[i].Y + 10);
                            sb.Draw(numbers[chestInventory[i].GetQuantity() % 10], Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, chestQuantityPosition), Color.White);
                            if (chestInventory[i].GetQuantity() >= 10)
                            {
                                chestQuantityPosition.X -= 4;
                                sb.Draw(numbers[chestInventory[i].GetQuantity() / 10], Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, chestQuantityPosition), Color.White);
                            }
                        }
                    }
                }
            }
            else if (interfaceState == InterfaceState.EXIT)
            {
                sb.Draw(black_background, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, BACKGROUND_BLACK_OFFSET), Color.White * BLACK_BACKGROUND_OPACITY);
                sb.Draw(exitPrompt, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, EXIT_PROMPT_POSITION), Color.White);
            }
            else if (interfaceState == InterfaceState.SETTINGS)
            {
                sb.Draw(black_background, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, BACKGROUND_BLACK_OFFSET), Color.White * BLACK_BACKGROUND_OPACITY);
                sb.Draw(settings, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SETTINGS_POSITION), Color.White);

                if (GameState.CheckFlag(GameState.FLAG_SETTINGS_HIDE_CONTROLS))
                {
                    sb.Draw(checkmark, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, settingsRectangles[0].TopLeft) + new Vector2(1, 1), Color.White);
                }
                if (GameState.CheckFlag(GameState.FLAG_SETTINGS_HIDE_GRID))
                {
                    sb.Draw(checkmark, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, settingsRectangles[1].TopLeft) + new Vector2(1, 1), Color.White);
                }
                if (GameState.CheckFlag(GameState.FLAG_SETTINGS_HIDE_RETICLE))
                {
                    sb.Draw(checkmark, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, settingsRectangles[2].TopLeft) + new Vector2(1, 1), Color.White);
                }
            }
            else if (interfaceState == InterfaceState.CRAFTING)
            {
                sb.Draw(black_background, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, BACKGROUND_BLACK_OFFSET), Color.White * BLACK_BACKGROUND_OPACITY);
                string pageName = "";
                sb.Draw(workbench, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_POSITION), Color.White);
                switch (workbenchCurrentTab)
                {
                    case 0:
                        sb.Draw(workbenchMachineTab, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_MACHINE_TAB_POSITION), Color.White);
                        pageName = "Machines " + (workbenchCurrentPage + 1);
                        break;
                    case 1:
                        sb.Draw(workbenchScaffoldingTab, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_SCAFFOLDING_TAB_POSITION), Color.White);
                        pageName = "Scaffolding " + (workbenchCurrentPage + 1);
                        break;
                    case 2:
                        sb.Draw(workbenchFurnitureTab, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_FURNITURE_TAB_POSITION), Color.White);
                        pageName = "Furniture " + (workbenchCurrentPage + 1);
                        break;
                    case 3:
                        sb.Draw(workbenchFloorWallTab, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_HOUSE_TAB_POSITION), Color.White);
                        pageName = "Wallpaper & Flooring " + (workbenchCurrentPage + 1);
                        break;
                    case 4:
                        sb.Draw(workbenchClothingTab, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_CLOTHING_TAB_POSITION), Color.White);
                        pageName = "Clothing " + (workbenchCurrentPage + 1);
                        break;
                }

                if (hoveringLeftArrow)
                {
                    sb.Draw(workbenchArrowLeft, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_LEFT_ARROW_POSITION) - new Vector2(1, 1));
                }
                else if (hoveringRightArrow)
                {
                    sb.Draw(workbenchArrowRight, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_RIGHT_ARROW_POSITION) - new Vector2(1, 1));
                }

                bool currentWorkbenchRecipePossible = true;
                if (selectedRecipe == null)
                {
                    currentWorkbenchRecipePossible = false;
                }
                else
                {
                    for (int i = 0; i < selectedRecipe.components.Length; i++)
                    {
                        if (numMaterialsOfRecipe[i] < selectedRecipe.components[i].GetQuantity())
                        {
                            currentWorkbenchRecipePossible = false;
                        }
                    }
                }

                if (hoveringCraftButton && currentWorkbenchRecipePossible)
                {
                    sb.Draw(workbenchCraftButtonEnlarged, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_CRAFT_BUTTON) - new Vector2(1, 1));
                }
                else if (currentWorkbenchRecipePossible)
                {
                    sb.Draw(workbenchCraftButton, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_CRAFT_BUTTON));
                }

                foreach (Vector2 possible in workbenchCraftablePosition)
                {
                    sb.Draw(inventory_selected, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, possible), Color.LightGreen);
                }

                sb.Draw(inventory_selected, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, workbenchInventorySelectedPosition), Color.Blue);

                for (int i = 0; i < workbenchBlueprintRectangles.Length; i++)
                {
                    if (currentRecipes[i] != null)
                    {
                        Vector2 recipePosition = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, workbenchBlueprintRectangles[i].TopLeft + new Vector2(1, 1));
                        if (selectedRecipeSlot == i)
                        {
                            sb.Draw(workbenchBlueprintDepression, recipePosition, Color.White);
                        }

                        currentRecipes[i].result.GetItem().Draw(sb,
                            recipePosition,
                            currentRecipes[i].haveBlueprint ? Color.White : Color.Black, layerDepth);

                        if (!currentRecipes[i].haveBlueprint)
                        {
                            sb.Draw(workbenchQuestionMark,
                                recipePosition, Color.White);
                        }
                        else
                        {
                            if (currentRecipes[i].result.GetQuantity() != 1)
                            {
                                Vector2 itemQuantityPosition = new Vector2(recipePosition.X + 11, recipePosition.Y + 9);
                                sb.Draw(numbers[currentRecipes[i].result.GetQuantity() % 10], itemQuantityPosition, Color.White);
                                if (currentRecipes[i].result.GetQuantity() >= 10)
                                {
                                    itemQuantityPosition.X -= 4;
                                    sb.Draw(numbers[currentRecipes[i].result.GetQuantity() / 10], itemQuantityPosition, Color.White);
                                }
                            }
                        }

                    }
                }

                if (selectedRecipe != null)
                {
                    GameState.CraftingRecipe selected = selectedRecipe;
                    Vector2 selectedRecipePos = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_SELECTED_RECIPE_POSITION);
                    selected.result.GetItem().Draw(sb, selectedRecipePos, Color.White, layerDepth);
                    if (selected.result.GetQuantity() != 1)
                    {
                        Vector2 itemQuantityPosition = new Vector2(selectedRecipePos.X + 11, selectedRecipePos.Y + 9);
                        sb.Draw(numbers[selected.result.GetQuantity() % 10], itemQuantityPosition, Color.White);
                        if (selected.result.GetQuantity() >= 10)
                        {
                            itemQuantityPosition.X -= 4;
                            sb.Draw(numbers[selected.result.GetQuantity() / 10], itemQuantityPosition, Color.White);
                        }
                    }
                    for (int i = 0; i < selected.components.Length; i++)
                    {
                        Vector2 pos = new Vector2(-100, -100);
                        switch (i)
                        {
                            case 0:
                                pos = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_SELECTED_RECIPE_COMPONENT_1);
                                break;
                            case 1:
                                pos = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_SELECTED_RECIPE_COMPONENT_2);
                                break;
                            case 2:
                                pos = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_SELECTED_RECIPE_COMPONENT_3);
                                break;
                            case 3:
                                pos = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_SELECTED_RECIPE_COMPONENT_4);
                                break;
                        }
                        //draw the "need" part
                        selected.components[i].GetItem().Draw(sb, pos, Color.White, layerDepth);
                        Vector2 itemQuantityPosition = new Vector2(pos.X + 11, pos.Y + 9);
                        sb.Draw(numbers[selected.components[i].GetQuantity() % 10], itemQuantityPosition, Color.White);
                        if (selected.components[i].GetQuantity() >= 10)
                        {
                            itemQuantityPosition.X -= 4;
                            sb.Draw(numbers[selected.components[i].GetQuantity() / 10], itemQuantityPosition, Color.White);
                        }

                        //draw the "have" part
                        pos.Y += haveBoxesDeltaY;
                        selected.components[i].GetItem().Draw(sb, pos, Color.White, layerDepth);
                        itemQuantityPosition = new Vector2(pos.X + 11, pos.Y + 9);
                        int quantity = numMaterialsOfRecipe[i];
                        bool haveEnough = quantity >= selectedRecipe.components[i].GetQuantity();
                        do
                        {
                            sb.Draw(numbers[quantity % 10], itemQuantityPosition, haveEnough ? Color.Green : Color.Red);
                            quantity /= 10;
                            itemQuantityPosition.X -= 4;
                        } while (quantity != 0);
                    }
                }
                Vector2 pageNameLen = Plateau.FONT.MeasureString(pageName) * Plateau.FONT_SCALE;
                Vector2 pageNamePos = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, WORKBENCH_PAGE_NAME_POSITION - new Vector2(pageNameLen.X / 2, pageNameLen.Y));
                sb.DrawString(Plateau.FONT, pageName, pageNamePos, Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
            }

            //draw controls
            if (!GameState.CheckFlag(GameState.FLAG_SETTINGS_HIDE_CONTROLS))
            {
                if (!isHidden || currentDialogue != null)
                {
                    sb.Draw(mouseControl, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MOUSE_CONTROL_POSITION), Color.White);
                    if (isMouseRightDown)
                    {
                        sb.Draw(mouseRightDown, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MOUSE_CONTROL_POSITION), Color.White);
                    }
                    if (isMouseLeftDown)
                    {
                        sb.Draw(mouseLeftDown, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MOUSE_CONTROL_POSITION), Color.White);
                    }
                    Vector2 leftMouseSize = Plateau.FONT.MeasureString(mouseLeftAction) * Plateau.FONT_SCALE;
                    Vector2 leftShiftMouseSize = Plateau.FONT.MeasureString(mouseLeftShiftAction) * Plateau.FONT_SCALE;
                    Vector2 rightMouseSize = Plateau.FONT.MeasureString(mouseRightAction) * Plateau.FONT_SCALE;
                    Vector2 rightShiftMouseSize = Plateau.FONT.MeasureString(mouseRightShiftAction) * Plateau.FONT_SCALE;
                    if (controller.IsKeyDown(KeyBinds.SHIFT))
                    {
                        sb.DrawString(Plateau.FONT, mouseLeftShiftAction, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MOUSE_LEFT_TEXT_POSITION) - (0.5f * leftShiftMouseSize), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                        sb.DrawString(Plateau.FONT, mouseRightShiftAction, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MOUSE_RIGHT_TEXT_POSITION) - (0.5f * rightShiftMouseSize), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                    }
                    else
                    {
                        sb.DrawString(Plateau.FONT, mouseLeftAction, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MOUSE_LEFT_TEXT_POSITION) - (0.5f * leftMouseSize), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                        sb.DrawString(Plateau.FONT, mouseRightAction, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MOUSE_RIGHT_TEXT_POSITION) - (0.5f * rightMouseSize), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                    }

                    if (mouseLeftShiftAction != "" || mouseRightShiftAction != "")
                    {
                        sb.Draw(controller.IsKeyDown(KeyBinds.SHIFT) ? shiftOnPressed : shiftOnUnpressed, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SHIFT_CONTROL_POSITION), Color.White);
                        if (currentDialogue == null && interfaceState == InterfaceState.NONE)
                        {
                            string shiftTooltip = controller.IsKeyDown(KeyBinds.SHIFT) ? "" : "Hold Shift for\nmore options";
                            if (shiftTooltip != "")
                            {
                                sb.DrawString(Plateau.FONT, shiftTooltip, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SHIFT_TEXT_POSITION), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                            }
                        }
                    }
                    else
                    {
                        sb.Draw(controller.IsKeyDown(KeyBinds.SHIFT) ? shiftOffPressed : shiftOffUnpressed, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SHIFT_CONTROL_POSITION), Color.White);
                    }

                    sb.Draw(controller.IsKeyDown(KeyBinds.ESCAPE) ? escPressed : escUnpressed, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, ESC_CONTROL_POSITION), Color.White);
                    sb.DrawString(Plateau.FONT, interfaceState == InterfaceState.NONE ? "Exit" : "Back", Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, ESC_TEXT_POSITION), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                }
            }

            Vector2 leftActionStrSize = Plateau.FONT.MeasureString(leftAction) * Plateau.FONT_SCALE;
            Vector2 rightActionStrSize = Plateau.FONT.MeasureString(rightAction) * Plateau.FONT_SCALE;
            Vector2 upActionStrSize = Plateau.FONT.MeasureString(upAction) * Plateau.FONT_SCALE;
            Vector2 downActionStrSize = Plateau.FONT.MeasureString(downAction) * Plateau.FONT_SCALE;
            if (currentDialogue == null)
            {
                if (!isHidden)
                {
                    if (!GameState.CheckFlag(GameState.FLAG_SETTINGS_HIDE_CONTROLS))
                    {
                        sb.Draw(keyControl, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_CONTROL_POSITION), Color.White);
                        if (isSDown)
                        {
                            sb.Draw(keyControlSDown, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_CONTROL_POSITION), Color.White);
                        }
                        if (isADown)
                        {
                            sb.Draw(keyControlADown, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_CONTROL_POSITION), Color.White);
                        }
                        if (isWDown)
                        {
                            sb.Draw(keyControlWDown, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_CONTROL_POSITION), Color.White);
                        }
                        if (isDDown)
                        {
                            sb.Draw(keyControlDDown, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_CONTROL_POSITION), Color.White);
                        }
                        sb.DrawString(Plateau.FONT, leftAction, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_LEFT_TEXT_POSITION) - (0.5f * leftActionStrSize), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                        sb.DrawString(Plateau.FONT, rightAction, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_RIGHT_TEXT_POSITION) - (0.5f * rightActionStrSize), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                        sb.DrawString(Plateau.FONT, upAction, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_UP_TEXT_POSITION) - (0.5f * upActionStrSize), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                        sb.DrawString(Plateau.FONT, downAction, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_DOWN_TEXT_POSITION) - (0.5f * downActionStrSize), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                    }
                }
            }
            else if (currentDialogue.Splits())
            {
                Vector2 leftPosition = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_LEFT_TEXT_POSITION_DIALOGUE) - (0.5f * leftActionStrSize);
                Vector2 rightPosition = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_RIGHT_TEXT_POSITION_DIALOGUE) - (0.5f * rightActionStrSize);
                Vector2 upPosition = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_UP_TEXT_POSITION_DIALOGUE) - (0.5f * upActionStrSize);
                Vector2 downPosition = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_DOWN_TEXT_POSITION_DIALOGUE) - (0.5f * downActionStrSize);
                if (!currentDialogue.decisionLeftText.Equals(""))
                {
                    DrawFilledRectangle(sb, leftPosition - new Vector2(0, 1), leftActionStrSize, 0.8f, 1);
                }
                if (!currentDialogue.decisionRightText.Equals(""))
                {
                    DrawFilledRectangle(sb, rightPosition - new Vector2(0, 1), rightActionStrSize, 0.8f, 1);
                }
                if (!currentDialogue.decisionUpText.Equals(""))
                {
                    DrawFilledRectangle(sb, upPosition - new Vector2(0, 1), upActionStrSize, 0.8f, 2);
                }
                if (!currentDialogue.decisionDownText.Equals(""))
                {
                    DrawFilledRectangle(sb, downPosition - new Vector2(0, 1), downActionStrSize, 0.8f, 1);
                }
                sb.Draw(keyControl, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, KEY_CONTROL_POSITION_DIALOGUE), Color.White);
                sb.DrawString(Plateau.FONT, leftAction, leftPosition, Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                sb.DrawString(Plateau.FONT, rightAction, rightPosition, Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                sb.DrawString(Plateau.FONT, upAction, upPosition, Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                sb.DrawString(Plateau.FONT, downAction, downPosition, Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
            }

            //draw all the general ui stuff
            if (!isHidden)
            {
                //draw name of hotbar item
                if (interfaceState == InterfaceState.NONE)
                {
                    if (!selectedHotbarItemName.Equals(ItemDict.NONE.GetName()))
                    {
                        Vector2 nameLength = Plateau.FONT.MeasureString(selectedHotbarItemName) * Plateau.FONT_SCALE;
                        Vector2 shbinPos = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, SELECTED_HOTBAR_ITEM_NAME - new Vector2(nameLength.X / 2, nameLength.Y));
                        sb.DrawString(Plateau.FONT, selectedHotbarItemName, shbinPos, Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                    }
                }


                //draw the area/zone strings
                string areaAndZone = areaName + " ";
                if (!zoneName.Equals(""))
                {
                    areaAndZone = areaAndZone + "- " + zoneName + " ";
                }
                INTERFACE_9SLICE.DrawString(sb, areaAndZone, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, AREA_NAME_POSITION), cameraBoundingBox, Color.Black);
                //sb.DrawString(Plateau.FONT, areaName, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, AREA_NAME_POSITION), Color.LightGray, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                //sb.DrawString(Plateau.FONT, zoneName, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, ZONE_NAME_POSITION), Color.DarkGray, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);


                //draw the datetime panel and relevant text/numbers
                sb.Draw(dateTimePanel, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, DATETIME_PANEL_POSITION), Color.White);
                sb.Draw(seasonText[seasonIndex], Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, Vector2.Add(DATETIME_PANEL_POSITION, DATETIME_PANEL_SEASONTEXT_OFFSET)), Color.White);
                sb.Draw(dayText[dayIndex], Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, Vector2.Add(DATETIME_PANEL_POSITION, DATETIME_PANEL_DAYTEXT_OFFSET)), Color.White);
                Vector2 timePos = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, Vector2.Add(DATETIME_PANEL_POSITION, DATETIME_PANEL_TIME_OFFSET));
                sb.Draw(numbersNoBorder[hourTensIndex], timePos, Color.LightGray);
                timePos.X += 4;
                sb.Draw(numbersNoBorder[hourOnesIndex], timePos, Color.LightGray);
                timePos.X += 6;
                sb.Draw(numbersNoBorder[minuteTensIndex], timePos, Color.LightGray);
                timePos.X += 4;
                sb.Draw(numbersNoBorder[minuteOnesIndex], timePos, Color.LightGray);

                //draw the effect icons
                for (int i = 0; i < appliedEffects.Count; i++)
                {
                    if (effectRects.Count == appliedEffects.Count)
                    {
                        EntityPlayer.TimedEffect effect = appliedEffects[i];
                        effect.effect.DrawIcon(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, effectRects[i].TopLeft));
                    }
                }

                //draw gold amount
                Vector2 goldPos = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, Vector2.Add(DATETIME_PANEL_POSITION, DATETIME_PANEL_GOLD_OFFSET));
                string numberStr = displayGold.ToString();
                while (numberStr.Length < 9)
                {
                    numberStr = "0" + numberStr;
                }
                char[] digits = numberStr.ToCharArray();
                for (int i = 8; i >= 0; i--)
                {
                    sb.Draw(numbersNoBorder[Int32.Parse(digits[i].ToString())], goldPos, Color.White);
                    goldPos.X -= 4;
                    if (i == 6 || i == 3)
                    {
                        goldPos.X--;
                    }
                }


                //draw hotbar
                sb.Draw(hotbar, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, HOTBAR_POSITION), Color.White);
                Vector2 adjustedHotbarSelectedPos = new Vector2(HOTBAR_SELECTED_POSITION_0.X + HOTBAR_SELECTED_DELTA_X * selectedHotbarPosition, HOTBAR_SELECTED_POSITION_0.Y);
                sb.Draw(hotbar_selected, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, adjustedHotbarSelectedPos), Color.White);
                sb.Draw(inventory_selected, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, inventorySelectedPosition), Color.White);

                for (int i = 0; i < GameplayInterface.HOTBAR_LENGTH; i++)
                {
                    Vector2 position = Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, new Vector2(itemRectangles[i].X, itemRectangles[i].Y)) + new Vector2(1, 1);
                    Item item = inventoryItems[i].GetItem();
                    item.Draw(sb, position, Color.White, layerDepth);
                    if (item.GetStackCapacity() != 1 && inventoryItems[i].GetQuantity() != 0)
                    {
                        Vector2 itemQuantityPosition = new Vector2(itemRectangles[i].X + 12, itemRectangles[i].Y + 10);
                        sb.Draw(numbers[inventoryItems[i].GetQuantity() % 10], Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, itemQuantityPosition), Color.White);
                        if (inventoryItems[i].GetQuantity() >= 10)
                        {
                            itemQuantityPosition.X -= 4;
                            sb.Draw(numbers[inventoryItems[i].GetQuantity() / 10], Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, itemQuantityPosition), Color.White);
                        }
                    }
                }

                if (currentDialogue == null)
                {
                    sb.Draw(menuControl, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MENU_CONTROL_POSITION), Color.White);
                    if (isMouseOverCraftingMC)
                    {
                        sb.Draw(menuControlsCraftingEnlarge, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MENU_CONTROL_POSITION), Color.White);
                    }
                    else if (isMouseOverInventoryMC)
                    {
                        sb.Draw(menuControlsInventoryEnlarge, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MENU_CONTROL_POSITION), Color.White);
                    }
                    else if (isMouseOverScrapbookMC)
                    {
                        sb.Draw(menuControlsScrapbookEnlarge, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MENU_CONTROL_POSITION), Color.White);
                    }
                    else if (isMouseOverSettingsMC)
                    {
                        sb.Draw(menuControlsSettingsEnlarge, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MENU_CONTROL_POSITION), Color.White);
                    }
                    if (interfaceState == InterfaceState.CRAFTING)
                    {
                        sb.Draw(menuControlsCraftingDepressed, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MENU_CONTROL_POSITION), Color.White);
                    }
                    else if (interfaceState == InterfaceState.INVENTORY)
                    {
                        sb.Draw(menuControlsInventoryDepressed, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MENU_CONTROL_POSITION), Color.White);
                    }
                    else if (interfaceState == InterfaceState.SCRAPBOOK)
                    {
                        sb.Draw(menuControlsScrapbookDepressed, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MENU_CONTROL_POSITION), Color.White);
                    }
                    else if (interfaceState == InterfaceState.SETTINGS)
                    {
                        sb.Draw(menuControlsSettingsDepressed, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MENU_CONTROL_POSITION), Color.White);
                    }
                }

                if (interfaceState == InterfaceState.INVENTORY)
                {
                    sb.DrawString(Plateau.FONT, "Bag: " + KeyBinds.OPEN_INVENTORY.ToString(), Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MENU_BAG_HOTKEY_POSITION), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                    sb.DrawString(Plateau.FONT, "Scrapbook: " + KeyBinds.OPEN_SCRAPBOOK.ToString(), Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MENU_SCRAPBOOK_HOTKEY_POSITION), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                    sb.DrawString(Plateau.FONT, "Crafting: " + KeyBinds.CRAFTING.ToString(), Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MENU_CRAFTING_HOTKEY_POSITION), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                    sb.DrawString(Plateau.FONT, "Settings: " + KeyBinds.SETTINGS.ToString(), Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, MENU_SETTINGS_HOTKEY_POSITION), Color.White, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                }

                if (currentNotification != null)
                {
                    currentNotification.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, NOTIFICATION_POSITION));
                }
            } //!cutscne

            //draw the dialogue bubble
            if(currentDialogue != null)
            { 
                dialogueBox.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, DIALOGUE_BOX_LOCATION), Color.White, layerDepth);
                if (dialogueBox.IsCurrentLoopFinished())
                {
                    sb.Draw(currentDialogue.portrait, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, DIALOGUE_PORTRAIT_LOCATION), Color.White);
                    currentDialogue.DrawText(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, DIALOGUE_TEXT_LOCATION), currentDialogueNumChars);
                    if(currentDialogueNumChars >= currentDialogue.dialogueText.Length)
                    {
                        bounceArrow.Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, DIALOGUE_BOUNCE_ARROW_LOCATION), Color.White, layerDepth);
                    }
                }
            }

            //draw held item in hand in inventory, or draw item preview in editmode, or otherwise the mouse cursor
            if (inventoryHeldItem.GetItem() != ItemDict.NONE)
            {
                Vector2 mousePos = controller.GetMousePos();
                mousePos.X -= 8;
                mousePos.Y -= 8;
                inventoryHeldItem.GetItem().Draw(sb, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, mousePos), Color.White, layerDepth);
                if (inventoryHeldItem.GetItem().GetStackCapacity() != 1 && inventoryHeldItem.GetQuantity() != 0)
                {
                    Vector2 itemQuantityPosition = new Vector2(mousePos.X + 11, mousePos.Y + 9);
                    sb.Draw(numbers[inventoryHeldItem.GetQuantity() % 10], Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, itemQuantityPosition), Color.White);
                    if (inventoryHeldItem.GetQuantity() >= 10)
                    {
                        itemQuantityPosition.X -= 4;
                        sb.Draw(numbers[inventoryHeldItem.GetQuantity() / 10], Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, itemQuantityPosition), Color.White);
                    }
                }
            } else
            {
                if (isHoldingPlaceable)
                {
                    sb.Draw(placeableTexture, placeableLocation, isPlaceableLocationValid ? Color.White * PLACEMENT_OPACITY : Color.Red * (showPlaceableTexture ? 1.0f : 0.0f));
                }
                else
                {
                    if (!tooltipName.Equals(""))
                    {
                        string tooltip = tooltipName + (!tooltipDescription.Equals("") ? ("\n" + tooltipDescription) : "");
                        TOOLTIP_9SLICE.DrawString(sb, tooltip, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, controller.GetMousePos() + (tooltipDescription.Equals("") ? new Vector2(-5, 5) : new Vector2(0, 0))) + TOOLTIP_OFFSET, cameraBoundingBox, Color.Black);
                    }
                }
            }

            if(paused)
            {
                sb.Draw(black_background, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, BACKGROUND_BLACK_OFFSET), Color.White * 0.8f);
            }

            if(interfaceState == InterfaceState.TRANSITION_TO_DOWN ||
                interfaceState == InterfaceState.TRANSITION_TO_LEFT ||
                interfaceState == InterfaceState.TRANSITION_TO_RIGHT ||
                interfaceState == InterfaceState.TRANSITION_TO_UP ||
                interfaceState == InterfaceState.TRANSITION_FADE_TO_BLACK ||
                interfaceState == InterfaceState.TRANSITION_FADE_IN)
            {
                sb.Draw(black_background, Util.ConvertFromAbsoluteToCameraVector(cameraBoundingBox, transitionPosition + BACKGROUND_BLACK_OFFSET), Color.White * transitionAlpha);
            }
        }

        private void DrawFilledRectangle(SpriteBatch sb, Vector2 position, Vector2 size, float transparency, int padding=0)
        {
            //TODODO?????
            sb.Draw(black_box, new RectangleF(position - new Vector2(padding, padding), size + new Vector2(padding*2, padding*2)).ToRectangle(), Color.White * transparency);
            /*if(rectangle != null)
            {
                rectangle.Dispose();
            }
            rectangle = new Texture2D(Plateau.GRAPHICS.GraphicsDevice, (int)size.X + 2*padding, (int)size.Y + 2*padding);
            Color[] data = new Color[(int)(size.X + 2*padding) * (int)(size.Y+2*padding)];
            for (int i = 0; i < data.Length; ++i)
                data[i] = color * transparency;
            rectangle.SetData(data);

            position = new Vector2(position.X - padding, position.Y - padding);
            sb.Draw(rectangle, position, Color.White);*/
        }


        public bool IsPaused()
        {
            return paused;
        }

        public void Hide()
        {
            isHidden = true;
        }

        public void Unhide()
        {
            isHidden = false;
        }
    }

}
