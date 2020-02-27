using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Components;
using Platfarm.Items;

namespace Platfarm.Entities
{
    public class EntityCharacter : EntityCollidable, ITickDaily, ITick, IInteract, IPersist
    {
        private static int HEIGHT = 24; //height of character's hitbox
        private static int WIDTH = 6; //width of character's hitbox
        private static int OFFSET_X = 29; //offset x from where the image is to where the hitbox begins
        private static int OFFSET_Y = 8; //offset y from where the image border is to where the hitbox begins

        private static float SPEED = 80;
        //private static float SPEED = 42;
        private static float SPEED_WHILE_JUMPING = 40;
        private static float GRAVITY = 8;
        private static float JUMP_SPEED = -2.55F;
        private static int COLLISION_STEPS = 3;

        private enum FadeState
        {
            FADE_OUT, FADE_IN, NONE
        }
        private static float FADE_SPEED = 2.5f;
        private FadeState fadeState;
        private float opacity;

        public enum CharacterEnum
        {
            AIDEN, CADE, CAMUS, CECILY, CHARLOTTE, CLAUDE, ELLE, FINLEY, HIMEKO, MEREDITH, OTIS, PAIGE, PIPER, RAUL, ROCKWELL, SKYE, TROY
        }

        public class ClothingSet
        {
            public ClothingItem hat, shirt, outerwear, pants, socks, shoes, gloves, earrings, scarf, glasses, back, sailcloth;
            public ClothingItem skin, hair, eyes;
            public Func<World, EntityCharacter, bool> conditionFunction;
            public int priority;

            public ClothingSet(ClothingItem skin, ClothingItem hair, ClothingItem eyes, ClothingItem hat, ClothingItem shirt, ClothingItem outerwear, ClothingItem pants,
                ClothingItem shoes, ClothingItem back, ClothingItem glasses, ClothingItem socks, ClothingItem gloves, ClothingItem earrings, ClothingItem scarf, ClothingItem sailcloth,
                Func<World, EntityCharacter, bool> conditionFunction, int priority)
            {
                this.hat = hat;
                this.shirt = shirt;
                this.outerwear = outerwear;
                this.pants = pants;
                this.socks = socks;
                this.shoes = shoes;
                this.gloves = gloves;
                this.earrings = earrings;
                this.scarf = scarf;
                this.glasses = glasses;
                this.back = back;
                this.sailcloth = sailcloth;
                this.skin = skin;
                this.hair = hair;
                this.eyes = eyes;
                this.conditionFunction = conditionFunction;
                this.priority = priority;
            }

            public bool CheckCondition(World world, EntityCharacter character)
            {
                return conditionFunction(world, character);
            }

            public void UpdateClothedSprite(float deltaTime, ClothedSprite sprite)
            {
                bool drawPantsOverShoes = pants.HasTag(Item.Tag.DRAW_OVER_SHOES);
                bool hideHair = false;
                if (hat != ItemDict.CLOTHING_NONE && hair.HasTag(Item.Tag.HIDE_WHEN_HAT))
                {
                    hideHair = true;
                }
                if (hat.HasTag(Item.Tag.ALWAYS_HIDE_HAIR))
                {
                    hideHair = true;
                }
                else if (hat.HasTag(Item.Tag.ALWAYS_SHOW_HAIR))
                {
                    hideHair = false;
                }
                sprite.Update(deltaTime, hat.GetSpritesheet(), shirt.GetSpritesheet(), outerwear.GetSpritesheet(),
                pants.GetSpritesheet(), socks.GetSpritesheet(), shoes.GetSpritesheet(),
                gloves.GetSpritesheet(), earrings.GetSpritesheet(), scarf.GetSpritesheet(), glasses.GetSpritesheet(),
                back.GetSpritesheet(), sailcloth.GetSpritesheet(), hair.GetSpritesheet(), skin.GetSpritesheet(), eyes.GetSpritesheet(), 
                drawPantsOverShoes, hideHair);
            }
        }

        private class ClothingManager
        {
            private List<ClothingSet> clothingSets;
            private ClothingSet currentSet;

            public ClothingManager(List<ClothingSet> sets)
            {
                this.clothingSets = sets;
                this.currentSet = clothingSets[0];
            }

            public void Update(float deltaTime, ClothedSprite sprite)
            {
                currentSet.UpdateClothedSprite(deltaTime, sprite);
            }

            public void TickDaily(World world, EntityCharacter character)
            {
                currentSet = clothingSets[0];
                foreach(ClothingSet set in clothingSets)
                {
                    if(set.CheckCondition(world, character) && set.priority > currentSet.priority)
                    {
                        currentSet = set;
                    }
                }
            } 
        }

