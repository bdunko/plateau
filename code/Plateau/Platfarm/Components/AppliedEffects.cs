using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Components
{
    public class AppliedEffects
    {
        public static float LENGTH_INFINITE = Int32.MaxValue;
        public static float LENGTH_VERY_SHORT = 90.0f;
        public static float LENGTH_SHORT = 180.0f;
        public static float LENGTH_MEDIUM = 300.0f;
        public static float LENGTH_LONG = 420.0f;
        public static float LENGTH_VERY_LONG = 600.0f;

        public class Effect
        {
            public string name, description;
            private Texture2D icon, modifier, frame;

            public Effect(string name, string description, Texture2D icon, Texture2D frame, Texture2D modifier)
            {
                this.name = name;
                this.description = description;
                this.icon = icon;
                this.frame = frame;
                this.modifier = modifier;
            }

            public void DrawIcon(SpriteBatch sb, Vector2 position)
            {
                sb.Draw(frame, position, Color.White);
                sb.Draw(modifier, position, Color.White * 0.75f);
                sb.Draw(icon, position, Color.White);
            }
        }

        public static Effect CHOPPING_I, CHOPPING_II, CHOPPING_III, CHOPPING_IV;
        public static Effect FISHING_I, FISHING_II, FISHING_III, FISHING_IV, FISHING_II_OCEAN, FISHING_III_CLOUD, FISHING_III_FRESHWATER, FISHING_III_LAVA, FISHING_III_OCEAN, FISHING_IV_CAVE, FISHING_IV_CLOUD, FISHING_IV_FRESHWATER,
            FISHING_IV_LAVA, FISHING_IV_OCEAN;
        public static Effect FORAGING_I, FORAGING_II, FORAGING_III, FORAGING_IV, FORAGING_II_MUSHROOMS, FORAGING_II_SPRING, FORAGING_II_SUMMER, FORAGING_II_WINTER, FORAGING_III_AUTUMN, FORAGING_III_SPRING, FORAGING_IV_BEACH, FORAGING_IV_FLOWERS,
            FORAGING_IV_MUSHROOMS, FORAGING_IV_SUMMER, FORAGING_IV_WINTER, FORAGING_II_FLOWERS, FORAGING_II_BEACH;
        public static Effect GATHERING_BOAR, GATHERING_CHICKEN, GATHERING_COW, GATHERING_SHEEP, GATHERING_PIG;
        public static Effect INSECT_CATCHING_I, INSECT_CATCHING_II, INSECT_CATCHING_III, INSECT_CATCHING_IV_MORNING, INSECT_CATCHING_IV_NIGHT, INSECT_CATCHING_IV;
        public static Effect MINING_I, MINING_II, MINING_III, MINING_IV;
        public static Effect SPEED_I, SPEED_II, SPEED_III, SPEED_IV, SPEED_I_MORNING, SPEED_II_MORNING, SPEED_III_AUTUMN, SPEED_III_MORNING, SPEED_III_WINTER, SPEED_IV_AUTUMN, SPEED_IV_SPRING, SPEED_IV_WINTER;

        public static void Initialize(ContentManager content)
        {
            Texture2D frame_i = content.Load<Texture2D>(Paths.INTERFACE_ICON_FRAME_I);
            Texture2D frame_ii = content.Load<Texture2D>(Paths.INTERFACE_ICON_FRAME_II);
            Texture2D frame_iii = content.Load<Texture2D>(Paths.INTERFACE_ICON_FRAME_III);
            Texture2D frame_iv = content.Load<Texture2D>(Paths.INTERFACE_ICON_FRAME_IV);
            Texture2D frame_v = content.Load<Texture2D>(Paths.INTERFACE_ICON_FRAME_V);
            Texture2D modifier_spring = content.Load<Texture2D>(Paths.INTERFACE_ICON_MODIFIER_SPRING);
            Texture2D modifier_summer = content.Load<Texture2D>(Paths.INTERFACE_ICON_MODIFIER_SUMMER);
            Texture2D modifier_autumn = content.Load<Texture2D>(Paths.INTERFACE_ICON_MODIFIER_AUTUMN);
            Texture2D modifier_winter = content.Load<Texture2D>(Paths.INTERFACE_ICON_MODIFIER_WINTER);
            Texture2D modifier_morning = content.Load<Texture2D>(Paths.INTERFACE_ICON_MODIFIER_MORNING);
            Texture2D modifier_night = content.Load<Texture2D>(Paths.INTERFACE_ICON_MODIFIER_NIGHT);
            Texture2D modifier_sunny = content.Load<Texture2D>(Paths.INTERFACE_ICON_MODIFIER_SUNNY);
            Texture2D modifier_rainy = content.Load<Texture2D>(Paths.INTERFACE_ICON_MODIFIER_RAINY);
            Texture2D modifier_cloudy = content.Load<Texture2D>(Paths.INTERFACE_ICON_MODIFIER_CLOUDY);
            Texture2D modifier_snowy = content.Load<Texture2D>(Paths.INTERFACE_ICON_MODIFIER_SNOWY);
            Texture2D icon_boar = content.Load<Texture2D>(Paths.INTERFACE_ICON_BOAR);
            Texture2D icon_chicken = content.Load<Texture2D>(Paths.INTERFACE_ICON_CHICKEN);
            Texture2D icon_chopping = content.Load<Texture2D>(Paths.INTERFACE_ICON_CHOPPING);
            Texture2D icon_cow = content.Load<Texture2D>(Paths.INTERFACE_ICON_COW);
            Texture2D icon_fishing = content.Load<Texture2D>(Paths.INTERFACE_ICON_FISHING);
            Texture2D icon_fishing_cave = content.Load<Texture2D>(Paths.INTERFACE_ICON_FISHING_CAVE);
            Texture2D icon_fishing_cloud = content.Load<Texture2D>(Paths.INTERFACE_ICON_FISHING_CLOUD);
            Texture2D icon_fishing_freshwater = content.Load<Texture2D>(Paths.INTERFACE_ICON_FISHING_FRESHWATER);
            Texture2D icon_fishing_ocean = content.Load<Texture2D>(Paths.INTERFACE_ICON_FISHING_OCEAN);
            Texture2D icon_fishing_lava = content.Load<Texture2D>(Paths.INTERFACE_ICON_FISHING_LAVA);
            Texture2D icon_foraging = content.Load<Texture2D>(Paths.INTERFACE_ICON_FORAGING);
            Texture2D icon_foraging_beach = content.Load<Texture2D>(Paths.INTERFACE_ICON_FORAGING_BEACH);
            Texture2D icon_foraging_flower = content.Load<Texture2D>(Paths.INTERFACE_ICON_FORAGING_FLOWER);
            Texture2D icon_foraging_mushroom = content.Load<Texture2D>(Paths.INTERFACE_ICON_FORAGING_MUSHROOM);
            Texture2D icon_insect_catching = content.Load<Texture2D>(Paths.INTERFACE_ICON_INSECT_CATCHING);
            Texture2D icon_mining = content.Load<Texture2D>(Paths.INTERFACE_ICON_MINING);
            Texture2D icon_pig = content.Load<Texture2D>(Paths.INTERFACE_ICON_PIG);
            Texture2D icon_sheep = content.Load<Texture2D>(Paths.INTERFACE_ICON_SHEEP);
            Texture2D icon_speed = content.Load<Texture2D>(Paths.INTERFACE_ICON_SPEED);
            Texture2D none = content.Load<Texture2D>(Paths.ITEM_NONE);

            //increases wood drops by 1 per level. 
            //if chopping a tree, increases tree-specific drops by 0.25 per level.
            CHOPPING_I = new Effect("Chopping I", "Slightly increases both the speed of woodcutting and\nthe quantity of items acquired.", icon_chopping, frame_i, none);
            CHOPPING_II = new Effect("Chopping II", "Increases both the speed of woodcutting and\nthe quantity of items acquired.", icon_chopping, frame_ii, none);
            CHOPPING_III = new Effect("Chopping III", "Greatly increases both the speed of woodcutting and\nthe quantity of items acquired.", icon_chopping, frame_iii, none);
            CHOPPING_IV = new Effect("Chopping IV", "Hugely increases both the speed of woodcutting and\nthe quantity of items acquired.", icon_chopping, frame_iv, none);

            //when rolling fish; rolls an extra 0.5x per level, and gives the item with the highest value out of all rolled fish.
            FISHING_I = new Effect("Fishing I", "Slightly decreases the time before a bite and\nincreases the rarity of items acquired while fishing.", icon_fishing, frame_i, none);
            FISHING_II = new Effect("Fishing II", "Decreases the time before a bite and\nincreases the rarity of items acquired while fishing.", icon_fishing, frame_ii, none);
            FISHING_III = new Effect("Fishing III", "Greatly decreases the time before a bite and\nincreases the rarity of items acquired while fishing.", icon_fishing, frame_iii, none);
            FISHING_IV = new Effect("Fishing IV", "Hugely decreases the time before a bite and\nincreases the rarity of items acquired while fishing.", icon_fishing, frame_iv, none);
            FISHING_II_OCEAN = new Effect("Fishing II (Ocean)", "Decreases the time before a bite and\nincreases the rarity of items acquired while fishing in the ocean.", icon_fishing_ocean, frame_ii, none);
            FISHING_III_CLOUD = new Effect("Fishing III (Cloud)", "Greatly decreases the time before a bite and\nincreases the rarity of items acquired while fishing in clouds.", icon_fishing_cloud, frame_iii, none);
            FISHING_III_FRESHWATER = new Effect("Fishing III (Freshwater)", "Greatly decreases the time before a bite and\nincreases the rarity of items acquired while fishing in freshwater.", icon_fishing_freshwater, frame_iii, none);
            FISHING_III_LAVA = new Effect("Fishing III (Lava)", "Greatly decreases the time before a bite and\nincreases the rarity of items acquired while fishing in magma.", icon_fishing_lava, frame_iii, none);
            FISHING_III_OCEAN = new Effect("Fishing III (Ocean)", "Greatly decreases the time before a bite and\nincreases the rarity of items acquired while fishing in the ocean.", icon_fishing_ocean, frame_iii, none);
            FISHING_IV_CAVE = new Effect("Fishing IV (Cave)", "Hugely decreases the time before a bite and\nincreases the rarity of items acquired while fishing in caves.", icon_fishing_cave, frame_iv, none);
            FISHING_IV_CLOUD = new Effect("Fishing IV (Cloud)", "Hugely decreases the time before a bite and\nincreases the rarity of items acquired while fishing in clouds.", icon_fishing_cloud, frame_iv, none);
            FISHING_IV_FRESHWATER = new Effect("Fishing IV (Freshwater)", "Hugely decreases the time before a bite and\nincreases the rarity of items acquired while fishing in freshwater.", icon_fishing_freshwater, frame_iv, none);
            FISHING_IV_LAVA = new Effect("Fishing IV (Lava)", "Hugely decreases the time before a bite and\nincreases the rarity of items acquired while fishing in magma.", icon_fishing_lava, frame_iv, none);
            FISHING_IV_OCEAN = new Effect("Fishing IV (Ocean)", "Hugely decreases the time before a bite and\nincreases the rarity of items acquired while fishing in the ocean.", icon_fishing_ocean, frame_iv, none);

            //increases forage by 0.5x per level
            //mushroom/flower forage: multiplies found mushrooms/flowers by 0.5x per level
            FORAGING_I = new Effect("Foraging I", "Slightly increases the amount of items found while foraging.", icon_foraging, frame_i, none);
            FORAGING_II = new Effect("Foraging II", "Increases the amount of items found while foraging.", icon_foraging, frame_ii, none);
            FORAGING_III = new Effect("Foraging III", "Greatly increases the amount of items found while foraging.", icon_foraging, frame_iii, none);
            FORAGING_IV = new Effect("Foraging IV", "Hugely increases the amount of items found while foraging.", icon_foraging, frame_iv, none);
            FORAGING_II_MUSHROOMS = new Effect("Foraging II (Mushrooms)", "Increases the amount of mushrooms found while foraging.", icon_foraging_mushroom, frame_ii, none);
            FORAGING_II_SPRING = new Effect("Foraging II (Spring)", "Increases the amount of items found while foraging in Spring.", icon_foraging, frame_ii, modifier_spring);
            FORAGING_II_SUMMER = new Effect("Foraging II (Summer)", "Increases the amount of items found while foraging in Summer.", icon_foraging, frame_ii, modifier_summer);
            FORAGING_II_WINTER = new Effect("Foraging II (Winter)", "Increases the amount of items found while foraging in Winter.", icon_foraging, frame_ii, modifier_winter);
            FORAGING_III_AUTUMN = new Effect("Foraging III (Autumn)", "Greatly increases the amount of items found while foraging in Autumn.", icon_foraging, frame_iii, modifier_autumn);
            FORAGING_III_SPRING = new Effect("Foraging III (Spring)", "Greatly increases the amount of items found while foraging in Spring.", icon_foraging, frame_iii, modifier_spring);
            FORAGING_IV_BEACH = new Effect("Foraging IV (Beach)", "Hugely increases the amount of items found while foraging on the beach.", icon_foraging_beach, frame_iv, none);
            FORAGING_IV_FLOWERS = new Effect("Foraging IV (Flowers)", "Hugely increases the amount of flowers found while foraging.", icon_foraging_flower, frame_iv, none);
            FORAGING_IV_MUSHROOMS = new Effect("Foraging IV (Mushrooms)", "Hugely increases the amount of mushrooms found while foraging.", icon_foraging_mushroom, frame_iv, none);
            FORAGING_IV_SUMMER = new Effect("Foraging IV (Summer)", "Hugely increases the amount of items found while foraging in Summer.", icon_foraging, frame_iv, modifier_summer);
            FORAGING_IV_WINTER = new Effect("Foraging IV (Winter)", "Hugely increases the amount of items found while foraging in Winter.", icon_foraging, frame_iv, modifier_winter);
            FORAGING_II_FLOWERS = new Effect("Foraging II (Flowers)", "Increases the amount of flowers found while foraging.", icon_foraging_flower, frame_i, none);

            //todo: all below
            //increases gathered items by 1-2
            GATHERING_BOAR = new Effect("Gathering (Boar)", "Boosts the quantity of items recieved when gathering from boar traps.", icon_boar, frame_v, none);
            GATHERING_CHICKEN = new Effect("Gathering (Chicken)", "Boosts the quantity of items recieved when gathering from chickens.", icon_chicken, frame_v, none);
            GATHERING_COW = new Effect("Gathering (Cow)", "Boosts the quantity of items recieved when milking cows.", icon_cow, frame_v, none);
            GATHERING_SHEEP = new Effect("Gathering (Sheep)", "Boosts the quantity of items recieved when shearing sheep.", icon_sheep, frame_v, none);
            GATHERING_PIG = new Effect("Gathering (Pig)", "Boosts the quantity of items produced by pigs.", icon_pig, frame_v, none);

            //increases gathered insects by 0.5x per level
            INSECT_CATCHING_I = new Effect("Insect Catching I", "Slightly increases the amount of insects caught and\nthe rarity of caught insects.", icon_insect_catching, frame_i, none);
            INSECT_CATCHING_II = new Effect("Insect Catching II", "Increases the amount of insects caught and\nthe rarity of caught insects.", icon_insect_catching, frame_ii, none);
            INSECT_CATCHING_III = new Effect("Insect Catching III", "Greatly increases the amount of insects caught and\nthe rarity of caught insects.", icon_insect_catching, frame_iii, none);
            INSECT_CATCHING_IV = new Effect("Insect Catching IV", "Hugely increases the amount of insects caught and\nthe rarity of caught insects.", icon_insect_catching, frame_iv, none);
            INSECT_CATCHING_IV_MORNING = new Effect("Insect Catching IV (Morning)", "Hugely increases the amount of insects caught and\nthe rarity of caught insects in the morning.", icon_insect_catching, frame_iv, modifier_morning);
            INSECT_CATCHING_IV_NIGHT = new Effect("Insect Catching IV (Night)", "Hugely increases the amount of insects caught and\nthe rarity of caught insects during nighttime.", icon_insect_catching, frame_iv, modifier_night);

            //increases mined items by 0.5x per level
            MINING_I = new Effect("Mining I", "Slightly increases both the speed of mining and\nthe quantity of items acquired.", icon_mining, frame_i, none);
            MINING_II = new Effect("Mining II", "Increases both the speed of mining and\nthe quantity of items acquired.", icon_mining, frame_ii, none);
            MINING_III = new Effect("Mining III", "Greatly increases both the speed of mining and\nthe quantity of items acquired.", icon_mining, frame_iii, none);
            MINING_IV = new Effect("Mining IV", "Hugely increases both the speed of mining and\nthe quantity of items acquired.", icon_mining, frame_iv, none);

            //increases maximum movement speed
            SPEED_I = new Effect("Speed I", "Slightly increases movement speed.", icon_speed, frame_ii, none);
            SPEED_II = new Effect("Speed II", "Increases movement speed.", icon_speed, frame_ii, none);
            SPEED_III = new Effect("Speed III", "Greatly increases movement speed.", icon_speed, frame_iii, none);
            SPEED_IV = new Effect("Speed IV", "Hugely increases movement speed.", icon_speed, frame_iv, none);
            SPEED_I_MORNING = new Effect("Speed I (Morning)", "Slightly increases movement speed.", icon_speed, frame_i, modifier_morning);
            SPEED_II_MORNING = new Effect("Speed II (Morning)", "Increases movement speed.", icon_speed, frame_ii, modifier_morning);
            SPEED_III_AUTUMN = new Effect("Speed III (Autumn)", "Greatly increases movement speed.", icon_speed, frame_iii, modifier_autumn);
            SPEED_III_MORNING = new Effect("Speed III (Morning)", "Greatly increases movement speed.", icon_speed, frame_iii, modifier_morning);
            SPEED_III_WINTER = new Effect("Speed III (Winter)", "Greatly increases movement speed.", icon_speed, frame_iii, modifier_winter);
            SPEED_IV_AUTUMN = new Effect("Speed IV (Autumn)", "Hugely increases movement speed.", icon_speed, frame_iv, modifier_autumn);
            SPEED_IV_SPRING = new Effect("Speed IV (Spring)", "Hugely increases movement speed.", icon_speed, frame_iv, modifier_spring);
            SPEED_IV_WINTER = new Effect("Speed IV (Winter)", "Hugely increases movement speed.", icon_speed, frame_iv, modifier_winter);
        }
    }
}
