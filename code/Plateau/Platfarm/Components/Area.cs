using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using Platfarm.Components;
using Platfarm.Entities;
using Platfarm.Items;
using Platfarm.Particles;
using Platfarm.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm
{
    public class Area
    {
        public static string BACKGROUND_LAYER_SKY_DAY = "skyDay";
        public static string BACKGROUND_LAYER_SKY_MORNING = "skyMorning";
        public static string BACKGROUND_LAYER_SKY_EVENING = "skyEvening";
        public static string BACKGROUND_LAYER_SKY_NIGHT = "skyNight";

        public static string BACKGROUND_LAYER_SUN = "sun";
        public static string BACKGROUND_LAYER_MOON = "moon";

        public static string BACKGROUND_LAYER_CLOUDS_FRONT = "cloudsFront";
        public static string BACKGROUND_LAYER_CLOUDS_MIDDLE = "cloudsMiddle";
        public static string BACKGROUND_LAYER_CLOUDS_BACK = "cloudsBack";
        public static string FOREGROUND_LAYER_CLOUDS = "fgClouds";

        public static string BACKGROUND_LAYER_RAIN_FRONT = "rainFront";
        public static string BACKGROUND_LAYER_RAIN_BACK = "rainBack";
        public static string FOREGROUND_LAYER_RAIN = "fgRain";

        public static string BACKGROUND_LAYER_SNOW_FRONT = "snowFront";
        public static string BACKGROUND_LAYER_SNOW_BACK = "snowBack";
        public static string FOREGROUND_LAYER_SNOW = "fgSnow";

        public static string FOREGROUND_LAYER_WEATHER_FILTER = "filterWeather";
        public static string FOREGROUND_LAYER_SEASON_FILTER = "filterSeason";

        public static string FOREGROUND_LAYER_PARTICLES = "particleFG";
        public static string BACKGROUND_LAYER_PARTICLES = "particleBG";

        public class XYTile
        {
            public int tileX, tileY;

            public XYTile(int tileX, int tileY)
            {
                this.tileX = tileX;
                this.tileY = tileY;
            }
        }

        public class SoundZone
        {
            public World.TimeOfDay time;
            public RectangleF rect;
            public SoundSystem.Sound sound;
            public World.Season season;

            public SoundZone(RectangleF zone, SoundSystem.Sound sound, World.TimeOfDay time, World.Season season)
            {
                this.time = time;
                this.rect = zone;
                this.sound = sound;
                this.season = season;
            }
        }

        public class Subarea
        {
            public enum NameEnum
            {
                APEX, BEACH, FARM, STORE, CAFE, BOOKSTORELOWER, BOOKSTOREUPPER, FARMHOUSE, ROCKWELLHOUSE, BEACHHOUSE,
                PIPERHOUSE, TOWNHALL, WORKSHOP, FORGE, S0WALK, S0WARP, S1WALK, S1WARP, S2, S3, S4, TOWN, INN
            }

            public RectangleF rect;
            public NameEnum subareaName;

            public Subarea(RectangleF rect, string subareaName)
            {
                this.rect = rect;
                if(!Enum.TryParse(subareaName.ToUpper(), out this.subareaName))
                {
                    throw new Exception("Couldn't parse " + subareaName + " to enum...");
                }
            }
        }

        public class MovingPlatformDirectorZone
        {
            public RectangleF rectangle;
            public Vector2 newVelocity;

            public MovingPlatformDirectorZone(RectangleF rectangle, Vector2 velocity)
            {
                this.rectangle = rectangle;
                this.newVelocity = velocity;
            }
        }

        public class FishingZone
        {
            public RectangleF rectangle;
            public LootTables.LootTable lootTable;
            public int difficulty;

            public FishingZone(int difficulty, LootTables.LootTable lootTable, RectangleF rectangle) {
                this.difficulty = difficulty;
                this.lootTable = lootTable;
                this.rectangle = rectangle;
            }
        }

        public class CutsceneTriggerZone
        {
            public string cutsceneID;
            public RectangleF rectangle;

            public CutsceneTriggerZone(string id, RectangleF rectangle)
            {
                this.cutsceneID = id;
                this.rectangle = rectangle;
            }
        }

        public class NamedZone
        {
            public string name;
            public RectangleF rectangle;

            public NamedZone(string name, RectangleF rectangle)
            {
                this.name = name;
                this.rectangle = rectangle;
            }
        }

        public class SpawnZone
        {
            public class Entry
            {
                public EntityType type;
                public int weight;
                
                public Entry(EntityType type, int weight)
                {
                    this.weight = weight;
                    this.type = type;
                }
            }

            protected List<EntityType> possibleSpawns;
            protected List<XYTile> validTiles;
            protected float spawnFrequency;

            public SpawnZone(List<XYTile> validTiles, float spawnFrequency, params Entry[] entries)
            {
                this.spawnFrequency = spawnFrequency;
                this.possibleSpawns = new List<EntityType>();
                this.validTiles = validTiles;

                foreach(Entry entry in entries)
                {
                    for(int i = 0; i < entry.weight; i++)
                    {
                        possibleSpawns.Add(entry.type);
                    }
                }
            }

            public virtual bool IsGivenSpawnLegal(Area area, Vector2 chosenTile, TileEntity toPlace)
            {
                return area.IsTileEntityPlacementValid((int)chosenTile.X, (int)chosenTile.Y, toPlace.GetTileWidth(), toPlace.GetTileHeight());
            }

            private void TryCreateSpawn(Area area)
            {
                //choose a random tile
                XYTile chosenXYTile = validTiles[Util.RandInt(0, validTiles.Count - 1)];
                Vector2 chosenTile = new Vector2(chosenXYTile.tileX, chosenXYTile.tileY);
                EntityType chosenType = GetSpawnType(area);
                TileEntity measurement = (TileEntity)EntityFactory.GetEntity(chosenType, ItemDict.NONE, chosenTile, area);
                chosenTile.Y -= (measurement.GetTileHeight() - 1);
                TileEntity toPlace = (TileEntity)EntityFactory.GetEntity(chosenType, ItemDict.NONE, chosenTile, area);
                if (IsGivenSpawnLegal(area, chosenTile, toPlace))
                {
                    area.AddTileEntity(toPlace);
                }
            }

            public virtual void TickDaily(Area area)
            {
                float spawnsLeft = spawnFrequency;
                while(spawnsLeft > 0)
                {
                    if(spawnsLeft >= 1)
                    {
                        TryCreateSpawn(area);
                        spawnsLeft--;
                    } else
                    {
                        float roll = Util.RandInt(1, 100) / 100.0f;
                        if(roll >= spawnsLeft)
                        {
                            TryCreateSpawn(area);
                        }
                        spawnsLeft = 0;
                    }
                } 
            }

            public virtual EntityType GetSpawnType(Area area)
            {
                EntityType chosenType = possibleSpawns[Util.RandInt(0, possibleSpawns.Count() - 1)];
                return chosenType;
            }
        }

        public class FarmSpawnZone : SpawnZone
        {
            public FarmSpawnZone(List<XYTile> validTiles, float spawnFrequency, params Entry[] entries) : base(validTiles, spawnFrequency, entries)
            {
                //do nothing...
            }

            public override EntityType GetSpawnType(Area area)
            {
                switch(Util.RandInt(1, 25))
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        return EntityType.WEEDS;
                    case 6:
                    case 7:
                        return EntityType.ROCK;
                    case 8:
                    case 9:
                        return EntityType.BRANCH;
                    case 10:
                        return EntityType.BRANCH_LARGE;
                    case 11:
                        return EntityType.ROCK_LARGE;
                    case 12: //forage
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                        switch (area.GetSeason())
                        {
                            case World.Season.SPRING:
                                int roll = Util.RandInt(1, 4);
                                switch (roll)
                                {
                                    case 1:
                                        return EntityType.BLUEBELL;
                                    case 2:
                                        return EntityType.NETTLES;
                                    case 3:
                                        return EntityType.CHICKWEED;
                                    case 4:
                                        return EntityType.SUNFLOWER;
                                }
                                break;
                            case World.Season.SUMMER:
                                int roll2 = Util.RandInt(1, 2);
                                switch (roll2)
                                {
                                    case 1:
                                        return EntityType.MARIGOLD;
                                    case 2:
                                        return EntityType.LAVENDER;
                                }
                                break;
                            case World.Season.AUTUMN:
                                return EntityType.FALL_LEAF_PILE;
                            case World.Season.WINTER:
                                return EntityType.WINTER_SNOW_PILE;
                        }
                        break;
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                        return EntityType.PINE_TREE;
                    case 23:
                    case 24:
                    case 25:
                        return EntityType.BUSH;
                }
                return EntityType.EMERALD_MOSS;
            }
        }

        public class FloorSpawnZone : SpawnZone
        {
            public FloorSpawnZone(List<XYTile> validTiles, int spawnFrequency, params Entry[] entries) : base (validTiles, spawnFrequency, entries)
            {
                foreach(XYTile tile in validTiles)
                {
                    tile.tileY++;
                }
            }

            public override bool IsGivenSpawnLegal(Area area, Vector2 chosenTile, TileEntity toPlace)
            {
                bool validPlacement = area.IsFloorEntityPlacementValid((int)chosenTile.X, (int)chosenTile.Y, toPlace.GetTileWidth());
                bool farmableAbove = area.GetTileEntity((int)chosenTile.X, (int)chosenTile.Y - 1) is TEntityFarmable;
                if (validPlacement && !farmableAbove)
                {
                    return true;
                }
                return false;
            }
        }

        public class TransitionZone
        {
            public enum Animation
            {
                TO_UP, TO_DOWN, TO_LEFT, TO_RIGHT
            }

            public string to;
            public string spawn;
            public RectangleF rectangle;
            public bool automatic;
            public Animation animation;

            public TransitionZone(RectangleF rectangle, string to, string spawn, bool auto, Animation animation)
            {
                this.to = to;
                this.spawn = spawn;
                this.rectangle = rectangle;
                this.automatic = auto;
                this.animation = animation;
            }
        }

        public class PathingHelper
        {
            public enum Type
            {
                CONDITIONALJUMP
            }

            public RectangleF rect;
            public Type type;

            public PathingHelper(RectangleF rect, Type type)
            {
                this.rect = rect;
                this.type = type;
            }
        }

        public class Waypoint
        {
            public Vector2 position;
            public Vector2 cameraLockPosition;
            public string name;
            public Area area;

            public Waypoint(Vector2 position, string name, Area area)
            {
                this.position = position;
                this.name = name;
                this.cameraLockPosition = new Vector2(-10000, -10000);
                this.area = area;
            }

            public Waypoint(Vector2 position, string name, Vector2 cameraLockPosition, Area area)
            {
                this.name = name;
                this.position = position;
                this.cameraLockPosition = cameraLockPosition;
                this.area = area;
            }

            public bool IsCameraLocked()
            {
                return cameraLockPosition.X != -10000 & cameraLockPosition.Y != -10000;
            }
        }

        public class LightSource
        {
            public enum Strength
            {
                SMALL, MEDIUM, LARGE
            }

            public Strength lightStrength;
            public Vector2 position;
            public Entity source;
            public Color color;

            public LightSource(Strength lightStrength, Vector2 position, Color color, Entity source = null)
            {
                this.lightStrength = lightStrength;
                this.position = position;
                this.source = source;
                this.color = color;
            }
        }

        private class EntityListManager
        {
            private List<Entity> entityList;
            private List<IInteract> interactableEntityList;
            private List<IInteractContact> contactInteractableEntityList;
            private List<IInteractTool> toolInteractableEntityList;
            private List<EntityCollidable> collideableEntityList;
            private List<MEntitySolid> solidEntityList;
            private Dictionary<DrawLayer, List<Entity>> entityListByLayer;

            public EntityListManager()
            {
                entityList = new List<Entity>();
                entityListByLayer = new Dictionary<DrawLayer, List<Entity>>();
                collideableEntityList = new List<EntityCollidable>();
                interactableEntityList = new List<IInteract>();
                toolInteractableEntityList = new List<IInteractTool>();
                contactInteractableEntityList = new List<IInteractContact>();
                solidEntityList = new List<MEntitySolid>();
            }

            public void Remove(Entity entity)
            {
                entityList.Remove(entity);
                entityListByLayer[entity.GetDrawLayer()].Remove(entity);
                if(entity is EntityCollidable)
                {
                    collideableEntityList.Remove((EntityCollidable)entity);
                }
                if(entity is MEntitySolid)
                {
                    solidEntityList.Remove((MEntitySolid)entity);
                }
                if(entity is IInteract)
                {
                    interactableEntityList.Remove((IInteract)entity);
                }
                if(entity is IInteractContact)
                {
                    contactInteractableEntityList.Remove((IInteractContact)entity);
                }
                if(entity is IInteractTool)
                {
                    toolInteractableEntityList.Remove((IInteractTool)entity);
                }
            }

            public void Add(Entity entity)
            {
                entityList.Add(entity);
                if(!entityListByLayer.ContainsKey(entity.GetDrawLayer()))
                {
                    entityListByLayer[entity.GetDrawLayer()] = new List<Entity>();
                }
                entityListByLayer[entity.GetDrawLayer()].Add(entity);
                if(entity is EntityCollidable)
                {
                    collideableEntityList.Add(((EntityCollidable)entity));
                }
                if(entity is MEntitySolid)
                {
                    solidEntityList.Add((MEntitySolid)entity);
                }
                if(entity is IInteract)
                {
                    interactableEntityList.Add((IInteract)entity);
                }
                if(entity is IInteractContact)
                {
                    contactInteractableEntityList.Add((IInteractContact)entity);
                }
                if(entity is IInteractTool)
                {
                    toolInteractableEntityList.Add((IInteractTool)entity);
                }
            }

            public List<Entity> GetEntitiesByDrawLayer(DrawLayer layer)
            {
                if(entityListByLayer.ContainsKey(layer))
                {
                    return entityListByLayer[layer];
                }
                return new List<Entity>();
            }

            public List<Entity> GetEntityList()
            {
                return entityList;
            }

            public List<EntityCollidable> GetCollideableEntityList()
            {
                return collideableEntityList;
            }

            public List<MEntitySolid> GetSolidEntityList()
            {
                return solidEntityList;
            }

            public List<IInteract> GetInteractableEntityList()
            {
                return interactableEntityList;
            }

            public List<IInteractContact> GetContactInteractableEntityList()
            {
                return contactInteractableEntityList;
            }

            public List<IInteractTool> GetToolInteractableEntityList()
            {
                return toolInteractableEntityList;
            }
        }

        public enum GroundTileType
        {
            EARTH, SAND, BRIDGE, INTERIOR
        }

        public enum AreaEnum {
            FARM, TOWN, BEACH, S0, S1, S2, S3, S4, APEX, INTERIOR, NONE
        }

        public enum CollisionTypeEnum
        {
            BOUNDARY, AIR, SOLID, BRIDGE, WATER,
            SCAFFOLDING, SCAFFOLDING_BRIDGE, SCAFFOLDING_BLOCK
        }

        private int tileHeight, tileWidth;
        private int widthInTiles, heightInTiles;

        private TiledMap tiledMap;
        private CollisionTypeEnum[,] collisionMap;
        private bool[,] wallMap;
        private List<PathingHelper> pathingHelpers;
        private TileEntity[,] tileEntityGrid;
        private BuildingBlock[,] buildingBlockGrid;
        private List<BuildingBlock> buildingBlockList;
        private TileEntity[,] wallEntityGrid;
        private PEntityWallpaper[,] wallpaperEntityGrid;
        private AreaEnum areaEnum;
        private string name;
        private TiledMapRenderer mapRenderer;
        private List<TransitionZone> transitions = new List<TransitionZone>();
        private List<Waypoint> waypoints = new List<Waypoint>();
        private EntityListManager entityListManager = new EntityListManager();
        private List<LightSource> lights = new List<LightSource>();
        private List<EntityItem> itemEntities = new List<EntityItem>();
        private List<Particle> particleList = new List<Particle>();
        private List<SpawnZone> spawnZones = new List<SpawnZone>();
        private List<FishingZone> fishingZones = new List<FishingZone>();
        private List<NamedZone> nameZones = new List<NamedZone>();
        private List<Subarea> subareas = new List<Subarea>();
        private List<SoundZone> soundZones = new List<SoundZone>();
        private List<CutsceneTriggerZone> cutsceneTriggerZones = new List<CutsceneTriggerZone>();
        private List<MovingPlatformDirectorZone> directorZones = new List<MovingPlatformDirectorZone>();
        private LayeredBackground background, foreground;
        private bool cameraMoves;
        private static float BLEND_MINUTES = 60.0f;
        private World.Weather areaWeather;
        private World.Season areaSeason;
        private World.Season worldSeason;
        private bool interior;

        //ALL NUMBERS ARE 1 HIGHER THAN TILED SAYS! TILED USES LACK OF A TILE AS ID=0; so IDS REALLY START AT 1
        //used for map collision
        private static int[] WATER_TILE_IDS = {204, 205};

        //used for particles
        private static int[] ORANGE_EARTH_TILE_IDS = {151, 152, 153, 154, 156};
        private static int[] BRIDGE_TILE_IDS = {31, 32, 33, 34, 35, 36, 38, 39, 40};
        private static int[] SAND_TILE_IDS = {191, 192, 193, 194, 195, 202, 203, 211, 212, 213, 214, 215};
        private static int[] INTERIOR_TILE_IDS = {68, 77, 78, 79};

        public Area(AreaEnum name, TiledMap map, bool cameraMoves, TiledMapRenderer mapRenderer, ContentManager content, EntityPlayer player, RectangleF cameraBoundingBox)
        {
            this.areaWeather = World.Weather.SUNNY;
            this.areaEnum = name;
            this.cameraMoves = cameraMoves;

            //Set the tiled map, create the collision map based off of the given map
            this.tiledMap = map;
            this.mapRenderer = mapRenderer;

            this.name = tiledMap.Properties["name"];
            string seasonProperty = tiledMap.Properties["season"];
            if(seasonProperty == "spring")
            {
                areaSeason = World.Season.SPRING;
            } else if (seasonProperty == "summer")
            {
                areaSeason = World.Season.SUMMER;
            } else if (seasonProperty == "fall" || seasonProperty == "autumn")
            {
                areaSeason = World.Season.AUTUMN;
            } else if (seasonProperty == "winter")
            {
                areaSeason = World.Season.WINTER;
            } else if (seasonProperty == "world" || seasonProperty == "defer")
            {
                areaSeason = World.Season.DEFER;
            } else
            {
                throw new Exception("Map file doesn't have a proper season property!");
            }
            this.interior = tiledMap.Properties["inside"] == "true";

            TiledMapObjectLayer transitionLayer = (TiledMapObjectLayer)tiledMap.GetLayer("transitions");
            foreach (TiledMapObject tiledObject in transitionLayer.Objects) {
                RectangleF rectangle = new RectangleF(tiledObject.Position, tiledObject.Size);
                string to = tiledObject.Properties["areaTo"];
                string spawn = tiledObject.Properties["areaSpawn"];
                string automatic = tiledObject.Properties["automatic"];
                string animation = tiledObject.Properties["animation"];
                TransitionZone.Animation anim = TransitionZone.Animation.TO_LEFT;
                if(animation.Equals("up"))
                {
                    anim = TransitionZone.Animation.TO_UP;
                }
                else if (animation.Equals("down"))
                {
                    anim = TransitionZone.Animation.TO_DOWN;
                }
                else if (animation.Equals("right"))
                {
                    anim = TransitionZone.Animation.TO_RIGHT;
                }
                transitions.Add(new TransitionZone(rectangle, to, spawn, automatic.Equals("yes"), anim));
            }

            //ENTITY LAYER
            TiledMapObjectLayer entityLayer = (TiledMapObjectLayer)tiledMap.GetLayer("entity");
            foreach (TiledMapObject tiledObject in entityLayer.Objects)
            {
                string entityType = tiledObject.Properties["entity"];
                if (entityType.Equals("farmhouse"))
                {
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_HOME), 1, 1, 1, Util.CreateAndFillArray(1, 1f));
                    MEntityHome home = new MEntityHome(tiledObject.Properties["id"], tiledObject.Position, sprite);
                    entityListManager.Add(home);
                } else if (entityType.Equals("door"))
                {
                    Texture2D tex = content.Load<Texture2D>(Paths.SPRITE_DOOR);
                    AnimatedSprite sprite = new AnimatedSprite(tex, 1, 1, 1, Util.CreateAndFillArray(1, 1f));
                    MEntityDoor door = new MEntityDoor(Util.GenerateAutomaticID("door", this), tiledObject.Position - new Vector2(0, tex.Height), sprite);
                    entityListManager.Add(door);
                } else if (entityType.Equals("storeShelf"))
                {
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_STORE_SHELF), 18, 2, 10, Util.CreateAndFillArray(18, 3000f));
                    MEntityStoreShelf shelf = new MEntityStoreShelf(tiledObject.Properties["id"], tiledObject.Position - new Vector2(0, sprite.GetFrameHeight()), sprite, this);
                    entityListManager.Add(shelf);
                } else if (entityType.Equals("storeCompost"))
                {
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_STORE_COMPOST_BIN), 8, 1, 8, Util.CreateAndFillArray(8, 3000f));
                    MEntityStoreCompost compost = new MEntityStoreCompost(tiledObject.Properties["id"], tiledObject.Position - new Vector2(0, sprite.GetFrameHeight()), sprite);
                    entityListManager.Add(compost);
                } else if (entityType.Equals("storeManikin"))
                {
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_STORE_MANIKIN), 1, 1, 1, Util.CreateAndFillArray(1, 3000f));
                    MEntityStoreManikin manikin = new MEntityStoreManikin(tiledObject.Properties["id"], tiledObject.Position - new Vector2(0, sprite.GetFrameHeight()), sprite, this);
                    entityListManager.Add(manikin);
                } else if (entityType.Equals("shipping_bin"))
                {
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_SHIPPING_BIN_SPRITESHEET), 3, 1, 3, Util.CreateAndFillArray(3, 0.15f));
                    MEntityShippingBin bin = new MEntityShippingBin(tiledObject.Properties["id"], tiledObject.Position - new Vector2(0, sprite.GetFrameHeight()), sprite);
                    entityListManager.Add(bin);
                } else if (entityType.Equals("storeFurniture"))
                {
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_STORE_PHANTOM_FURNITURE), 1, 1, 1, Util.CreateAndFillArray(1, 3000f));
                    MEntityStoreFurniture storeFurniture = new MEntityStoreFurniture(tiledObject.Properties["id"], tiledObject.Position - new Vector2(0, sprite.GetFrameHeight()), sprite);
                    entityListManager.Add(storeFurniture);
                } else if (entityType.Equals("shrineSpring")) //modify to be generic for all shrines...
                {
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_SHRINE), 1, 1, 1, Util.CreateAndFillArray(1, 2000f));
                    MEntityShrine springShrine = new MEntityShrine(tiledObject.Properties["id"], tiledObject.Position - new Vector2(0, sprite.GetFrameHeight()), sprite, "Spring Shrine", GameState.SHRINE_SEASON_SPRING_1, GameState.SHRINE_SEASON_SPRING_2);
                    entityListManager.Add(springShrine);
                } else if (entityType.Equals("directorZone"))
                {
                    float veloX = Int32.Parse(tiledObject.Properties["velocityX"]);
                    float veloY = Int32.Parse(tiledObject.Properties["velocityY"]);
                    MovingPlatformDirectorZone zone = new MovingPlatformDirectorZone(new RectangleF(tiledObject.Position, tiledObject.Size), new Vector2(veloX, veloY));
                    directorZones.Add(zone);
                } else if (entityType.Equals("platform"))
                {
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_MOVING_PLATFORM), 1, 1, 1, Util.CreateAndFillArray(1, 2000f));
                    float veloX = Int32.Parse(tiledObject.Properties["velocityX"]);
                    float veloY = Int32.Parse(tiledObject.Properties["velocityY"]);
                    MEntitySolid.PlatformType platType = MEntitySolid.PlatformType.SOLID;
                    string platTypeStr = tiledObject.Properties["type"].ToLower();
                    switch(platTypeStr)
                    {
                        case "solid":
                            platType = MEntitySolid.PlatformType.SOLID;
                            break;
                        case "bridge":
                            platType = MEntitySolid.PlatformType.BRIDGE;
                            break;
                    }
                    bool collideWithTerrain = tiledObject.Properties["collideTerrain"].ToLower().Equals("true");
                    MEntityMovingPlatform platform = new MEntityMovingPlatform(Util.GenerateAutomaticID("movingPlatform", this), tiledObject.Position, sprite, player, platType, new Vector2(veloX, veloY), collideWithTerrain);
                    entityListManager.Add(platform);
                }
                else if (entityType.Equals("solid"))
                {
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_MOVING_PLATFORM), 1, 1, 1, Util.CreateAndFillArray(1, 2000f));
                    MEntitySolid.PlatformType platType = MEntitySolid.PlatformType.SOLID;
                    string platTypeStr = tiledObject.Properties["type"].ToLower();
                    switch (platTypeStr)
                    {
                        case "solid":
                            platType = MEntitySolid.PlatformType.SOLID;
                            break;
                        case "bridge":
                            platType = MEntitySolid.PlatformType.BRIDGE;
                            break;
                    }
                    bool collideWithTerrain = tiledObject.Properties["collideTerrain"].ToLower().Equals("true");
                    MEntitySolid platform = new MEntitySolid(Util.GenerateAutomaticID("solid", this), tiledObject.Position, sprite, player, platType, collideWithTerrain);
                    entityListManager.Add(platform);
                }
                else if (entityType.Equals("trampoline") || entityType.Equals("trompolineUp"))
                {
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_TRAMPOLINE), 6, 1, 6, Util.CreateAndFillArray(6, 0.075f));
                    float veloX = Int32.Parse(tiledObject.Properties["velocityX"]);
                    float veloY = Int32.Parse(tiledObject.Properties["velocityY"]);
                    sprite.AddLoop("idle", 0, 0, true, false);
                    sprite.AddLoop("anim", 1, 5, false, false);
                    sprite.SetLoop("idle");
                    MEntityTrampoline trampoline = new MEntityTrampoline(sprite, tiledObject.Position + new Vector2(-2, -sprite.GetFrameHeight() + 1), new Vector2(veloX, veloY), MEntityTrampoline.TrampolineType.UP);
                    entityListManager.Add(trampoline);
                } else if (entityType.Equals("trampolineLeft"))
                {
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_TRAMPOLINE_SIDE), 6, 1, 6, Util.CreateAndFillArray(6, 0.075f));
                    float veloX = Int32.Parse(tiledObject.Properties["velocityX"]);
                    float veloY = Int32.Parse(tiledObject.Properties["velocityY"]);
                    sprite.AddLoop("idle", 0, 0, true, false);
                    sprite.AddLoop("anim", 1, 5, false, false);
                    sprite.SetLoop("idle");
                    MEntityTrampoline trampoline = new MEntityTrampoline(sprite, tiledObject.Position + new Vector2(-sprite.GetFrameWidth()+1, -2), new Vector2(veloX, veloY), MEntityTrampoline.TrampolineType.LEFT);
                    entityListManager.Add(trampoline);
                } else if (entityType.Equals("trampolineRight"))
                {
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_TRAMPOLINE_SIDE), 6, 1, 6, Util.CreateAndFillArray(6, 0.075f));
                    float veloX = Int32.Parse(tiledObject.Properties["velocityX"]);
                    float veloY = Int32.Parse(tiledObject.Properties["velocityY"]);
                    sprite.AddLoop("idle", 0, 0, true, false);
                    sprite.AddLoop("anim", 1, 5, false, false);
                    sprite.SetLoop("idle");
                    MEntityTrampoline trampoline = new MEntityTrampoline(sprite, tiledObject.Position + new Vector2(-1, -2), new Vector2(veloX, veloY), MEntityTrampoline.TrampolineType.RIGHT);
                    entityListManager.Add(trampoline);
                } else if (entityType.Equals("trampolineDown"))
                {
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_TRAMPOLINE), 6, 1, 6, Util.CreateAndFillArray(6, 0.075f));
                    float veloX = Int32.Parse(tiledObject.Properties["velocityX"]);
                    float veloY = Int32.Parse(tiledObject.Properties["velocityY"]);
                    sprite.AddLoop("idle", 0, 0, true, false);
                    sprite.AddLoop("anim", 1, 5, false, false);
                    sprite.SetLoop("idle");
                    MEntityTrampoline trampoline = new MEntityTrampoline(sprite, tiledObject.Position + new Vector2(-2, -1), new Vector2(veloX, veloY), MEntityTrampoline.TrampolineType.DOWN);
                    entityListManager.Add(trampoline);
                } else if (entityType.Equals("windCurrent"))
                {
                    World.Season season = World.Season.NONE;
                    string seasonStr = tiledObject.Properties["season"].ToLower();
                    switch(seasonStr)
                    {
                        case "spring":
                            season = World.Season.SPRING;
                            break;
                        case "summer":
                            season = World.Season.SUMMER;
                            break;
                        case "fall":
                        case "autumn":
                            season = World.Season.AUTUMN;
                            break;
                        case "winter":
                            season = World.Season.WINTER;
                            break;
                        case "defer":
                            season = World.Season.DEFER;
                            break;
                    }
                    EntityWindCurrent windCurrent = new EntityWindCurrent(new RectangleF(tiledObject.Position, tiledObject.Size), this, season);
                    entityListManager.Add(windCurrent);
                } else if (entityType.Equals("reverseCrystal"))
                {
                    float[] frameLengths = Util.CreateAndFillArray(12, 0.075f);
                    frameLengths[0] = 2.0f;
                    frameLengths[6] = 2.0f;
                    AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(Paths.SPRITE_REVERSE_CRYSTAL), 12, 2, 10, frameLengths);
                    sprite.AddLoop("normal", 0, 5, true, false);
                    sprite.AddLoop("reversed", 6, 11, true, false);
                    sprite.SetLoop("normal");
                    bool upsideDown = tiledObject.Properties["type"].Equals("down");
                    MEntityReverseCrystal crystal = new MEntityReverseCrystal(Util.GenerateAutomaticID("reverseCrystal", this), tiledObject.Position - (upsideDown ? new Vector2(0, 1) : new Vector2(0, sprite.GetFrameHeight()-1)), sprite, upsideDown);
                    entityListManager.Add(crystal);
                } else if (entityType.Equals("placeable"))
                {
                    string baseItem = tiledObject.Properties["item"];
                    PlaceableItem itemForm = (PlaceableItem)ItemDict.GetItemByName(baseItem);
                    Vector2 position = (tiledObject.Position/8.0f) - new Vector2(0, itemForm.GetPlaceableHeight());
                    PlacedEntity toAdd = (PlacedEntity)EntityFactory.GetEntity(EntityType.USE_ITEM, itemForm, position, this);
                    toAdd.MarkAsUnremovable();
                    AddEntity(toAdd);
                }
            }

            TiledMapObjectLayer waypointLayer = (TiledMapObjectLayer)tiledMap.GetLayer("waypoints");
            foreach (TiledMapObject tiledObject in waypointLayer.Objects)
            {
                Vector2 position = tiledObject.Position;
                string waypointName = tiledObject.Properties["name"];
                Vector2 camLock = new Vector2(-10000, -10000);
                if(tiledObject.Properties.ContainsKey("cameraX") && tiledObject.Properties.ContainsKey("cameraY"))
                {
                    camLock.X = Int32.Parse(tiledObject.Properties["cameraX"]);
                    camLock.Y = Int32.Parse(tiledObject.Properties["cameraY"]);
                }
                waypoints.Add(new Waypoint(position, waypointName, camLock, this));
            }

            TiledMapTileLayer baseLayer = (TiledMapTileLayer)tiledMap.GetLayer("base");
            TiledMapTileLayer wallLayer = (TiledMapTileLayer)tiledMap.GetLayer("walls");
            tileHeight = baseLayer.TileHeight;
            tileWidth = baseLayer.TileWidth;
            widthInTiles = baseLayer.Width;
            heightInTiles = baseLayer.Height;
            collisionMap = new CollisionTypeEnum[baseLayer.Width, baseLayer.Height];
            wallMap = new bool[baseLayer.Width, baseLayer.Height];
            pathingHelpers = new List<PathingHelper>();
            tileEntityGrid = new TileEntity[baseLayer.Width, baseLayer.Height];
            wallEntityGrid = new TileEntity[baseLayer.Width, baseLayer.Height];
            wallpaperEntityGrid = new PEntityWallpaper[baseLayer.Width, baseLayer.Height];
            buildingBlockGrid = new BuildingBlock[baseLayer.Width, baseLayer.Height];
            buildingBlockList = new List<BuildingBlock>();
            for (int x = 0; x < baseLayer.Width; x++)
            {
                for (int y = 0; y < baseLayer.Height; y++)
                {
                    tileEntityGrid[x, y] = null;
                    buildingBlockGrid[x, y] = null;
                    wallMap[x, y] = false;
                    wallEntityGrid[x, y] = null;
                }
            }
            for (int x = 0; x < baseLayer.Width; x++)
            {
                for (int y = 0; y < baseLayer.Height; y++)
                {
                    TiledMapTile? t;
                    baseLayer.TryGetTile(x, y, out t);

                    //process base layer
                    int tileGlobalId = t.Value.GlobalIdentifier;
                    if (BRIDGE_TILE_IDS.Contains(tileGlobalId))
                    {
                        collisionMap[x, y] = CollisionTypeEnum.BRIDGE;
                    }
                    else if (tileGlobalId != 0)
                    {
                        collisionMap[x, y] = CollisionTypeEnum.SOLID;
                    } else
                    {
                        collisionMap[x, y] = CollisionTypeEnum.AIR;
                    }

                    //process wall layer
                    TiledMapTile? t2;
                    wallLayer.TryGetTile(x, y, out t2);
                    tileGlobalId = t2.Value.GlobalIdentifier;
                    if (tileGlobalId != 0)
                    {
                        wallMap[x, y] = true;
                    } else
                    {
                        wallMap[x, y] = false;
                    }
                }
            }

            //read pathinghelper layer
            //read fishingzone layer
            TiledMapObjectLayer pathingLayer = (TiledMapObjectLayer)tiledMap.GetLayer("pathing");
            foreach (TiledMapObject tiledObject in pathingLayer.Objects)
            {
                string type = tiledObject.Properties["type"];
                PathingHelper.Type pht;
                if (!Enum.TryParse(type.ToUpper(), out pht))
                {
                    throw new Exception("Couldn't parse " + type.ToUpper() + " to enum...");
                }
                pathingHelpers.Add(new PathingHelper(new RectangleF(tiledObject.Position, tiledObject.Size), pht));
            }

            //read water layer
            TiledMapTileLayer waterLayer = (TiledMapTileLayer)tiledMap.GetLayer("water");
            for (int x = 0; x < baseLayer.Width; x++)
            {
                for (int y = 0; y < baseLayer.Height; y++)
                {
                    TiledMapTile? t;
                    waterLayer.TryGetTile(x, y, out t);

                    int tileGlobalId = t.Value.GlobalIdentifier;
                    if (collisionMap[x, y] == CollisionTypeEnum.AIR && Util.ArrayContains(WATER_TILE_IDS, tileGlobalId))
                    {
                        collisionMap[x, y] = CollisionTypeEnum.WATER;
                    }
                }
            }

            //read fishingzone layer
            TiledMapObjectLayer fishingzoneLayer = (TiledMapObjectLayer)tiledMap.GetLayer("fishingzones");
            foreach (TiledMapObject tiledObject in fishingzoneLayer.Objects)
            {
                string pool = tiledObject.Properties["pool"];
                int difficulty = Int32.Parse(tiledObject.Properties["difficulty"]);
                LootTables.LootTable table = LootTables.EMPTY;
                if (pool.Equals("ocean"))
                {
                    table = LootTables.FISH_OCEAN;
                }

                fishingZones.Add(new FishingZone(difficulty, table, new RectangleF(tiledObject.Position, tiledObject.Size)));
            }

            //read namedzone layer
            nameZones = new List<NamedZone>();
            TiledMapObjectLayer namezoneLayer = (TiledMapObjectLayer)tiledMap.GetLayer("zones");
            foreach (TiledMapObject tiledObject in namezoneLayer.Objects)
            {
                string zoneName = tiledObject.Properties["name"];
                nameZones.Add(new NamedZone(zoneName, new RectangleF(tiledObject.Position, tiledObject.Size)));
            }

            //read soundzones
            soundZones = new List<SoundZone>();
            TiledMapObjectLayer soundzoneLayer = (TiledMapObjectLayer)tiledMap.GetLayer("sounds");
            foreach (TiledMapObject tiledObject in soundzoneLayer.Objects)
            {
                SoundSystem.Sound sound;
                World.TimeOfDay time;
                World.Season season;
                if (!Enum.TryParse(tiledObject.Properties["sound"], out sound))
                {
                    throw new Exception("Couldn't parse " + tiledObject.Properties["sound"] + " to enum...");
                }
                if (!Enum.TryParse(tiledObject.Properties["time"], out time))
                {
                    throw new Exception("Couldn't parse " + tiledObject.Properties["time"] + " to enum...");
                }
                if(!Enum.TryParse(tiledObject.Properties["season"], out season))
                {
                    throw new Exception("Couldn't parse " + tiledObject.Properties["season"] + " to enum...");
                }
                soundZones.Add(new SoundZone(new RectangleF(tiledObject.Position, tiledObject.Size), sound, time, season));
            }

            //read subarea layer
            subareas = new List<Subarea>();
            TiledMapObjectLayer subareaLayer = (TiledMapObjectLayer)tiledMap.GetLayer("subareas");
            foreach (TiledMapObject tiledObject in subareaLayer.Objects)
            {
                string subareaName = tiledObject.Properties["name"];
                subareas.Add(new Subarea(new RectangleF(tiledObject.Position, tiledObject.Size), subareaName));
            }

            //read cutscene trigger layer
            cutsceneTriggerZones = new List<CutsceneTriggerZone>();
            TiledMapObjectLayer cutsceneTriggerLayer = (TiledMapObjectLayer)tiledMap.GetLayer("cutscenes");
            foreach (TiledMapObject tiledObject in cutsceneTriggerLayer.Objects)
            {
                string cutsceneID = tiledObject.Properties["id"];
                cutsceneTriggerZones.Add(new CutsceneTriggerZone(cutsceneID, new RectangleF(tiledObject.Position, tiledObject.Size)));
            }

            //read spawnzone layer
            TiledMapObjectLayer spawnzoneLayer = (TiledMapObjectLayer)tiledMap.GetLayer("spawnzones");
            foreach (TiledMapObject tiledObject in spawnzoneLayer.Objects)
            {
                List<XYTile> validTiles = new List<XYTile>();
                for(int x = (int)tiledObject.Position.X; x <= tiledObject.Size.Width + tiledObject.Position.X; x+=8)
                {
                    for(int y = (int)tiledObject.Position.Y; y <= tiledObject.Size.Height + tiledObject.Position.Y; y+=8)
                    {
                        XYTile toCheck = new XYTile(x / 8, y / 8);
                        if(IsTileEntityPlacementValid(toCheck.tileX, toCheck.tileY, 1, 1))
                        {
                            validTiles.Add(toCheck);
                        }
                    }
                }

                string pool = tiledObject.Properties["pool"];
                if(pool.Equals("farm"))
                {
                    SpawnZone farmZone = new FarmSpawnZone(validTiles, (validTiles.Count() / 5) + 1);
                    spawnZones.Add(farmZone);
                } else if (pool.Equals("beachdry"))
                {
                    SpawnZone beachSpawnZone = new SpawnZone(validTiles, 0.7f,
                        new SpawnZone.Entry(EntityType.BEACH_FORAGE, 1));
                    spawnZones.Add(beachSpawnZone);
                } else if (pool.Equals("beachwet"))
                {
                    SpawnZone beachSpawnZone = new SpawnZone(validTiles, 1.3f,
                        new SpawnZone.Entry(EntityType.BEACH_FORAGE, 1));
                    spawnZones.Add(beachSpawnZone);
                } else if (pool.Equals("grass"))
                {
                    spawnZones.Add(new FloorSpawnZone(validTiles, (validTiles.Count() / 3)+1, 
                        new SpawnZone.Entry(EntityType.GRASS, 1)));
                }
            }

            //create foreground/background
            GenerateForeground(content, cameraBoundingBox);
            GenerateBackground(content, cameraBoundingBox);


            //debug output of collision layout
            /*for (int y = 0; y < baseLayer.Height; y++)
            {
                for(int x = 0; x < baseLayer.Width; x++)
                {
                    if(collisionMap[x,y] == CollisionTypeEnum.BRIDGE)
                    {
                        Console.Write("B");
                    }
                    else if(collisionMap[x,y] == CollisionTypeEnum.SOLID)
                    {
                        Console.Write("X");
                    }
                    else if (collisionMap[x, y] == CollisionTypeEnum.WATER)
                    {
                        Console.Write("W");
                    }
                    else
                    {
                        Console.Write(" ");
                    } 
                }
                Console.Write("\n");
            }*/
            /*Console.WriteLine("\n\n\n" + GetAreaName());
            for(int y = 0; y < baseLayer.Height; y++)
            {
                for(int x = 0; x < baseLayer.Width; x++)
                {
                    if(wallMap[x, y] == true)
                    {
                        Console.Write("W");
                    } else
                    {
                        Console.Write(".");
                    }
                }
                Console.Write("\n");
            }*/
        }

        private static int FOREGROUND_PX_PER_CLOUD = 80000;
        private static int FOREGROUND_PX_PER_RAIN = 1400;
        private static int FOREGROUND_PX_PER_SNOW = 1600;
        private static int FOREGROUND_PX_PER_PARTICLE = 3500;
        private static int BACKGROUND_PX_PER_CLOUD = 60000;
        private static int BACKGROUND_PX_PER_RAIN = 1200;
        private static int BACKGROUND_PX_PER_SNOW = 1400;
        private static int BACKGROUND_PX_PER_PARTICLE = 2500;

        private void GenerateForeground(ContentManager Content, RectangleF cameraBoundingBox)
        {
            RectangleF inflatedBounds = LayeredBackground.CalculateInflatedBoundsRect(cameraBoundingBox);
            int area = (int)Util.CalculateArea(inflatedBounds);
            
            foreground = new LayeredBackground();
            LayeredBackground.Layer foregroundCloudLayer = new LayeredBackground.Layer(FOREGROUND_LAYER_CLOUDS, Util.CLOUD_FRONT_DAY.color * Util.FOREGROUND_TRANSPARENCY, 0.4f);
            for (int i = 0; i < area/FOREGROUND_PX_PER_CLOUD; i++)
            {
                foregroundCloudLayer.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_LG1), inflatedBounds, new Vector2(Util.RandInt(30, 40), 0)));
                foregroundCloudLayer.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_LG2), inflatedBounds, new Vector2(Util.RandInt(30, 40), 0)));
                foregroundCloudLayer.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_LG3), inflatedBounds, new Vector2(Util.RandInt(30, 40), 0)));
                foregroundCloudLayer.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_LG4), inflatedBounds, new Vector2(Util.RandInt(30, 40), 0)));
                foregroundCloudLayer.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_LG5), inflatedBounds, new Vector2(Util.RandInt(30, 40), 0)));
                foregroundCloudLayer.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_LG6), inflatedBounds, new Vector2(Util.RandInt(30, 40), 0)));
            }

            LayeredBackground.Layer foregroundRainLayer = new LayeredBackground.Layer(FOREGROUND_LAYER_RAIN, Color.White * Util.FOREGROUND_TRANSPARENCY, 0.3f);
            for (int i = 0; i < area/FOREGROUND_PX_PER_RAIN; i++)
            {
                foregroundRainLayer.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.FOREGROUND_RAIN), inflatedBounds, new Vector2(Util.RandInt(-10, 10), 600)));
            }

            LayeredBackground.Layer foregroundSnowLayer = new LayeredBackground.Layer(FOREGROUND_LAYER_SNOW, Color.White * Util.FOREGROUND_TRANSPARENCY, 0.3f);
            for (int i = 0; i < area/FOREGROUND_PX_PER_SNOW; i++)
            {
                foregroundSnowLayer.AddElement(new LayeredBackground.RotatingElement(Content.Load<Texture2D>(Paths.FOREGROUND_SNOW), inflatedBounds, new Vector2(Util.RandInt(-23, 23), 45),
                    Util.RandInt(-3, 3) / 100.0f));
            }

            LayeredBackground.Layer foregroundWeatherFilter = new LayeredBackground.Layer(FOREGROUND_LAYER_WEATHER_FILTER, Color.White, 0);
            foregroundWeatherFilter.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.FOREGROUND_320x200), new Vector2(0, 0), new Vector2(0, 0)));
            LayeredBackground.Layer foregroundSeasonFilter = new LayeredBackground.Layer(FOREGROUND_LAYER_SEASON_FILTER, Color.White, 0);
            foregroundSeasonFilter.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.FOREGROUND_320x200), new Vector2(0, 0), new Vector2(0, 0)));

            LayeredBackground.Layer foregroundParticleLayer = new LayeredBackground.Layer(FOREGROUND_LAYER_PARTICLES, Color.White * Util.FOREGROUND_TRANSPARENCY, 0.3f);
            for (int i = 0; i < area/FOREGROUND_PX_PER_PARTICLE; i++)
            {
                foregroundParticleLayer.AddElement(new LayeredBackground.RotatingElement(Content.Load<Texture2D>(Paths.SPRITE_PARTICLE_2x2),
                    inflatedBounds,
                    new Vector2(Util.RandInt(100, 140), Util.RandInt(-25, 25)), Util.RandInt(-8, 8) / 100.0f));
            }

            foreground.AddLayer(foregroundParticleLayer);
            foreground.AddLayer(foregroundSnowLayer);
            foreground.AddLayer(foregroundRainLayer);
            foreground.AddLayer(foregroundCloudLayer);
            foreground.AddLayer(foregroundWeatherFilter);
            foreground.AddLayer(foregroundSeasonFilter);
        }

        private void GenerateBackground(ContentManager Content, RectangleF cameraBoundingBox)
        {
            RectangleF inflatedBounds = LayeredBackground.CalculateInflatedBoundsRect(cameraBoundingBox);
            int area = (int)Util.CalculateArea(inflatedBounds);

            background = new LayeredBackground();

            LayeredBackground.Layer morningSkyLayer = new LayeredBackground.Layer(BACKGROUND_LAYER_SKY_MORNING, Color.White, 1.0f);
            morningSkyLayer.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_SKY_MORNING), new Vector2(0, 0), new Vector2(0, 0)));

            LayeredBackground.Layer daySkyLayer = new LayeredBackground.Layer(BACKGROUND_LAYER_SKY_DAY, Color.White, 1.0f);
            daySkyLayer.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_SKY_DAY), new Vector2(0, 0), new Vector2(0, 0)));

            LayeredBackground.Layer eveningSkyLayer = new LayeredBackground.Layer(BACKGROUND_LAYER_SKY_EVENING, Color.White, 1.0f);
            eveningSkyLayer.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_SKY_EVENING), new Vector2(0, 0), new Vector2(0, 0)));

            LayeredBackground.Layer nightSkyLayer = new LayeredBackground.Layer(BACKGROUND_LAYER_SKY_NIGHT, Color.White, 1.0f);
            nightSkyLayer.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_SKY_NIGHT), new Vector2(0, 0), new Vector2(0, 0)));

            LayeredBackground.Layer sunLayer = new LayeredBackground.Layer(BACKGROUND_LAYER_SUN, Color.White, 1.0f);
            sunLayer.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_SUN), new Vector2(30.0f/320.0f, 8.0f/200.0f), new Vector2(0, 0)));
            LayeredBackground.Layer moonLayer = new LayeredBackground.Layer(BACKGROUND_LAYER_MOON, Color.White, 1.0f);
            moonLayer.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_MOON), new Vector2(30.0f/320.0f, 8.0f/200.0f), new Vector2(0, 0)));
            //starLayer

            LayeredBackground.Layer cloudLayerBack = new LayeredBackground.Layer(BACKGROUND_LAYER_CLOUDS_BACK, Util.CLOUD_BACK_DAY.color, -0.6f);
            LayeredBackground.Layer cloudLayerMiddle = new LayeredBackground.Layer(BACKGROUND_LAYER_CLOUDS_MIDDLE, Util.CLOUD_MIDDLE_DAY.color, -0.3f);
            LayeredBackground.Layer cloudLayerFront = new LayeredBackground.Layer(BACKGROUND_LAYER_CLOUDS_FRONT, Util.CLOUD_FRONT_DAY.color, -0.1f);

            for (int i = 0; i < area/BACKGROUND_PX_PER_CLOUD; i++)
            {
                cloudLayerFront.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_LG1), inflatedBounds, new Vector2(Util.RandInt(30, 40), 0)));
                cloudLayerFront.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_LG2), inflatedBounds, new Vector2(Util.RandInt(30, 40), 0)));
                cloudLayerFront.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_LG3), inflatedBounds, new Vector2(Util.RandInt(30, 40), 0)));
                cloudLayerFront.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_LG4), inflatedBounds, new Vector2(Util.RandInt(30, 40), 0)));
                cloudLayerFront.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_LG5), inflatedBounds, new Vector2(Util.RandInt(30, 40), 0)));
                cloudLayerFront.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_LG6), inflatedBounds, new Vector2(Util.RandInt(30, 40), 0)));
            }
            for (int i = 0; i < area/BACKGROUND_PX_PER_CLOUD; i++)
            {
                cloudLayerMiddle.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_MD1), inflatedBounds, new Vector2(Util.RandInt(10, 20), 0)));
                cloudLayerMiddle.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_MD2), inflatedBounds, new Vector2(Util.RandInt(10, 20), 0)));
                cloudLayerMiddle.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_MD3), inflatedBounds, new Vector2(Util.RandInt(10, 20), 0)));
                cloudLayerMiddle.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_MD4), inflatedBounds, new Vector2(Util.RandInt(10, 20), 0)));
                cloudLayerMiddle.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_MD5), inflatedBounds, new Vector2(Util.RandInt(10, 20), 0)));
                cloudLayerMiddle.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_MD6), inflatedBounds, new Vector2(Util.RandInt(10, 20), 0)));
                cloudLayerMiddle.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_MD7), inflatedBounds, new Vector2(Util.RandInt(10, 20), 0)));
                cloudLayerMiddle.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_MD8), inflatedBounds, new Vector2(Util.RandInt(10, 20), 0)));
            }
            for (int i = 0; i < area/BACKGROUND_PX_PER_CLOUD; i++)
            {
                cloudLayerBack.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_SM1), inflatedBounds, new Vector2(Util.RandInt(3, 8), 0)));
                cloudLayerBack.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_SM2), inflatedBounds, new Vector2(Util.RandInt(3, 8), 0)));
                cloudLayerBack.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_SM3), inflatedBounds, new Vector2(Util.RandInt(3, 8), 0)));
                cloudLayerBack.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_SM4), inflatedBounds, new Vector2(Util.RandInt(3, 8), 0)));
                cloudLayerBack.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_SM5), inflatedBounds, new Vector2(Util.RandInt(3, 8), 0)));
                cloudLayerBack.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_CLOUD_SM6), inflatedBounds, new Vector2(Util.RandInt(3, 8), 0)));
            }

            LayeredBackground.Layer rainLayerFront = new LayeredBackground.Layer(BACKGROUND_LAYER_RAIN_FRONT, Color.White, -0.5f);
            for (int i = 0; i < area/BACKGROUND_PX_PER_RAIN; i++)
            {
                rainLayerFront.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_RAIN_FRONT), inflatedBounds, new Vector2(Util.RandInt(-10, 10), 450)));
            }

            LayeredBackground.Layer rainLayerBack = new LayeredBackground.Layer(BACKGROUND_LAYER_RAIN_BACK, Color.White, -0.7f);
            for (int i = 0; i < area/BACKGROUND_PX_PER_RAIN; i++)
            {
                rainLayerBack.AddElement(new LayeredBackground.Element(Content.Load<Texture2D>(Paths.BACKGROUND_RAIN_BACK), inflatedBounds, new Vector2(Util.RandInt(-10, 10), 350)));
            }

            LayeredBackground.Layer snowLayerFront = new LayeredBackground.Layer(BACKGROUND_LAYER_SNOW_FRONT, Color.White, -0.5f);
            for (int i = 0; i < area/BACKGROUND_PX_PER_SNOW; i++)
            {
                snowLayerFront.AddElement(new LayeredBackground.RotatingElement(Content.Load<Texture2D>(Paths.BACKGROUND_SNOW_FRONT), inflatedBounds, new Vector2(Util.RandInt(-15, 15), 35),
                    Util.RandInt(-6, 6) / 100.0f));
            }

            LayeredBackground.Layer snowLayerBack = new LayeredBackground.Layer(BACKGROUND_LAYER_SNOW_BACK, Color.White, -0.5f);
            for (int i = 0; i < area/BACKGROUND_PX_PER_SNOW; i++)
            {
                snowLayerBack.AddElement(new LayeredBackground.RotatingElement(Content.Load<Texture2D>(Paths.BACKGROUND_SNOW_BACK), inflatedBounds, new Vector2(Util.RandInt(-7, 7), 25),
                    Util.RandInt(-4, 4) / 100.0f));
            }

            LayeredBackground.Layer backgroundParticleLayer = new LayeredBackground.Layer(BACKGROUND_LAYER_PARTICLES, Color.White, -0.5f);
            for (int i = 0; i < area/BACKGROUND_PX_PER_PARTICLE; i++)
            {
                backgroundParticleLayer.AddElement(new LayeredBackground.RotatingElement(Content.Load<Texture2D>(Paths.SPRITE_PARTICLE_1x2),
                    inflatedBounds,
                    new Vector2(Util.RandInt(60, 90), Util.RandInt(-10, 10)), Util.RandInt(-8, 8) / 100.0f));
            }


            background.AddLayer(morningSkyLayer);
            background.AddLayer(daySkyLayer);
            background.AddLayer(eveningSkyLayer);
            background.AddLayer(nightSkyLayer);
            background.AddLayer(sunLayer);
            background.AddLayer(moonLayer);
            background.AddLayer(cloudLayerBack);
            background.AddLayer(rainLayerBack);
            background.AddLayer(snowLayerBack);
            background.AddLayer(cloudLayerMiddle);
            background.AddLayer(backgroundParticleLayer);
            background.AddLayer(cloudLayerFront);
            background.AddLayer(rainLayerFront);
            background.AddLayer(snowLayerFront);
        }

        public string GetName()
        {
            return this.name;
        }

        public Subarea.NameEnum GetSubareaAt(RectangleF rect)
        {
            foreach(Subarea sa in subareas)
            {
                if(sa.rect.Intersects(rect))
                {
                    return sa.subareaName;
                }
            }
            throw new Exception("No subarea found at " + rect);
        }

        public string GetZoneName(Vector2 playerPosition)
        {
            foreach(NamedZone nz in nameZones)
            {
                if(nz.rectangle.Contains(playerPosition))
                {
                    return nz.name;
                }
            }
            return "";
        }

        public List<CutsceneManager.Cutscene> GetPossibleCutscenes(Vector2 playerPosition)
        {
            List<CutsceneManager.Cutscene> possible = new List<CutsceneManager.Cutscene>();
            foreach(CutsceneTriggerZone ctZone in cutsceneTriggerZones)
            {
                if(ctZone.rectangle.Contains(playerPosition)) {
                    possible.Add(CutsceneManager.GetCutsceneById(ctZone.cutsceneID));
                }
            }
            return possible;
        }

        public List<PathingHelper> GetPathingHelpers(RectangleF hitbox)
        {
            List<PathingHelper> collidingHelpers = new List<PathingHelper>();
            foreach(PathingHelper ph in pathingHelpers)
            {
                if(ph.rect.Intersects(hitbox))
                {
                    collidingHelpers.Add(ph);
                }
            }
            return collidingHelpers;
        }

        public List<SoundZone> GetSoundZonesAtPosAndTimeAndSeason(Vector2 pos, World.TimeOfDay time, World.Season season)
        {
            List<SoundZone> validSoundZones = new List<SoundZone>();
            foreach(SoundZone sz in soundZones)
            {
                if(sz.rect.Contains(pos) && (sz.time == World.TimeOfDay.ALL || sz.time == time) && (sz.season == World.Season.NONE || sz.season == season))
                {
                    validSoundZones.Add(sz);
                }
            }
            return validSoundZones;
        }

        public bool IsCollideWithPathingHelperType(RectangleF hitbox, PathingHelper.Type type)
        {
            foreach (PathingHelper ph in pathingHelpers)
            {
                if (ph.rect.Intersects(hitbox) && ph.type == type)
                {
                    return true;
                }
            }
            return false;
        }

        public AreaEnum GetAreaEnum()
        {
            return this.areaEnum;
        }

        public bool DoesCameraMove()
        {
            return cameraMoves;
        }

        public World.Season GetWorldSeason()
        {
            return worldSeason;
        }

        public World.Season GetSeason()
        {
            if(areaSeason == World.Season.DEFER)
            {
                return worldSeason;
            }
            return areaSeason;
        }

        public CollisionTypeEnum GetCollisionTypeAt(int x, int y)
        {
            if (x < 0 || x >= collisionMap.GetLength(0) || y < 0 || y >= collisionMap.GetLength(1))
            {
                return CollisionTypeEnum.BOUNDARY;
            }

            if (collisionMap[x, y] == CollisionTypeEnum.AIR && buildingBlockGrid[x, y] != null)
            {
                if (buildingBlockGrid[x, y].GetBlockType() == BlockType.PLATFORM || buildingBlockGrid[x, y].GetBlockType() == BlockType.PLATFORM_FARM)
                {
                    return CollisionTypeEnum.SCAFFOLDING_BRIDGE;
                } else if (buildingBlockGrid[x, y].GetBlockType() == BlockType.BLOCK)
                {
                    return CollisionTypeEnum.SCAFFOLDING_BLOCK;
                } else
                {
                    return CollisionTypeEnum.SCAFFOLDING;
                }
            }

            return collisionMap[x, y];
        }

        public FishingZone GetFishingZoneAt(int x, int y)
        {
            foreach(FishingZone fz in fishingZones)
            {
                if(fz.rectangle.Contains(new Point2(x, y))) {
                    return fz;
                }
            }
            throw new Exception("Water not covered by fishingzone?");
            return null;
        }

        public Vector2 GetPositionOfTile(int x, int y)
        {
            return new Vector2(x * tileWidth, y * tileHeight);
        }

        public int MapPixelWidth()
        {
            return tiledMap.WidthInPixels;
        }

        public int MapPixelHeight()
        {
            return tiledMap.HeightInPixels;
        }

        private void CheckItemCollision(EntityPlayer player)
        {
            for (int i = 0; i < itemEntities.Count; i++)
            {
                EntityItem ei = itemEntities[i];
                if (ei.CanBeCollected() && ei.GetCollisionRectangle().Contains(player.GetAdjustedPosition()))
                {
                    if (player.AddItemToInventory(ei.GetItemForm()))
                    {
                        itemEntities.Remove(ei);
                    }
                }
            }
        }

        private void CheckTileCollision(EntityPlayer player)
        {
            RectangleF baseHitbox = player.GetCollisionRectangle();
            baseHitbox.X += 2;
            baseHitbox.Width -= 4;
            Rectangle tileHitbox = new Rectangle((int)(baseHitbox.Left / 8), (int)(baseHitbox.Top / 8), (int)(baseHitbox.Width / 8), (int)(baseHitbox.Height / 8));
            for (int x = tileHitbox.Left; x <= tileHitbox.Left + tileHitbox.Width; x++)
            {
                for (int y = tileHitbox.Top; y <= tileHitbox.Top + tileHitbox.Height; y++)
                {
                    TileEntity en = GetTileEntity(x, y);
                    if (en != null && en is IInteractContact)
                    {
                        ((IInteractContact)en).OnContact(player, this);
                    }
                }
            }
        }

        public void CheckEntityCollisions(EntityPlayer player)
        {
            RectangleF playerHitbox = player.GetCollisionRectangle();
            CheckItemCollision(player);
            CheckTileCollision(player);
            foreach (Entity en in entityListManager.GetContactInteractableEntityList())
            {
                if (en is IInteractContact && playerHitbox.Intersects(en.GetCollisionRectangle()))
                {
                    ((IInteractContact)en).OnContact(player, this);
                }
            }
        }

        public List<EntityCollidable> GetCollideableEntities()
        {
            return entityListManager.GetCollideableEntityList();
        }

        public void DrawBackground(SpriteBatch sb, RectangleF cameraBoundingBox, float layerDepth)
        {
            background.Draw(sb, cameraBoundingBox, layerDepth);
        }

        public World.Weather GetWeather()
        {
            return areaWeather;
        }

        public bool IsInside()
        {
            return interior;
        }

        public List<MEntitySolid> GetSolidEntities()
        {
            return entityListManager.GetSolidEntityList();
        }

        public void Update(float deltaTime, GameTime gameTime, World.TimeData timeData, World.Weather worldWeather, RectangleF cameraBoundingBox)
        {
            this.areaWeather = worldWeather; //update this eventually?
            this.worldSeason = timeData.season;
            if (areaWeather == World.Weather.SNOWY)
            {
                if (areaSeason != World.Season.DEFER && areaSeason != World.Season.WINTER)
                {
                    areaWeather = World.Weather.RAINY;
                }
            }
            
            mapRenderer.Update(tiledMap, gameTime); //update map renderer
            background.Update(deltaTime, cameraBoundingBox);
            foreground.Update(deltaTime, cameraBoundingBox);

            background.TryDisableLayer(BACKGROUND_LAYER_SKY_MORNING);
            background.TrySetTransparency(BACKGROUND_LAYER_SKY_MORNING, 1.0f);
            background.TryDisableLayer(BACKGROUND_LAYER_SKY_DAY);
            background.TrySetTransparency(BACKGROUND_LAYER_SKY_DAY, 1.0f);
            background.TryDisableLayer(BACKGROUND_LAYER_SKY_EVENING);
            background.TrySetTransparency(BACKGROUND_LAYER_SKY_EVENING, 1.0f);
            background.TryDisableLayer(BACKGROUND_LAYER_SKY_NIGHT);
            background.TrySetTransparency(BACKGROUND_LAYER_SKY_NIGHT, 1.0f);

            foreground.TryDisableLayer(FOREGROUND_LAYER_WEATHER_FILTER);
            foreground.TryDisableLayer(FOREGROUND_LAYER_RAIN);
            background.TryDisableLayer(BACKGROUND_LAYER_RAIN_BACK);
            background.TryDisableLayer(BACKGROUND_LAYER_RAIN_FRONT);
            foreground.TryDisableLayer(FOREGROUND_LAYER_SNOW);
            background.TryDisableLayer(BACKGROUND_LAYER_SNOW_BACK);
            background.TryDisableLayer(BACKGROUND_LAYER_SNOW_FRONT);

            foreground.TryEnableLayer(FOREGROUND_LAYER_CLOUDS);
            background.TryEnableLayer(BACKGROUND_LAYER_CLOUDS_MIDDLE);

            foreground.TryEnableLayer(FOREGROUND_LAYER_PARTICLES);
            background.TryEnableLayer(BACKGROUND_LAYER_PARTICLES);

            background.TryEnableLayer(BACKGROUND_LAYER_SUN);

            //update background/foreground according to time...
            World.TimeOfDay mainTime = timeData.timeOfDay;
            World.TimeOfDay blendTime = World.NextTimeOfDay(mainTime);
            int minsTillTransition = World.MinutesUntilTransition(timeData.hour, timeData.minute);

            switch (timeData.timeOfDay)
            {
                case World.TimeOfDay.MORNING:
                    background.TryEnableLayer(BACKGROUND_LAYER_SKY_MORNING);
                    background.TrySetTransparency(BACKGROUND_LAYER_SUN, 1.0f);
                    background.TrySetTransparency(BACKGROUND_LAYER_MOON, 0.0f);
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_FRONT, Util.CLOUD_FRONT_DAY.color);
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_MIDDLE, Util.CLOUD_MIDDLE_DAY.color);
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_BACK, Util.CLOUD_BACK_DAY.color);
                    foreground.TrySetTint(FOREGROUND_LAYER_CLOUDS, Util.CLOUD_FRONT_DAY.color * Util.FOREGROUND_TRANSPARENCY_CLOUDS);
                    if (minsTillTransition <= BLEND_MINUTES)
                    {
                        background.TryEnableLayer(BACKGROUND_LAYER_SKY_DAY);
                        background.TrySetTransparency(BACKGROUND_LAYER_SKY_DAY, 1.0f - (minsTillTransition / BLEND_MINUTES));
                        
                    }
                    break;
                case World.TimeOfDay.DAY:
                    background.TryEnableLayer(BACKGROUND_LAYER_SKY_DAY);
                    background.TrySetTransparency(BACKGROUND_LAYER_SUN, 1.0f);
                    background.TrySetTransparency(BACKGROUND_LAYER_MOON, 0.0f);
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_FRONT, Util.CLOUD_FRONT_DAY.color);
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_MIDDLE, Util.CLOUD_MIDDLE_DAY.color);
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_BACK, Util.CLOUD_BACK_DAY.color);
                    foreground.TrySetTint(FOREGROUND_LAYER_CLOUDS, Util.CLOUD_FRONT_DAY.color * Util.FOREGROUND_TRANSPARENCY_CLOUDS);
                    if (minsTillTransition <= BLEND_MINUTES)
                    {
                        background.TryEnableLayer(BACKGROUND_LAYER_SKY_EVENING);
                        background.TrySetTransparency(BACKGROUND_LAYER_SKY_EVENING, 1.0f - (minsTillTransition / BLEND_MINUTES));
                        background.TrySetTint(BACKGROUND_LAYER_CLOUDS_FRONT, Util.BlendColors(Util.CLOUD_FRONT_DAY.color, Util.CLOUD_FRONT_EVENING.color, 1.0f - (minsTillTransition / BLEND_MINUTES)));
                        background.TrySetTint(BACKGROUND_LAYER_CLOUDS_MIDDLE, Util.BlendColors(Util.CLOUD_MIDDLE_DAY.color, Util.CLOUD_MIDDLE_EVENING.color, 1.0f - (minsTillTransition / BLEND_MINUTES)));
                        background.TrySetTint(BACKGROUND_LAYER_CLOUDS_BACK, Util.BlendColors(Util.CLOUD_BACK_DAY.color, Util.CLOUD_BACK_EVENING.color, 1.0f - (minsTillTransition / BLEND_MINUTES)));
                        foreground.TrySetTint(FOREGROUND_LAYER_CLOUDS, Util.BlendColors(Util.CLOUD_FRONT_DAY.color, Util.CLOUD_FRONT_EVENING.color, 1.0f - (minsTillTransition / BLEND_MINUTES)) * Util.FOREGROUND_TRANSPARENCY_CLOUDS);
                    }
                    break;
                case World.TimeOfDay.EVENING:
                    background.TryEnableLayer(BACKGROUND_LAYER_SKY_EVENING);
                    background.TrySetTransparency(BACKGROUND_LAYER_SUN, 1.0f);
                    background.TrySetTransparency(BACKGROUND_LAYER_MOON, 0.0f);
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_FRONT, Util.CLOUD_FRONT_EVENING.color);
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_MIDDLE, Util.CLOUD_MIDDLE_EVENING.color);
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_BACK, Util.CLOUD_BACK_EVENING.color);
                    foreground.TrySetTint(FOREGROUND_LAYER_CLOUDS, Util.CLOUD_FRONT_EVENING.color * Util.FOREGROUND_TRANSPARENCY_CLOUDS);
                    if (minsTillTransition <= BLEND_MINUTES)
                    {
                        background.TryEnableLayer(BACKGROUND_LAYER_SKY_NIGHT);
                        background.TrySetTransparency(BACKGROUND_LAYER_SKY_NIGHT, 1.0f - (minsTillTransition / BLEND_MINUTES));
                        background.TrySetTint(BACKGROUND_LAYER_CLOUDS_FRONT, Util.BlendColors(Util.CLOUD_FRONT_EVENING.color, Util.CLOUD_FRONT_NIGHT.color, 1.0f - (minsTillTransition / BLEND_MINUTES)));
                        background.TrySetTint(BACKGROUND_LAYER_CLOUDS_MIDDLE, Util.BlendColors(Util.CLOUD_MIDDLE_EVENING.color, Util.CLOUD_MIDDLE_NIGHT.color, 1.0f - (minsTillTransition / BLEND_MINUTES)));
                        background.TrySetTint(BACKGROUND_LAYER_CLOUDS_BACK, Util.BlendColors(Util.CLOUD_BACK_EVENING.color, Util.CLOUD_BACK_NIGHT.color, 1.0f - (minsTillTransition / BLEND_MINUTES)));
                        foreground.TrySetTint(FOREGROUND_LAYER_CLOUDS, Util.BlendColors(Util.CLOUD_FRONT_EVENING.color, Util.CLOUD_FRONT_NIGHT.color, 1.0f - (minsTillTransition / BLEND_MINUTES)) * Util.FOREGROUND_TRANSPARENCY_CLOUDS);
                        background.TrySetTransparency(BACKGROUND_LAYER_SUN, minsTillTransition / BLEND_MINUTES);
                    }
                    break;
                case World.TimeOfDay.NIGHT:
                    background.TryEnableLayer(BACKGROUND_LAYER_SKY_NIGHT);
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_FRONT, Util.CLOUD_FRONT_NIGHT.color);
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_MIDDLE, Util.CLOUD_MIDDLE_NIGHT.color);
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_BACK, Util.CLOUD_BACK_NIGHT.color);
                    foreground.TrySetTint(FOREGROUND_LAYER_CLOUDS, Util.CLOUD_FRONT_NIGHT.color * Util.FOREGROUND_TRANSPARENCY_CLOUDS);
                    background.TrySetTransparency(BACKGROUND_LAYER_SUN, 0.0f);
                    background.TrySetTransparency(BACKGROUND_LAYER_MOON, ((timeData.hour*60 + timeData.minute) - World.EVENING_END_HOUR*60)/60.0f);
                    break;
            }

            switch (areaWeather)
            {
                case World.Weather.CLOUDY:
                    foreground.TryEnableLayer(FOREGROUND_LAYER_WEATHER_FILTER);
                    foreground.TrySetTint(FOREGROUND_LAYER_WEATHER_FILTER, Util.CLOUDY_FILTER.color);
                    break;
                case World.Weather.RAINY:
                    //disable particles
                    foreground.TryDisableLayer(FOREGROUND_LAYER_PARTICLES);
                    background.TryDisableLayer(BACKGROUND_LAYER_PARTICLES);
                    //rain fx
                    foreground.TryEnableLayer(FOREGROUND_LAYER_RAIN);
                    background.TryEnableLayer(BACKGROUND_LAYER_RAIN_BACK);
                    background.TryEnableLayer(BACKGROUND_LAYER_RAIN_FRONT);
                    foreground.TryEnableLayer(FOREGROUND_LAYER_WEATHER_FILTER);
                    foreground.TrySetTint(FOREGROUND_LAYER_WEATHER_FILTER, Util.RAIN_FILTER.color);
                    //color clouds gray
                    foreground.TrySetTint(FOREGROUND_LAYER_CLOUDS, Util.BlendColors(foreground.TryGetTint(FOREGROUND_LAYER_CLOUDS), Util.CLOUD_RAIN.color, 0.5f) * Util.FOREGROUND_TRANSPARENCY_CLOUDS);
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_BACK, Util.BlendColors(background.TryGetTint(BACKGROUND_LAYER_CLOUDS_BACK), Util.CLOUD_RAIN.color, 0.5f));
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_MIDDLE, Util.BlendColors(background.TryGetTint(BACKGROUND_LAYER_CLOUDS_MIDDLE), Util.CLOUD_RAIN.color, 0.5f));
                    background.TrySetTint(BACKGROUND_LAYER_CLOUDS_FRONT, Util.BlendColors(background.TryGetTint(BACKGROUND_LAYER_CLOUDS_FRONT), Util.CLOUD_RAIN.color, 0.5f));
                    //disable sun
                    background.TryDisableLayer(BACKGROUND_LAYER_SUN);
                    break;
                case World.Weather.SNOWY:
                    foreground.TryDisableLayer(FOREGROUND_LAYER_PARTICLES);
                    background.TryDisableLayer(BACKGROUND_LAYER_PARTICLES);
                    foreground.TryEnableLayer(FOREGROUND_LAYER_SNOW);
                    background.TryEnableLayer(BACKGROUND_LAYER_SNOW_BACK);
                    background.TryEnableLayer(BACKGROUND_LAYER_SNOW_FRONT);
                    foreground.TryEnableLayer(FOREGROUND_LAYER_WEATHER_FILTER);
                    foreground.TrySetTint(FOREGROUND_LAYER_WEATHER_FILTER, Util.SNOWY_FILTER.color);
                    //disable sun
                    background.TryDisableLayer(BACKGROUND_LAYER_SUN);
                    break;
                case World.Weather.SUNNY:
                    foreground.TryDisableLayer(FOREGROUND_LAYER_CLOUDS);
                    background.TryDisableLayer(BACKGROUND_LAYER_CLOUDS_MIDDLE);
                    break;
            }

            switch (this.GetSeason())
            {
                case World.Season.SPRING:
                    foreground.TrySetTint(FOREGROUND_LAYER_SEASON_FILTER, Util.SPRING_FILTER.color);
                    foreground.TrySetTint(FOREGROUND_LAYER_PARTICLES, Util.PARTICLE_SPRING_PETAL_FOREGROUND_TRANSPARENT.color);
                    background.TrySetTint(BACKGROUND_LAYER_PARTICLES, Util.PARTICLE_SPRING_PETAL_BACKGROUND.color);
                    break;
                case World.Season.SUMMER:
                    foreground.TrySetTint(FOREGROUND_LAYER_SEASON_FILTER, Util.SUMMER_FILTER.color);
                    foreground.TrySetTint(FOREGROUND_LAYER_PARTICLES, Util.PARTICLE_SUMMER_LEAF_FOREGROUND_TRANSPARENT.color);
                    background.TrySetTint(BACKGROUND_LAYER_PARTICLES, Util.PARTICLE_SUMMER_LEAF_BACKGROUND.color);
                    break;
                case World.Season.AUTUMN:
                    foreground.TrySetTint(FOREGROUND_LAYER_SEASON_FILTER, Util.FALL_FILTER.color);
                    foreground.TrySetTint(FOREGROUND_LAYER_PARTICLES, Util.PARTICLE_FALL_LEAF_FOREGROUND_TRANSPARENT.color);
                    background.TrySetTint(BACKGROUND_LAYER_PARTICLES, Util.PARTICLE_FALL_LEAF_BACKGROUND.color);
                    break;
                case World.Season.WINTER:
                    foreground.TrySetTint(FOREGROUND_LAYER_SEASON_FILTER, Util.WINTER_FILTER.color);
                    foreground.TrySetTint(FOREGROUND_LAYER_PARTICLES, Util.PARTICLE_WINTER_SNOW_FOREGROUND_TRANSPARENT.color);
                    background.TrySetTint(BACKGROUND_LAYER_PARTICLES, Util.PARTICLE_WINTER_SNOW_BACKGROUND.color);
                    break;
            }

            if(interior)
            {
                foreground.TryDisableLayer(FOREGROUND_LAYER_WEATHER_FILTER);
                foreground.TryDisableLayer(FOREGROUND_LAYER_RAIN);
                foreground.TryDisableLayer(FOREGROUND_LAYER_SNOW);
                foreground.TryDisableLayer(FOREGROUND_LAYER_CLOUDS);
                foreground.TryDisableLayer(FOREGROUND_LAYER_PARTICLES);
            }

            for (int i = 0; i < entityListManager.GetEntityList().Count; i++)
            {
                entityListManager.GetEntityList()[i].Update(deltaTime, this);
            }

            foreach (EntityItem ei in itemEntities)
            {
                ei.Update(deltaTime, this);
            }

            List<Particle> removeList = new List<Particle>();
            foreach (Particle pa in particleList)
            {
                pa.Update(deltaTime, this);

                if (pa.IsDisposable())
                {
                    removeList.Add(pa);
                }
            }
            foreach (Particle paR in removeList)
            {
                particleList.Remove(paR);
            }


        }

        public Entity GetInteractableEntityAt(Vector2 atPoint)
        {
            foreach (Entity en in entityListManager.GetInteractableEntityList())
            {
                if (!(en is TileEntity) && en.GetCollisionRectangle().Contains(atPoint)) {
                    return en;
                }
            }
            foreach(Entity en in entityListManager.GetToolInteractableEntityList())
            {
                if (!(en is TileEntity) && en.GetCollisionRectangle().Contains(atPoint))
                {
                    return en;
                }
            }
            return null;
        }

        public void DrawTerrain(SpriteBatch sb, Matrix viewMatrix, float layerDepth)
        {
            //var projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Plateau.GRAPHICS.GraphicsDevice.Viewport.Width,
            //    Plateau.GRAPHICS.GraphicsDevice.Viewport.Height, 0, 0f, -1f);
            foreach (TiledMapLayer layer in tiledMap.Layers)
            {
                if (!layer.Name.Equals("water") && !layer.Name.Equals("walls") && !layer.Name.Equals("buildings") && !layer.Name.Equals("decoration"))
                {
                    mapRenderer.Draw(layer, viewMatrix, null, null, layerDepth);
                }
            }
            //mapRenderer.Draw(tiledMap, viewMatrix); //draw the map
        }

        public void DrawParticles(SpriteBatch sb, float layerDepth)
        {
            foreach (Particle pa in particleList)
            {
                pa.Draw(sb, layerDepth);
            }
        }

        public void DrawWater(SpriteBatch sb, Matrix viewMatrix, float layerDepth)
        {
            mapRenderer.Draw(tiledMap.GetLayer("water"), viewMatrix, null, null, layerDepth);
        }

        public List<LightSource> GetAreaLights()
        {
            return lights;
        }

        public void DrawBuildingBlocks(SpriteBatch sb, float layerDepth)
        {
            foreach(BuildingBlock block in buildingBlockList)
            {
                block.Draw(sb, layerDepth);
            }
        }

        public bool IsWallEntityPlacementValid(int tileX, int tileY, int tileWidth, int tileHeight)
        {
            for (int x = 0; x < tileWidth; x++)
            {
                for (int y = 0; y < tileHeight; y++)
                {
                    if(wallEntityGrid[tileX + x, tileY + y] != null)
                    {
                        return false;
                    }
                    if (wallMap[tileX + x, tileY+y] == false)
                    {
                        if(buildingBlockGrid[tileX + x, tileY + y] == null || 
                            (buildingBlockGrid[tileX + x, tileY + y].GetBlockType() != BlockType.SCAFFOLDING &&
                            buildingBlockGrid[tileX + x, tileY + y].GetBlockType() != BlockType.PLATFORM &&
                            buildingBlockGrid[tileX + x, tileY + y].GetBlockType() != BlockType.PLATFORM_FARM))
                        return false;
                    }
                    if(GetCollisionTypeAt(tileX + x, tileY + y) != CollisionTypeEnum.AIR && 
                        GetCollisionTypeAt(tileX + x, tileY + y) != CollisionTypeEnum.SCAFFOLDING &&
                        GetCollisionTypeAt(tileX + x, tileY + y) != CollisionTypeEnum.SCAFFOLDING_BRIDGE)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsWallpaperPlacementValid(int tileX, int tileY, int tileWidth, int tileHeight)
        {
            if(tileX > MapPixelWidth()/8 || tileX < 0)
            {
                return false;
            }
            if(tileY > MapPixelHeight()/8 || tileY < 0)
            {
                return false;
            }

            for (int x = 0; x < tileWidth; x++)
            {
                for (int y = 0; y < tileHeight; y++)
                {
                    if (wallpaperEntityGrid[tileX + x, tileY + y] != null)
                    {
                        return false;
                    }
                    if (wallMap[tileX + x, tileY + y] == false)
                    {
                        if (buildingBlockGrid[tileX + x, tileY + y] == null ||
                            (buildingBlockGrid[tileX + x, tileY + y].GetBlockType() != BlockType.SCAFFOLDING &&
                            buildingBlockGrid[tileX + x, tileY + y].GetBlockType() != BlockType.PLATFORM &&
                            buildingBlockGrid[tileX + x, tileY + y].GetBlockType() != BlockType.PLATFORM_FARM))
                            return false;
                    }
                    if (GetCollisionTypeAt(tileX + x, tileY + y) != CollisionTypeEnum.AIR &&
                        GetCollisionTypeAt(tileX + x, tileY + y) != CollisionTypeEnum.SCAFFOLDING &&
                        GetCollisionTypeAt(tileX + x, tileY + y) != CollisionTypeEnum.SCAFFOLDING_BRIDGE)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public MovingPlatformDirectorZone GetDirectorZoneAt(RectangleF collisionRect)
        {
            foreach(MovingPlatformDirectorZone director in directorZones)
            {
                if(director.rectangle.Intersects(collisionRect))
                {
                    return director;
                }
            }
            return null;
        }

        public EntityCharacter GetCharacter(EntityCharacter.CharacterEnum cEnum)
        {
            foreach(Entity en in entityListManager.GetEntityList())
            {
                if(en is EntityCharacter && ((EntityCharacter)en).GetCharacterEnum() == cEnum)
                {
                    return (EntityCharacter)en;
                }
            }
            return null;
        }

        public bool IsTileEntityPlacementValid(int tileX, int tileY, int tileWidth, int tileHeight)
        {
            for (int x = 0; x < tileWidth; x++)
            {
                for (int y = 0; y < tileHeight; y++)
                {
                    if ((GetCollisionTypeAt(tileX + x, tileY + y) != CollisionTypeEnum.AIR &&
                        GetCollisionTypeAt(tileX + x, tileY + y) != CollisionTypeEnum.SCAFFOLDING &&
                        GetCollisionTypeAt(tileX + x, tileY + y) != CollisionTypeEnum.BRIDGE) || tileEntityGrid[tileX + x, tileY + y] != null)
                    {
                        return false;
                    }
                }
            }

            for (int x = 0; x < tileWidth; x++)
            {
                if (GetCollisionTypeAt(tileX + x, tileY + tileHeight) == CollisionTypeEnum.AIR || GetCollisionTypeAt(tileX + x, tileY + tileHeight) == CollisionTypeEnum.SCAFFOLDING || GetCollisionTypeAt(tileX + x, tileY + tileHeight) == CollisionTypeEnum.WATER)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsFloorEntityPlacementValid(int tileX, int tileY, int tileWidth)
        {
            if(tileX < 0 || tileX >= tileEntityGrid.GetLength(0))
            {
                return false;
            }
            if(tileY < 0 || tileY >= tileEntityGrid.GetLength(1))
            {
                return false;
            }

            for(int x = 0; x < tileWidth; x++)
            {
                if(tileEntityGrid[tileX + x, tileY] != null)
                {
                    return false;
                }
                if(GetCollisionTypeAt(tileX + x, tileY) != CollisionTypeEnum.SOLID)
                {
                    return false;
                }
                if(GetCollisionTypeAt(tileX + x, tileY - 1) != CollisionTypeEnum.AIR &&
                    GetCollisionTypeAt(tileX + x, tileY - 1) != CollisionTypeEnum.SCAFFOLDING &&
                    GetCollisionTypeAt(tileX + x, tileY - 1) != CollisionTypeEnum.SCAFFOLDING_BRIDGE &&
                    GetCollisionTypeAt(tileX + x, tileY - 1) != CollisionTypeEnum.BRIDGE)
                {
                    return false;
                }
            }

            return true;
        }

        public MapEntity GetPlacedFromMapEntityById(string id)
        {
            foreach(Entity en in entityListManager.GetEntityList())
            {
                if(en is MapEntity && ((MapEntity)en).GetID() == id)
                {
                    return (MapEntity)en;
                }
            }
            return null;
        }

        public bool IsBuildingBlockPlacementValid(int tileX, int tileY, bool isBlock, bool ignoreScaffolding = false, bool checkingExistingBlock = false)
        {
            if (!checkingExistingBlock && GetCollisionTypeAt(tileX, tileY) != CollisionTypeEnum.AIR)
            {
                return false;
            }
            
            bool validAdjacentTile = false;
            if(GetCollisionTypeAt(tileX, tileY + 1) != CollisionTypeEnum.AIR && GetCollisionTypeAt(tileX, tileY + 1) != CollisionTypeEnum.WATER)
            {
                validAdjacentTile = true;
                if(ignoreScaffolding &&
                    (GetCollisionTypeAt(tileX, tileY + 1) == CollisionTypeEnum.SCAFFOLDING ||
                    GetCollisionTypeAt(tileX, tileY + 1) == CollisionTypeEnum.SCAFFOLDING_BLOCK ||
                    GetCollisionTypeAt(tileX, tileY + 1) == CollisionTypeEnum.SCAFFOLDING_BRIDGE))
                {
                    validAdjacentTile = false;
                }
            }
            if (!validAdjacentTile && GetCollisionTypeAt(tileX, tileY - 1) != CollisionTypeEnum.AIR && GetCollisionTypeAt(tileX, tileY - 1) != CollisionTypeEnum.WATER && GetCollisionTypeAt(tileX, tileY - 1) != CollisionTypeEnum.BRIDGE)
            {
                validAdjacentTile = true;
                if (ignoreScaffolding &&
                    (GetCollisionTypeAt(tileX, tileY - 1) == CollisionTypeEnum.SCAFFOLDING ||
                    GetCollisionTypeAt(tileX, tileY - 1) == CollisionTypeEnum.SCAFFOLDING_BLOCK ||
                    GetCollisionTypeAt(tileX, tileY - 1) == CollisionTypeEnum.SCAFFOLDING_BRIDGE))
                {
                    validAdjacentTile = false;
                }
            }
            if (!validAdjacentTile && GetCollisionTypeAt(tileX + 1, tileY) != CollisionTypeEnum.AIR && GetCollisionTypeAt(tileX + 1, tileY) != CollisionTypeEnum.WATER)
            {
                validAdjacentTile = true;
                if (ignoreScaffolding &&
                    (GetCollisionTypeAt(tileX + 1, tileY) == CollisionTypeEnum.SCAFFOLDING ||
                    GetCollisionTypeAt(tileX + 1, tileY) == CollisionTypeEnum.SCAFFOLDING_BLOCK ||
                    GetCollisionTypeAt(tileX + 1, tileY) == CollisionTypeEnum.SCAFFOLDING_BRIDGE))
                {
                    validAdjacentTile = false;
                }
            }
            if (!validAdjacentTile && GetCollisionTypeAt(tileX - 1, tileY) != CollisionTypeEnum.AIR && GetCollisionTypeAt(tileX - 1, tileY) != CollisionTypeEnum.WATER)
            {
                validAdjacentTile = true;
                if (ignoreScaffolding &&
                    (GetCollisionTypeAt(tileX - 1, tileY) == CollisionTypeEnum.SCAFFOLDING ||
                    GetCollisionTypeAt(tileX - 1, tileY) == CollisionTypeEnum.SCAFFOLDING_BLOCK ||
                    GetCollisionTypeAt(tileX - 1, tileY) == CollisionTypeEnum.SCAFFOLDING_BRIDGE))
                {
                    validAdjacentTile = false;
                }
            }
            if (!validAdjacentTile)
            {
                return false;
            }

            if(isBlock && GetTileEntity(tileX, tileY) != null)
            {
                return false;
            }

            return true;
        }

        public bool IsFarmablePlacementValid(int tileX, int tileY)
        {
            if(GetGroundTileType(tileX, tileY+2) != GroundTileType.EARTH && GetGroundTileType(tileX, tileY + 2) != GroundTileType.SAND)
            {
                return false;
            }
            tileY++;
            if ((GetCollisionTypeAt(tileX, tileY) != CollisionTypeEnum.AIR  && GetCollisionTypeAt(tileX, tileY) != CollisionTypeEnum.BRIDGE && 
                GetCollisionTypeAt(tileX, tileY) != CollisionTypeEnum.SCAFFOLDING) && GetCollisionTypeAt(tileX, tileY) != CollisionTypeEnum.SCAFFOLDING_BRIDGE)
            {
                 return false;
            }
            if(GetTileEntity(tileX, tileY) != null || GetTileEntity(tileX, tileY + 1) != null)
            {
                return false;
            }
            if (GetCollisionTypeAt(tileX, tileY + 1) != CollisionTypeEnum.SOLID) 
            {
                if(GetBuildingBlockAt(tileX, tileY + 1) != null && 
                    GetBuildingBlockAt(tileX, tileY + 1).GetBlockType() == BlockType.PLATFORM_FARM)
                {
                    return true;
                }
                return false;
            }

            return true;
        }

        public void AddWallEntity(TileEntity entity)
        {
            Vector2 tilePosition = entity.GetTilePosition();
            entityListManager.Add(entity);
            for (int x = 0; x < entity.GetTileWidth(); x++)
            {
                for (int y = 0; y < entity.GetTileHeight(); y++)
                {
                    wallEntityGrid[(int)tilePosition.X + x, (int)tilePosition.Y + y] = entity;
                }
            }
        }


        public void AddWallpaperEntity(PEntityWallpaper entity)
        {
            Vector2 tilePosition = entity.GetTilePosition();
            entityListManager.Add(entity);
            for (int x = 0; x < entity.GetTileWidth(); x++)
            {
                for (int y = 0; y < entity.GetTileHeight(); y++)
                {
                    wallpaperEntityGrid[(int)tilePosition.X + x, (int)tilePosition.Y + y] = entity;
                }
            }
        }

        public void RandomizeBackground(RectangleF cameraBoundingBox)
        {
            background.Randomize(cameraBoundingBox);
            foreground.Randomize(cameraBoundingBox);
        }

        public void AddTileEntity(TileEntity entity)
        {
            Vector2 tilePosition = entity.GetTilePosition();
            entityListManager.Add(entity);
            for(int x = 0; x < entity.GetTileWidth(); x++)
            {
                for(int y = 0; y < entity.GetTileHeight(); y++)
                {
                    tileEntityGrid[(int)tilePosition.X + x, (int)tilePosition.Y + y] = entity;
                }
            }

            if(entity is PEntityLightSource)
            {
                PEntityLightSource els = (PEntityLightSource)entity;
                Vector2 lightPosition = els.GetLightPosition();
                lights.Add(new LightSource(els.GetLightStrength(), lightPosition, els.GetLightColor(), els));
            }
        }

        public TileEntity GetTileEntity(int tileX, int tileY)
        {
            if (tileX > tileEntityGrid.GetLength(0)-1 || tileX < 0 || tileY > tileEntityGrid.GetLength(1)-1 || tileY < 0)
            {
                return null;
            } else {
                return tileEntityGrid[tileX, tileY];
            }
        }

        public Item GetTileEntityItemForm(int tileX, int tileY)
        {
            TileEntity en = GetTileEntity(tileX, tileY);

            if(en == null)
            {
                return ItemDict.NONE;
            }

            if(en is PlacedEntity)
            {
                return ((PlacedEntity)en).GetItemForm();
            }

            return ItemDict.NONE; 
        }

        public Item GetWallEntityItemForm(int tileX, int tileY)
        {
            TileEntity en = wallEntityGrid[tileX, tileY];
            if(en == null)
            {
                return ItemDict.NONE;
            }

            if (en is PlacedEntity)
            {
                return ((PlacedEntity)en).GetItemForm();
            }
            return ItemDict.NONE;
        }

        public Item GetWallpaperItemForm(int tileX, int tileY)
        {
            PEntityWallpaper en = wallpaperEntityGrid[tileX, tileY];
            if (en == null)
            {
                return ItemDict.NONE;
            }
            return en.GetItemForm();
        }

        public TileEntity RemoveWallEntity(EntityPlayer player, int tileX, int tileY, World world)
        {
            TileEntity toRemove = wallEntityGrid[tileX, tileY];

            if (toRemove != null)
            {
                tileX = (int)toRemove.GetTilePosition().X;
                tileY = (int)toRemove.GetTilePosition().Y;
                for (int x = 0; x < toRemove.GetTileWidth(); x++)
                {
                    for (int y = 0; y < toRemove.GetTileHeight(); y++)
                    {
                        wallEntityGrid[tileX + x, tileY + y] = null;
                    }
                }
                entityListManager.Remove(toRemove);
            }

            if (toRemove is PlacedEntity)
            {
                ((PlacedEntity)toRemove).OnRemove(player, this, world);
            }

            return toRemove;
        }

        public PEntityWallpaper RemoveWallpaperEntity(EntityPlayer player, int tileX, int tileY, World world)
        {
            PEntityWallpaper toRemove = wallpaperEntityGrid[tileX, tileY];

            if (toRemove != null)
            {
                tileX = (int)toRemove.GetTilePosition().X;
                tileY = (int)toRemove.GetTilePosition().Y;
                for (int x = 0; x < toRemove.GetTileWidth(); x++)
                {
                    for (int y = 0; y < toRemove.GetTileHeight(); y++)
                    {
                        wallpaperEntityGrid[tileX + x, tileY + y] = null;
                    }
                }
                entityListManager.Remove(toRemove);
            }

            if (toRemove is PlacedEntity)
            {
                ((PlacedEntity)toRemove).OnRemove(player, this, world);
            }

            return toRemove;
        }

        public void RemoveTileEntity(EntityPlayer player, int tileX, int tileY, World world)
        {
            TileEntity toRemove = (TileEntity)tileEntityGrid[tileX, tileY];

            if(toRemove is PlacedEntity && !((PlacedEntity)toRemove).IsRemovable()) {
                player.AddNotification(new EntityPlayer.Notification("This isn't mine, I shouldn't remove this.", Color.Red));
            } else {

                if (toRemove != null)
                {
                    tileX = (int)toRemove.GetTilePosition().X;
                    tileY = (int)toRemove.GetTilePosition().Y;
                    for (int x = 0; x < toRemove.GetTileWidth(); x++)
                    {
                        for (int y = 0; y < toRemove.GetTileHeight(); y++)
                        {
                            tileEntityGrid[tileX + x, tileY + y] = null;
                        }
                    }
                    entityListManager.Remove(toRemove);
                    if (toRemove is PEntityLightSource)
                    {
                        foreach (LightSource ls in lights)
                        {
                            if (toRemove == ls.source)
                            {
                                lights.Remove(ls);
                                break; //PREVENTS SINGLE ENTITY FROM HAVING MULTIPLE LS>>>>
                            }
                        }
                    }
                }

                if (toRemove is PlacedEntity)
                {
                    ((PlacedEntity)toRemove).OnRemove(player, this, world);
                }
            }
        }

        public void AddBuildingBlock(BuildingBlock block)
        {
            buildingBlockGrid[(int)block.GetTilePosition().X, (int)block.GetTilePosition().Y] = block;
            buildingBlockList.Add(block);
        }

        public Item GetBuildingBlockItemForm(int tileX, int tileY)
        {
            BuildingBlock bb = buildingBlockGrid[tileX, tileY];

            if (bb == null)
            {
                return ItemDict.NONE;
            }

            return bb.GetItemForm();
        }

        public void AddEntity(Entity en)
        {
            if(en == null)
            {
                throw new Exception("Added a null entity");
            }

            if (en is EntityItem)
            {
                itemEntities.Add((EntityItem)en);
            }
            else
            {
                entityListManager.Add(en);
            }
        }

        public GroundTileType GetGroundTileType(int tileX, int tileY)
        {
            int baseTileId = GetTileIdFor(tileX, tileY);
            if(SAND_TILE_IDS.Contains(baseTileId))
            {
                return GroundTileType.SAND;
            } else if (BRIDGE_TILE_IDS.Contains(baseTileId))
            {
                return GroundTileType.BRIDGE;
            } else if (INTERIOR_TILE_IDS.Contains(baseTileId))
            {
                return GroundTileType.INTERIOR;
            }

            return GroundTileType.EARTH;
        }

        private bool CheckRemoveBuildingBlock(int tileX, int tileY, EntityPlayer player, List<XYTile> alreadyCovered)
        {
            foreach(XYTile pair in alreadyCovered)
            {
                if(tileX == pair.tileX && tileY == pair.tileY)
                {
                    return false;
                }
            }
            alreadyCovered.Add(new XYTile(tileX, tileY));

            if (IsBuildingBlockPlacementValid(tileX, tileY, false, true, true))
            {
                return true;
            }

            bool legal = false;
            if(GetBuildingBlockAt(tileX + 1, tileY) != null)
            {
                legal = CheckRemoveBuildingBlock(tileX + 1, tileY, player, alreadyCovered);
            }
            if(!legal && GetBuildingBlockAt(tileX - 1, tileY) != null)
            {
                legal = CheckRemoveBuildingBlock(tileX - 1, tileY, player, alreadyCovered);
            }
            if(!legal && GetBuildingBlockAt(tileX, tileY + 1) != null)
            {
                legal = CheckRemoveBuildingBlock(tileX, tileY + 1, player, alreadyCovered);
            }
            if (!legal && GetBuildingBlockAt(tileX, tileY - 1) != null)
            {
                legal = CheckRemoveBuildingBlock(tileX, tileY - 1, player, alreadyCovered);
            }

            return legal;
        }

        public void RemoveBuildingBlock(int tileX, int tileY, EntityPlayer player, World world)
        {
            BuildingBlock toRemove = buildingBlockGrid[tileX, tileY];
            buildingBlockList.Remove(toRemove);
            buildingBlockGrid[tileX, tileY] = null;

            toRemove.OnRemove(player, this, world);

            List<XYTile> alreadyCovered = new List<XYTile>();
            alreadyCovered.Add(new XYTile(tileX, tileY));

            //check all 4 adjacent..
            if(GetBuildingBlockAt(tileX + 1, tileY) != null)
            {
                if(!CheckRemoveBuildingBlock(tileX + 1, tileY, player, alreadyCovered))
                {
                    foreach(XYTile pair in alreadyCovered)
                    {
                        toRemove = buildingBlockGrid[pair.tileX, pair.tileY];
                        buildingBlockList.Remove(toRemove);
                        buildingBlockGrid[pair.tileX, pair.tileY] = null;
                        if (toRemove != null)
                        {
                            toRemove.OnRemove(player, this, world);
                        }
                        if (wallEntityGrid[pair.tileX, pair.tileY] != null)
                        {
                            RemoveWallEntity(player, pair.tileX, pair.tileY, world);
                        }
                        if (wallpaperEntityGrid[pair.tileX, pair.tileY] != null)
                        {
                            RemoveWallpaperEntity(player, pair.tileX, pair.tileY, world);
                        }
                    }
                }
            }
            alreadyCovered.Clear();
            if(GetBuildingBlockAt(tileX - 1, tileY) != null)
            {
                if(!CheckRemoveBuildingBlock(tileX - 1, tileY, player, alreadyCovered))
                {
                    foreach (XYTile pair in alreadyCovered)
                    {
                        toRemove = buildingBlockGrid[pair.tileX, pair.tileY];
                        buildingBlockList.Remove(toRemove);
                        buildingBlockGrid[pair.tileX, pair.tileY] = null;
                        if (toRemove != null)
                        {
                            toRemove.OnRemove(player, this, world);
                        }
                        if (wallEntityGrid[pair.tileX, pair.tileY] != null)
                        {
                            RemoveWallEntity(player, pair.tileX, pair.tileY, world);
                        }
                        if (wallpaperEntityGrid[pair.tileX, pair.tileY] != null)
                        {
                            RemoveWallpaperEntity(player, pair.tileX, pair.tileY, world);
                        }
                    }
                }
            }
            alreadyCovered.Clear();
            if(GetBuildingBlockAt(tileX, tileY + 1) != null)
            {
                if(!CheckRemoveBuildingBlock(tileX, tileY + 1, player, alreadyCovered))
                {
                    foreach (XYTile pair in alreadyCovered)
                    {
                        toRemove = buildingBlockGrid[pair.tileX, pair.tileY];
                        buildingBlockList.Remove(toRemove);
                        buildingBlockGrid[pair.tileX, pair.tileY] = null;
                        if (toRemove != null)
                        {
                            toRemove.OnRemove(player, this, world);
                        }
                        if (wallEntityGrid[pair.tileX, pair.tileY] != null)
                        {
                            RemoveWallEntity(player, pair.tileX, pair.tileY, world);
                        }
                        if (wallpaperEntityGrid[pair.tileX, pair.tileY] != null)
                        {
                            RemoveWallpaperEntity(player, pair.tileX, pair.tileY, world);
                        }
                    }
                }
            }
            alreadyCovered.Clear();
            if(GetBuildingBlockAt(tileX, tileY - 1) != null)
            {
                if(!CheckRemoveBuildingBlock(tileX, tileY - 1, player, alreadyCovered))
                {
                    foreach (XYTile pair in alreadyCovered)
                    {
                        toRemove = buildingBlockGrid[pair.tileX, pair.tileY];
                        buildingBlockList.Remove(toRemove);
                        buildingBlockGrid[pair.tileX, pair.tileY] = null;
                        if (toRemove != null)
                        {
                            toRemove.OnRemove(player, this, world);
                        }
                        if (wallEntityGrid[pair.tileX, pair.tileY] != null)
                        {
                            RemoveWallEntity(player, pair.tileX, pair.tileY, world);
                        }
                        if (wallpaperEntityGrid[pair.tileX, pair.tileY] != null)
                        {
                            RemoveWallpaperEntity(player, pair.tileX, pair.tileY, world);
                        }
                    }
                }
            }
        }

        public BuildingBlock GetBuildingBlockAt(int tileX, int tileY)
        {
            if(tileX < 0 || tileX > widthInTiles)
            {
                return null;
            }
            if(tileY < 0 || tileY > heightInTiles)
            {
                return null;
            }
            return buildingBlockGrid[tileX, tileY];
        }

        public void RemoveEntity(Entity en)
        {
            if (en is EntityItem)
            {
                itemEntities.Remove((EntityItem)en);
            }
            else
            {
                entityListManager.Remove(en);
            }
        }

        public bool IsEntityInEntityList(Entity en)
        {
            if(entityListManager.GetEntityList().Contains(en))
            {
                return true;
            }
            return false;
        }

        public void AddEntitySaveStates(List<SaveState> saveStates)
        {
            foreach(Entity entity in entityListManager.GetEntityList())
            {
                if (entity is IPersist)
                {
                    SaveState entitySaveState = ((IPersist)entity).GenerateSave();
                    entitySaveState.AddData("area", this.areaEnum.ToString());
                    saveStates.Add(entitySaveState);
                }
            }
        }

        public void AddBuildingBlockSaveStates(List<SaveState> saveStates)
        {
            foreach(BuildingBlock block in buildingBlockList)
            {
                SaveState bbSaveState = block.GenerateSave();
                bbSaveState.AddData("area", this.areaEnum.ToString());
                saveStates.Add(bbSaveState);
            }
        }

        public void MoveToWaypoint(EntityPlayer player, string waypointName)
        {
            foreach(Waypoint waypoint in waypoints)
            {
                if(waypoint.name.Equals(waypointName))
                {
                    player.SetPosition(new Vector2(waypoint.position.X, waypoint.position.Y - 32.1f));
                    return;
                }
            }
            throw new Exception("Spawn point not found!");
        }

        public void TickDay(World world, EntityPlayer player)
        {
            this.areaSeason = world.GetSeason();
            List<Entity> removedEn = new List<Entity>();

            foreach(Entity en in entityListManager.GetEntityList())
            {
                if(en is TEntityFarmable)
                {
                    if (world.GetDay() == 0 && ((TEntityFarmable)en).RemovedAtSeasonShift())
                    {
                        removedEn.Add(en);
                    } 
                }
                else if(en is TEntityForage)
                {
                    if(en is TEntitySeasonalForage && ((TEntitySeasonalForage)en).GetSeason() != this.areaSeason)
                    {
                        removedEn.Add(en);
                    }
                    if(Util.RandInt(1, 8) == 1)
                    {
                        removedEn.Add(en);
                    }
                } else if (en is TEntityToolable)
                {
                    if(Util.RandInt(1, 5) == 1)
                    {
                        removedEn.Add(en);
                    }
                }
            }
            foreach (Entity en in removedEn)
            {
                if (en is TileEntity)
                {
                    RemoveTileEntity(player, (int)((TileEntity)en).GetTilePosition().X, (int)((TileEntity)en).GetTilePosition().Y, world);
                } else
                {
                    RemoveEntity(en);
                }
            }
            itemEntities.Clear(); 

            foreach (SpawnZone sz in spawnZones)
            {
                sz.TickDaily(this);
            }

            //POTENTAILLY BAD! MAKE SURE ENTITYLIST SIZE DOESN"T DECREASE...
            int initialSize = entityListManager.GetEntityList().Count;
            for(int i = 0; i < entityListManager.GetEntityList().Count; i++)
            {
                Entity en = entityListManager.GetEntityList()[i];
                if (en is ITickDaily)
                {
                    ((ITickDaily)en).TickDaily(world, this, player);
                }
                if (entityListManager.GetEntityList().Count < initialSize)
                {
                    throw new Exception("entitylist[i] tried to remove something (or self) from list; only adding is allowed");
                }
            }
        }

        public void DrawEntities(SpriteBatch sb, DrawLayer layer, RectangleF cameraBoundingBox, float layerDepth)
        {
            foreach (Entity en in entityListManager.GetEntitiesByDrawLayer(layer))
            {
                RectangleF colRect = en.GetCollisionRectangle();
                colRect.Inflate(48, 48);
                if (colRect.Intersects(cameraBoundingBox))
                {
                    en.Draw(sb, layerDepth);
                }
            }
        }

        public void DrawItemEntities(SpriteBatch sb, float layerDepth)
        {
            foreach(EntityItem ien in itemEntities)
            {
                ien.Draw(sb, layerDepth);
            }
        }

        public void DrawWalls(SpriteBatch sb, Matrix cameraMatrix, float layerDepth)
        {
            TiledMapLayer walls = tiledMap.GetLayer("walls");
            TiledMapLayer buildings = tiledMap.GetLayer("buildings");
            TiledMapLayer decoration = tiledMap.GetLayer("decoration");

            if (walls != null)
            {
                mapRenderer.Draw(tiledMap.GetLayer("walls"), cameraMatrix, null, null, layerDepth);
            }
            if (buildings != null)
            {
                mapRenderer.Draw(tiledMap.GetLayer("buildings"), cameraMatrix, null, null, layerDepth);
            }
            if (decoration != null)
            {
                mapRenderer.Draw(tiledMap.GetLayer("decoration"), cameraMatrix, null, null, layerDepth);
            }
        }

        public void DrawForeground(SpriteBatch sb, RectangleF cameraBoundingBox, float layerDepth)
        {
            foreground.Draw(sb, cameraBoundingBox, layerDepth);
        }

        private int GetTileIdFor(int tileX, int tileY)
        {
            int tileGlobalId = 0;
            TiledMapTileLayer layer = (TiledMapTileLayer)tiledMap.GetLayer("base");
            TiledMapTile? t;
            layer.TryGetTile(tileX, tileY, out t);
            if (t != null)
            {
                tileGlobalId = t.Value.GlobalIdentifier;
            }
            return tileGlobalId;
        }

        public Color GetPrimaryColorForTile(int tileX, int tileY)
        {
            int id = GetTileIdFor(tileX, tileY);
            if(BRIDGE_TILE_IDS.Contains(id))
            {
                return Util.BRIDGE_PRIMARY.color;
            } else if (ORANGE_EARTH_TILE_IDS.Contains(id))
            {
                return Util.PLATEAU_ORANGE_PRIMARY.color;
            } else if (SAND_TILE_IDS.Contains(id))
            {
                return Util.SAND_PRIMARY.color;
            } else if (INTERIOR_TILE_IDS.Contains(id))
            {
                return Util.WOOD_PRIMARY.color;
            }
            return Util.TRANSPARENT.color;
        }

        public Color GetSecondaryColorForTile(int tileX, int tileY)
        {
            int id = GetTileIdFor(tileX, tileY);
            if (BRIDGE_TILE_IDS.Contains(id))
            {
                return Util.BRIDGE_SECONDARY.color;
            }
            else if (ORANGE_EARTH_TILE_IDS.Contains(id))
            {
                return Util.PLATEAU_ORANGE_SECONDARY.color;
            }
            else if (SAND_TILE_IDS.Contains(id))
            {
                return Util.SAND_SECONDARY.color;
            }
            else if (INTERIOR_TILE_IDS.Contains(id))
            {
                return Util.WOOD_SECONDARY.color;
            }
            return Util.TRANSPARENT.color;
        }

        public void Tick(int length, EntityPlayer player, World world)
        {
            for(int i = 0; i < entityListManager.GetEntityList().Count; i++)
            {
                if(entityListManager.GetEntityList()[i] is ITick)
                {
                    ((ITick)entityListManager.GetEntityList()[i]).Tick(length, player, this, world);
                }
            }
        }

        public Waypoint GetWaypoint(string name)
        {
            foreach(Waypoint sp in waypoints)
            {
                if(sp.name.Equals(name))
                {
                    return sp;
                }
            }
            throw new Exception("Waypoint not found!");
        }

        public TransitionZone CheckTransition(Vector2 position, bool interacting)
        {
            foreach(TransitionZone tz in transitions)
            {
                if (tz.automatic || interacting)
                {
                    if (tz.rectangle.Contains(position))
                    {
                        return tz;
                    }
                }
            }
            return null;
        }

        public void AddParticle(Particle particle)
        {
            this.particleList.Add(particle);
        }
    }
}
