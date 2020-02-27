using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Platfarm.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Components
{
    public class CutsceneManager
    {
        public class Cutscene
        {
            public abstract class CSAction
            {
                public abstract void Update(float deltaTime, Area area, World world, Camera camera);
                public abstract bool IsFinished();
            }

            public class WaitCSAction : CSAction
            {
                private float waitTime;

                public WaitCSAction(float waitTime)
                {
                    this.waitTime = waitTime;
                }

                public override void Update(float deltaTime, Area area, World world, Camera camera)
                {
                    if (waitTime > 0)
                    {
                        waitTime -= deltaTime;
                    }
                }

                public override bool IsFinished()
                {
                    return waitTime <= 0;
                }
            }

            public class PanCameraToCSAction : CSAction
            {
                private Vector2 target;
                private Vector2 currentPos;
                private static float PAN_SPEED = 75.0f;
                private bool isStuck;

                public PanCameraToCSAction(Vector2 target)
                {
                    this.target = target;
                    this.currentPos = new Vector2(0, 0);
                    this.isStuck = false;
                }

                public override bool IsFinished()
                {
                    return isStuck || (currentPos.X == target.X && currentPos.Y == target.Y);
                }

                public override void Update(float deltaTime, Area area, World world, Camera camera)
                {
                    Vector2 startingPos = camera.GetBoundingBox().Center;
                    currentPos = camera.GetBoundingBox().Center;
                    currentPos.X = Util.AdjustTowards(currentPos.X, target.X, PAN_SPEED * deltaTime);
                    currentPos.Y = Util.AdjustTowards(currentPos.Y, target.Y, PAN_SPEED * deltaTime);

                    camera.Update(deltaTime, currentPos, world.GetCurrentArea().MapPixelWidth(), world.GetCurrentArea().MapPixelHeight());
                    if(startingPos.X == camera.GetBoundingBox().Center.X && startingPos.Y == camera.GetBoundingBox().Center.Y)
                    {
                        isStuck = true;
                    }
                }
            }

            public class PanCameraByCSAction : CSAction
            {
                private Vector2 movementAmount, target, currentPos;
                private bool isStuck;
                private static float PAN_SPEED = 75.0f;
                private bool firstUpdate;

                public PanCameraByCSAction(Vector2 movementAmount)
                {
                    this.isStuck = false;
                    this.movementAmount = movementAmount;
                    this.firstUpdate = true;
                    this.currentPos = new Vector2(0, 0);
                }
                
                public override bool IsFinished()
                {
                    return isStuck || (currentPos.X == target.X && currentPos.Y == target.Y);
                }

                public override void Update(float deltaTime, Area area, World world, Camera camera)
                {
                    if(firstUpdate)
                    {
                        target = camera.GetBoundingBox().Center + movementAmount;
                        firstUpdate = false;
                    }

                    Vector2 startingPos = camera.GetBoundingBox().Center;
                    currentPos = camera.GetBoundingBox().Center;
                    currentPos.X = Util.AdjustTowards(currentPos.X, target.X, PAN_SPEED * deltaTime);
                    currentPos.Y = Util.AdjustTowards(currentPos.Y, target.Y, PAN_SPEED * deltaTime);

                    camera.Update(deltaTime, currentPos, world.GetCurrentArea().MapPixelWidth(), world.GetCurrentArea().MapPixelHeight());
                    if (startingPos.X == camera.GetBoundingBox().Center.X && startingPos.Y == camera.GetBoundingBox().Center.Y)
                    {
                        isStuck = true;
                    }
                }
            }

            //public class MoveCharacterCSAction : CSACtion
            //public class GroupedCSAction : CSAction

            public class GroupedCSAction : CSAction
            {
                CSAction[] groupedActions;

                public GroupedCSAction(params CSAction[] group)
                {
                    groupedActions = group;
                }

                public override bool IsFinished()
                {
                    foreach(CSAction action in groupedActions)
                    {
                        if(!action.IsFinished())
                        {
                            return false;
                        }
                    }
                    return true;
                }

                public override void Update(float deltaTime, Area area, World world, Camera camera)
                {
                    foreach(CSAction action in groupedActions)
                    {
                        action.Update(deltaTime, area, world, camera);
                    }
                }
            }

            public class DialogueCSAction : CSAction
            {
                private DialogueNode root;
                private EntityPlayer player;
                private bool init;

                public DialogueCSAction(EntityPlayer player, DialogueNode root)
                {
                    this.root = root;
                    this.player = player;
                    this.init = false;
                }

                public override bool IsFinished()
                {
                    return player.GetCurrentDialogue() == null;
                }

                public override void Update(float deltaTime, Area area, World world, Camera camera)
                {
                    if(!init)
                    {
                        player.SetCurrentDialogue(root);
                        init = true;
                    }
                }
            }

            public class MovePlayerByCSAction : CSAction
            {
                private int movementX, targetX;
                private EntityPlayer player;
                private bool init, stuck;
                private DirectionEnum directionToGo;

                public MovePlayerByCSAction(int movementAmount, EntityPlayer player)
                {
                    this.movementX = movementAmount;
                    this.player = player;
                    this.init = false;
                    this.stuck = false;
                }

                public override bool IsFinished()
                {
                    if(stuck)
                    {
                        return true;
                    }
                    if (directionToGo == DirectionEnum.LEFT && player.GetAdjustedPosition().X < targetX)
                    {
                        return true;
                    }
                    else if (directionToGo == DirectionEnum.RIGHT && player.GetAdjustedPosition().X > targetX)
                    {
                        return true;
                    }
                    return false;
                }

                public override void Update(float deltaTime, Area area, World world, Camera camera)
                {
                    if(!init)
                    {
                        init = true;
                        targetX = (int)player.GetAdjustedPosition().X + movementX;
                        if (player.GetAdjustedPosition().X > targetX)
                        {
                            directionToGo = DirectionEnum.LEFT;
                        }
                        else
                        {
                            directionToGo = DirectionEnum.RIGHT;
                        }
                    }

                    Vector2 startingPos = player.GetAdjustedPosition();
                    MouseState realState = Mouse.GetState();
                    dummyController.Update(new MouseState(realState.X, realState.Y, realState.ScrollWheelValue, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released),
                        new KeyboardState(directionToGo == DirectionEnum.LEFT ? KeyBinds.LEFT : KeyBinds.RIGHT));
                    player.SetController(dummyController);
                    player.Update(deltaTime, area, true);
                    if (IsFinished())
                    {
                        player.SetToDefaultPose();
                        player.StopAllMovement();
                    }
                    Vector2 endingPos = player.GetAdjustedPosition();
                    if(startingPos == endingPos)
                    {
                        stuck = true;
                    }
                }
            }

            public class MovePlayerToCSAction : CSAction
            {
                private int targetX;
                private EntityPlayer player;
                private DirectionEnum directionToGo;
                private bool init, stuck;

                public MovePlayerToCSAction(int targetX, EntityPlayer player)
                {
                    this.targetX = targetX;
                    this.player = player;
                    this.init = false;
                    this.stuck = false;
                }

                public override void Update(float deltaTime, Area area, World world, Camera camera)
                {
                    if(!init)
                    {
                        init = true;
                        if(player.GetAdjustedPosition().X > targetX)
                        {
                            directionToGo = DirectionEnum.LEFT;
                        } else
                        {
                            directionToGo = DirectionEnum.RIGHT;
                        }
                    }

                    Vector2 startingPos = player.GetAdjustedPosition();
                    MouseState realState = Mouse.GetState();
                    dummyController.Update(new MouseState(realState.X, realState.Y, realState.ScrollWheelValue, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released),
                        new KeyboardState(directionToGo == DirectionEnum.LEFT ? KeyBinds.LEFT : KeyBinds.RIGHT));
                    player.SetController(dummyController);
                    player.Update(deltaTime, area, true);
                    if(IsFinished())
                    {
                        player.SetToDefaultPose();
                        player.StopAllMovement();
                    }
                    Vector2 endingPos = player.GetAdjustedPosition();
                    if (startingPos == endingPos)
                    {
                        stuck = true;
                    }
                }

                public override bool IsFinished()
                {
                    if(stuck)
                    {
                        return true;
                    }
                    if(directionToGo == DirectionEnum.LEFT && player.GetAdjustedPosition().X < targetX)
                    {
                        return true;
                    } else if (directionToGo == DirectionEnum.RIGHT && player.GetAdjustedPosition().X > targetX)
                    {
                        return true;
                    }
                    return false;
                }
            }

            public int currentAction;
            public List<CSAction> script;
            public bool isComplete;
            public string id;
            public Func<EntityPlayer, World, bool> activationConditionFunction;
            public Action<EntityPlayer, World, Camera> onActivationFunction; 

            public Cutscene(string id, Func<EntityPlayer, World, bool> activationConditionFunction, Action<EntityPlayer, World, Camera> onActivationFunction, params CSAction[] script)
            {
                this.id = id;
                this.script = new List<CSAction>();
                foreach(CSAction action in script)
                {
                    this.script.Add(action);
                }
                this.isComplete = false;
                this.onActivationFunction = onActivationFunction;
                this.currentAction = 0;
                this.activationConditionFunction = activationConditionFunction;
            }

            public void OnActivation(EntityPlayer player, World world, Camera camera)
            {
                this.onActivationFunction(player, world, camera);
            }

            public bool CheckActivationCondition(EntityPlayer player, World world)
            {
                if(isComplete)
                {
                    return false;
                }
                return activationConditionFunction(player, world);
            }

            public bool IsComplete()
            {
                return isComplete;
            }

            public void Update(float deltaTime, EntityPlayer player, Area currentArea, World world, Camera camera)
            {
                if (currentAction < script.Count)
                {
                    script[currentAction].Update(deltaTime, currentArea, world, camera);
                    if (script[currentAction].IsFinished())
                    {
                        currentAction++;
                    }
                } else
                {
                    isComplete = true;
                }
            }
        }

        private static Cutscene CUTSCENE_TEST;
        private static DummyController dummyController;

        private static List<Cutscene> CUTSCENES;

        public static void Initialize(EntityPlayer player, Camera camera)
        {
            dummyController = new DummyController();
            CUTSCENES = new List<Cutscene>();

            CUTSCENES.Add(CUTSCENE_TEST = new Cutscene("CUTSCENE_TEST",
                (entityPlayer, world) => { return true; },
                (entityPlayer, world, cam) => {
                    entityPlayer.SetGroundedPosition(new Vector2(920, 176));
                    cam.Update(0.0f, player.GetAdjustedPosition() + new Vector2(0, -30), world.GetCurrentArea().MapPixelWidth(), world.GetCurrentArea().MapPixelHeight());
                },
                new Cutscene.GroupedCSAction(new Cutscene.MovePlayerByCSAction(100, player), new Cutscene.PanCameraByCSAction(new Vector2(100, 0))),
                new Cutscene.WaitCSAction(1.0f),
                new Cutscene.DialogueCSAction(player, new DialogueNode("THIS IS A TEST DIALOGUE", DialogueNode.PORTRAIT_BAD)),
                new Cutscene.MovePlayerByCSAction(-50, player),
                new Cutscene.PanCameraByCSAction(new Vector2(-50, 0))));
        }

        public static Cutscene GetCutsceneById(string id)
        {
            foreach(Cutscene cutscene in CUTSCENES)
            {
                if(cutscene.id.Equals(id))
                {
                    return cutscene;
                }
            }
            throw new Exception("No cutscene found for id: " + id);
        }

        public static SaveState GenerateSave()
        {
            SaveState state = new SaveState(SaveState.Identifier.CUTSCENES);
            foreach(Cutscene cutscene in CUTSCENES)
            {
                state.AddData(cutscene.id, cutscene.isComplete.ToString());
            }
            return state;
        }

        public static void LoadSave(SaveState state)
        {
            foreach(Cutscene cutscene in CUTSCENES)
            {
                cutscene.isComplete = state.TryGetData(cutscene.id, false.ToString()) == true.ToString();
            }
        }
    }
}
