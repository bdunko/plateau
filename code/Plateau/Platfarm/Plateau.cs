using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using Platfarm.Components;
using Platfarm.Entities;
using Platfarm.Items;
using Microsoft.Xna.Framework.Content;
using Platfarm.Particles;
using Platfarm.Sound;

namespace Platfarm
{
    public class Plateau : Game
    {
        public static SpriteFont FONT;
        public static float FONT_SPACING = 0.4f;
        public static int FONT_LINE_SPACING = 42;
        public static float FONT_SCALE = 0.2f;

        public enum PlateauGameState
        {
            NORMAL, MAINMENU, DEBUG, CUTSCENE
        }

        public PlateauGameState currentState;

        public static RectangleF SCREEN_DIMENSIONS;
        public static int RESOLUTION_X = 320;
        public static int RESOLUTION_Y = 200;
        public static int SCALE = 5;
        public static GraphicsDeviceManager GRAPHICS;
        public static ContentManager CONTENT;

        public static string CONTENT_ROOT_DIRECTORY = "Content";

        private static SpriteBatch spriteBatch;

        private Camera camera;
        public static Controller controller;
        private EntityPlayer player;
        private GameplayInterface ui;
        private SaveManager saveManager;
        private MainMenuInterface mmi;
        private World world;
        private DebugConsole debugConsole;
        private SoundSystem soundSystem;

        private RenderTarget2D mainTarget, lightsTarget;
        private Effect lightsShader;
        private CutsceneManager.Cutscene currentCutscene;
        private bool cutsceneTransitionFadeToBlack;
        private bool cutsceneTransitionDone;
        private float blackFadedTimer;
        private static float WAIT_TIME_WHILE_BLACK_FADED = 0.5f;

        private AnimatedSprite lightMaskSmall, lightMaskMedium, lightMaskLarge;

        public Plateau()
        {
            GRAPHICS = new GraphicsDeviceManager(this);

            Content.RootDirectory = CONTENT_ROOT_DIRECTORY;
            currentState = PlateauGameState.MAINMENU;
            currentCutscene = null;
            cutsceneTransitionDone = false;
            cutsceneTransitionFadeToBlack = false;
            blackFadedTimer = 0.0f;

            IsFixedTimeStep = true; //variable time steps...
            TargetElapsedTime = TimeSpan.FromMilliseconds(16.66667f); //60fps
        }

        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Window.Position = new Point(0, 0);
            Window.IsBorderless = true;
            //RESOLUTION_X = (int)GraphicsDevice.DisplayMode.Width;
            //RESOLUTION_Y = (int)GraphicsDevice.DisplayMode.Height;
            GRAPHICS.PreferredBackBufferWidth = RESOLUTION_X * SCALE; //GraphicsDevice.DisplayMode.Width; 320
            GRAPHICS.PreferredBackBufferHeight = RESOLUTION_Y * SCALE; //GraphicsDevice.DisplayMode.Height; 200
            IsMouseVisible = true;
            GRAPHICS.ApplyChanges();

            camera = new Camera(new Camera2D(GraphicsDevice));
            camera.SetZoom(SCALE);

            controller = new Controller();

            ui = new GameplayInterface(controller);
            mmi = new MainMenuInterface(controller, SaveManager.DoesSaveExist(1), SaveManager.DoesSaveExist(2), SaveManager.DoesSaveExist(3));

            Util.Initialize();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            CONTENT = Content;

            FONT = Content.Load<SpriteFont>(Paths.FONT);
            FONT.Spacing = FONT_SPACING;
            FONT.LineSpacing = FONT_LINE_SPACING;

            SoundSystem.Initialize(Content);

            ParticleFactory.LoadContent(Content);
            AppliedEffects.Initialize(Content);
            ItemDict.LoadContent(Content);
            
            GameState.Initialize();
            DialogueNode.LoadPortraits();
            world = new World();
            player = new EntityPlayer(world, controller);
            LootTables.Initialize();
            ui.LoadContent(Content);
            mmi.LoadContent(Content);
            world.LoadContent(Content, GraphicsDevice, player, camera.GetBoundingBox());
            //set spawn here
            world.SetCurrentArea(world.GetAreaDict()[Area.AreaEnum.FARM]);
            world.GetCurrentArea().MoveToWaypoint(player, "SPoutside");

            lightsShader = Content.Load<Effect>(Paths.SHADER_LIGHTS);

