using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using Platfarm.Entities;
using Platfarm.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Components
{
    public class World
    {
        public enum Weather
        {
            SUNNY, CLOUDY, RAINY, SNOWY
        }

        public enum TimeOfDay
        {
            MORNING, DAY, EVENING, NIGHT, ALL
        }

        public class TimeData
        {
            public Season season;
            public int day;
            public int hour, minute;
            public TimeOfDay timeOfDay;

            public TimeData(Season season, int day, int hour, int minute, TimeOfDay timeOfDay)
            {
                this.season = season;
                this.day = day;
                this.hour = hour;
                this.minute = minute;
                this.timeOfDay = timeOfDay;
            }

            public int CalculateGameTime()
            {
                return (60 * hour) + minute;
            }
        }

        public static int MinutesUntilTransition(int currentHour, int currentMinute)
        {
            int hours = 0;
            
            if (currentHour < MORNING_START_HOUR)
            {
                hours = MORNING_START_HOUR - currentHour;
            }
            else if (currentHour < MORNING_END_HOUR)
            {
                hours = MORNING_END_HOUR - currentHour;
            }
            else if (currentHour < DAY_END_HOUR)
            {
                hours = DAY_END_HOUR - currentHour;
            }
            else if (currentHour < EVENING_END_HOUR)
            {
                hours = EVENING_END_HOUR - currentHour;
            } else
            {
                hours = 24 - currentHour + 7;
            }
            return (hours * 60) - currentMinute;
        }

        public static TimeOfDay NextTimeOfDay(TimeOfDay current)
        {
            switch(current)
            {
                case TimeOfDay.MORNING:
                    return TimeOfDay.DAY;
                case TimeOfDay.DAY:
                    return TimeOfDay.EVENING;
                case TimeOfDay.EVENING:
                    return TimeOfDay.NIGHT;
                default:
                    return TimeOfDay.MORNING;
            }
        }

        public enum Season
        {
            SPRING, SUMMER, AUTUMN, WINTER, DEFER, NONE
        }

        private Season StringToSeason(string str)
        {
            foreach (Season season in Enum.GetValues(typeof(Season)))
            {
                if (season.ToString() == str)
                {
                    return season;
                }
            }

            throw new Exception("No season found for string.");
        }

        private Dictionary<Area.AreaEnum, Area> areas;
        private Area currentArea;
        private TiledMapRenderer mapRenderer;
        private Weather currentWeather;
        private float currentTime;
        private int currentDay;
        private List<EntityCharacter> characters;
        private Season currentSeason;
        //1 2 3 4 5 6 7 8 9 10 11 12 1 2 3 4 5 6 7 8 9 10 11 12
        private static float[] hourlyLight = { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f};
        private static float[] hourlyDark = {0.5f, 0.6f, 0.7f, 0.8f, 0.85f, 1f, 1f, 1f, 1.0f, 1.0f, 1.0f, 1.05f, 1.05f, 1.0f, 1.0f, 1.0f, 0.95f, 0.95f, 0.9f, 0.7f, 0.5f, 0.4f, 0.35f, 0.3f };
        private static float DAY_STARTING_TIME = 420;
        private static float DAY_ENDING_TIME = 1440;
        private static int DAYS_IN_SEASON = 7;
        public static int MORNING_START_HOUR = 5;
        public static int MORNING_END_HOUR = 9;
        public static int DAY_END_HOUR = 17;
        public static int EVENING_END_HOUR = 20;

        private static int TICK_LENGTH = 5; //5 minutes ingame = 5 seconds
        private float tickTimer = 0.0f;

        private Area transitionToArea;
        private Area.Waypoint transitionToSpawn;
        private bool paused;

        public World()
        {
            this.currentWeather = Weather.SUNNY;
            paused = false;
            transitionToArea = null;
            transitionToSpawn = null;
            currentTime = DAY_STARTING_TIME;
            currentDay = 0;
            currentSeason = Season.SPRING;
        }

        public CutsceneManager.Cutscene GetCutsceneIfPossible(EntityPlayer player)
        {
            List<CutsceneManager.Cutscene> possibleCutscenes = currentArea.GetPossibleCutscenes(player.GetAdjustedPosition());
            foreach(CutsceneManager.Cutscene possibleCutscene in possibleCutscenes)
            {
                if(possibleCutscene.CheckActivationCondition(player, this))
                {
                    return possibleCutscene;
                }
            }
            return null;
        }

        public void LoadContent(ContentManager Content, GraphicsDevice graphics, EntityPlayer player, RectangleF cameraBoundingBox)
        {
            characters = new List<EntityCharacter>();
            mapRenderer = new TiledMapRenderer(graphics);
            areas = new Dictionary<Area.AreaEnum, Area>();

            areas[Area.AreaEnum.FARM] = new Area(Area.AreaEnum.FARM, Content.Load<TiledMap>(Paths.MAP_FARM), true, mapRenderer, Content, player, cameraBoundingBox);
            areas[Area.AreaEnum.TOWN] = new Area(Area.AreaEnum.TOWN, Content.Load<TiledMap>(Paths.MAP_TOWN), true, mapRenderer, Content, player, cameraBoundingBox);
            areas[Area.AreaEnum.INTERIOR] = new Area(Area.AreaEnum.INTERIOR, Content.Load<TiledMap>(Paths.MAP_INTERIOR), false, mapRenderer, Content, player, cameraBoundingBox);
            areas[Area.AreaEnum.BEACH] = new Area(Area.AreaEnum.BEACH, Content.Load<TiledMap>(Paths.MAP_BEACH), true, mapRenderer, Content, player, cameraBoundingBox);
            areas[Area.AreaEnum.S0] = new Area(Area.AreaEnum.S0, Content.Load<TiledMap>(Paths.MAP_S0), true, mapRenderer, Content, player, cameraBoundingBox);
            areas[Area.AreaEnum.S1] = new Area(Area.AreaEnum.S1, Content.Load<TiledMap>(Paths.MAP_S1), true, mapRenderer, Content, player, cameraBoundingBox);
            areas[Area.AreaEnum.S2] = new Area(Area.AreaEnum.S2, Content.Load<TiledMap>(Paths.MAP_S2), true, mapRenderer, Content, player, cameraBoundingBox);
            areas[Area.AreaEnum.S3] = new Area(Area.AreaEnum.S3, Content.Load<TiledMap>(Paths.MAP_S3), true, mapRenderer, Content, player, cameraBoundingBox);
            areas[Area.AreaEnum.S4] = new Area(Area.AreaEnum.S4, Content.Load<TiledMap>(Paths.MAP_S4), true, mapRenderer, Content, player, cameraBoundingBox);
            areas[Area.AreaEnum.APEX] = new Area(Area.AreaEnum.APEX, Content.Load<TiledMap>(Paths.MAP_APEX), true, mapRenderer, Content, player, cameraBoundingBox);
            //Console.WriteLine("FARM xy: " + areas[0].MapPixelWidth() + "  " + areas[0].MapPixelHeight());

            currentArea = areas[Area.AreaEnum.FARM]; //set starting area...

            //load characters!
            //schedule
            //clothing sets

            //Func<World, EntityCharacter, bool>
            Func<World, EntityCharacter, bool> trueCondition = (world, chara) => { return true; };
            Func<World, EntityCharacter, bool> springCondition = (world, chara) => { return world.GetSeason() == Season.SPRING; };
            Func<World, EntityCharacter, bool> summerCondition = (world, chara) => { return world.GetSeason() == Season.SUMMER; };
            Func<World, EntityCharacter, bool> fallCondition = (world, chara) => { return world.GetSeason() == Season.AUTUMN; };
            Func<World, EntityCharacter, bool> winterCondition = (world, chara) => { return world.GetSeason() == Season.WINTER; };

            EntityCharacter rockwell, camus;
            //ROCKWELL
            List<EntityCharacter.ClothingSet> rockwellClothing = Util.GenerateClothingSetList(
                new EntityCharacter.ClothingSet(ItemDict.SKIN_PEACH, ItemDict.GetColoredItem(ItemDict.HAIR_LUCKY_LUKE, Util.HAIR_CHARCOAL_BLACK), ItemDict.EYES_BROWN,
                ItemDict.CLOTHING_NONE, ItemDict.GetColoredItem(ItemDict.SHORT_SLEEVE_TEE, Util.LIGHT_GREY), ItemDict.GetColoredItem(ItemDict.ALL_SEASON_JACKET, Util.BLACK), ItemDict.GetColoredItem(ItemDict.JEANS, Util.NAVY), ItemDict.GetColoredItem(ItemDict.SNEAKERS, Util.WHITE), ItemDict.CLOTHING_NONE,
                ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE,
                springCondition, 1),
                new EntityCharacter.ClothingSet(ItemDict.SKIN_PEACH, ItemDict.GetColoredItem(ItemDict.HAIR_LUCKY_LUKE, Util.HAIR_CHARCOAL_BLACK), ItemDict.EYES_BROWN,
                ItemDict.CLOTHING_NONE, ItemDict.BUTTON_DOWN, ItemDict.CLOTHING_NONE, ItemDict.GetColoredItem(ItemDict.CHINO_SHORTS, Util.NAVY), ItemDict.GetColoredItem(ItemDict.SNEAKERS, Util.WHITE), ItemDict.CLOTHING_NONE,
                ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE,
                summerCondition, 1),
                new EntityCharacter.ClothingSet(ItemDict.SKIN_PEACH, ItemDict.GetColoredItem(ItemDict.HAIR_LUCKY_LUKE, Util.HAIR_CHARCOAL_BLACK), ItemDict.EYES_BROWN,
                ItemDict.GetColoredItem(ItemDict.BASEBALL_CAP, Util.BLACK), ItemDict.GetColoredItem(ItemDict.BUTTON_DOWN, Util.WHITE), ItemDict.GetColoredItem(ItemDict.ALL_SEASON_JACKET, Util.BLACK), ItemDict.GetColoredItem(ItemDict.CHINOS, Util.BROWN), ItemDict.GetColoredItem(ItemDict.SNEAKERS, Util.WHITE), ItemDict.CLOTHING_NONE,
                ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE,
                fallCondition, 1),
                new EntityCharacter.ClothingSet(ItemDict.SKIN_PEACH, ItemDict.GetColoredItem(ItemDict.HAIR_LUCKY_LUKE, Util.HAIR_CHARCOAL_BLACK), ItemDict.EYES_BROWN,
                ItemDict.GetColoredItem(ItemDict.BASEBALL_CAP, Util.BLACK), ItemDict.GetColoredItem(ItemDict.SHORT_SLEEVE_TEE, Util.LIGHT_GREY), ItemDict.GetColoredItem(ItemDict.HOODED_SWEATSHIRT, Util.LIGHT_GREY), ItemDict.GetColoredItem(ItemDict.JEANS, Util.BLACK), ItemDict.GetColoredItem(ItemDict.SNEAKERS, Util.WHITE), ItemDict.CLOTHING_NONE,
                ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE,
                winterCondition, 1));

            List<EntityCharacter.Schedule.Event> rockwellSchedule = Util.GenerateSchedule(
                new EntityCharacter.Schedule.StandAtEvent(areas[Area.AreaEnum.FARM], areas[Area.AreaEnum.FARM].GetWaypoint("SPleft"), 7, 0, 7, 4, trueCondition, 0),
                new EntityCharacter.Schedule.StandAtEvent(areas[Area.AreaEnum.S1], areas[Area.AreaEnum.S1].GetWaypoint("SPs1entrance"), 7, 5, 24, 59, trueCondition, 0));

            List<EntityCharacter.DialogueOption> rockwellDialogue = Util.GenerateDialogueList(
                new EntityCharacter.DialogueOption(new DialogueNode("I'm Rockwell.", DialogueNode.PORTRAIT_BAD), trueCondition),
                new EntityCharacter.DialogueOption(new DialogueNode("I'm Rockwell. (In Spring)", DialogueNode.PORTRAIT_BAD), springCondition, 3));
            characters.Add(rockwell = new EntityCharacter(this, EntityCharacter.CharacterEnum.ROCKWELL, rockwellClothing, rockwellSchedule, rockwellDialogue));
            areas[0].AddEntity(rockwell);
            MoveCharacter(rockwell, currentArea, GetAreaByEnum(Area.AreaEnum.FARM));
            rockwell.SetPosition(areas[Area.AreaEnum.FARM].GetWaypoint("SPleft").position - new Vector2(0, 32.1f));

            //CAMUS
            List<EntityCharacter.ClothingSet> camusClothing = Util.GenerateClothingSetList(
                new EntityCharacter.ClothingSet(ItemDict.SKIN_PEACH, ItemDict.GetColoredItem(ItemDict.HAIR_LUCKY_LUKE, Util.HAIR_CHARCOAL_BLACK), ItemDict.EYES_BROWN,
                ItemDict.CLOTHING_NONE, ItemDict.GetColoredItem(ItemDict.SHORT_SLEEVE_TEE, Util.LIGHT_GREY), ItemDict.GetColoredItem(ItemDict.ALL_SEASON_JACKET, Util.BLACK), ItemDict.GetColoredItem(ItemDict.JEANS, Util.NAVY), ItemDict.GetColoredItem(ItemDict.SNEAKERS, Util.WHITE), ItemDict.CLOTHING_NONE,
                ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE,
                springCondition, 1),
                new EntityCharacter.ClothingSet(ItemDict.SKIN_PEACH, ItemDict.GetColoredItem(ItemDict.HAIR_LUCKY_LUKE, Util.HAIR_CHARCOAL_BLACK), ItemDict.EYES_BROWN,
                ItemDict.CLOTHING_NONE, ItemDict.BUTTON_DOWN, ItemDict.CLOTHING_NONE, ItemDict.GetColoredItem(ItemDict.CHINO_SHORTS, Util.NAVY), ItemDict.GetColoredItem(ItemDict.SNEAKERS, Util.WHITE), ItemDict.CLOTHING_NONE,
                ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE,
                summerCondition, 1),
                new EntityCharacter.ClothingSet(ItemDict.SKIN_PEACH, ItemDict.GetColoredItem(ItemDict.HAIR_LUCKY_LUKE, Util.HAIR_CHARCOAL_BLACK), ItemDict.EYES_BROWN,
                ItemDict.GetColoredItem(ItemDict.BASEBALL_CAP, Util.BLACK), ItemDict.GetColoredItem(ItemDict.BUTTON_DOWN, Util.WHITE), ItemDict.GetColoredItem(ItemDict.ALL_SEASON_JACKET, Util.BLACK), ItemDict.GetColoredItem(ItemDict.CHINOS, Util.BROWN), ItemDict.GetColoredItem(ItemDict.SNEAKERS, Util.WHITE), ItemDict.CLOTHING_NONE,
                ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE,
                fallCondition, 1),
                new EntityCharacter.ClothingSet(ItemDict.SKIN_PEACH, ItemDict.GetColoredItem(ItemDict.HAIR_LUCKY_LUKE, Util.HAIR_CHARCOAL_BLACK), ItemDict.EYES_BROWN,
                ItemDict.GetColoredItem(ItemDict.BASEBALL_CAP, Util.BLACK), ItemDict.GetColoredItem(ItemDict.SHORT_SLEEVE_TEE, Util.LIGHT_GREY), ItemDict.GetColoredItem(ItemDict.HOODED_SWEATSHIRT, Util.LIGHT_GREY), ItemDict.GetColoredItem(ItemDict.JEANS, Util.BLACK), ItemDict.GetColoredItem(ItemDict.SNEAKERS, Util.WHITE), ItemDict.CLOTHING_NONE,
                ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE, ItemDict.CLOTHING_NONE,
                winterCondition, 1));

            List<EntityCharacter.Schedule.Event> camusSchedule = Util.GenerateSchedule(
                new EntityCharacter.Schedule.StandAtEvent(areas[Area.AreaEnum.FARM], areas[Area.AreaEnum.FARM].GetWaypoint("SPleft"), 7, 0, 7, 4, trueCondition, 0));

            List<EntityCharacter.DialogueOption> camusDialogue = Util.GenerateDialogueList(
                new EntityCharacter.DialogueOption(new DialogueNode("I'm Camus.", DialogueNode.PORTRAIT_BAD), trueCondition));
            characters.Add(camus = new EntityCharacter(this, EntityCharacter.CharacterEnum.CAMUS, camusClothing, camusSchedule, camusDialogue));
            areas[0].AddEntity(camus);
            MoveCharacter(camus, currentArea, GetAreaByEnum(Area.AreaEnum.FARM));
            camus.SetPosition(areas[Area.AreaEnum.FARM].GetWaypoint("SPleft").position - new Vector2(0, 32.1f));
        }

        public void MoveCharacter(EntityCharacter character, Area areaFrom, Area areaTo)
        {
            areaFrom.RemoveEntity(character);
            areaTo.AddEntity(character);
            character.SetCurrentArea(areaTo);
        }

        public EntityCharacter GetCharacter(EntityCharacter.CharacterEnum cEnum)
        {
            foreach(Area.AreaEnum aEnum in areas.Keys)
            {
                Area area = areas[aEnum];
                EntityCharacter character = area.GetCharacter(cEnum);
                if(character != null)
                {
                    return character;
                }
            }
            return null;
        }

        private int GetDayAdjustment()
        {
            switch(currentSeason)
            {
                case Season.SPRING:
                    return 0;
                case Season.SUMMER:
                    return 1;
                case Season.AUTUMN:
                    return 0;
                case Season.WINTER:
                    return 1;
            }
            return 10000;
        }

        public void DrawForeground(SpriteBatch sb, RectangleF cameraBoundingBox, float layerDepth)
        {
            currentArea.DrawForeground(sb, cameraBoundingBox, layerDepth);
        }

        public float GetDarkLevel()
        {
            float hourStart = 0.0f;
            if (GetHour() == 0)
            {
                hourStart = hourlyDark[23];
            }
            else
            {
                hourStart = hourlyDark[GetHour() - 1];
            }
            float hourEnd = hourlyDark[GetHour()];
            float hourChange = hourEnd - hourStart;
            hourChange *= GetMinute() / 60.0f;
            return hourStart + hourChange;
        }

        public float GetLightLevel()
        {
            float hourStart = 0.0f;
            if (GetHour() == 0)
            {
                hourStart = hourlyLight[23];
            }
            else
            {
                hourStart = hourlyLight[GetHour() - 1];
            }
            float hourEnd = hourlyLight[GetHour()];
            float hourChange = hourEnd - hourStart;
            hourChange *= GetMinute() / 60.0f;
            return hourStart + hourChange;
        }

        public Dictionary<Area.AreaEnum, Area> GetAreaDict()
        {
            return areas;
        }

        public void SetCurrentArea(Area newArea)
        {
            this.currentArea = newArea;
        }

        public Area GetCurrentArea()
        {
            return currentArea;
        }

        public Season GetSeason()
        {
            return currentSeason;
        }

        public int GetDay()
        {
            return currentDay;
        }

        public int GetHour()
        {
            return (int)(currentTime / 60);
        }

        public int GetMinute()
        {
            return ((int)currentTime) % 60;
        }

        public TimeOfDay GetTimeOfDay()
        {
            if (GetHour() < MORNING_START_HOUR)
            {
                return TimeOfDay.NIGHT;
            } else if(GetHour() < MORNING_END_HOUR)
            {
                return TimeOfDay.MORNING;
            } else if (GetHour() < DAY_END_HOUR)
            {
                return TimeOfDay.DAY;
            } else if (GetHour() < EVENING_END_HOUR)
            {
                return TimeOfDay.EVENING;
            } else
            {
                return TimeOfDay.NIGHT;
            }
        }

        public TimeData GetTimeData()
        {
            return new TimeData(currentSeason, GetDay(), GetHour(), GetMinute(), GetTimeOfDay());
        }

        public void Update(float deltaTime, GameTime gameTime, EntityPlayer player, GameplayInterface ui, Camera camera, bool cutscene)
        {
            if (!paused)
            {
                if (transitionToArea != null && ui.IsTransitionReady())
                {
                    currentArea = transitionToArea;
                    currentArea.MoveToWaypoint(player, transitionToSpawn.name);
                    
                    if (transitionToSpawn.IsCameraLocked())
                    {
                        camera.Update(deltaTime, transitionToSpawn.cameraLockPosition, currentArea.MapPixelWidth(), currentArea.MapPixelHeight());
                        camera.Lock();
                    } else
                    {
                        camera.Update(deltaTime, player.GetAdjustedPosition(), currentArea.MapPixelWidth(), currentArea.MapPixelHeight());
                        camera.Unlock();
                    }
                    currentArea.RandomizeBackground(camera.GetBoundingBox());
                    transitionToArea = null;
                    transitionToSpawn = null;
                    player.Unpause();
                }

                if (!cutscene)
                {
                    currentTime += deltaTime * 1;
                }
                currentArea.Update(deltaTime, gameTime, GetTimeData(), currentWeather, camera.GetBoundingBox());
                foreach(EntityCharacter chara in characters)
                {
                    if (chara.GetCurrentArea() != currentArea)
                    {
                        chara.Update(deltaTime, chara.GetCurrentArea());
                    }
                }

                tickTimer += deltaTime;
                if (tickTimer > TICK_LENGTH)
                {
                    tickTimer -= TICK_LENGTH;
                    foreach (Area.AreaEnum areaEnum in areas.Keys)
                    {
                        areas[areaEnum].Tick(TICK_LENGTH, player, this);
                    }
                }
            }
        }

        public Area GetAreaByEnum(Area.AreaEnum areaEnum)
        {
            return areas[areaEnum];
        }

        public void DrawBackground(SpriteBatch sb, RectangleF cameraBoundingBox, float layerDepth)
        {
            currentArea.DrawBackground(sb, cameraBoundingBox, layerDepth);
        }

        public void DrawBuildingBlocks(SpriteBatch sb, float layerDepth)
        {
            currentArea.DrawBuildingBlocks(sb, layerDepth);
        }

        public void DrawItemEntities(SpriteBatch sb, float layerDepth)
        {
            currentArea.DrawItemEntities(sb, layerDepth);
        }

        public void DrawEntities(SpriteBatch sb, DrawLayer layer, RectangleF cameraBoundingBox, float layerDepth)
        {
            currentArea.DrawEntities(sb, layer, cameraBoundingBox, layerDepth);
        }

        public void DrawTerrain(SpriteBatch sb, Matrix cameraMatrix, float layerDepth)
        {
            currentArea.DrawTerrain(sb, cameraMatrix ,layerDepth); //draw the tiledmap
        }

        public void DrawWater(SpriteBatch sb, Matrix cameraMatrix, float layerDepth)
        {
            currentArea.DrawWater(sb, cameraMatrix, layerDepth);
        }

        public bool IsDayOver()
        {
            return currentTime >= DAY_ENDING_TIME;
        }

        public void AdvanceDay(EntityPlayer player)
        {
            tickTimer = 0;
            int numTicksSkipped = (int)(DAY_STARTING_TIME / TICK_LENGTH);
            float timeSkipped = DAY_ENDING_TIME - currentTime;
            numTicksSkipped += (int)(timeSkipped / TICK_LENGTH);
            for(int i = 0; i < numTicksSkipped; i++)
            {
                foreach (Area.AreaEnum areaEnum in areas.Keys)
                {
                    areas[areaEnum].Tick(TICK_LENGTH, player, this);
                }
            }

            currentTime = DAY_STARTING_TIME;
            //currentTime = 0;
            currentDay++;
            if (currentDay >= DAYS_IN_SEASON)
            {
                currentDay = 0;
                switch (currentSeason)
                {
                    case Season.SPRING:
                        currentSeason = Season.SUMMER;
                        break;
                    case Season.SUMMER:
                        currentSeason = Season.AUTUMN;
                        break;
                    case Season.AUTUMN:
                        currentSeason = Season.WINTER;
                        break;
                    case Season.WINTER:
                        currentSeason = Season.SPRING;
                        break;
                }
            }

            int weatherSeed = Util.RandInt(1, 10);
            switch(currentSeason)
            {
                case Season.SPRING:
                    if(weatherSeed <= 2)
                    {
                        currentWeather = Weather.SUNNY;
                    }  else if (weatherSeed <= 7)
                    {
                        currentWeather = Weather.CLOUDY;
                    } else
                    {
                        currentWeather = Weather.RAINY;
                    }
                    break;
                case Season.SUMMER:
                    if(weatherSeed <= 5)
                    {
                        currentWeather = Weather.SUNNY;
                    } else if (weatherSeed <= 7)
                    {
                        currentWeather = Weather.RAINY;
                    } else
                    {
                        currentWeather = Weather.CLOUDY;
                    }
                    break;
                case Season.AUTUMN:
                    if(weatherSeed <= 6)
                    {
                        currentWeather = Weather.CLOUDY;
                    } else if (weatherSeed <= 8)
                    {
                        currentWeather = Weather.SUNNY;
                    } else
                    {
                        currentWeather = Weather.RAINY;
                    }
                    break;
                case Season.WINTER:
                    if(weatherSeed <= 5)
                    {
                        currentWeather = Weather.SNOWY;
                    } else if (weatherSeed <= 9)
                    {
                        currentWeather = Weather.CLOUDY;
                    } else
                    {
                        currentWeather = Weather.SUNNY;
                    }
                    break;
            }

            foreach (Area.AreaEnum areaEnum in areas.Keys)
            {
                areas[areaEnum].TickDay(this, player);
            }
            
        }

        //save
        public SaveState GenerateSave()
        {
            SaveState state = new SaveState(SaveState.Identifier.WORLD);
            state.AddData("season", currentSeason.ToString());
            state.AddData("day", currentDay.ToString());
            state.AddData("weather", currentWeather.ToString());
            return state;
        }

        public void Pause()
        {
            paused = true;
        }

        public void Unpause()
        {
            paused = false;
        }

        public Weather GetWeather()
        {
            return currentWeather;
        }

        //load
        public void LoadSave(SaveState save)
        {
            currentSeason = StringToSeason(save.TryGetData("season", "SPRING"));
            currentDay = Int32.Parse(save.TryGetData("day", "0"));
            string weatherName = save.TryGetData("weather", Weather.CLOUDY.ToString());
            World.Weather weather;
            Enum.TryParse(weatherName, out weather);
            currentWeather = weather;
        }

        public void SetWeather(Weather newWeather)
        {
            this.currentWeather = newWeather;
        }

        public void DrawWalls(SpriteBatch sb, Matrix cameraMatrix, float layerDepth)
        {
            currentArea.DrawWalls(sb, cameraMatrix, layerDepth);
        }

        public void DrawParticles(SpriteBatch sb, float layerDepth)
        {
            currentArea.DrawParticles(sb, layerDepth);
        }

        public void SetTime(int hour, int minute)
        {
            currentTime = (hour * 60) + minute;
        }

        public Area GetAreaByName(string areaName)
        {
            foreach (Area.AreaEnum areaEnum in areas.Keys)
            {
                if (areaEnum.ToString().Equals(areaName))
                {
                    return GetAreaByEnum(areaEnum);
                }
            }
            throw new Exception("Exception in GetAreaByName...");
        }

        public void HandleTransitions(EntityPlayer player, GameplayInterface ui)
        {
            Area.TransitionZone tz = GetCurrentArea().CheckTransition(player.GetAdjustedPosition(), player.AttemptingTransition());
            if (tz != null)
            {
                if(player.AttemptingTransition())
                {
                    player.ToggleAttemptTransition();
                }
                foreach (Area.AreaEnum areaEnum in areas.Keys)
                {
                    if (areaEnum.ToString().Equals(tz.to))
                    {
                        switch(tz.animation)
                        {
                            case Area.TransitionZone.Animation.TO_DOWN:
                                ui.TransitionDown();
                                player.Pause();
                                break;
                            case Area.TransitionZone.Animation.TO_UP:
                                ui.TransitionUp();
                                player.Pause();
                                break;
                            case Area.TransitionZone.Animation.TO_LEFT:
                                ui.TransitionLeft();
                                player.Pause();
                                break;
                            case Area.TransitionZone.Animation.TO_RIGHT:
                                ui.TransitionRight();
                                player.Pause();
                                break;
                        }
                        transitionToArea = areas[areaEnum];
                        transitionToSpawn = transitionToArea.GetWaypoint(tz.spawn);
                    }
                }
            }
        }
    }
}