        public class Schedule
        {
            private class SubzoneMap
            {
                private class Node
                {
                    public class ConnectedNode
                    {
                        public MovementTypeWaypoint waypoint;
                        public Node nodeTo;

                        public ConnectedNode(Node nodeTo, MovementTypeWaypoint waypoint)
                        {
                            this.nodeTo = nodeTo;
                            this.waypoint = waypoint;
                        }
                    }

                    public List<ConnectedNode> neighbors;
                    public Area.Subarea.NameEnum subzone;
                    public Area.Waypoint waypoint;

                    public Node(Area.Subarea.NameEnum subzone)
                    {
                        this.subzone = subzone;
                        this.neighbors = new List<ConnectedNode>();
                    }

                    public void AddNeighbor(Node neighbor, MovementTypeWaypoint connection)
                    {
                        this.neighbors.Add(new ConnectedNode(neighbor, connection));
                    }
                }

                private List<Node> allNodesInMap;

                public SubzoneMap(World world)
                {
                    Node apex, beach, farm, s0walk, s0warp, store, cafe, bookstoreLower, bookstoreUpper, s1walk, s1warp,
                        farmhouse, rockwellHouse, beachHouse, piperHouse, townhall, workshop, forge, town, s2, s3, s4, inn;
                    allNodesInMap = new List<Node>();
                    allNodesInMap.Add(apex = new Node(Area.Subarea.NameEnum.APEX));
                    allNodesInMap.Add(beach = new Node(Area.Subarea.NameEnum.BEACH));
                    allNodesInMap.Add(farm = new Node(Area.Subarea.NameEnum.FARM));
                    allNodesInMap.Add(s0walk = new Node(Area.Subarea.NameEnum.S0WALK));
                    allNodesInMap.Add(s0warp = new Node(Area.Subarea.NameEnum.S0WARP));
                    allNodesInMap.Add(store = new Node(Area.Subarea.NameEnum.STORE));
                    allNodesInMap.Add(cafe = new Node(Area.Subarea.NameEnum.CAFE));
                    allNodesInMap.Add(bookstoreLower = new Node(Area.Subarea.NameEnum.BOOKSTORELOWER));
                    allNodesInMap.Add(bookstoreUpper = new Node(Area.Subarea.NameEnum.BOOKSTOREUPPER));
                    allNodesInMap.Add(s1walk = new Node(Area.Subarea.NameEnum.S1WALK));
                    allNodesInMap.Add(s1warp = new Node(Area.Subarea.NameEnum.S1WARP));
                    allNodesInMap.Add(farmhouse = new Node(Area.Subarea.NameEnum.FARMHOUSE));
                    allNodesInMap.Add(rockwellHouse = new Node(Area.Subarea.NameEnum.ROCKWELLHOUSE));
                    allNodesInMap.Add(beachHouse = new Node(Area.Subarea.NameEnum.BEACHHOUSE));
                    allNodesInMap.Add(piperHouse = new Node(Area.Subarea.NameEnum.PIPERHOUSE));
                    allNodesInMap.Add(townhall = new Node(Area.Subarea.NameEnum.TOWNHALL));
                    allNodesInMap.Add(workshop = new Node(Area.Subarea.NameEnum.WORKSHOP));
                    allNodesInMap.Add(forge = new Node(Area.Subarea.NameEnum.FORGE));
                    allNodesInMap.Add(town = new Node(Area.Subarea.NameEnum.TOWN));
                    allNodesInMap.Add(s2 = new Node(Area.Subarea.NameEnum.S2));
                    allNodesInMap.Add(s3 = new Node(Area.Subarea.NameEnum.S3));
                    allNodesInMap.Add(s4 = new Node(Area.Subarea.NameEnum.S4));
                    allNodesInMap.Add(inn = new Node(Area.Subarea.NameEnum.INN));

                    s0warp.AddNeighbor(s0walk, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S0).GetWaypoint("SPtop"), MovementTypeWaypoint.MovementEnum.WARP));
                    s0walk.AddNeighbor(farm, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S0).GetWaypoint("TRs1top"), MovementTypeWaypoint.MovementEnum.WALK));
                    s0walk.AddNeighbor(s0warp, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S0).GetWaypoint("SPtop"), MovementTypeWaypoint.MovementEnum.WARP));
                    farm.AddNeighbor(s0walk, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.FARM).GetWaypoint("TRfarmRight"), MovementTypeWaypoint.MovementEnum.WALK));
                    farm.AddNeighbor(farmhouse, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.FARM).GetWaypoint("TRfarmhouse"), MovementTypeWaypoint.MovementEnum.WALK));
                    farm.AddNeighbor(town, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.FARM).GetWaypoint("TRfarmLeft"), MovementTypeWaypoint.MovementEnum.WALK));
                    farmhouse.AddNeighbor(farm, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.INTERIOR).GetWaypoint("TRhome"), MovementTypeWaypoint.MovementEnum.WALK));

                    town.AddNeighbor(farm, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.TOWN).GetWaypoint("TRtownRight"), MovementTypeWaypoint.MovementEnum.WALK));
                    town.AddNeighbor(cafe, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.TOWN).GetWaypoint("TRcafe"), MovementTypeWaypoint.MovementEnum.WALK));
                    town.AddNeighbor(bookstoreUpper, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.TOWN).GetWaypoint("TRbookstoreTop"), MovementTypeWaypoint.MovementEnum.WALK));
                    town.AddNeighbor(bookstoreLower, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.TOWN).GetWaypoint("TRbookstoreBottom"), MovementTypeWaypoint.MovementEnum.WALK));
                    town.AddNeighbor(townhall, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.TOWN).GetWaypoint("TRtownhall"), MovementTypeWaypoint.MovementEnum.WALK));
                    town.AddNeighbor(store, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.TOWN).GetWaypoint("TRstore"), MovementTypeWaypoint.MovementEnum.WALK));
                    town.AddNeighbor(workshop, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.TOWN).GetWaypoint("TRworkshop"), MovementTypeWaypoint.MovementEnum.WALK));
                    town.AddNeighbor(forge, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.TOWN).GetWaypoint("TRforge"), MovementTypeWaypoint.MovementEnum.WALK));
                    town.AddNeighbor(piperHouse, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.TOWN).GetWaypoint("TRpipers"), MovementTypeWaypoint.MovementEnum.WALK));
                    town.AddNeighbor(beach, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.TOWN).GetWaypoint("TRbeach"), MovementTypeWaypoint.MovementEnum.WALK));
                    town.AddNeighbor(s1walk, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.TOWN).GetWaypoint("TRcablecar"), MovementTypeWaypoint.MovementEnum.WALK));

                    cafe.AddNeighbor(town, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.INTERIOR).GetWaypoint("TRcafe"), MovementTypeWaypoint.MovementEnum.WALK));
                    bookstoreUpper.AddNeighbor(town, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.INTERIOR).GetWaypoint("TRbookstoreTop"), MovementTypeWaypoint.MovementEnum.WALK));
                    bookstoreLower.AddNeighbor(town, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.INTERIOR).GetWaypoint("TRbookstoreBottom"), MovementTypeWaypoint.MovementEnum.WALK));
                    townhall.AddNeighbor(town, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.INTERIOR).GetWaypoint("TRtownhall"), MovementTypeWaypoint.MovementEnum.WALK));
                    store.AddNeighbor(town, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.INTERIOR).GetWaypoint("TRstore"), MovementTypeWaypoint.MovementEnum.WALK));
                    workshop.AddNeighbor(town, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.INTERIOR).GetWaypoint("TRworkshop"), MovementTypeWaypoint.MovementEnum.WALK));
                    forge.AddNeighbor(town, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.INTERIOR).GetWaypoint("TRforgeTop"), MovementTypeWaypoint.MovementEnum.WALK));
                    piperHouse.AddNeighbor(town, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.INTERIOR).GetWaypoint("TRpipers"), MovementTypeWaypoint.MovementEnum.WALK));

                    beach.AddNeighbor(town, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.BEACH).GetWaypoint("TRbeachElevator"), MovementTypeWaypoint.MovementEnum.WALK));
                    beach.AddNeighbor(rockwellHouse, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.BEACH).GetWaypoint("TRrockwell"), MovementTypeWaypoint.MovementEnum.WALK));
                    beach.AddNeighbor(beachHouse, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.BEACH).GetWaypoint("TRclaude"), MovementTypeWaypoint.MovementEnum.WALK));

                    rockwellHouse.AddNeighbor(beach, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.INTERIOR).GetWaypoint("TRrockwell"), MovementTypeWaypoint.MovementEnum.WALK));
                    beachHouse.AddNeighbor(beach, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.INTERIOR).GetWaypoint("TRclaude"), MovementTypeWaypoint.MovementEnum.WALK));
                    
                    s1walk.AddNeighbor(town, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S1).GetWaypoint("TRs1entrance"), MovementTypeWaypoint.MovementEnum.WALK));
                    s1walk.AddNeighbor(inn, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S1).GetWaypoint("TRinn"), MovementTypeWaypoint.MovementEnum.WALK));
                    s1walk.AddNeighbor(s1warp, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S1).GetWaypoint("SPs1entrance"), MovementTypeWaypoint.MovementEnum.WARP));

                    inn.AddNeighbor(s1walk, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.INTERIOR).GetWaypoint("TRinn"), MovementTypeWaypoint.MovementEnum.WALK));

                    s1warp.AddNeighbor(s1walk, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S1).GetWaypoint("SPs1entrance"), MovementTypeWaypoint.MovementEnum.WARP));
                    s1warp.AddNeighbor(s2, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S1).GetWaypoint("TRs1exit"), MovementTypeWaypoint.MovementEnum.WARP));

                    s2.AddNeighbor(s1warp, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S2).GetWaypoint("TRs2entrance"), MovementTypeWaypoint.MovementEnum.WARP));
                    s2.AddNeighbor(s3, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S2).GetWaypoint("TRs2exit"), MovementTypeWaypoint.MovementEnum.WARP));

                    s3.AddNeighbor(s2, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S3).GetWaypoint("TRs3entrance"), MovementTypeWaypoint.MovementEnum.WARP));
                    s3.AddNeighbor(s4, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S3).GetWaypoint("TRs3exit"), MovementTypeWaypoint.MovementEnum.WARP));

                    s4.AddNeighbor(s3, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S4).GetWaypoint("TRs4entrance"), MovementTypeWaypoint.MovementEnum.WARP));
                    s4.AddNeighbor(apex, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.S4).GetWaypoint("TRs4exit"), MovementTypeWaypoint.MovementEnum.WARP));

                    apex.AddNeighbor(s4, new MovementTypeWaypoint(world.GetAreaByEnum(Area.AreaEnum.APEX).GetWaypoint("TRapexEntrance"), MovementTypeWaypoint.MovementEnum.WARP));
                }

                private MovementTypeWaypoint.MovementEnum GetMovementTypeForSubzone(Area.Subarea.NameEnum subzone)
                {
                    switch(subzone)
                    {
                        case Area.Subarea.NameEnum.S0WARP:
                        case Area.Subarea.NameEnum.APEX:
                        case Area.Subarea.NameEnum.S1WARP:
                        case Area.Subarea.NameEnum.S2:
                        case Area.Subarea.NameEnum.S3:
                        case Area.Subarea.NameEnum.S4:
                            return MovementTypeWaypoint.MovementEnum.WARP;
                    }
                    return MovementTypeWaypoint.MovementEnum.WALK;
                }

                private Node GetNodeForSubzone(Area.Subarea.NameEnum subzone)
                {
                    foreach(Node node in allNodesInMap)
                    {
                        if(node.subzone == subzone)
                        {
                            return node;
                        }
                    }
                    throw new Exception("Error in GetNodeForSubzone");
                }

                public Queue<MovementTypeWaypoint> FindPath(Area.Subarea.NameEnum source, Area.Waypoint sourceWaypoint, Area.Subarea.NameEnum destinationSubzone, Area.Waypoint destinationWaypoint)
                {
                    List<Node.ConnectedNode> startingPath = new List<Node.ConnectedNode>();
                    startingPath.Add(new Node.ConnectedNode(GetNodeForSubzone(source), new MovementTypeWaypoint(sourceWaypoint, MovementTypeWaypoint.MovementEnum.WALK)));

                    Queue<MovementTypeWaypoint> path = BFS(startingPath, destinationSubzone);
                    path.Enqueue(new MovementTypeWaypoint(destinationWaypoint, GetMovementTypeForSubzone(destinationSubzone))); //add the ending node
                    return path;
                }

                private bool DoesCurrentPathIncludeSubzone(List<Node.ConnectedNode> path, Area.Subarea.NameEnum subzone)
                {
                    foreach(Node.ConnectedNode node in path)
                    {
                        if(node.nodeTo.subzone == subzone)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                private Queue<MovementTypeWaypoint> BFS(List<Node.ConnectedNode> currentPath, Area.Subarea.NameEnum dest)
                {
                    Node.ConnectedNode currentNode = currentPath[currentPath.Count - 1];
                    if(currentNode.nodeTo.subzone == dest)
                    {
                        /*Console.WriteLine("SUCCESSPATH");
                        foreach (Node.ConnectedNode cn in currentPath)
                        {
                            Console.WriteLine(cn.nodeTo.subzone + "  ");
                        }*/
                        //SUCCESS - found path
                        Queue<MovementTypeWaypoint> returnQueue = new Queue<MovementTypeWaypoint>();
                        foreach (Node.ConnectedNode node in currentPath)
                        {
                            returnQueue.Enqueue(node.waypoint);
                        }
                        return returnQueue;
                    } else
                    {
                        foreach(Node.ConnectedNode neighbor in currentNode.nodeTo.neighbors)
                        {
                            if (!DoesCurrentPathIncludeSubzone(currentPath, neighbor.nodeTo.subzone))
                            {
                                List<Node.ConnectedNode> currentPathCopy = new List<Node.ConnectedNode>();
                                foreach(Node.ConnectedNode cn in currentPath)
                                {
                                    currentPathCopy.Add(cn);
                                }
                                currentPathCopy.Add(neighbor);
                                Queue<MovementTypeWaypoint> result = BFS(currentPathCopy, dest);
                                if (result != null)
                                {
                                    return result;
                                }
                            }
                        }
                    }
                    /*Console.WriteLine("FAILEDPATH");
                    foreach(Node.ConnectedNode cn in currentPath)
                    {
                        Console.WriteLine(cn.nodeTo.subzone + "  ");
                    }*/
                    return null; //dead end case
                }
            }

            public class MovementTypeWaypoint
            {
                public enum MovementEnum
                {
                    WALK, WARP
                }

                public MovementEnum movementType;
                public Area.Waypoint waypoint;

                public MovementTypeWaypoint(Area.Waypoint waypoint, MovementEnum movementType)
                {
                    this.movementType = movementType;
                    this.waypoint = waypoint;
                }
            }

            public abstract class Event
            {
                private Area area;
                private Area.Waypoint waypoint;
                private int startHour, startMinute, endHour, endMinute;
                private Func<World, EntityCharacter, bool> conditionFunction;
                private int priority;

                public Event(Area area, Area.Waypoint waypoint, int startHour, int startMinute, int endHour, int endMinute, Func<World, EntityCharacter, bool> conditionFunction, int priority)
                {
                    this.area = area;
                    this.waypoint = waypoint;
                    this.startHour = startHour;
                    this.startMinute = startMinute;
                    this.endHour = endHour;
                    this.endMinute = endMinute;
                    this.conditionFunction = conditionFunction;
                    this.priority = priority;
                }

                public Area GetArea()
                {
                    return area;
                }

                public Area.Waypoint GetWaypoint()
                {
                    return waypoint;
                }

                public int GetPriority()
                {
                    return this.priority;
                }

                public bool CheckActivation(World world, EntityCharacter character)
                {
                    int currHour = world.GetHour();
                    int currMin = world.GetMinute();
                    if (currHour < startHour || currHour > endHour)
                    {
                        return false;
                    }
                    if (currHour == startHour && currMin < startMinute)
                    {
                        return false;
                    }
                    if (currHour == endHour && currMin > endMinute)
                    {
                        return false;
                    }
                    return conditionFunction(world, character);
                }

                public abstract void Update(float deltaTime, EntityCharacter character, Area area, Queue<MovementTypeWaypoint> waypoints);
            }

            public class StandAtEvent : Event
            {
                public StandAtEvent(Area area, Area.Waypoint spawn, int startHour, int startMinute, int endHour, int endMinute, Func<World, EntityCharacter, bool> conditionFunction, int priority) : base(area, spawn, startHour, startMinute, endHour, endMinute, conditionFunction, priority)
                {
                    //does nothing
                }

                public override void Update(float deltaTime, EntityCharacter character, Area area, Queue<MovementTypeWaypoint> waypoints)
                {
                    //just standing...
                    //do nothing
                }
            }

            private static SubzoneMap subzoneMap;
            private Queue<MovementTypeWaypoint> waypoints;
            private List<Event> events;
            private Event currentEvent;

            public Schedule(List<Event> events, World world)
            {
                waypoints = new Queue<MovementTypeWaypoint>();
                this.events = events;
                currentEvent = null;
                if(subzoneMap == null)
                {
                    subzoneMap = new SubzoneMap(world);
                }
            }

            public void Update(float deltaTime, Area area, EntityCharacter character, World world)
            {
                if(character.fadeState == FadeState.FADE_OUT)
                {
                    character.velocityX = 0;
                    character.velocityY = 0;
                    character.opacity -= FADE_SPEED * deltaTime;
                    if (character.opacity < 0)
                    {
                        if (waypoints.Count != 0)
                        {
                            Area.TransitionZone transitionZone = area.CheckTransition(character.GetCollisionRectangle().Center, true);
                            if (transitionZone != null)
                            {
                                Area transitionTo = world.GetAreaByName(transitionZone.to);
                                world.MoveCharacter(character, area, transitionTo);
                                character.SetPosition(transitionTo.GetWaypoint(transitionZone.spawn).position - new Vector2(0, 32.1f));
                            }
                        }
                        character.fadeState = FadeState.FADE_IN;
                    }
                } else if (character.fadeState == FadeState.FADE_IN)
                {
                    character.velocityX = 0;
                    character.velocityY = 0;
                    character.opacity += FADE_SPEED * deltaTime;
                    if(character.opacity > 1)
                    {
                        character.opacity = 1;
                        character.fadeState = FadeState.NONE;
                    }
                }
                else if(waypoints.Count == 0)
                {
                    character.velocityX = 0;
                    character.velocityY = 0;
                    if (currentEvent != null)
                    {
                        currentEvent.Update(deltaTime, character, area, waypoints);
                    }
                } else
                {
                    if (waypoints.Peek().movementType == MovementTypeWaypoint.MovementEnum.WALK) //WALK
                    {
                        if (waypoints.Peek().waypoint.position.X > character.GetCollisionRectangle().Center.X)
                        {
                            character.Walk(DirectionEnum.RIGHT, deltaTime);
                        }
                        else if (waypoints.Peek().waypoint.position.X < character.GetCollisionRectangle().Center.X)
                        {
                            character.Walk(DirectionEnum.LEFT, deltaTime);
                        }
                        
                        if ((waypoints.Peek().waypoint.position.Y < character.GetCollisionRectangle().Center.Y && 
                            area.IsCollideWithPathingHelperType(character.GetCollisionRectangle(), Area.PathingHelper.Type.CONDITIONALJUMP)))
                        {
                            character.TryJump();
                        }
                    } else if (waypoints.Peek().movementType == MovementTypeWaypoint.MovementEnum.WARP) //WARP
                    {
                        world.MoveCharacter(character, area, waypoints.Peek().waypoint.area);
                        character.SetPosition(waypoints.Peek().waypoint.position);
                    }

                    RectangleF destinationCheck = character.GetCollisionRectangle();
                    destinationCheck.Height += 16;
                    destinationCheck.Y -= 8;
                    destinationCheck.Width += 16;
                    destinationCheck.Width -= 8;
                    if(destinationCheck.Contains(waypoints.Peek().waypoint.position))
                    {
                        waypoints.Dequeue();
                        if (area.CheckTransition(character.GetCollisionRectangle().Center, true) != null)
                        {
                            character.fadeState = FadeState.FADE_OUT;
                        }
                    }
                }
            }

            public void Tick(World world, EntityCharacter character, Area currentArea)
            {
                if(currentEvent != null && !currentEvent.CheckActivation(world, character))
                {
                    currentEvent = null;
                }
                foreach (Schedule.Event scEvent in events)
                {
                    if (scEvent.CheckActivation(world, character) && (currentEvent == null || scEvent.GetPriority() > currentEvent.GetPriority()))
                    {
                        waypoints.Clear();
                        currentEvent = scEvent;

                        waypoints = subzoneMap.FindPath(currentArea.GetSubareaAt(character.GetCollisionRectangle()), 
                            new Area.Waypoint(character.GetCollisionRectangle().Center, "CHAR", currentArea), 
                            scEvent.GetArea().GetSubareaAt(new RectangleF(scEvent.GetWaypoint().position, new Size2(2, 2))),
                            scEvent.GetWaypoint());

                    }
                }
            }
        }

        public class DialogueOption
        {
            private Func<World, EntityCharacter, bool> conditionFunction;
            private DialogueNode root;
            private int weight;

            public DialogueOption(DialogueNode root, Func<World, EntityCharacter, bool> condition, int weight = 1)
            {
                this.root = root;
                this.conditionFunction = condition;
                this.weight = weight;
            }

            public int GetWeight()
            {
                return weight;
            }

            public DialogueNode GetDialogue()
            {
                return root;
            }

            public bool CanActivate(World world, EntityCharacter character)
            {
                return conditionFunction(world, character);
            }
        }

        public class DialogueManager
        {
            private List<DialogueOption> dialogueOptions;

            public DialogueManager(List<DialogueOption> options)
            {
                this.dialogueOptions = options;
            }

            public DialogueNode GetDialogue(World world, EntityCharacter character)
            {
                List<DialogueNode> choices = new List<DialogueNode>();

                foreach(DialogueOption option in dialogueOptions)
                {
                    if(option.CanActivate(world, character))
                    {
                        for(int i = 0; i < option.GetWeight(); i++)
                        {
                            choices.Add(option.GetDialogue());
                        }
                    }
                }

                return choices[Util.RandInt(0, choices.Count-1)];
            }

        }

        private ClothedSprite sprite;
        private ClothingManager clothingManager;
        private string name;
        private Schedule schedule;
        private DialogueManager dialogueManager;
        private CharacterEnum characterEnum;
        private int heartPoints;
        private float velocityX, velocityY;
        private bool grounded;
        private DirectionEnum direction;
        private Area currentArea;
        private World world;

        private static int[] HEART_LEVEL_BREAKPOINTS = { 0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700, 3500 };

        public EntityCharacter(World world, CharacterEnum characterEnum, List<ClothingSet> clothingSets, List<Schedule.Event> scheduleEvents, List<DialogueOption> dialogueOptions)
        {
            sprite = new ClothedSprite();
            this.world = world;
            this.clothingManager = new ClothingManager(clothingSets);
            this.schedule = new Schedule(scheduleEvents, world);
            this.dialogueManager = new DialogueManager(dialogueOptions);
            this.direction = DirectionEnum.LEFT;
            this.drawLayer = DrawLayer.PRIORITY;
            this.characterEnum = characterEnum;
            this.heartPoints = heartPoints;
            this.velocityX = 0;
            this.velocityY = 0;
            this.grounded = false;
            this.fadeState = FadeState.NONE;
            this.opacity = 1.0f;
        }

        public Area GetCurrentArea()
        {
            return this.currentArea;
        }

        public void SetCurrentArea(Area newArea)
        {
            this.currentArea = newArea;
        } 

        public int GetHeartLevel()
        {
            int level = 0;
            while(heartPoints > HEART_LEVEL_BREAKPOINTS[level+1] && level < 10) {
                level++;
            }
            return level;
        }

        public void Walk(DirectionEnum direction, float deltaTime)
        {
            switch(direction)
            {
                case DirectionEnum.LEFT:
                    this.direction = DirectionEnum.LEFT;
                    velocityX = -SPEED * deltaTime;
                    break;
                case DirectionEnum.RIGHT:
                    this.direction = DirectionEnum.RIGHT;
                    velocityX = SPEED * deltaTime;
                    break;
            }
        }

        public void TryJump()
        {
            if(grounded)
            {
                velocityY = JUMP_SPEED;
            }
        }

        public void GainHeartPoints(int amount)
        {
            heartPoints += amount;
            if(heartPoints < 0)
            {
                heartPoints = 0;
            }
        }
             
        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            Vector2 modifiedPosition = new Vector2(position.X, position.Y);
            if (direction == DirectionEnum.LEFT)
            {
                modifiedPosition.X++;
            }
            sprite.Draw(sb, position, layerDepth, SpriteEffects.None, 1.0f, opacity);
        }

        public override RectangleF GetCollisionRectangle()
        {
            return new RectangleF((position.X + OFFSET_X) - (WIDTH/2), position.Y + OFFSET_Y + 1, WIDTH*2, HEIGHT - 1);
        }

        public override void Update(float deltaTime, Area area)
        {
            clothingManager.Update(deltaTime, sprite);
            schedule.Update(deltaTime, area, this, world);

            if(!grounded)
            {
                switch(direction)
                {
                    case DirectionEnum.LEFT:
                        velocityX = -SPEED_WHILE_JUMPING * deltaTime;
                        break;
                    case DirectionEnum.RIGHT:
                        velocityX = SPEED_WHILE_JUMPING * deltaTime;
                        break;
                }
            }

            velocityY += GRAVITY * deltaTime;

            //calculate collisions
            RectangleF collisionBox = GetCollisionRectangle();
            float stepX = velocityX / COLLISION_STEPS;
            for (int step = 0; step < COLLISION_STEPS; step++)
            {
                if (stepX != 0) //move X
                {
                    collisionBox = GetCollisionRectangle();
                    RectangleF stepXCollisionBox = new RectangleF(collisionBox.X + stepX, collisionBox.Y, collisionBox.Width, collisionBox.Height);
                    bool xCollision = CollisionHelper.CheckCollision(stepXCollisionBox, area, false);
                    RectangleF stepXCollisionBoxForesight = new RectangleF(collisionBox.X + (stepX * 15), collisionBox.Y, collisionBox.Width, collisionBox.Height);
                    bool xCollisionSoon = CollisionHelper.CheckCollision(stepXCollisionBoxForesight, area, false);

                    if (xCollisionSoon)
                    {
                        TryJump();
                    }

                    if (xCollision) //if next movement = collision
                    {
                        TryJump();
                        stepX = 0; //stop moving if collision
                        if (grounded)
                        {
                            velocityX = 0;
                        }
                    }
                    else
                    {
                        this.position.X += stepX;
                    }
                }
            }


            float stepY = velocityY / COLLISION_STEPS;
            for (int step = 0; step < COLLISION_STEPS; step++)
            {
                if (stepY != 0) //move Y
                {
                    collisionBox = GetCollisionRectangle();
                    RectangleF stepYCollisionBox = new RectangleF(collisionBox.X, collisionBox.Y + stepY, collisionBox.Width, collisionBox.Height);
                    bool yCollision = CollisionHelper.CheckCollision(stepYCollisionBox, area, stepY > 0);

                    if (yCollision)
                    {
                        if (velocityY > 0)
                        {
                            grounded = true;
                        }
                        stepY = 0;
                        velocityY = 0;

                    }
                    else
                    {
                        this.position.Y += stepY;
                        grounded = false;
                    }
                }
            }

            UpdateAnimation(deltaTime);
        }

        public void TickDaily(World world, Area area, EntityPlayer player)
        {
            clothingManager.TickDaily(world, this);
        }

        public override void SetPosition(Vector2 position)
        {
            this.position = new Vector2(position.X - OFFSET_X, position.Y);
        }

        public void Tick(int minutesTicked, EntityPlayer player, Area area, World world)
        {
            schedule.Tick(world, this, area);
        }

        public string GetLeftShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public string GetRightShiftClickAction(EntityPlayer player)
        {
            return "";
        }

        public string GetLeftClickAction(EntityPlayer player)
        {
            if(!player.GetHeldItem().GetItem().HasTag(Item.Tag.NO_TRASH))
            {
                return "Give";
            }
            return "";
        }

        public string GetRightClickAction(EntityPlayer player)
        {
            return "Talk";
        }

        public void InteractRight(EntityPlayer player, Area area, World world)
        {
            player.SetCurrentDialogue(dialogueManager.GetDialogue(world, this));
        }

        public void InteractLeft(EntityPlayer player, Area area, World world)
        {
            //give item
            
        }

        public void InteractRightShift(EntityPlayer player, Area area, World world)
        {
            
        }

        public void InteractLeftShift(EntityPlayer player, Area area, World world)
        {
            
        }

        public CharacterEnum GetCharacterEnum()
        {
            return this.characterEnum;
        }

        public DirectionEnum GetDirection()
        {
            return this.direction;
        }

        public SaveState GenerateSave()
        {
            SaveState state = new SaveState(SaveState.Identifier.CHARACTER);

            state.AddData("character", characterEnum.ToString());
            state.AddData("heartpoints", heartPoints.ToString());

            return state;
        }

        public void LoadSave(SaveState state)
        {
            heartPoints = Int32.Parse(state.TryGetData("heartpoints", "0"));
        }

        public bool IsJumping()
        {
            return velocityY < 0;
        }

        private void UpdateAnimation(float deltaTime)
        {
            /*else if (swimming)
            {
                if (inputVelocityX != 0)
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.WALK_CYCLE_L : ClothedSprite.WALK_CYCLE_R);
                }
                else
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.JUMP_ANIM_L : ClothedSprite.JUMP_ANIM_R);
                }
            }*/
           
            if (IsJumping())
            {
                sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.JUMP_ANIM_L : ClothedSprite.JUMP_ANIM_R);
            }
            else if (grounded && velocityX != 0)
            {
                sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.WALK_CYCLE_L : ClothedSprite.WALK_CYCLE_R);
            }
            else if (!grounded)
            {
                sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.FALLING_ANIM_L : ClothedSprite.FALLING_ANIM_R);
            }
            else
            {
                if (sprite.IsCurrentLoop(ClothedSprite.FALLING_ANIM_L) || sprite.IsCurrentLoop(ClothedSprite.FALLING_ANIM_R) || sprite.IsCurrentLoop(ClothedSprite.ROLLING_CYCLE_L) ||
                    sprite.IsCurrentLoop(ClothedSprite.ROLLING_CYCLE_R) || sprite.IsCurrentLoop(ClothedSprite.LANDING_ANIM_R) || sprite.IsCurrentLoop(ClothedSprite.LANDING_ANIM_L))
                {
                    if (!sprite.IsCurrentLoop(ClothedSprite.IDLE_CYCLE_L) && !sprite.IsCurrentLoop(ClothedSprite.LANDING_ANIM_L) && !sprite.IsCurrentLoop(ClothedSprite.IDLE_CYCLE_R) && !sprite.IsCurrentLoop(ClothedSprite.LANDING_ANIM_R))
                    {
                        sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.LANDING_ANIM_L : ClothedSprite.LANDING_ANIM_R);
                    }
                    else if (sprite.IsCurrentLoopFinished())
                    {
                        sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.IDLE_CYCLE_L : ClothedSprite.IDLE_CYCLE_R);
                    }
                }
                else
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.IDLE_CYCLE_L : ClothedSprite.IDLE_CYCLE_R);
                }
            }
        }

        protected override void OnXCollision()
        {
            velocityX = 0;
        }

        protected override void OnYCollision()
        {
            velocityY = 0;
        }
    }
}
