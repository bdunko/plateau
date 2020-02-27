﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Platfarm.Components;
using Platfarm.Items;
using Platfarm.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Entities
{
    public class EntityPlayer : EntityCollidable
    {
        public static int INVENTORY_SIZE = 50;

        public class FishlinePart
        {
            private static Texture2D LINE_TEXTURE;
            private static Texture2D HOOK_TEXTURE;
            private static Texture2D EXCLAIMATION_TEXTURE;

            public enum Type
            {
                LINE, HOOK, EXCLAIMATION
            }

            public static void LoadContent(ContentManager cm)
            {
                LINE_TEXTURE = cm.Load<Texture2D>(Paths.SPRITE_FISHLINE_LINE);
                HOOK_TEXTURE = cm.Load<Texture2D>(Paths.SPRITE_FISHLINE_HOOK);
                EXCLAIMATION_TEXTURE = cm.Load<Texture2D>(Paths.SPRITE_FISHLINE_EXCLAIMATION);
            }

            private Type type;
            private Vector2 position;
            private Texture2D tex;

            public bool IsExclaimation()
            {
                return this.type == Type.EXCLAIMATION;
            }

            public FishlinePart(Type type, Vector2 position)
            {
                this.type = type;
                this.position = position;
                switch (type)
                {
                    case Type.LINE:
                        tex = LINE_TEXTURE;
                        break;
                    case Type.HOOK:
                        tex = HOOK_TEXTURE;
                        break;
                    case Type.EXCLAIMATION:
                        tex = EXCLAIMATION_TEXTURE;
                        break;
                }
            }

            public Type GetType()
            {
                return this.type;
            }

            public void Draw(SpriteBatch sb, float layerDepth)
            {
                sb.Draw(tex, position, tex.Bounds, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, layerDepth);
            }
        }

        public class TimedEffect
        {
            public AppliedEffects.Effect effect;
            public float timeRemaining;

            public TimedEffect(AppliedEffects.Effect effect, float timeRemaining)
            {
                this.effect = effect;
                this.timeRemaining = timeRemaining;
            }

            public void Update(float deltaTime)
            {
                if (timeRemaining != AppliedEffects.LENGTH_INFINITE)
                {
                    this.timeRemaining -= deltaTime;
                }
            }

            public bool IsFinished()
            {
                return timeRemaining <= 0;
            }
        }

        private class CollisionData
        {
            public CollisionData(bool collide, bool fullBody, bool topBody)
            {
                this.didCollide = collide;
                this.didFullbodyCollide = fullBody;
                this.didTopBodyCollide = topBody;
            }

            public bool didCollide;
            public bool didFullbodyCollide;
            public bool didTopBodyCollide;
        }

        public class Notification {
            public enum Length
            {
                SHORT, LONG
            }

            private static float DISPLAY_TIME_SHORT = 3.0f;
            private static float DISPLAY_TIME_LONG = 6.0f;
            private float timeElapsed;
            private float displayTime;
            public Color textColor;
            public string message;
            private static Texture2D blackBox;
            private Length length;

            public Notification(string message, Color textColor, Length lengthEnum = Length.SHORT)
            {
                this.timeElapsed = 0;
                this.textColor = textColor;
                this.message = message;
                this.length = lengthEnum;
                ResetLength();
            }

            public void ResetLength()
            {
                switch (length)
                {
                    case Length.SHORT:
                        displayTime = DISPLAY_TIME_SHORT;
                        break;
                    case Length.LONG:
                        displayTime = DISPLAY_TIME_LONG;
                        break;
                }
            }

            public bool IsDone()
            {
                return timeElapsed >= displayTime;
            }

            public void Draw(SpriteBatch sb, Vector2 basePosition)
            {
                Vector2 adjustment = (Plateau.FONT.MeasureString(message)*Plateau.FONT_SCALE) * 0.5f;
                Vector2 fadeAdjustment = new Vector2(0, 0);
                float transparency = 1.0f;
                if (timeElapsed >= displayTime * 0.9f)
                {
                    transparency = 1- ((timeElapsed - (displayTime * 0.9f)) / (displayTime * 0.1f));
                    fadeAdjustment.Y = -12 * (1-transparency);
                }
                sb.DrawString(Plateau.FONT, message, basePosition - adjustment + fadeAdjustment, textColor * transparency, 0.0f, Vector2.Zero, Plateau.FONT_SCALE, SpriteEffects.None, 0.0f);
            }

            public void Update(float deltaTime)
            {
                timeElapsed += deltaTime;
            }
        }

        public enum GravityState
        {
            NORMAL, REVERSED
        }

        private World world;
        private Controller controller;

        private Queue<Notification> notifications;
        
        private static float GRAVITY = 8;
 
        private static int HEIGHT = 24; //height of character's hitbox
        private static int HEIGHT_ROLL = 8;
        private static int WIDTH = 6; //width of character's hitbox
        private static int OFFSET_X = 29; //offset x from where the image is to where the hitbox begins
        private static int OFFSET_Y = 8; //offset y from where the image border is to where the hitbox begins
        private static int TARGET_OFFSET_LEFT = -5;
        private static int TARGET_OFFSET_RIGHT = 12;
        private static int COLLISION_STEPS = 6; //number of collision steps; higher = more performance intensive, but more accurate
        private static int WATER_COLLISION_OFFSET_Y = -1;

        private float elapsedSinceNotGrounded = 0;
        private float elapsedSinceWall = 0;
        private float elapsedSinceGroundPoundLanding = 0;

        private RectangleF collisionRectangle;
        private ClothedSprite sprite;
        private Vector2 position;
        private Vector2 targetTile;
        private Entity targetEntity;
        private Entity targetTileEntity;

        private bool interactThisFrame;

        private ItemStack[] inventory;
        private List<Item> itemsCollectedRecently;
        private int selectedHotbarPosition;

        private static float SPEED_EFFECT_BOOST_PER_LEVEL = 12.5f;
        private static float MAX_SPEED_X_WALK = 50;
        private static float MAX_SPEED_X_ROLL = 150;

        private static float WALL_JUMP_X_VELOCITY = 4;
        
        private static float X_FRICTION_GROUND = 15F;
        private static float X_FRICTION_AIR = 10F;
        private static float X_FRICTION_GROUND_ROLL = 6F;
        private static float X_FRICTION_AIR_ROLL = 2F;

        private static float ROLL_BOUNCE_MULTIPLIER = 0.64F; //increasing increases the size of bounces when landing while rolling
        private static float ROLL_MINIMUM_BOUNCE = 2.6F; //the minimum velocity required to do a bounce

        private static float SPEED_X = 30; //movement speed in the x axis
        private static float JUMP_INSTANT_VELOCITY = -2.55F;
        private static float GROUND_POUND_JUMP_INSTANT_VELOCITY = -3.4F;
        public static float GLIDE_CONSTANT_VELOCITY = 0.3F;
        private static int GRAB_HITBOXES_FROM_TOP = 1;
        private static int GRAB_HITBOXES_FROM_TOP_REVERSE = 2;
        private static float GROUND_POUND_GRAVITY_MULTIPLIER = 4f;
        private static float GROUND_POUND_LANDING_LOCK_LENGTH = 0.30f;
        private static float SWIMMING_VELOCITY_Y = -GRAVITY * 2f;
        private static float MAXIMUM_SWIMMING_VELOCITY_Y = -1.5f;

        private static float WALKOFF_LEDGE_JUMP_ALLOWANCE = 0.15F; //time where you can be not grounded but still input a jump after walking off a ledge
        private static float WALL_GRAB_CLING_ALLOWANCE = 0.2F; //time where you can have let go of wall but still input a jump
        private static float WALL_GRAB_WALL_JUMP_ALLOWANCE = 0.1F; //time where you can have jumped off of wall but can still input a walljump

        private float inputVelocityX = 0;
        private float velocityY = 0;
        private float externalVelocityX = 0;

        private DirectionEnum direction = DirectionEnum.RIGHT; //direction facing
        private bool grounded = false; //if player is touching ground
        private bool wall_cling = false; //full body clinging to wall
        private bool wall_grab = false; //only hand clinging to wall
        private bool gliding = false; //if player is gliding
        private bool overwriteGlideThisFrame = false;
        private bool rolling = false; //if player is rolling
        private bool canStandFromRoll = true;
        private bool skipGravityThisFrame = false;
        private bool groundPound = false; //if player is groundpounding
        private bool groundPoundLock = false; //if the player is stalled on the ground after landing while groundpounding
        private bool useTool = false; //if the player is using a tool
        private bool attemptToolHit = false;
        private bool paused = false; //if player is paused (generally means in inventory)
        private bool attemptTransition = false; //if player is attempting to transition areas
        private bool ignoreMouseThisFrame = false;
        private bool swimming = false; //if player is currently in water
        private bool jumpFromSwim = false;
        private InterfaceState interfaceState = InterfaceState.NONE;

        private ItemStack hair, skin, eyes;
        private ItemStack hat, shirt, outerwear, pants, socks, shoes, gloves, earrings, scarf, glasses, back, sailcloth;
        private ItemStack accessory1, accessory2, accessory3;

        private string leftClickAction, rightClickAction, leftShiftClickAction, rightShiftClickAction;

        private DialogueNode currentDialogue = null;

        private int gold;

        private List<TimedEffect> effects;
        private GravityState gravityState;

        private List<FishlinePart> fishlineParts;
        private float fishingDamage;
        private Area.FishingZone currentFishingPool;

        public EntityPlayer(World world, Controller controller)
        {
            this.interactThisFrame = false;
            this.gravityState = GravityState.NORMAL;
            this.sprite = new ClothedSprite();
            this.leftClickAction = "";
            this.rightClickAction = "";
            this.leftShiftClickAction = "";
            this.rightShiftClickAction = "";
            this.gold = 3000;
            this.notifications = new Queue<Notification>();
            this.fishlineParts = new List<FishlinePart>();
            this.fishingDamage = 0;
            this.currentFishingPool = null;
            this.position = new Vector2(100, 50);
            collisionRectangle = new RectangleF(position.X + OFFSET_X, position.Y + OFFSET_Y, WIDTH, HEIGHT);
            this.world = world;
            this.controller = controller;
            this.attemptToolHit = false;

            inventory = new ItemStack[INVENTORY_SIZE];
            for(int i = 0; i < inventory.Length; i++)
            {
                inventory[i] = new ItemStack(ItemDict.NONE, 0);
            }
            inventory[0] = new ItemStack(ItemDict.HOE, 1);
            inventory[1] = new ItemStack(ItemDict.WATERING_CAN, 1);
            inventory[2] = new ItemStack(ItemDict.AXE, 1);
            inventory[3] = new ItemStack(ItemDict.PICKAXE, 1);
            inventory[4] = new ItemStack(ItemDict.FISHING_ROD, 1);

            hat = new ItemStack(ItemDict.CLOTHING_NONE, 0);
            shirt = new ItemStack(ItemDict.SHORT_SLEEVE_TEE, 1);
            outerwear = new ItemStack(ItemDict.ALL_SEASON_JACKET, 1);
            pants = new ItemStack(ItemDict.JEANS, 1);
            socks = new ItemStack(ItemDict.SHORT_SOCKS, 1);
            shoes = new ItemStack(ItemDict.SNEAKERS, 1);
            gloves = new ItemStack(ItemDict.CLOTHING_NONE, 0);
            earrings = new ItemStack(ItemDict.CLOTHING_NONE, 0);
            scarf = new ItemStack(ItemDict.CLOTHING_NONE, 0);
            glasses = new ItemStack(ItemDict.CLOTHING_NONE, 0);
            back = new ItemStack(ItemDict.RUCKSACK, 1);
            sailcloth = new ItemStack(ItemDict.SAILCLOTH, 1);
            accessory1 = new ItemStack(ItemDict.CLOTHING_NONE, 0);
            accessory2 = new ItemStack(ItemDict.CLOTHING_NONE, 0);
            accessory3 = new ItemStack(ItemDict.CLOTHING_NONE, 0);
            hair = new ItemStack(ItemDict.GetColoredItem(ItemDict.HAIR_FREDDIE_FRINGE, Util.HAIR_DUST_BLOND), 1);
            skin = new ItemStack(ItemDict.SKIN_PEACH, 1);
            eyes = new ItemStack(ItemDict.EYES_DOT, 1);
            selectedHotbarPosition = 0;

            itemsCollectedRecently = new List<Item>();

            effects = new List<TimedEffect>();
        }

        public List<TimedEffect> GetEffects()
        {
            return this.effects;
        }

        public void ApplyEffect(AppliedEffects.Effect toApply, float length)
        {
            bool found = false;
            foreach(TimedEffect effect in effects)
            {
                if(effect.effect == toApply)
                {
                    found = true;
                    if(effect.timeRemaining < length)
                    {
                        effect.timeRemaining = length;
                    }
                }
            }

            if (!found)
            {
                effects.Add(new TimedEffect(toApply, length));
            }
        }

        public bool DidInteractThisFrame()
        {
            return interactThisFrame;
        }

        public void ClearEffects()
        {
            effects.Clear();
        }

        public void RemoveEffect(AppliedEffects.Effect toRemove)
        {
            for(int i = 0; i < effects.Count; i++)
            {
                TimedEffect effect = effects[i];
                if(toRemove == effect.effect)
                {
                    effects.Remove(effect);
                }
            }
        }

        public bool HasEffect(AppliedEffects.Effect toCheck)
        {
            foreach(TimedEffect effect in effects)
            {
                if(toCheck == effect.effect)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckSwimmingCollision(Area area)
        {
            RectangleF pHitbox = GetCollisionRectangle();
            int waterCheckTileX = (int)(pHitbox.Center.X / 8);
            int waterCheckTileY = (int)((pHitbox.Center.Y + WATER_COLLISION_OFFSET_Y) / 8);
            return area.GetCollisionTypeAt(waterCheckTileX, waterCheckTileY) == Area.CollisionTypeEnum.WATER;
        }

        public int GetGold()
        {
            return gold;
        }

        public void SpendGold(int amount)
        {
            gold -= amount;
        }

        public void SetController(Controller controller)
        {
            this.controller = controller;
        }

        public void GainGold(int amount)
        {
            gold += amount;
        }

        public Vector2 GetTileStandingOn()
        {
            Vector2 targettedTile = GetTargettedTile();
            if(direction == DirectionEnum.LEFT)
            {
                return targettedTile + new Vector2(1, gravityState == GravityState.REVERSED ? -1 : 1);
            } else
            {
                return targettedTile + new Vector2(-1, gravityState == GravityState.REVERSED ? -1 : 1);
            }
        }

        public string GetLeftShiftClickAction()
        {
            return leftShiftClickAction;
        }

        public string GetRightShiftClickAction()
        {
            return rightShiftClickAction;
        }

        public Vector2 GetTargettedTile()
        {
            return targetTile;
        }

        public string GetLeftClickAction()
        {
            return leftClickAction;
        }

        public bool IsSwimming()
        {
            return swimming;
        }

        public string GetRightClickAction()
        {
            return rightClickAction;
        }

        public void SetInterfaceState(InterfaceState state)
        {
            interfaceState = state;
        }

        public void IgnoreMouseInputThisFrame()
        {
            ignoreMouseThisFrame = true;
        }

        public InterfaceState GetInterfaceState()
        {
            return interfaceState;
        }

        public ItemStack GetEyes()
        {
            return eyes;
        }

        public void SetEyes(ItemStack newEyes)
        {
            eyes = newEyes;
        }

        public List<Item> GetItemsCollectedRecently()
        {
            return itemsCollectedRecently;
        }

        public bool GetUseTool()
        {
            return useTool;
        }

        public ItemStack GetAccessory1()
        {
            return accessory1;
        }

        public ItemStack GetAccessory2()
        {
            return accessory2;
        }

        public ItemStack GetAccessory3()
        {
            return accessory3;
        }

        public bool GetInventoryItemStackable(int i)
        {
            return inventory[i].GetMaxQuantity() > 1;
        }

        public ItemStack GetInventoryItemStack(int i)
        {
            return inventory[i];
        } 

        public Texture2D GetInventoryItemTexture(int i)
        {
            return inventory[i].GetItem().GetTexture();
        }

        public int GetInventoryItemQuantity(int i)
        {
            return inventory[i].GetQuantity();
        }

        public int GetSelectedHotbarPosition()
        {
            return selectedHotbarPosition;
        }

        public ItemStack GetHeldItem()
        {
            return inventory[selectedHotbarPosition];
        }

        public Vector2 GetCenteredPosition()
        {
            return GetAdjustedPosition() + new Vector2(0.5f * WIDTH, 0.5f * HEIGHT);
        }

        public Vector2 GetAdjustedPosition()
        {
            if(gravityState == GravityState.REVERSED)
            {
                return new Vector2(this.position.X + OFFSET_X, this.position.Y);
            }
            return new Vector2(this.position.X + OFFSET_X, this.position.Y + OFFSET_Y);
        }

        public void SetCurrentDialogue(DialogueNode dialogue)
        {
            this.currentDialogue = dialogue;
        }

        public DialogueNode GetCurrentDialogue()
        {
            return currentDialogue;
        }

        public void ClearDialogueNode()
        {
            this.currentDialogue = null;
        }

        public bool IsGrounded()
        {
            return grounded;
        }

        public bool IsWallCling()
        {
            return wall_cling;
        }

        public bool IsWallGrab()
        {
            return wall_grab;
        }

        public bool IsGliding()
        {
            return gliding;
        }

        public bool IsJumping()
        {
            if (gravityState == GravityState.NORMAL)
            {
                return velocityY < 0;
            } else if (gravityState == GravityState.REVERSED)
            {
                return velocityY > 0;
            }
            return false;
        }

        public bool IsRolling()
        {
            return rolling;
        }

        public DirectionEnum GetDirection()
        {
            return direction;
        }

        public bool IsGroundPound()
        {
            return groundPound;
        }

        public bool CanStandFromRoll()
        {
            return canStandFromRoll;
        }

        public bool IsFalling()
        {
            return velocityY > 0;
        }

        public bool IsGroundPoundLock()
        {
            return groundPoundLock;
        }

        public void Pause()
        {
            paused = true;
        }
        
        public void Unpause()
        {
            paused = false;
        }

        public void RemoveItemStackAt(int inventorySlot)
        {
            inventory[inventorySlot] = new ItemStack(ItemDict.NONE, 0);
        }

        public void AddItemStackAt(ItemStack itemStack, int inventorySlot)
        {
            inventory[inventorySlot] = itemStack;
        }

        public bool AddItemToInventory(Item item, bool notification=true, bool hotbarFirst = true, bool recall = false)
        {
            int startIndex = hotbarFirst ? 0 : 10;
            for(int i = startIndex; i < inventory.Length; i++)
            {
                if(!inventory[i].IsFull() && inventory[i].GetItem() == item)
                {
                    inventory[i].Add(1);
                    if (notification)
                    {
                        itemsCollectedRecently.Add(item);
                    }
                    return true;
                }
            }
            for (int i = startIndex; i < inventory.Length; i++)
            {
                if (inventory[i].GetItem() == ItemDict.NONE)
                {
                    inventory[i] = new ItemStack(item, 1);
                    if (notification)
                    {
                        itemsCollectedRecently.Add(item);
                    }
                    return true;
                }
            }
            if(!hotbarFirst && !recall)
            {
                AddItemToInventory(item, notification, false, true);
            }
            return false;
        }

        public void CancelGroundPound()
        {
            groundPound = false;
            groundPoundLock = false;
        }

        public void CancelGlide()
        {
            gliding = false;
        }

        public bool RemoveItemFromInventory(Item item)
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].GetItem() == item)
                {
                    inventory[i].Subtract(1);
                    if(inventory[i].GetQuantity() == 0)
                    {
                        inventory[i] = new ItemStack(ItemDict.NONE, 0);
                    }
                    return true;
                }
            }
            return false;
        }

        public bool RemoveItemStackFromInventory(ItemStack stack)
        {
            bool result = true;
            for(int i = 0; i < stack.GetQuantity(); i++)
            {
                result = RemoveItemFromInventory(stack.GetItem());
                if(!result)
                {
                    break;
                }
            }
            return result;
        }

        public int GetNumberOfItemInInventory(Item item)
        {
            int nFound = 0;
            for(int i = 0; i < inventory.Length; i++)
            {
                if(inventory[i].GetItem() == item)
                {
                    nFound += inventory[i].GetQuantity();
                }
            }
            return nFound;
        }

        public bool HasItemStack(ItemStack stack)
        {
            int numFound = 0;
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].GetItem() == stack.GetItem())
                {
                    numFound += inventory[i].GetQuantity();
                    if(numFound >= stack.GetQuantity())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void ToggleAttemptTransition()
        {
            attemptTransition = !attemptTransition;
        }

        public bool AttemptingTransition()
        {
            return attemptTransition;
        }

        public void TryWallJump(float deltaTime, Controller controller)
        {
            if (IsJumping())
            {
                if (controller.IsKeyDown(KeyBinds.LEFT) && direction == DirectionEnum.RIGHT)
                {
                    externalVelocityX += -WALL_JUMP_X_VELOCITY;
                    direction = DirectionEnum.LEFT;
                    inputVelocityX = 0;
                }
                else if (controller.IsKeyDown(KeyBinds.RIGHT) && direction == DirectionEnum.LEFT)
                {
                    externalVelocityX += WALL_JUMP_X_VELOCITY;
                    direction = DirectionEnum.RIGHT;
                    inputVelocityX = 0;
                }
            }
        }

        public override RectangleF GetCollisionRectangle()
        {
            return collisionRectangle;
        }

        private CollisionData CheckCollision(float posX, float posY, Area area, bool useRollingHitbox, bool updateCollisionRect = true)
        {
            bool fullBodyCollisionR = true;
            bool fullBodyCollisionL = true;
            bool topBodyCollision = false;
            bool collision = false;
            RectangleF tempCollisionRectangle = new RectangleF(collisionRectangle.Position, collisionRectangle.Size);

            if (useRollingHitbox)
            {
                if (gravityState == GravityState.NORMAL)
                {
                    tempCollisionRectangle = new RectangleF(posX + OFFSET_X, posY + OFFSET_Y + 1 + (HEIGHT - HEIGHT_ROLL), WIDTH, HEIGHT_ROLL - 1);
                } else if (gravityState == GravityState.REVERSED)
                {
                    tempCollisionRectangle = new RectangleF(posX + OFFSET_X, posY, WIDTH, HEIGHT_ROLL - 1);
                }
            }
            else
            {
                if (gravityState == GravityState.NORMAL)
                {
                    tempCollisionRectangle = new RectangleF(posX + OFFSET_X, posY + OFFSET_Y + 1, WIDTH, HEIGHT - 1);
                } else if (gravityState == GravityState.REVERSED)
                {
                    tempCollisionRectangle = new RectangleF(posX + OFFSET_X, posY, WIDTH, HEIGHT - 1);
                }
            }


            //indexes for each corner of hitbox...
            int indexXLeft = (int)(tempCollisionRectangle.X / 8);
            int indexYTop = (int)(tempCollisionRectangle.Y / 8);
            int indexXRight = (int)((tempCollisionRectangle.X + tempCollisionRectangle.Width) / 8);
            int indexYBottom = (int)((tempCollisionRectangle.Y + tempCollisionRectangle.Height) / 8);

            for (int indexYCurrent = indexYTop; indexYCurrent <= indexYBottom; indexYCurrent++)
            {
                Area.CollisionTypeEnum cTypeL = area.GetCollisionTypeAt(indexXLeft, indexYCurrent);
                if (cTypeL == Area.CollisionTypeEnum.SOLID || cTypeL == Area.CollisionTypeEnum.SCAFFOLDING_BLOCK)
                {
                    collision = true;
                    if (indexYCurrent == indexYTop + (gravityState == GravityState.REVERSED ? GRAB_HITBOXES_FROM_TOP_REVERSE : GRAB_HITBOXES_FROM_TOP) && (cTypeL == Area.CollisionTypeEnum.SOLID || cTypeL == Area.CollisionTypeEnum.SCAFFOLDING_BLOCK))
                    {
                        topBodyCollision = true;
                    }
                } else if (cTypeL == Area.CollisionTypeEnum.BRIDGE || cTypeL == Area.CollisionTypeEnum.SCAFFOLDING_BRIDGE)
                {
                    if (!IsJumping() && position.Y + HEIGHT + OFFSET_Y < area.GetPositionOfTile(indexXLeft, indexYCurrent).Y)
                    {
                        collision = true;
                    }
                } else 
                {
                    fullBodyCollisionL = false;
                }

                Area.CollisionTypeEnum cTypeR = area.GetCollisionTypeAt(indexXRight, indexYCurrent);
                if (cTypeR== Area.CollisionTypeEnum.SOLID || cTypeR == Area.CollisionTypeEnum.SCAFFOLDING_BLOCK)
                {
                    collision = true;
                    if (indexYCurrent == indexYTop + (gravityState == GravityState.REVERSED ? GRAB_HITBOXES_FROM_TOP_REVERSE : GRAB_HITBOXES_FROM_TOP) && (cTypeR == Area.CollisionTypeEnum.SOLID || cTypeR == Area.CollisionTypeEnum.SCAFFOLDING_BLOCK))
                    {
                        topBodyCollision = true;
                    }
                } else if (cTypeR == Area.CollisionTypeEnum.BRIDGE || cTypeR == Area.CollisionTypeEnum.SCAFFOLDING_BRIDGE)
                {
                    if (!IsJumping() && position.Y+HEIGHT + OFFSET_Y < area.GetPositionOfTile(indexXRight, indexYCurrent).Y) {
                        collision = true;
                    }
                } else 
                {
                    fullBodyCollisionR = false;
                }
            }

            //EntitySolids
            List<MEntitySolid> solids = area.GetSolidEntities();
            foreach(MEntitySolid solid in solids)
            {
                RectangleF solidHitbox = solid.GetCollisionRectangle();
                if(solidHitbox.Intersects(tempCollisionRectangle) && solid.GetPlatformType() != MEntitySolid.PlatformType.AIR)
                {
                    if (solid.GetPlatformType() == MEntitySolid.PlatformType.BRIDGE)
                    {
                        if (!IsJumping() && position.Y + HEIGHT + OFFSET_Y < solidHitbox.Top)
                        {
                            collision = true;
                        }
                    }
                    else if (solid.GetPlatformType() == MEntitySolid.PlatformType.SOLID)
                    {
                        collision = true;
                    }

                    if (((MEntitySolid)solid).IsGrabbable())
                    {
                        float grabY = (indexYTop + (gravityState == GravityState.REVERSED ? GRAB_HITBOXES_FROM_TOP_REVERSE : GRAB_HITBOXES_FROM_TOP)) * 8;

                        //check left grab
                        if (solidHitbox.Contains(new Vector2(tempCollisionRectangle.Left, grabY)))
                        {
                            topBodyCollision = true;
                        }

                        //check right grab
                        if (solidHitbox.Contains(new Vector2(tempCollisionRectangle.Right, grabY)))
                        {
                            topBodyCollision = true;
                        }
                    }
                }
            }

            if(updateCollisionRect)
            {
                collisionRectangle = tempCollisionRectangle;
            }

            return new CollisionData(collision, fullBodyCollisionL || fullBodyCollisionR, topBodyCollision);
        }

        public Entity GetTargettedTileEntity()
        {
            return targetTileEntity;
        }

        public Entity GetTargettedEntity()
        {
            return targetEntity;
        }

        public ItemStack GetHair()
        {
            return hair;
        }

        public Vector2 GetVelocity()
        {
            return new Vector2(inputVelocityX + externalVelocityX, velocityY);
        }

        private void Jump(float jumpModifier, Area area)
        {
            if (groundPoundLock)
            {
                if (gravityState == GravityState.NORMAL)
                {
                    velocityY = GROUND_POUND_JUMP_INSTANT_VELOCITY * jumpModifier;
                } else if (gravityState == GravityState.REVERSED)
                {
                    velocityY = -GROUND_POUND_JUMP_INSTANT_VELOCITY * jumpModifier;
                }

                for (int i = 0; i < 6; i++)
                {
                    area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(Util.RandInt(0, WIDTH), gravityState == GravityState.REVERSED ? 0 : HEIGHT),
                        gravityState == GravityState.REVERSED ? ParticleBehavior.RUSH_UPWARD_STRONG_REVERSED : ParticleBehavior.RUSH_UPWARD_STRONG, ParticleTextureStyle.CHUNK, Color.White, ParticleFactory.DURATION_SHORT));
                }
                for (int i = 0; i < 2; i++)
                {
                    area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(Util.RandInt(0, WIDTH), gravityState == GravityState.REVERSED ? 0 : HEIGHT),
                        gravityState == GravityState.REVERSED ? ParticleBehavior.RUSH_UPWARD_STRONG_REVERSED : ParticleBehavior.RUSH_UPWARD_STRONG, ParticleTextureStyle.CHUNK, Util.PARTICLE_BLUE_RISER.color, ParticleFactory.DURATION_SHORT));
                }

                groundPoundLock = false;
            }
            else
            {
                if (!swimming)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(Util.RandInt(0, WIDTH), gravityState == GravityState.REVERSED ? 0 : HEIGHT),
                            gravityState == GravityState.REVERSED ? ParticleBehavior.RUSH_UPWARD_REVERSED : ParticleBehavior.RUSH_UPWARD, ParticleTextureStyle.SMALL, Color.White, ParticleFactory.DURATION_SHORT));
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(Util.RandInt(0, WIDTH), gravityState == GravityState.REVERSED ? 0 : HEIGHT    ),
                            gravityState == GravityState.REVERSED ? ParticleBehavior.RUSH_UPWARD_REVERSED : ParticleBehavior.RUSH_UPWARD, ParticleTextureStyle.SMALL, Util.PARTICLE_BLUE_RISER.color, ParticleFactory.DURATION_SHORT));
                    }
                }
                if (gravityState == GravityState.NORMAL)
                {
                    velocityY = JUMP_INSTANT_VELOCITY * jumpModifier;
                } else if (gravityState == GravityState.REVERSED)
                {
                    velocityY = -JUMP_INSTANT_VELOCITY * jumpModifier;
                }
            }
            
        }

        public ItemStack GetSailcloth()
        {
            return sailcloth;
        }

        public ItemStack GetBack()
        {
            return back;
        }

        public ItemStack GetGlasses()
        {
            return glasses;
        }

        public ItemStack GetScarf()
        {
            return scarf;
        }

        public ItemStack GetEarrings()
        {
            return earrings;
        }

        public ItemStack GetGloves()
        {
            return gloves;
        }

        public ItemStack GetShoes()
        {
            return shoes;
        }

        public ItemStack GetSocks()
        {
            return socks;
        }

        public ItemStack GetPants()
        {
            return pants;
        }

        public ItemStack GetOuterwear()
        {
            return outerwear;
        }

        public ItemStack GetShirt()
        {
            return shirt;
        }

        public ItemStack GetHat()
        {
            return hat;
        }

        public void SetSailcloth(ItemStack newSailcloth)
        {
            sailcloth = newSailcloth;
        }

        public void SetBack(ItemStack newBack)
        {
            back = newBack;
        }

        public void SetGlasses(ItemStack newGlasses)
        {
            glasses = newGlasses;
        }

        public void SetScarf(ItemStack newScarf)
        {
            scarf = newScarf;
        }

        public void SetEarrings(ItemStack newEarrings)
        {
            earrings = newEarrings;
        }

        public void SetGloves(ItemStack newGloves)
        {
            gloves = newGloves;
        }

        public void SetShoes(ItemStack newShoes)
        {
            shoes = newShoes;
        }

        public void SetSocks(ItemStack newSocks)
        {
            socks = newSocks;
        }

        public void SetPants(ItemStack newPants)
        {
            pants = newPants;
        }

        public void SetOuterwear(ItemStack newOuterwear)
        {
            outerwear = newOuterwear;
        }

        public void SetShirt(ItemStack newShirt)
        {
            shirt = newShirt;
        }

        public void SetHair(ItemStack newHair)
        {
            this.hair = newHair;
        }

        public void SetSkin(ItemStack newSkin)
        {
            this.skin = newSkin;
        }

        public void SetHat(ItemStack newHat)
        {
            hat = newHat;
        }

        public void SetAccessory1(ItemStack newAccessory)
        {
            accessory1 = newAccessory;
        }

        public void SetAccessory2(ItemStack newAccessory)
        {
            accessory2 = newAccessory;
        }

        public void SetAccessory3(ItemStack newAccessory)
        {
            accessory3 = newAccessory;
        }

        public bool IsPaused()
        {
            return paused;
        }

        public ClothedSprite GetSprite()
        {
            return sprite;
        }

        private void Jump(Area area)
        {
            Jump(1f, area);
        }

        public void StopAllMovement()
        {
            externalVelocityX = 0;
            inputVelocityX = 0;
            velocityY = 0;
        }

        public override void Update(float deltaTime, Area area)
        {
            Update(deltaTime, area, false);
        }

        public void Update(float deltaTime, Area area, bool cutscene)
        {
            if (notifications.Count != 0)
            {
                notifications.Peek().Update(deltaTime);
                if (notifications.Peek().IsDone())
                {
                    notifications.Dequeue();
                }
            }

            leftClickAction = "";
            rightClickAction = "";
            rightShiftClickAction = "";
            leftShiftClickAction = "";

            bool leftMousePress = controller.GetMouseLeftPress();
            bool leftMouseDown = controller.GetMouseLeftDown();
            bool rightMousePress = controller.GetMouseRightPress();
            bool rightMouseDown = controller.GetMouseRightDown();

            interactThisFrame = false;

            targetTile = new Vector2((int)((position.X + OFFSET_X + (direction == DirectionEnum.LEFT ? TARGET_OFFSET_LEFT : TARGET_OFFSET_RIGHT)) / 8), (int)((position.Y + OFFSET_Y + 24) / 8));
            if(gravityState == GravityState.REVERSED)
            {
                targetTile.Y -= 4;
            }

            if (!cutscene)
            {
                List<TimedEffect> toRemove = new List<TimedEffect>();

                foreach (TimedEffect effect in effects)
                {
                    effect.Update(deltaTime);
                    if (effect.IsFinished())
                    {
                        toRemove.Add(effect);
                    }
                }

                foreach (TimedEffect effect in toRemove)
                {
                    RemoveEffect(effect.effect);
                }
            }

            if (!paused && currentDialogue == null)
            {
                if (ignoreMouseThisFrame)
                {
                    ignoreMouseThisFrame = false;
                    leftMousePress = false;
                    leftMouseDown = false;
                    rightMousePress = false;
                    rightMouseDown = false;
                }

                targetEntity = null;
                targetTileEntity = null;

                if (!rolling)
                {
                    targetEntity = area.GetInteractableEntityAt(targetTile * 8);
                    //when targetting solid ground, aim one above
                    if(area.GetCollisionTypeAt((int)targetTile.X, (int)targetTile.Y) == Area.CollisionTypeEnum.SOLID  &&
                        area.GetCollisionTypeAt((int)targetTile.X, (int)targetTile.Y-1) != Area.CollisionTypeEnum.SOLID)
                    {
                        targetTile.Y--;
                    }
                    targetTileEntity = area.GetTileEntity((int)targetTile.X, (int)targetTile.Y);
                }

                if (targetEntity == null && targetTileEntity == null)
                {
                    leftClickAction = "";
                    leftShiftClickAction = "";
                    rightClickAction = "";
                    rightShiftClickAction = "";
                }
                else if(targetEntity != null && !(targetEntity is TEntityGrass))
                {
                    if (targetEntity is IInteract)
                    {
                        leftClickAction = ((IInteract)targetEntity).GetLeftClickAction(this);
                        rightClickAction = ((IInteract)targetEntity).GetRightClickAction(this);
                        leftShiftClickAction = ((IInteract)targetEntity).GetLeftShiftClickAction(this);
                        rightShiftClickAction = ((IInteract)targetEntity).GetRightShiftClickAction(this);
                    }
                } else
                {
                    if(targetTileEntity is IInteract)
                    {
                        leftClickAction = ((IInteract)targetTileEntity).GetLeftClickAction(this);
                        rightClickAction = ((IInteract)targetTileEntity).GetRightClickAction(this);
                        leftShiftClickAction = ((IInteract)targetTileEntity).GetLeftShiftClickAction(this);
                        rightShiftClickAction = ((IInteract)targetTileEntity).GetRightShiftClickAction(this);
                    }
                }
               
                if(GetHeldItem().GetItem() is PlantableItem && grounded && !groundPoundLock && !rolling)
                {
                    leftClickAction = "Plant";
                } else if (GetHeldItem().GetItem() is EdibleItem && grounded && !groundPoundLock && !rolling) {
                    leftClickAction = "Eat";
                }
                //usable item
                if (GetHeldItem().GetItem().HasTag(Item.Tag.TOOL) && grounded && !rolling && !groundPoundLock && !controller.IsKeyDown(KeyBinds.SHIFT)) { //using a tool
                    if(GetHeldItem().GetItem() == ItemDict.HOE)
                    {
                        leftClickAction = "Hoe";
                    } else if (GetHeldItem().GetItem() == ItemDict.WATERING_CAN)
                    {
                        leftClickAction = "Water";
                    }
                    else if (GetHeldItem().GetItem() == ItemDict.FISHING_ROD)
                    {
                        if(sprite.IsCurrentLoopFinished() && grounded && !rolling)
                        {
                            leftClickAction = "Reel";
                        } else {
                            leftClickAction = "Cast";
                        }
                    } else if (GetHeldItem().GetItem() == ItemDict.AXE)
                    {
                        leftClickAction = "Chop";
                    } else if (GetHeldItem().GetItem() == ItemDict.PICKAXE)
                    {
                        leftClickAction = "Smash";
                    }

                    if (!leftMouseDown && GetHeldItem().GetItem() == ItemDict.FISHING_ROD && sprite.IsCurrentLoopFinished())
                    {
                        if(currentFishingPool != null && fishingDamage > currentFishingPool.difficulty)
                        {
                            Vector2 originSpot = new Vector2(position.X + (direction == DirectionEnum.LEFT ? 10 : WIDTH + 30), position.Y + 15);
                            List<Item> drops = currentFishingPool.lootTable.RollLoot(this, area, world.GetTimeData());
                            foreach (Item drop in drops)
                            {
                                area.AddEntity(new EntityItem(drop, originSpot, new Vector2((direction==DirectionEnum.LEFT?1:-1) * Util.RandInt(108, 125)/100.0f, -3.5f)));
                            }
                            fishlineParts.Clear();
                            for (int i = 0; i < 12; i++)
                            {
                                area.AddParticle(ParticleFactory.GenerateParticle(originSpot + new Vector2(direction==DirectionEnum.LEFT?10:8, 16), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                                    Util.PARTICLE_WATER_PRIMARY.color, ParticleFactory.DURATION_SHORT));
                                area.AddParticle(ParticleFactory.GenerateParticle(originSpot + new Vector2(direction == DirectionEnum.LEFT?10:8, 16), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                                    Util.PARTICLE_WATER_SECONDARY.color, ParticleFactory.DURATION_SHORT));
                            }
                        }
                        fishingDamage = 0;
                        useTool = false;
                        attemptToolHit = false;
                        currentFishingPool = null;
                        fishlineParts.Clear();
                    }

                    if(currentFishingPool != null)
                    {
                        if (GetHeldItem().GetItem().HasTag(Item.Tag.FISHING_ROD)) {
                            fishingDamage += ((DamageDealingItem)GetHeldItem().GetItem()).GetDamage() * deltaTime;
                        } else
                        {
                            fishingDamage = 0;
                            currentFishingPool = null;
                        }
                    }

                    if (leftMouseDown && grounded && !rolling && !useTool && !groundPoundLock && !controller.IsKeyDown(KeyBinds.SHIFT))
                    {
                        attemptToolHit = false;
                        inputVelocityX = 0;
                        useTool = true;
                        if (GetHeldItem().GetItem() == ItemDict.HOE)
                        {
                            bool isLocationValid = area.IsFarmablePlacementValid((int)targetTile.X, (int)(targetTile.Y - 1));
                            Vector2 placementLocation = new Vector2(targetTile.X * 8, (targetTile.Y - 1) * 8);
                            if (isLocationValid)
                            {
                                TileEntity toPlace = (TileEntity)EntityFactory.GetEntity(EntityType.FARMABLE, ItemDict.NONE, new Vector2((int)targetTile.X, (int)(targetTile.Y - 1)), area);
                                area.AddTileEntity(toPlace);
                                if (Util.RandInt(0, 2) == 0)
                                {
                                    area.AddEntity(new EntityItem(ItemDict.CLAY, new Vector2(placementLocation.X, placementLocation.Y)));
                                }

                                Vector2 currentTile = GetTileStandingOn();
                                for (int i = 0; i < 5; i++)
                                {
                                    area.AddParticle(ParticleFactory.GenerateParticle(placementLocation + new Vector2(0, 14), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.SMALL,
                                        area.GetPrimaryColorForTile((int)currentTile.X, (int)currentTile.Y), ParticleFactory.DURATION_MEDIUM));
                                }
                                for (int i = 0; i < 3; i++)
                                {
                                    area.AddParticle(ParticleFactory.GenerateParticle(placementLocation + new Vector2(0, 14), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.SMALL,
                                        area.GetSecondaryColorForTile((int)currentTile.X, (int)currentTile.Y), ParticleFactory.DURATION_MEDIUM));
                                }
                            }
                        } else if (GetHeldItem().GetItem().HasTag(Item.Tag.FISHING_ROD))
                        {
                            if (currentFishingPool == null) {
                                Vector2 targetSpot = GetAdjustedPosition() + new Vector2(direction == DirectionEnum.LEFT ? -9 : 13, HEIGHT + 1 - 8); ;
                                bool foundWater = false;
                                bool foundLand = false;
                                int lineLength = 0;

                                while (!foundWater && !foundLand && lineLength <= 3)
                                {
                                    if (area.GetCollisionTypeAt((int)(((direction == DirectionEnum.LEFT ? 1 : 1) + targetSpot.X) / 8), (int)(targetSpot.Y / 8)) == Area.CollisionTypeEnum.WATER)
                                    {
                                        foundWater = true;
                                    } else if (area.GetCollisionTypeAt((int)(((direction == DirectionEnum.LEFT ? 1 : 1) + targetSpot.X) / 8), (int)(targetSpot.Y / 8)) == Area.CollisionTypeEnum.SOLID)
                                    {
                                        foundLand = true;
                                    }

                                    if (foundWater)
                                    {
                                        fishlineParts.Add(new FishlinePart(FishlinePart.Type.HOOK, targetSpot));
                                    } else if(!foundLand)
                                    {
                                        fishlineParts.Add(new FishlinePart(FishlinePart.Type.LINE, targetSpot));
                                        lineLength++;
                                    }
                                    targetSpot.Y += 8;
                                }

                                if (foundWater)
                                {
                                    fishlineParts.Add(new FishlinePart(FishlinePart.Type.EXCLAIMATION, GetAdjustedPosition() + new Vector2(2, -9)));
                                    currentFishingPool = area.GetFishingZoneAt((int)targetSpot.X, (int)targetSpot.Y);

                                    for (int i = 0; i < 6; i++)
                                    {
                                        area.AddParticle(ParticleFactory.GenerateParticle(targetSpot - new Vector2(2, 2), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                                            Util.PARTICLE_WATER_PRIMARY.color, ParticleFactory.DURATION_SHORT));
                                        area.AddParticle(ParticleFactory.GenerateParticle(targetSpot - new Vector2(2, 2), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.ONEXONE,
                                            Util.PARTICLE_WATER_SECONDARY.color, ParticleFactory.DURATION_SHORT));
                                    }
                                } else
                                {
                                    AddNotification(new Notification("I can't fish here; my hook isn't in the water.", Color.Red, Notification.Length.SHORT));
                                }
                            } 
                        }

                    } else if (useTool && sprite.GetCurrentFrameOfLoop() == 4 && !attemptToolHit)
                    {
                        attemptToolHit = true;
                        if (targetEntity is IInteractTool)
                        {
                            ((IInteractTool)targetEntity).InteractTool(this, area, world);
                        }
                        if (targetTileEntity is IInteractTool)
                        {
                            ((IInteractTool)targetTileEntity).InteractTool(this, area, world);
                        }

                        TileEntity entityBelowTargetTile = area.GetTileEntity((int)targetTile.X, (int)targetTile.Y + 1);
                        if (entityBelowTargetTile != null && entityBelowTargetTile is IInteractTool)
                        {
                            ((IInteractTool)entityBelowTargetTile).InteractTool(this, area, world);
                        }
                    }
                }

                if(targetEntity != null && grounded && !rolling && !useTool) //interacting with free roaming entity (ex person)
                {
                    if(targetEntity is IInteract)
                    {
                        if (leftMousePress)
                        {
                            if (controller.IsKeyDown(KeyBinds.SHIFT))
                            {
                                ((IInteract)targetEntity).InteractLeftShift(this, area, world);
                                if (!((IInteract)targetEntity).GetLeftShiftClickAction(this).Equals(""))
                                {
                                    interactThisFrame = true;
                                }
                            }
                            else
                            {
                                ((IInteract)targetEntity).InteractLeft(this, area, world);
                                if (!((IInteract)targetEntity).GetLeftClickAction(this).Equals(""))
                                {
                                    interactThisFrame = true;
                                }
                            }
                        }
                        else if (rightMousePress)
                        {
                            if (controller.IsKeyDown(KeyBinds.SHIFT))
                            {
                                ((IInteract)targetEntity).InteractRightShift(this, area, world);
                                if (!((IInteract)targetEntity).GetRightShiftClickAction(this).Equals(""))
                                {
                                    interactThisFrame = true;
                                }
                            }
                            else
                            {
                                ((IInteract)targetEntity).InteractRight(this, area, world);
                                if (!((IInteract)targetEntity).GetRightClickAction(this).Equals(""))
                                {
                                    interactThisFrame = true;
                                }
                            }
                        }
                    }
                } else if (targetTileEntity != null && grounded && !rolling && !useTool) //interacting with tile entity
                {
                    if (targetTileEntity is IInteract)
                    {
                        if (targetTileEntity is TEntityFarmable)
                        { //it's nice to be able to fertilize farmables by holding down...
                            if (leftMouseDown)
                            {
                                if (controller.IsKeyDown(KeyBinds.SHIFT))
                                {
                                    ((IInteract)targetTileEntity).InteractLeftShift(this, area, world);
                                    if(!((IInteract)targetTileEntity).GetLeftShiftClickAction(this).Equals(""))
                                    {
                                        interactThisFrame = true;
                                    }
                                }
                                else
                                {
                                    ((IInteract)targetTileEntity).InteractLeft(this, area, world);
                                    if (!((IInteract)targetTileEntity).GetLeftClickAction(this).Equals(""))
                                    {
                                        interactThisFrame = true;
                                    }
                                }
                            }
                        }
                        else if (leftMousePress)
                        {
                            if (controller.IsKeyDown(KeyBinds.SHIFT))
                            {
                                ((IInteract)targetTileEntity).InteractLeftShift(this, area, world);
                                if (!((IInteract)targetTileEntity).GetLeftShiftClickAction(this).Equals(""))
                                {
                                    interactThisFrame = true;
                                }
                            }
                            else
                            {
                                ((IInteract)targetTileEntity).InteractLeft(this, area, world);
                                if (!((IInteract)targetTileEntity).GetLeftClickAction(this).Equals(""))
                                {
                                    interactThisFrame = true;
                                }
                            }
                        }
                        else if (rightMousePress)
                        {
                            if (controller.IsKeyDown(KeyBinds.SHIFT))
                            {
                                ((IInteract)targetTileEntity).InteractRightShift(this, area, world);
                                if (!((IInteract)targetTileEntity).GetRightShiftClickAction(this).Equals(""))
                                {
                                    interactThisFrame = true;
                                }
                            }
                            else
                            {
                                ((IInteract)targetTileEntity).InteractRight(this, area, world);
                                if (!((IInteract)targetTileEntity).GetRightClickAction(this).Equals(""))
                                {
                                    interactThisFrame = true;
                                }
                            }
                        }
                    }
                }

                if (!interactThisFrame)
                {
                    if (leftMousePress && GetHeldItem().GetItem() is PlantableItem && grounded && !rolling && !useTool)
                    {
                        PlantableItem plantableItem = (PlantableItem)GetHeldItem().GetItem();
                        TileEntity toAdd = (TileEntity)EntityFactory.GetEntity(plantableItem.GetPlantedType(), null, targetTile + (direction == DirectionEnum.LEFT ? new Vector2(-1, 0) : new Vector2(0, 0)) + plantableItem.GetPlacementOffset(), area);
                        if (area.IsTileEntityPlacementValid((int)toAdd.GetTilePosition().X, (int)toAdd.GetTilePosition().Y, toAdd.GetTileWidth(), toAdd.GetTileHeight()))
                        {
                            if (plantableItem.HasTag(Item.Tag.SOIL_PLANT_ONLY))
                            {
                                if (area.GetGroundTileType((int)targetTile.X, (int)targetTile.Y + 1) == Area.GroundTileType.EARTH &&
                                    area.GetGroundTileType((int)targetTile.X + (direction == DirectionEnum.LEFT ? -1 : 1), (int)targetTile.Y + 1) == Area.GroundTileType.EARTH)
                                {
                                    area.AddTileEntity(toAdd);
                                } else
                                {
                                    AddNotification(new EntityPlayer.Notification("This type of sapling has to be planted on normal soil.", Color.Red));
                                }
                            }
                            else if (plantableItem.HasTag(Item.Tag.SAND_PLANT_ONLY))
                            {
                                if (area.GetGroundTileType((int)targetTile.X, (int)targetTile.Y + 1) == Area.GroundTileType.SAND &&
                                    area.GetGroundTileType((int)targetTile.X + (direction == DirectionEnum.LEFT ? -1 : 1), (int)targetTile.Y + 1) == Area.GroundTileType.SAND)
                                {
                                    area.AddTileEntity(toAdd);
                                } else
                                {
                                    AddNotification(new EntityPlayer.Notification("This type of sapling has to be planted on sand.", Color.Red));
                                }
                            }
                            else
                            {
                                area.AddTileEntity(toAdd);
                            }
                        } else
                        {
                            AddNotification(new EntityPlayer.Notification("I can't plant the sapling in this spot.", Color.Red));
                        }
                    }
                    else if (leftMousePress && GetHeldItem().GetItem() is EdibleItem && grounded && !rolling && !useTool)
                    {
                        externalVelocityX = 0;
                        DialogueNode foodPrompt = new DialogueNode("Eat the " + GetHeldItem().GetItem().GetName() + "?", DialogueNode.PORTRAIT_BAD);
                        foodPrompt.decisionLeftText = "Chow Down";
                        foodPrompt.decisionRightText = "Cancel";
                        foodPrompt.decisionLeftNode = new DialogueNode(((EdibleItem)GetHeldItem().GetItem()).GetOnEatDescription() , DialogueNode.PORTRAIT_BAD, (EntityPlayer player) =>
                        {
                            player.ApplyEffect(((EdibleItem)player.GetHeldItem().GetItem()).GetEffect(), ((EdibleItem)player.GetHeldItem().GetItem()).GetEffectLength());
                            player.GetHeldItem().Subtract(1);
                        });
                        foodPrompt.decisionRightNode = new DialogueNode("I'm not so hungry anyway.", DialogueNode.PORTRAIT_BAD);
                        SetCurrentDialogue(foodPrompt);
                    }
                    //handle alchemy items here too...
                }

                if (!grounded)
                {
                    elapsedSinceNotGrounded += deltaTime;
                }
                if (!(wall_cling || wall_grab))
                {
                    elapsedSinceWall += deltaTime;
                }
                if (groundPoundLock)
                {
                    elapsedSinceGroundPoundLanding += deltaTime;
                    if (elapsedSinceGroundPoundLanding > GROUND_POUND_LANDING_LOCK_LENGTH)
                    {
                        groundPoundLock = false;
                        elapsedSinceGroundPoundLanding = 0;
                    }
                }

                //apply gravity
                if (!skipGravityThisFrame)
                {
                    if (gliding && sailcloth.GetItem() != ItemDict.CLOTHING_NONE)  //gliding sets gravity to a constant
                    {
                        if (!overwriteGlideThisFrame)
                        {
                            if (gravityState == GravityState.NORMAL)
                            {
                                velocityY = GLIDE_CONSTANT_VELOCITY;
                            }
                            else if (gravityState == GravityState.REVERSED)
                            {
                                velocityY = -GLIDE_CONSTANT_VELOCITY;
                            }
                        }
                    }
                    else if (groundPound) //ground pound increases gravity
                    {
                        if (gravityState == GravityState.NORMAL)
                        {
                            velocityY += GRAVITY * deltaTime * GROUND_POUND_GRAVITY_MULTIPLIER;
                        }
                        else if (gravityState == GravityState.REVERSED)
                        {
                            velocityY += -GRAVITY * deltaTime * GROUND_POUND_GRAVITY_MULTIPLIER;
                        }
                    }
                    else //otherwise apply gravity normally
                    {
                        if (!swimming)
                        {
                            if (gravityState == GravityState.NORMAL)
                            {
                                velocityY += GRAVITY * deltaTime;
                                if (velocityY > GRAVITY * 0.55f)
                                {
                                    velocityY = GRAVITY * 0.55f;
                                }
                            }
                            else if (gravityState == GravityState.REVERSED)
                            {
                                velocityY += -GRAVITY * deltaTime;
                                if (velocityY < -GRAVITY * 0.55f)
                                {
                                    velocityY = -GRAVITY * 0.55f;
                                }
                            }
                        }
                    }
                }
                skipGravityThisFrame = false;
                overwriteGlideThisFrame = false;

                if (!useTool)
                {
                    //adjust selectedhotbarposition according to mouse wheel movement
                    selectedHotbarPosition += controller.GetChangeInMouseWheel();
                    if (selectedHotbarPosition >= GameplayInterface.HOTBAR_LENGTH)
                    {
                        selectedHotbarPosition = 0;
                    }
                    else if (selectedHotbarPosition < 0)
                    {
                        selectedHotbarPosition = GameplayInterface.HOTBAR_LENGTH - 1;
                    }


                    //adjust selectedhotbarposition if any of the 1-9 keys are pressed down
                    for (int i = 0; i < GameplayInterface.HOTBAR_LENGTH; i++)
                    {
                        if (controller.IsKeyPressed(KeyBinds.HOTBAR_SELECT[i]))
                        {
                            selectedHotbarPosition = i;
                        }
                    }

                    //debug
                    if(controller.IsKeyPressed(Keys.G))
                    {
                        
                    }

                    //handle controller up
                    if (controller.IsKeyPressed(KeyBinds.UP))
                    {
                        if (rolling && canStandFromRoll)
                        {
                            rolling = false;
                            if (!grounded)
                            {
                                Jump(area);
                            }
                        } else if (swimming)
                        {
                            Jump(area);
                            jumpFromSwim = true;
                        }
                        else if ((grounded || (elapsedSinceNotGrounded < WALKOFF_LEDGE_JUMP_ALLOWANCE)) && !IsJumping() && !rolling)
                        {
                            Jump(area);
                        }
                        else if (!IsJumping() && controller.IsKeyPressed(KeyBinds.UP) && ((wall_cling || wall_grab) || elapsedSinceWall < WALL_GRAB_CLING_ALLOWANCE))
                        {
                            Jump(area);
                            TryWallJump(deltaTime, controller);
                        }
                        else if (controller.IsKeyPressed(KeyBinds.UP) && !rolling && !groundPound)
                        {
                            gliding = true;
                        }
                    }

                    //if holding down when landing from ground pound, immediately goes into high speed roll
                    if (controller.IsKeyDown(KeyBinds.DOWN) && groundPoundLock)
                    {
                        rolling = true;
                        groundPoundLock = false;
                        inputVelocityX += direction == DirectionEnum.LEFT ? -MAX_SPEED_X_ROLL * deltaTime : MAX_SPEED_X_ROLL * deltaTime;
                    }

                    //handle controller down
                    if (controller.IsKeyPressed(KeyBinds.DOWN) && !rolling && !wall_cling && !wall_grab)
                    {
                        if (grounded) //when grounded, enter rolling state
                        {
                            rolling = true;
                        }
                        else //if not grounded, cancel glide or start a groundpound
                        {
                            if (gliding)
                            {
                                gliding = false;
                            }
                            else
                            {
                                groundPound = true;
                            }
                        }
                    }

                    if (!groundPound)
                    {
                        //handle controller left/right
                        if (controller.IsKeyDown(KeyBinds.LEFT) && controller.IsKeyDown(KeyBinds.RIGHT))
                        {
                            wall_grab = false;
                            wall_cling = false;
                        }
                        else
                        {
                            if (!groundPoundLock)
                            {
                                float maxSpeedIncreaseFromEffect = GetMaxSpeedIncreaseFromEffects(world.GetTimeData());
                                if (controller.IsKeyDown(KeyBinds.LEFT))
                                {
                                    if (elapsedSinceWall < WALL_GRAB_WALL_JUMP_ALLOWANCE)
                                    {
                                        TryWallJump(deltaTime, controller);
                                    }

                                    inputVelocityX -= SPEED_X * deltaTime;
                                    if (rolling)
                                    {
                                        inputVelocityX = Math.Max(inputVelocityX, (-MAX_SPEED_X_ROLL - maxSpeedIncreaseFromEffect) * deltaTime);
                                    }
                                    else
                                    {
                                        inputVelocityX = Math.Max(inputVelocityX, (-MAX_SPEED_X_WALK - maxSpeedIncreaseFromEffect) * deltaTime);
                                    }
                                    direction = DirectionEnum.LEFT;
                                }
                                if (controller.IsKeyDown(KeyBinds.RIGHT))
                                {
                                    if (elapsedSinceWall < WALL_GRAB_WALL_JUMP_ALLOWANCE)
                                    {
                                        TryWallJump(deltaTime, controller);
                                    }

                                    inputVelocityX += SPEED_X * deltaTime;
                                    if (rolling)
                                    {
                                        inputVelocityX = Math.Min(inputVelocityX, (MAX_SPEED_X_ROLL + maxSpeedIncreaseFromEffect) * deltaTime);
                                    }
                                    else
                                    {
                                        inputVelocityX = Math.Min(inputVelocityX, (MAX_SPEED_X_WALK + maxSpeedIncreaseFromEffect) * deltaTime);
                                    }

                                    direction = DirectionEnum.RIGHT;
                                }
                            }
                        }
                    }

                    //check if player has let go of wallgrab/cling
                    if (wall_grab || wall_cling)
                    {
                        if ((direction == DirectionEnum.LEFT && controller.IsKeyUp(KeyBinds.LEFT)) || (direction == DirectionEnum.RIGHT && controller.IsKeyUp(KeyBinds.RIGHT)))
                        {
                            wall_grab = false;
                            wall_cling = false;
                        }
                    }

                    //when wall clinging, reduce velocity to zero
                    if (wall_cling && !IsJumping())
                    {
                        velocityY = 0;
                    }

                    //if ground pounding, disable horizontal input movement
                    if (groundPound)
                    {
                        inputVelocityX = 0;
                    }
                }
                

                //calculate collisions
                float stepX = (externalVelocityX + inputVelocityX) / COLLISION_STEPS;
                float stepY = velocityY / COLLISION_STEPS;

                if (wall_grab || wall_cling)
                {
                    stepY = 0;
                }

                for (int step = 0; step < COLLISION_STEPS; step++)
                {
                    if (stepX != 0) //move X
                    {
                        CollisionData xCollisionData = CheckCollision(this.position.X + stepX, this.position.Y, area, rolling);
                        if (xCollisionData.didCollide) //if next movement = collision
                        {
                            stepX = 0; //stop moving if collision
                            if (!rolling && !grounded && !IsJumping() && ((controller.IsKeyDown(KeyBinds.LEFT) && direction == DirectionEnum.LEFT) || (controller.IsKeyDown(KeyBinds.RIGHT) && direction == DirectionEnum.RIGHT)))
                            {
                                if (xCollisionData.didTopBodyCollide)
                                {
                                    wall_grab = true;
                                    velocityY = 0;
                                    stepY = 0;
                                    if (xCollisionData.didFullbodyCollide)
                                    {
                                        wall_cling = true;
                                    }
                                    elapsedSinceWall = 0;
                                    gliding = false;
                                }
                            }
                            else
                            {
                                wall_grab = false;
                                wall_cling = false;
                            }
                            inputVelocityX = 0;
                        }
                        else
                        {
                            this.position.X += stepX;
                            wall_grab = false;
                            wall_cling = false;
                        }
                    }
                    if (stepY != 0) //move Y
                    {
                        CollisionData yCollisionData = CheckCollision(this.position.X, this.position.Y + stepY, area, rolling);
                        if (yCollisionData.didCollide)
                        {
                            stepY = 0;
                            if (!IsJumping())
                            {
                                if(grounded == false && swimming == false)
                                {
                                    //landing particles
                                    for (int i = 0; i < 5; i++)
                                    {
                                        Vector2 particlePosition = GetAdjustedPosition() + new Vector2(WIDTH/2,(gravityState == GravityState.REVERSED ? 0.75f : HEIGHT - 0.5f));
                                        area.AddParticle(ParticleFactory.GenerateParticle(particlePosition, ParticleBehavior.RUSH_OUTWARD, ParticleTextureStyle.ONEXONE, Util.DEFAULT_COLOR.color, ParticleFactory.DURATION_VERY_SHORT));
                                    }
                                }
                                grounded = true;
                                jumpFromSwim = false;
                                elapsedSinceNotGrounded = 0;
                                gliding = false;

                                if (groundPound)
                                {
                                    groundPoundLock = true;
                                    elapsedSinceGroundPoundLanding = 0;
                                    groundPound = false;
                                    Vector2 currentTile = GetTileStandingOn();
                                    for (int i = 0; i < 8; i++)
                                    {
                                        area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(Util.RandInt(0, WIDTH), gravityState == GravityState.REVERSED ? 1.5f : HEIGHT - 1f), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.SMALL, 
                                            area.GetPrimaryColorForTile((int)currentTile.X, (int)currentTile.Y), ParticleFactory.DURATION_MEDIUM));
                                    }
                                    for (int i = 0; i < 3; i++)
                                    {
                                        area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(Util.RandInt(0, WIDTH), gravityState == GravityState.REVERSED ? 1.5f : HEIGHT - 1f), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.SMALL,
                                            area.GetSecondaryColorForTile((int)currentTile.X, (int)currentTile.Y), ParticleFactory.DURATION_MEDIUM));
                                    }
                                }
                            }
                            if (rolling && velocityY > ROLL_MINIMUM_BOUNCE)
                            {
                                velocityY = -velocityY * ROLL_BOUNCE_MULTIPLIER;
                                Vector2 currentTile = GetTileStandingOn();
                                for (int i = 0; i < 4; i++)
                                {
                                    area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(Util.RandInt(0, WIDTH), gravityState == GravityState.REVERSED ? 1.5f : HEIGHT - 1f), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.SMALL,
                                        area.GetPrimaryColorForTile((int)currentTile.X, (int)currentTile.Y), ParticleFactory.DURATION_MEDIUM));
                                }
                                for (int i = 0; i < 2; i++)
                                {
                                    area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(Util.RandInt(0, WIDTH), gravityState == GravityState.REVERSED ? 1.5f : HEIGHT - 1f), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.SMALL,
                                        area.GetPrimaryColorForTile((int)currentTile.X, (int)currentTile.Y), ParticleFactory.DURATION_MEDIUM));
                                }
                            }
                            else
                            {
                                velocityY = 0;
                            }
                        }
                        else
                        {
                            this.position.Y += stepY;
                            if(!jumpFromSwim && swimming && !CheckSwimmingCollision(area))
                            {
                                this.position.Y -= stepY;
                                velocityY = 0;
                            }
                            grounded = false;
                        }
                    }
                }

                if (CheckSwimmingCollision(area))
                {
                    gliding = false;
                    rolling = false;
                    groundPound = false;
                    swimming = true;
                }  else
                {
                    jumpFromSwim = false;
                    swimming = false;
                }

                if (swimming && velocityY >= MAXIMUM_SWIMMING_VELOCITY_Y)
                {
                    velocityY += SWIMMING_VELOCITY_Y * deltaTime;
                }

                if (rolling)
                {
                    canStandFromRoll = !CheckCollision(this.position.X, this.position.Y, area, false, false).didCollide;
                }

                float friction = rolling ? X_FRICTION_GROUND_ROLL : X_FRICTION_GROUND;
                if (!grounded)
                {
                    friction = rolling ? X_FRICTION_AIR_ROLL : X_FRICTION_AIR;
                }

                //particles
                if (gliding && sailcloth.GetItem() != ItemDict.CLOTHING_NONE && inputVelocityX != 0)
                {
                    if (Util.RandInt(1, 2) == 2)
                    {
                        area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(direction == DirectionEnum.LEFT ? WIDTH + 2 : 0, gravityState == GravityState.REVERSED ? HEIGHT-1.5f : -1.5f),
                            ParticleBehavior.ROTATE_STATIONARY, ParticleTextureStyle.ONEXONE, Color.White, ParticleFactory.DURATION_MEDIUM));
                        area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(direction == DirectionEnum.LEFT ? -2 : WIDTH + 4, gravityState == GravityState.REVERSED ? HEIGHT-1.5f : -1.5f),
                            ParticleBehavior.ROTATE_STATIONARY, ParticleTextureStyle.ONEXONE, Color.White, ParticleFactory.DURATION_MEDIUM));
                    }
                }
                else if (grounded && inputVelocityX != 0)
                {
                    if (Util.RandInt(1, 3) == 3)
                    {
                        area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(direction == DirectionEnum.LEFT ? WIDTH : 0, gravityState == GravityState.REVERSED ? 0.5f : HEIGHT - 0.5f), ParticleBehavior.ROTATE_STATIONARY, ParticleTextureStyle.SMALL, Util.PARTICLE_DUST.color, ParticleFactory.DURATION_SHORT));
                    }
                }
                if (rolling)
                {
                    if (grounded && inputVelocityX != 0)
                    {
                        area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(direction == DirectionEnum.LEFT ? WIDTH + 2 : 0, gravityState == GravityState.REVERSED ? 0.5f : HEIGHT - 0.5f), ParticleBehavior.ROTATE_STATIONARY, ParticleTextureStyle.CHUNK, Util.PARTICLE_DUST.color, ParticleFactory.DURATION_SHORT));
                        if (Util.RandInt(1, 8) == 8)
                        {
                            Vector2 currentTile = GetTileStandingOn();
                            area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(direction == DirectionEnum.LEFT ? WIDTH + 2 : 0, gravityState == GravityState.REVERSED ? 1f : HEIGHT - 1f), ParticleBehavior.BOUNCE_DOWN, ParticleTextureStyle.SMALL,
                                area.GetPrimaryColorForTile((int)currentTile.X, (int)currentTile.Y), ParticleFactory.DURATION_MEDIUM));
                        }
                    }
                    else if (!grounded && inputVelocityX != 0)
                    {
                        area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(direction == DirectionEnum.LEFT ? WIDTH + 3 : -1, gravityState == GravityState.REVERSED ? 3.5f : HEIGHT - 3.5f), ParticleBehavior.ROTATE_STATIONARY, ParticleTextureStyle.SMALL, Color.White, ParticleFactory.DURATION_SHORT));
                    }
                }
                if ((externalVelocityX > 0 && inputVelocityX > 0) ||(externalVelocityX < 0 && inputVelocityX < 0))
                {
                    area.AddParticle(ParticleFactory.GenerateParticle(GetAdjustedPosition() + new Vector2(direction == DirectionEnum.LEFT ? WIDTH+5 : -3, (HEIGHT / 2)+3), ParticleBehavior.ROTATE_STATIONARY,
                        ParticleTextureStyle.CHUNK, Color.White, ParticleFactory.DURATION_SHORT));
                }

                externalVelocityX = Util.AdjustTowards(externalVelocityX, 0, (grounded ? X_FRICTION_GROUND : X_FRICTION_AIR) * deltaTime);
                inputVelocityX = Util.AdjustTowards(inputVelocityX, 0, friction * deltaTime);

                UpdateAnimation(deltaTime);
            }

            if (!paused)
            {
                UpdateSprite(deltaTime);
            }
        } //end update

        public void ResetInputVelocityX()
        {
            inputVelocityX = 0;
        }

        public void ResetInputVelocityY()
        {
            velocityY = 0;
        }

        public void SetExternalVelocityX(float velocity)
        {
            externalVelocityX = velocity;
        }

        public void SetVelocityY(float velocity)
        {
            velocityY = velocity;
        }

        public void OverwriteGlideThisFrame()
        {
            overwriteGlideThisFrame = true;
        }

        private void UpdateAnimation(float deltaTime)
        {
            if(useTool)
            {
                if(GetHeldItem().GetItem() == ItemDict.HOE)
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.HOE_L : ClothedSprite.HOE_R);
                }
                else if (GetHeldItem().GetItem() == ItemDict.WATERING_CAN)
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.WATER_L : ClothedSprite.WATER_R);
                }
                else if (GetHeldItem().GetItem() == ItemDict.AXE)
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.AXE_L : ClothedSprite.AXE_R);
                }
                else if (GetHeldItem().GetItem() == ItemDict.PICKAXE)
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.PICKAXE_L : ClothedSprite.PICKAXE_R);
                }
                else if (GetHeldItem().GetItem() == ItemDict.FISHING_ROD)
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.FISH_L : ClothedSprite.FISH_R);
                } else
                {
                    throw new Exception("No tool animation?");
                }
                if (sprite.IsCurrentLoopFinished() && GetHeldItem().GetItem() != ItemDict.FISHING_ROD)
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.IDLE_CYCLE_L : ClothedSprite.IDLE_CYCLE_R);
                    useTool = false;
                }
            }
            else if(groundPound)
            {
                sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.GROUND_POUND_L : ClothedSprite.GROUND_POUND_R);
            }
            else if(groundPoundLock)
            {
                if (elapsedSinceGroundPoundLanding < 0.75f * GROUND_POUND_LANDING_LOCK_LENGTH)
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.GROUND_POUND_L : ClothedSprite.GROUND_POUND_R);
                } else
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.LANDING_ANIM_L : ClothedSprite.LANDING_ANIM_R);
                }
            }
            else if(rolling)
            {
                sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.ROLLING_CYCLE_L : ClothedSprite.ROLLING_CYCLE_R);
                if (inputVelocityX == 0)
                {
                    sprite.Pause();
                }
                else
                {
                    sprite.Unpause();
                }
            } else if (swimming)
            {
                if (inputVelocityX != 0)
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.WALK_CYCLE_L : ClothedSprite.WALK_CYCLE_R);
                } else
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.JUMP_ANIM_L : ClothedSprite.JUMP_ANIM_R);
                }
            }
            else if (gliding)
            {
                if (!sprite.IsCurrentLoop(ClothedSprite.GLIDE_START_ANIM_L) && !sprite.IsCurrentLoop(ClothedSprite.GLIDE_CYCLE_L) && !sprite.IsCurrentLoop(ClothedSprite.GLIDE_START_ANIM_R) && !sprite.IsCurrentLoop(ClothedSprite.GLIDE_CYCLE_R))
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.GLIDE_START_ANIM_L : ClothedSprite.GLIDE_START_ANIM_R);
                }
                else
                {
                    if (sprite.IsCurrentLoopFinished() || (direction == DirectionEnum.LEFT && sprite.IsCurrentLoop(ClothedSprite.GLIDE_CYCLE_R) || (direction == DirectionEnum.RIGHT && sprite.IsCurrentLoop(ClothedSprite.GLIDE_CYCLE_L))))
                    {
                        sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.GLIDE_CYCLE_L : ClothedSprite.GLIDE_CYCLE_R);
                    }
                }
            }
            else if (IsJumping())
            {
                sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.JUMP_ANIM_L : ClothedSprite.JUMP_ANIM_R);
            }
            else if (wall_cling)
            {
                sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.WALL_GRAB_ANIM_L : ClothedSprite.WALL_GRAB_ANIM_R);
            }
            else if (wall_grab)
            {
                sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.WALL_GRAB_ANIM_L : ClothedSprite.WALL_GRAB_ANIM_R);
            }
            else if (grounded && inputVelocityX != 0)
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
                } else
                {
                    sprite.SetLoopIfNot(direction == DirectionEnum.LEFT ? ClothedSprite.IDLE_CYCLE_L : ClothedSprite.IDLE_CYCLE_R);
                }
            }
        }

        public void SetPosition(Vector2 position)
        {
            this.position = new Vector2(position.X - OFFSET_X, position.Y);
        }

        public void SetGroundedPosition(Vector2 groundedPosition)
        {
            this.position = new Vector2(groundedPosition.X - OFFSET_X, groundedPosition.Y - HEIGHT - OFFSET_Y - 0.001f);
            grounded = true;
        }

        public override void Draw(SpriteBatch sb, float layerDepth)
        {
            //Vector2 modifiedPosition = Util.Vec2FloatToInt(position); 
            Vector2 modifiedPosition = new Vector2(position.X, position.Y);
            if (gravityState == GravityState.REVERSED)
            {
                modifiedPosition.Y--;
            }
            else
            {
                modifiedPosition.Y++;
            }
            if(direction == DirectionEnum.LEFT)
            {
                modifiedPosition.X++;
            }


            sprite.Draw(sb, modifiedPosition, layerDepth, gravityState == GravityState.REVERSED ? SpriteEffects.FlipVertically : SpriteEffects.None, 1.0f, 1.0f);
            if (sprite.IsCurrentLoopFinished())
            {
                foreach (FishlinePart part in fishlineParts)
                {
                    if (!part.IsExclaimation())
                    {
                        part.Draw(sb, layerDepth);
                    } else if (fishingDamage >= currentFishingPool.difficulty)
                    {
                        part.Draw(sb, layerDepth);
                    }
                }
            }
        }

        public void SetToDefaultPose()
        {
            sprite.SetLoop(direction == DirectionEnum.LEFT ? ClothedSprite.IDLE_CYCLE_L : ClothedSprite.IDLE_CYCLE_R);
            rolling = false;
        }

        public void UpdateSprite(float deltaTime)
        {
            bool drawPantsOverShoes = pants.GetItem().HasTag(Item.Tag.DRAW_OVER_SHOES);
            bool hideHair = false;
            if(hat.GetItem() != ItemDict.CLOTHING_NONE && hair.GetItem().HasTag(Item.Tag.HIDE_WHEN_HAT))
            {
                hideHair = true;
            }
            if(hat.GetItem().HasTag(Item.Tag.ALWAYS_HIDE_HAIR))
            {
                hideHair = true;
            } else if (hat.GetItem().HasTag(Item.Tag.ALWAYS_SHOW_HAIR))
            {
                hideHair = false;
            }
            sprite.Update(deltaTime, ((ClothingItem)hat.GetItem()).GetSpritesheet(), ((ClothingItem)shirt.GetItem()).GetSpritesheet(), ((ClothingItem)outerwear.GetItem()).GetSpritesheet(),
            ((ClothingItem)pants.GetItem()).GetSpritesheet(), ((ClothingItem)socks.GetItem()).GetSpritesheet(), ((ClothingItem)shoes.GetItem()).GetSpritesheet(),
            ((ClothingItem)gloves.GetItem()).GetSpritesheet(), ((ClothingItem)earrings.GetItem()).GetSpritesheet(), ((ClothingItem)scarf.GetItem()).GetSpritesheet(), ((ClothingItem)glasses.GetItem()).GetSpritesheet(),
            ((ClothingItem)back.GetItem()).GetSpritesheet(), ((ClothingItem)sailcloth.GetItem()).GetSpritesheet(), ((ClothingItem)hair.GetItem()).GetSpritesheet(), ((ClothingItem)skin.GetItem()).GetSpritesheet(), ((ClothingItem)eyes.GetItem()).GetSpritesheet(), drawPantsOverShoes, hideHair);
        }

        public void LoadSave(SaveState playerSave)
        {
            for(int i = 0; i < inventory.Length; i++)
            {
                inventory[i] = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("item" + i, ItemDict.NONE.GetName()))
                    , Int32.Parse(playerSave.TryGetData("item"+i+"quantity", "0")));
            }

            hat = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("hat", ItemDict.CLOTHING_NONE.GetName())), 1);
            shirt = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("shirt", ItemDict.CLOTHING_NONE.GetName())), 1);
            outerwear = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("outerwear", ItemDict.CLOTHING_NONE.GetName())), 1);
            pants = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("pants", ItemDict.CLOTHING_NONE.GetName())), 1);
            socks = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("socks", ItemDict.CLOTHING_NONE.GetName())), 1);
            shoes = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("shoes", ItemDict.CLOTHING_NONE.GetName())), 1);
            gloves = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("gloves", ItemDict.CLOTHING_NONE.GetName())), 1);
            earrings = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("earrings", ItemDict.CLOTHING_NONE.GetName())), 1);
            scarf = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("scarf", ItemDict.CLOTHING_NONE.GetName())), 1);
            glasses = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("glasses", ItemDict.CLOTHING_NONE.GetName())), 1);
            back = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("back", ItemDict.CLOTHING_NONE.GetName())), 1);
            sailcloth = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("sailcloth", ItemDict.CLOTHING_NONE.GetName())), 1);

            accessory1 = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("accessory1", ItemDict.CLOTHING_NONE.GetName())), 1);
            accessory2 = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("accessory2", ItemDict.CLOTHING_NONE.GetName())), 1);
            accessory3 = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("accessory3", ItemDict.CLOTHING_NONE.GetName())), 1);

            skin = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("skin", ItemDict.CLOTHING_NONE.GetName())), 1);
            hair = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("hair", ItemDict.CLOTHING_NONE.GetName())), 1);
            eyes = new ItemStack(ItemDict.GetItemByName(playerSave.TryGetData("eyes", ItemDict.CLOTHING_NONE.GetName())), 1);

            gold = Int32.Parse(playerSave.TryGetData("gold", "0"));
        }

        public SaveState GenerateSave()
        {
            SaveState playerSave = new SaveState(SaveState.Identifier.PLAYER);
            for(int i = 0; i < inventory.Length; i++)
            {
                ItemStack toSave = inventory[i];
                playerSave.AddData("item"+i, toSave.GetItem().GetName());
                playerSave.AddData("item" + i + "quantity", toSave.GetQuantity().ToString());
            }

            playerSave.AddData("hat", hat.GetItem().GetName());
            playerSave.AddData("shirt", shirt.GetItem().GetName());
            playerSave.AddData("outerwear", outerwear.GetItem().GetName());
            playerSave.AddData("pants", pants.GetItem().GetName());
            playerSave.AddData("socks", socks.GetItem().GetName());
            playerSave.AddData("shoes", shoes.GetItem().GetName());
            playerSave.AddData("gloves", gloves.GetItem().GetName());
            playerSave.AddData("earrings", earrings.GetItem().GetName());
            playerSave.AddData("scarf", scarf.GetItem().GetName());
            playerSave.AddData("glasses", glasses.GetItem().GetName());
            playerSave.AddData("back", back.GetItem().GetName());
            playerSave.AddData("sailcloth", sailcloth.GetItem().GetName());
            playerSave.AddData("accessory1", accessory1.GetItem().GetName());
            playerSave.AddData("accessory2", accessory2.GetItem().GetName());
            playerSave.AddData("accessory3", accessory3.GetItem().GetName());
            playerSave.AddData("skin", skin.GetItem().GetName());
            playerSave.AddData("hair", hair.GetItem().GetName());
            playerSave.AddData("eyes", eyes.GetItem().GetName());
            playerSave.AddData("gold", gold.ToString());

            return playerSave;
        }

        public float GetMaxSpeedIncreaseFromEffects(World.TimeData timeData)
        {
            if(HasEffect(AppliedEffects.SPEED_IV) ||
                (HasEffect(AppliedEffects.SPEED_IV_SPRING) && timeData.season == World.Season.SPRING) ||
                (HasEffect(AppliedEffects.SPEED_IV_AUTUMN) && timeData.season == World.Season.AUTUMN) ||
                (HasEffect(AppliedEffects.SPEED_IV_WINTER) && timeData.season == World.Season.WINTER))
            {
                return SPEED_EFFECT_BOOST_PER_LEVEL * 4;
            } else if (HasEffect(AppliedEffects.SPEED_III) ||
                (HasEffect(AppliedEffects.SPEED_III_MORNING) && timeData.timeOfDay == World.TimeOfDay.MORNING) ||
                (HasEffect(AppliedEffects.SPEED_III_AUTUMN) && timeData.season == World.Season.AUTUMN) ||
                (HasEffect(AppliedEffects.SPEED_III_WINTER) && timeData.season == World.Season.WINTER))
            {
                return SPEED_EFFECT_BOOST_PER_LEVEL * 3;
            } else if (HasEffect(AppliedEffects.SPEED_II) ||
                (HasEffect(AppliedEffects.SPEED_II_MORNING) && timeData.timeOfDay == World.TimeOfDay.MORNING))
            {
                return SPEED_EFFECT_BOOST_PER_LEVEL * 2;
            } else if (HasEffect(AppliedEffects.SPEED_I) ||
                (HasEffect(AppliedEffects.SPEED_I_MORNING) && timeData.timeOfDay == World.TimeOfDay.MORNING)) 
            {
                return SPEED_EFFECT_BOOST_PER_LEVEL * 1;
            }
            return 0;
        }

        public void AddNotification(Notification newNotification)
        {
            foreach(Notification notif in notifications)
            {
                if(newNotification.message.Equals(notif.message))
                {
                    notif.ResetLength();
                    return;
                }
            }
            notifications.Enqueue(newNotification);
        }

        public Notification GetCurrentNotification()
        {
            if(notifications.Count == 0)
            {
                return null;
            }
            return notifications.Peek();
        }

        public bool IsColliding(RectangleF solidRect)
        {
            return solidRect.Intersects(collisionRectangle);
        }

        public bool IsRiding(RectangleF solidRect)
        {
            RectangleF grabRideHitbox = new RectangleF(collisionRectangle.Position.X - 1f, collisionRectangle.Position.Y, collisionRectangle.Width + 2f, collisionRectangle.Height);
            if ((IsWallCling() || IsWallGrab()) && solidRect.Intersects(grabRideHitbox)) {
                return true;
            }
            RectangleF ridingHitbox = new RectangleF(collisionRectangle.Position, collisionRectangle.Size);
            ridingHitbox.Y += 0.5f;
            return solidRect.Intersects(ridingHitbox) && collisionRectangle.Bottom - 0.2f < solidRect.Top;
        }

        public bool PushX(float pushX, Area area)
        {
            CollisionData xCollisionData = CheckCollision(this.position.X + pushX, this.position.Y, area, rolling);
            if (xCollisionData.didCollide) //if next movement = collision
            {
                inputVelocityX = 0;
                return false;
            }
            else
            {
                this.position.X += pushX;
                wall_grab = false;
                wall_cling = false;
                return true;
            }
        }

        public void SkipGravityThisFrame()
        {
            skipGravityThisFrame = true;
        }

        public bool PushY(float pushY, Area area)
        {
            CollisionData yCollisionData = CheckCollision(this.position.X, this.position.Y + pushY, area, rolling);
            if (yCollisionData.didCollide) //if next movement = collision
            {
                wall_cling = false;
                wall_grab = false;
                velocityY = 0;
                return false;
            }
            else
            {
                position.Y += pushY;
                return true;
            }
        }

        protected override void OnXCollision()
        {
            externalVelocityX = 0;
            inputVelocityX = 0;
        }

        protected override void OnYCollision()
        {
            velocityY = 0;
        }

        public void SetGravityState(GravityState gravityState)
        {
            this.gravityState = gravityState;
        }

        public GravityState GetGravityState()
        {
            return gravityState;
        }
    }
}