            var pp = GraphicsDevice.PresentationParameters;
            lightsTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            float[] lengths = Util.CreateAndFillArray(4, 0.2f);
            lightMaskSmall = new AnimatedSprite(Content.Load<Texture2D>(Paths.TEXTURE_LIGHT_SMALL_SPRITESHEET), 4, 1, 4, lengths);
            lightMaskSmall.AddLoop("loop", 0, 3, true);
            lightMaskSmall.SetLoop("loop");
            lightMaskMedium = new AnimatedSprite(Content.Load<Texture2D>(Paths.TEXTURE_LIGHT_MEDIUM_SPRITESHEET), 4, 1, 4, lengths);
            lightMaskMedium.AddLoop("loop", 0, 3, true);
            lightMaskMedium.SetLoop("loop");
            lightMaskLarge = new AnimatedSprite(Content.Load<Texture2D>(Paths.TEXTURE_LIGHT_LARGE_SPRITESHEET), 4, 1, 4, lengths);
            lightMaskLarge.AddLoop("loop", 0, 3, true);
            lightMaskLarge.SetLoop("loop");
            mainTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);

            EntityPlayer.FishlinePart.LoadContent(Content);
            CutsceneManager.Initialize(player, camera);
        }

        protected override void UnloadContent()
        {
            Content.Unload();
            Util.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (this.IsActive)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (ui.IsPaused())
                {
                    ui.Unpause();
                    player.IgnoreMouseInputThisFrame();
                }
                if (player.GetInterfaceState() == InterfaceState.EXIT_CONFIRMED)
                {
                    Exit();
                }

                if (currentState == PlateauGameState.NORMAL && player.GetCurrentDialogue() == null && !ui.IsItemHeld() && (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || controller.IsKeyPressed(KeyBinds.ESCAPE) || player.GetInterfaceState() == InterfaceState.EXIT_CONFIRMED))
                {
                    if (player.GetInterfaceState() == InterfaceState.NONE)
                    {
                        player.SetInterfaceState(InterfaceState.EXIT);
                        player.Pause();
                        world.Pause();
                    }
                    else
                    {
                        player.SetInterfaceState(InterfaceState.NONE);
                        player.Unpause();
                        world.Unpause();
                    }
                }
                controller.Update(); //read in inputs from mouse/keyboard

                if (currentState == PlateauGameState.CUTSCENE)
                {
                    if (!cutsceneTransitionDone)
                    {
                        player.StopAllMovement();
                        player.SetToDefaultPose();
                        ui.Update(deltaTime, player, camera.GetBoundingBox(), world.GetCurrentArea(), world.GetTimeData(), world); //update the interface
                        world.Update(deltaTime, gameTime, player, ui, camera, true); //update current area
                        if (ui.IsTransitionReady())
                        {
                            if (!cutsceneTransitionFadeToBlack)
                            {
                                if (currentCutscene != null)
                                {
                                    currentCutscene.OnActivation(player, world, camera);
                                    ui.Hide();
                                } else
                                {
                                    camera.Update(1f, player, world.GetCurrentArea().MapPixelWidth(), world.GetCurrentArea().MapPixelHeight());
                                    ui.Unhide();
                                }
                                blackFadedTimer += deltaTime;

                                if (blackFadedTimer >= WAIT_TIME_WHILE_BLACK_FADED) {
                                    ui.TransitionFadeIn();
                                    cutsceneTransitionFadeToBlack = true;
                                    blackFadedTimer = 0.0f;
                                }
                            } else
                            {
                                cutsceneTransitionDone = true;
                                if(currentCutscene == null)
                                {
                                    currentState = PlateauGameState.NORMAL;
                                }
                            }
                        } 
                    }
                    else
                    {
                        UpdateLightMasks(deltaTime);
                        currentCutscene.Update(deltaTime, player, world.GetCurrentArea(), world, camera);
                        ui.Update(deltaTime, player, camera.GetBoundingBox(), world.GetCurrentArea(), world.GetTimeData(), world); //update the interface
                        world.Update(deltaTime, gameTime, player, ui, camera, true); //update current area
                        if (currentCutscene.IsComplete())
                        {
                            currentCutscene = null;
                            cutsceneTransitionFadeToBlack = false;
                            cutsceneTransitionDone = false;
                            ui.TransitionFadeToBlack();
                        }
                    }
                }
                else if(currentState == PlateauGameState.DEBUG)
                {
                    controller.ActivateStringInput();
                    if(controller.IsKeyPressed(KeyBinds.ESCAPE) || controller.IsKeyPressed(KeyBinds.CONSOLE))
                    {
                        currentState = PlateauGameState.NORMAL;
                    }
                    if (controller.IsKeyPressed(KeyBinds.ENTER))
                    {
                        string command = controller.GetStringInput();
                        debugConsole.RunCommand(command);
                        controller.ClearStringInput();
                        ui.Update(0, player, camera.GetBoundingBox(), world.GetCurrentArea(), world.GetTimeData(), world);
                        if (!camera.IsLocked())
                        {
                            camera.Update(0, player, world.GetCurrentArea().MapPixelWidth(), world.GetCurrentArea().MapPixelHeight());
                        }
                    }
                }
                else if (currentState == PlateauGameState.MAINMENU)
                {
                    mmi.Update(camera.GetBoundingBox());
                    if (mmi.GetState() != MainMenuInterface.MainMenuState.NONE)
                    {
                        if (mmi.GetState() == MainMenuInterface.MainMenuState.CLICKED_SAVE_1)
                        {
                            saveManager = new SaveManager(SaveManager.FILE_NAME_1);
                        }
                        else if (mmi.GetState() == MainMenuInterface.MainMenuState.CLICKED_SAVE_2)
                        {
                            saveManager = new SaveManager(SaveManager.FILE_NAME_2);
                        }
                        else if (mmi.GetState() == MainMenuInterface.MainMenuState.CLICKED_SAVE_3)
                        {
                            saveManager = new SaveManager(SaveManager.FILE_NAME_3);
                        }
                        saveManager.LoadFile(player, world);
                        currentState = PlateauGameState.NORMAL;
                        Update(gameTime);
                        debugConsole = new DebugConsole(world, saveManager, player);
                    }
                }
                else if (currentState == PlateauGameState.NORMAL)
                {
                    UpdateLightMasks(deltaTime);

                    
                    world.Update(deltaTime, gameTime, player, ui, camera, false); //update current area
                    player.SetController(controller);
                    player.Update(deltaTime, world.GetCurrentArea());
                    ui.Update(deltaTime, player, camera.GetBoundingBox(), world.GetCurrentArea(), world.GetTimeData(), world); //update the interface

                    if (!player.IsPaused())
                    {
                        world.GetCurrentArea().CheckEntityCollisions(player);
                        world.HandleTransitions(player, ui);
                    }
                    
                    if (!camera.IsLocked())
                    {
                        camera.Update(deltaTime, player, world.GetCurrentArea().MapPixelWidth(), world.GetCurrentArea().MapPixelHeight()); //move camera
                    }
                    if (world.IsDayOver())
                    {
                        world.AdvanceDay(player);
                        saveManager.SaveFile(player, world);
                    }

                    if (controller.IsKeyPressed(KeyBinds.CONSOLE))
                    {
                        currentState = PlateauGameState.DEBUG;
                        controller.ClearStringInput();
                    }

                    currentCutscene = world.GetCutsceneIfPossible(player);
                    if (currentCutscene != null)
                    {
                        currentState = PlateauGameState.CUTSCENE;
                        cutsceneTransitionDone = false;
                        cutsceneTransitionFadeToBlack = false;
                        ui.TransitionFadeToBlack();
                    }
                }
                SoundSystem.Update(deltaTime, player, world);
            } else
            {
                ui.Pause();
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (currentState == PlateauGameState.MAINMENU)
            {
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.CadetBlue);
                spriteBatch.Begin(transformMatrix: camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                mmi.Draw(spriteBatch, camera.GetBoundingBox());
                spriteBatch.End();
            }
            else if (currentState == PlateauGameState.NORMAL || currentState == PlateauGameState.DEBUG || currentState == PlateauGameState.CUTSCENE)
            {
                //lighting mask
                GraphicsDevice.SetRenderTarget(lightsTarget);
                GraphicsDevice.Clear(Color.WhiteSmoke);
                spriteBatch.Begin(SpriteSortMode.Immediate, transformMatrix: camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                foreach(Area.LightSource ls in world.GetCurrentArea().GetAreaLights())
                {
                    AnimatedSprite lightMask = null;
                    switch(ls.lightStrength)
                    {
                        case Area.LightSource.Strength.SMALL:
                            lightMask = lightMaskSmall;
                            break;
                        case Area.LightSource.Strength.MEDIUM:
                            lightMask = lightMaskMedium;
                            break;
                        case Area.LightSource.Strength.LARGE:
                            lightMask = lightMaskLarge;
                            break;
                    }
                    lightMask.Draw(spriteBatch, Util.CenterTextureOnPoint(ls.position, lightMask.GetFrameWidth(), lightMask.GetFrameHeight()), ls.color, 0.0f);
                }
                /*Vector2 playerLightPos = Util.CenterTextureOnPoint(player.GetPosition(), lightMaskSmall);
                playerLightPos.Y += 10;
                playerLightPos.X += 2;
                spriteBatch.Draw(lightMaskSmall, playerLightPos, Color.White);*/
                spriteBatch.End();

                GraphicsDevice.SetRenderTarget(mainTarget);
                GraphicsDevice.Clear(Color.CadetBlue);
                spriteBatch.Begin(transformMatrix: camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend, sortMode: SpriteSortMode.Deferred);
                world.DrawBackground(spriteBatch, camera.GetBoundingBox(), 0.0f); //depth 0.0f
                spriteBatch.End();
                spriteBatch.Begin(transformMatrix: camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend, sortMode: SpriteSortMode.Deferred);
                world.DrawWalls(spriteBatch, camera.GetViewMatrix(), 0.05f); //depth 0.05f
                world.DrawBuildingBlocks(spriteBatch, 0.075f); //depth 0.075f
                world.DrawEntities(spriteBatch, DrawLayer.BACKGROUND_WALLPAPER, camera.GetBoundingBox(), 0.10f); //depth 0.10f
                world.DrawEntities(spriteBatch, DrawLayer.BACKGROUND_WALL, camera.GetBoundingBox(), 0.15f); //depth 0.15f
                world.DrawEntities(spriteBatch, DrawLayer.NORMAL, camera.GetBoundingBox(), 0.20f); //depth 0.2f
                spriteBatch.End();
                spriteBatch.Begin(transformMatrix: camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend, sortMode: SpriteSortMode.Deferred);
                world.DrawTerrain(spriteBatch, camera.GetViewMatrix(), 0.25f); //depth 0.25f
                world.DrawEntities(spriteBatch, DrawLayer.FOREGROUND_CARPET, camera.GetBoundingBox(), 0.30f); //depth 0.3f
                world.DrawParticles(spriteBatch, 0.35f); //depth 0.35f
                world.DrawEntities(spriteBatch, DrawLayer.PRIORITY, camera.GetBoundingBox(), 0.4f); //depth 0.4f
                world.DrawItemEntities(spriteBatch, 0.45f); //depth 0.45f
                player.Draw(spriteBatch, 0.5f); //depth 0.5f
                world.DrawEntities(spriteBatch, DrawLayer.FOREGROUND, camera.GetBoundingBox(), 0.55f); //depth 0.55f
                world.DrawWater(spriteBatch, camera.GetViewMatrix(), 0.60f); //depth 0.60f;
                world.DrawForeground(spriteBatch, camera.GetBoundingBox(), 0.65f); //depth 0.65f
                spriteBatch.End();

                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.CadetBlue);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
                lightsShader.Parameters["lightMask"].SetValue(lightsTarget);
                lightsShader.Parameters["darknessLevel"].SetValue(world.GetDarkLevel());
                lightsShader.Parameters["lightLevel"].SetValue(world.GetLightLevel());
                lightsShader.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(mainTarget, Vector2.Zero, Color.White);
                //spriteBatch.Draw(mainTarget, )
                spriteBatch.End();

                spriteBatch.Begin(transformMatrix: camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend, sortMode: SpriteSortMode.Deferred);
                ui.Draw(spriteBatch, camera.GetBoundingBox(), 0.99f);
                Util.Draw(spriteBatch, 1.0f);
                spriteBatch.End();

                if(currentState == PlateauGameState.DEBUG)
                {
                    spriteBatch.Begin(transformMatrix: camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend, sortMode: SpriteSortMode.Deferred);
                    spriteBatch.DrawString(Plateau.FONT, "Command: " + controller.GetStringInput(), Util.ConvertFromAbsoluteToCameraVector(camera.GetBoundingBox(), new Vector2(4, 30)), 
                        debugConsole.DidLastSucceed() ? Color.LightGreen : Color.DarkRed, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
                    spriteBatch.End();
                }
            }

            base.Draw(gameTime);
        }

        private void UpdateLightMasks(float deltaTime)
        {
            lightMaskSmall.Update(deltaTime);
            lightMaskMedium.Update(deltaTime);
            lightMaskLarge.Update(deltaTime);
        }
    }
}
