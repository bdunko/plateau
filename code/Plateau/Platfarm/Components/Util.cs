﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Platfarm.Entities;
using Platfarm.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Components
{
    public class Util
    {
        public class NamedColor
        {
            public string name;
            public Color color;

            public NamedColor(Color color, string name)
            {
                this.color = color;
                this.name = name;
            }
        }

        public class RecolorMap {
            public string name;
            public Dictionary<Color, Color> map;

            public RecolorMap(string name)
            {
                this.name = name;
                this.map = new Dictionary<Color, Color>();
            }

            public void AddMapping(Color greyscale, Color recolor) {
                map[greyscale] = recolor;
            }

            public Color GetRecolorForGreyscale(Color greyscale)
            {
                if(map.ContainsKey(greyscale))
                {
                    return map[greyscale];
                }
                return greyscale;
            }

            public string GetName()
            {
                return name;
            }
        }

        private static Texture2D debugDrawRect;
        private static List<DebugDrawElement> debugDrawElements;
        private static Dictionary<string, int> idCounterDict;

        public static NamedColor PLATEAU_ORANGE_PRIMARY = new NamedColor(new Color(168, 57, 14), "Plateau Orange Primary");
        public static NamedColor PLATEAU_ORANGE_SECONDARY = new NamedColor(new Color(84, 23, 13), "Plateau Orange Secondary");
        public static NamedColor SAND_PRIMARY = new NamedColor(new Color(224, 190, 136), "Sand Primary");
        public static NamedColor SAND_SECONDARY = new NamedColor(new Color(187, 117, 71), "Sand Secondary");
        public static NamedColor BRIDGE_PRIMARY = new NamedColor(new Color(186, 76, 33), "Bridge Primary");
        public static NamedColor BRIDGE_SECONDARY = new NamedColor(new Color(74, 18, 8), "Bridge Secondary");
        public static NamedColor WOOD_PRIMARY = new NamedColor(new Color(186, 117, 106),"Wood Primary");
        public static NamedColor WOOD_SECONDARY = new NamedColor(new Color(91, 49, 56), "Wood Secondary");

        public static NamedColor CLOUD_FRONT_DAY = new NamedColor(new Color(245, 245, 245), "Day Clouds Front");
        public static NamedColor CLOUD_MIDDLE_DAY = new NamedColor(new Color(200, 200, 200), "Day Clouds Middle");
        public static NamedColor CLOUD_BACK_DAY = new NamedColor(new Color(160, 160, 160), "Day Clouds Back");
        public static NamedColor CLOUD_FRONT_EVENING = new NamedColor(new Color(245, 245, 0), "Evening Clouds Front");
        public static NamedColor CLOUD_MIDDLE_EVENING = new NamedColor(new Color(255, 165, 0), "Evening Clouds Middle");
        public static NamedColor CLOUD_BACK_EVENING = new NamedColor(new Color(235, 145, 0), "Evening Clouds Back");
        public static NamedColor CLOUD_FRONT_NIGHT = new NamedColor(new Color(184, 169, 255), "Night Clouds Front");
        public static NamedColor CLOUD_MIDDLE_NIGHT = new NamedColor(new Color(124, 109, 211), "Night Clouds Middle");
        public static NamedColor CLOUD_BACK_NIGHT = new NamedColor(new Color(120, 68, 185), "Night Clouds Back");
        public static NamedColor CLOUD_RAIN = new NamedColor(new Color(74, 84, 98), "Rain Clouds");
        public static float FOREGROUND_TRANSPARENCY = 0.85f;
        public static float FOREGROUND_TRANSPARENCY_CLOUDS = 0.4f;

        public static NamedColor TRANSPARENT = new NamedColor(new Color(0, 0, 0, 0), "Transparent");

        public static NamedColor PARTICLE_STONE_PRIMARY = new NamedColor(new Color(109, 117, 141), "Particle Stone Primary");
        public static NamedColor PARTICLE_STONE_SECONDARY = new NamedColor(new Color(74, 84, 98), "Particle Stone Secondary");
        public static NamedColor PARTICLE_WEEDS_PRIMARY = new NamedColor(new Color(20, 160, 46), "Particle Weeds Primary");
        public static NamedColor PARTICLE_WEEDS_SECONDARY = new NamedColor(new Color(36, 82, 59), "Particle Weeds Secondary");
        public static NamedColor PARTICLE_BRANCH_PRIMARY = new NamedColor(new Color(187, 117, 71), "Particle Branch Primary");
        public static NamedColor PARTICLE_BRANCH_SECONDARY = new NamedColor(new Color(113, 65, 59), "Particle Branch Secondary");
        public static NamedColor PARTICLE_DUST = new NamedColor(new Color(187, 122, 48), "Particle Dust");
        public static NamedColor PARTICLE_BLUE_RISER = new NamedColor(new Color(166, 175, 255), "Particle Blue Riser");
        public static NamedColor PARTICLE_GRASS_SPRING_PRIMARY = new NamedColor(new Color(89, 193, 53), "Particle Grass Sp Primary");
        public static NamedColor PARTICLE_GRASS_SPRING_SECONDARY = new NamedColor(new Color(26, 122, 62), "Particle Grass Sp Secondary");
        public static NamedColor PARTICLE_GRASS_SUMMER_PRIMARY = new NamedColor(new Color(156, 219, 67), "Particle Grass Su Primary");
        public static NamedColor PARTICLE_GRASS_SUMMER_SECONDARY = new NamedColor(new Color(20, 160, 46), "Particle Grass Su Secondary");
        public static NamedColor PARTICLE_GRASS_FALL_PRIMARY = new NamedColor(new Color(250, 106, 10), "Particle Grass Au Primary");
        public static NamedColor PARTICLE_GRASS_FALL_SECONDARY = new NamedColor(new Color(180, 32, 42), "Particle Grass Au Secondary");
        public static NamedColor PARTICLE_GRASS_WINTER_PRIMARY = new NamedColor(new Color(146, 220, 186), "Particle Grass Wi Primary");
        public static NamedColor PARTICLE_GRASS_WINTER_SECONDARY = new NamedColor(new Color(50, 132, 100), "Particle Grass Wi Secondary");
        public static NamedColor PARTICLE_CHERRY_SPRING_PRIMARY = new NamedColor(new Color(250, 214, 184), "Particle Cherry Spring Primary");
        public static NamedColor PARTICLE_CHERRY_SPRING_SECONDARY = new NamedColor(new Color(232, 106, 115), "Particle Cherry Spring Secondary");
        public static NamedColor PARTICLE_WATER_PRIMARY = new NamedColor(new Color(32, 214, 199), "Particle Water Primary");
        public static NamedColor PARTICLE_WATER_SECONDARY = new NamedColor(new Color(40, 92, 196), "Particle Water Secondary");

        public static NamedColor RAIN_FILTER = new NamedColor(new Color(20, 52, 100) * 0.3f, "Filter Rain");
        public static NamedColor CLOUDY_FILTER = new NamedColor(new Color(179, 185, 209) * (16.0f/255.0f), "Filter Cloudy");
        public static NamedColor SNOWY_FILTER = new NamedColor(new Color(185, 191, 251) * (26.0f / 255.0f), "Filter Snowy");
        public static NamedColor SPRING_FILTER =  new NamedColor(new Color(232, 106, 115) *(10.0f/255.0f), "Filter Spring");
        public static NamedColor SUMMER_FILTER = new NamedColor(new Color(20, 160, 46) * (10.0f/255.0f), "Filter Summer");
        public static NamedColor FALL_FILTER = new NamedColor(new Color(180, 132, 42) * (10.0f / 255.0f), "Filter Fall");
        public static NamedColor WINTER_FILTER = new NamedColor(new Color(132, 155, 228) * (15.0f/255.0f), "Filter Winter");

        public static NamedColor PARTICLE_SPRING_PETAL_FOREGROUND = new NamedColor(new Color(250, 214, 184), "Particle Spring Petal FG");
        public static NamedColor PARTICLE_SPRING_PETAL_BACKGROUND = new NamedColor(new Color(245, 160, 151), "Particle Spring Petal BG");
        public static NamedColor PARTICLE_SUMMER_LEAF_FOREGROUND = new NamedColor(new Color(20, 160, 46), "Particle Summer Leaf FG");
        public static NamedColor PARTICLE_SUMMER_LEAF_BACKGROUND = new NamedColor(new Color(26, 122, 62), "Particle Summer Leaf BG");
        public static NamedColor PARTICLE_FALL_LEAF_FOREGROUND = new NamedColor(new Color(250, 106, 10), "Particle Fall Leaf FG");
        public static NamedColor PARTICLE_FALL_LEAF_BACKGROUND = new NamedColor(new Color(223, 62, 35), "Particle Fall Leaf BG");
        public static NamedColor PARTICLE_WINTER_SNOW_FOREGROUND = new NamedColor(new Color(205, 247, 226), "Particle Winter Snow FG");
        public static NamedColor PARTICLE_WINTER_SNOW_BACKGROUND = new NamedColor(new Color(146, 220, 186), "Particle Winter Snow BG");
        public static NamedColor PARTICLE_SPRING_PETAL_FOREGROUND_TRANSPARENT = new NamedColor(PARTICLE_SPRING_PETAL_FOREGROUND.color * FOREGROUND_TRANSPARENCY, "Particle Spring Petal FG Tr");
        public static NamedColor PARTICLE_SUMMER_LEAF_FOREGROUND_TRANSPARENT = new NamedColor(PARTICLE_SUMMER_LEAF_FOREGROUND.color * FOREGROUND_TRANSPARENCY, "Particle Summer Leaf FG Tr");
        public static NamedColor PARTICLE_FALL_LEAF_FOREGROUND_TRANSPARENT = new NamedColor(PARTICLE_FALL_LEAF_FOREGROUND.color * FOREGROUND_TRANSPARENCY, "Particle Fall Leaf FG Tr");
        public static NamedColor PARTICLE_WINTER_SNOW_FOREGROUND_TRANSPARENT = new NamedColor(PARTICLE_WINTER_SNOW_FOREGROUND.color * FOREGROUND_TRANSPARENCY, "Particle Winter Snow FG Tr");

        public static NamedColor GREYSCALE_255 = new NamedColor(new Color(255, 255, 255), "GS-255");
        public static NamedColor GREYSCALE_225 = new NamedColor(new Color(225, 225, 225), "GS-225");
        public static NamedColor GREYSCALE_195 = new NamedColor(new Color(195, 195, 195), "GS-195");
        public static NamedColor GREYSCALE_165 = new NamedColor(new Color(165, 165, 165), "GS-165");
        public static NamedColor GREYSCALE_135 = new NamedColor(new Color(135, 135, 135), "GS-135");
        public static NamedColor GREYSCALE_105 = new NamedColor(new Color(105, 105, 105), "GS-105");
        public static NamedColor GREYSCALE_75 = new NamedColor(new Color(75, 75, 75), "GS-75");
        public static NamedColor GREYSCALE_45 = new NamedColor(new Color(45, 45, 45), "GS-45");

        public static RecolorMap BLACK = new RecolorMap("Black");
        public static RecolorMap BLUE = new RecolorMap("Blue");
        public static RecolorMap BROWN = new RecolorMap("Brown");
        public static RecolorMap DARK_GREY = new RecolorMap("Dark Grey");
        public static RecolorMap LIGHT_GREY = new RecolorMap("Light Grey");
        public static RecolorMap NAVY = new RecolorMap("Navy");
        public static RecolorMap GREEN = new RecolorMap("Green");
        public static RecolorMap OLIVE = new RecolorMap("Olive");
        public static RecolorMap ORANGE = new RecolorMap("Orange");
        public static RecolorMap PINK = new RecolorMap("Pink");
        public static RecolorMap PURPLE = new RecolorMap("Purple");
        public static RecolorMap RED = new RecolorMap("Red");
        public static RecolorMap WHITE = new RecolorMap("White");
        public static RecolorMap YELLOW = new RecolorMap("Yellow");

        public static RecolorMap HAIR_TREEBARK_BROWN = new RecolorMap("Hair Treebark Brown");
        public static RecolorMap HAIR_CHARCOAL_BLACK = new RecolorMap("Hair Charcoal Black");
        public static RecolorMap HAIR_SNOW_WHITE = new RecolorMap("Hair Snow White");
        public static RecolorMap HAIR_SOLAR_BLOND = new RecolorMap("Hair Solar Blond");
        public static RecolorMap HAIR_FLARE_GINGER = new RecolorMap("Hair Flare Ginger");
        public static RecolorMap HAIR_CHERRYWOOD_BROWN = new RecolorMap("Hair Cherrywood Brown");
        public static RecolorMap HAIR_SAND_BLOND = new RecolorMap("Hair Sand Blond");
        public static RecolorMap HAIR_DUST_BLOND = new RecolorMap("Hair Dust Blond");
        public static RecolorMap HAIR_ANTIQUE_SILVER = new RecolorMap("Hair Antique Silver");
        public static RecolorMap HAIR_RUSTED_GREY = new RecolorMap("Hair Rusted Grey");
        public static RecolorMap HAIR_MAGMA_RED = new RecolorMap("Hair Magma Red");
        public static RecolorMap HAIR_FOREST_GREEN = new RecolorMap("Hair Forest Green");
        public static RecolorMap HAIR_NEON_GREEN = new RecolorMap("Hair Neon Green");
        public static RecolorMap HAIR_ABYSS_NAVY = new RecolorMap("Hair Abyss Navy");
        public static RecolorMap HAIR_CHRYSALIS_BLUE = new RecolorMap("Hair Crysalis Blue");
        public static RecolorMap HAIR_SWEET_PINK = new RecolorMap("Hair Sweet Pink");
        public static RecolorMap HAIR_MOTH_PURPLE = new RecolorMap("Hair Moth Purple");
        public static RecolorMap HAIR_GREY_FROST = new RecolorMap("Hair Grey Frost");
        public static RecolorMap HAIR_WINTER_MINT = new RecolorMap("Hair Winter Mint");
        public static RecolorMap HAIR_VINYL_BROWN = new RecolorMap("Hair Vinyl Brown");

        public static RecolorMap[] colors = { BLACK, BLUE, BROWN, DARK_GREY, LIGHT_GREY, NAVY, GREEN, OLIVE, ORANGE, PINK, PURPLE, RED, YELLOW, WHITE};
        public static RecolorMap[] hairColors = { HAIR_TREEBARK_BROWN, HAIR_CHARCOAL_BLACK, HAIR_SNOW_WHITE, HAIR_SOLAR_BLOND, HAIR_FLARE_GINGER, HAIR_CHERRYWOOD_BROWN, HAIR_SAND_BLOND, HAIR_DUST_BLOND,
        HAIR_ANTIQUE_SILVER, HAIR_RUSTED_GREY, HAIR_MAGMA_RED, HAIR_FOREST_GREEN, HAIR_NEON_GREEN, HAIR_ABYSS_NAVY, HAIR_CHRYSALIS_BLUE, HAIR_SWEET_PINK, HAIR_MOTH_PURPLE, HAIR_GREY_FROST, HAIR_WINTER_MINT, HAIR_VINYL_BROWN};

        public static NamedColor DEFAULT_COLOR = new NamedColor(new Color(255, 255, 255), "Default");

        public static void Initialize()
        {
            idCounterDict = new Dictionary<string, int>();
            debugDrawElements = new List<DebugDrawElement>();
            debugDrawRect = new Texture2D(Plateau.GRAPHICS.GraphicsDevice, 1, 1);
            debugDrawRect.SetData(new[] { Color.White });

            BLACK.AddMapping(GREYSCALE_255.color, new Color(50, 43, 40));
            BLACK.AddMapping(GREYSCALE_225.color, new Color(34, 28, 26));
            BLACK.AddMapping(GREYSCALE_195.color, new Color(20, 16, 19));
            BLACK.AddMapping(GREYSCALE_165.color, new Color(20, 16, 19));
            BLACK.AddMapping(GREYSCALE_135.color, new Color(6, 6, 6));
            BLACK.AddMapping(GREYSCALE_105.color, new Color(6, 6, 6));
            BLACK.AddMapping(GREYSCALE_75.color, new Color(6, 6, 6));
            BLACK.AddMapping(GREYSCALE_45.color, new Color(6, 6, 6));

            BLUE.AddMapping(GREYSCALE_255.color, new Color(32, 214, 199));
            BLUE.AddMapping(GREYSCALE_225.color, new Color(36, 159, 222));
            BLUE.AddMapping(GREYSCALE_195.color, new Color(40, 92, 196));
            BLUE.AddMapping(GREYSCALE_165.color, new Color(20, 52, 100));
            BLUE.AddMapping(GREYSCALE_135.color, new Color(20, 52, 100));
            BLUE.AddMapping(GREYSCALE_105.color, new Color(20, 52, 100));
            BLUE.AddMapping(GREYSCALE_75.color, new Color(20, 52, 100));
            BLUE.AddMapping(GREYSCALE_45.color, new Color(20, 52, 100));

            BROWN.AddMapping(GREYSCALE_255.color, new Color(219, 164, 99));
            BROWN.AddMapping(GREYSCALE_225.color, new Color(187, 117, 71));
            BROWN.AddMapping(GREYSCALE_195.color, new Color(113, 65, 59));
            BROWN.AddMapping(GREYSCALE_165.color, new Color(113, 65, 59));
            BROWN.AddMapping(GREYSCALE_135.color, new Color(91, 49, 56));
            BROWN.AddMapping(GREYSCALE_105.color, new Color(66, 36, 51));
            BROWN.AddMapping(GREYSCALE_75.color, new Color(50, 43, 40));
            BROWN.AddMapping(GREYSCALE_45.color, new Color(34, 28, 26));

            DARK_GREY.AddMapping(GREYSCALE_255.color, new Color(109, 117, 141));
            DARK_GREY.AddMapping(GREYSCALE_225.color, new Color(74, 84, 98));
            DARK_GREY.AddMapping(GREYSCALE_195.color, new Color(51, 57, 65));
            DARK_GREY.AddMapping(GREYSCALE_165.color, new Color(51, 57, 65));
            DARK_GREY.AddMapping(GREYSCALE_135.color, new Color(51, 57, 65));
            DARK_GREY.AddMapping(GREYSCALE_105.color, new Color(51, 57, 65));
            DARK_GREY.AddMapping(GREYSCALE_75.color, new Color(51, 57, 65));
            DARK_GREY.AddMapping(GREYSCALE_45.color, new Color(20, 16, 19));

            LIGHT_GREY.AddMapping(GREYSCALE_255.color, new Color(218, 224, 234));
            LIGHT_GREY.AddMapping(GREYSCALE_225.color, new Color(179, 185, 209));
            LIGHT_GREY.AddMapping(GREYSCALE_195.color, new Color(139, 147, 175));
            LIGHT_GREY.AddMapping(GREYSCALE_165.color, new Color(109, 117, 141));
            LIGHT_GREY.AddMapping(GREYSCALE_135.color, new Color(109, 117, 141));
            LIGHT_GREY.AddMapping(GREYSCALE_105.color, new Color(74, 84, 98));
            LIGHT_GREY.AddMapping(GREYSCALE_75.color, new Color(74, 84, 98));
            LIGHT_GREY.AddMapping(GREYSCALE_45.color, new Color(51, 57, 65));

            NAVY.AddMapping(GREYSCALE_255.color, new Color(40, 92, 196));
            NAVY.AddMapping(GREYSCALE_225.color, new Color(20, 52, 100));
            NAVY.AddMapping(GREYSCALE_195.color, new Color(36, 34, 52));
            NAVY.AddMapping(GREYSCALE_165.color, new Color(36, 34, 52));
            NAVY.AddMapping(GREYSCALE_135.color, new Color(36, 34, 52));
            NAVY.AddMapping(GREYSCALE_105.color, new Color(20, 16, 19));
            NAVY.AddMapping(GREYSCALE_75.color, new Color(20, 16, 19));
            NAVY.AddMapping(GREYSCALE_45.color, new Color(6, 6, 6));

            GREEN.AddMapping(GREYSCALE_255.color, new Color(156, 219, 67));
            GREEN.AddMapping(GREYSCALE_225.color, new Color(89, 193, 53));
            GREEN.AddMapping(GREYSCALE_195.color, new Color(20, 160, 46));
            GREEN.AddMapping(GREYSCALE_165.color, new Color(20, 160, 46));
            GREEN.AddMapping(GREYSCALE_135.color, new Color(26, 122, 62));
            GREEN.AddMapping(GREYSCALE_105.color, new Color(36, 82, 59));
            GREEN.AddMapping(GREYSCALE_75.color, new Color(36, 82, 59));
            GREEN.AddMapping(GREYSCALE_45.color, new Color(18, 32, 32));

            OLIVE.AddMapping(GREYSCALE_255.color, new Color(26, 122, 62));
            OLIVE.AddMapping(GREYSCALE_225.color, new Color(36, 82, 59));
            OLIVE.AddMapping(GREYSCALE_195.color, new Color(18, 32, 32));
            OLIVE.AddMapping(GREYSCALE_165.color, new Color(18, 32, 32));
            OLIVE.AddMapping(GREYSCALE_135.color, new Color(18, 32, 32));
            OLIVE.AddMapping(GREYSCALE_105.color, new Color(20, 16, 19));
            OLIVE.AddMapping(GREYSCALE_75.color, new Color(20, 16, 19));
            OLIVE.AddMapping(GREYSCALE_45.color, new Color(20, 16, 19));

            ORANGE.AddMapping(GREYSCALE_255.color, new Color(249, 163, 27));
            ORANGE.AddMapping(GREYSCALE_225.color, new Color(250, 106, 10));
            ORANGE.AddMapping(GREYSCALE_195.color, new Color(223, 62, 35));
            ORANGE.AddMapping(GREYSCALE_165.color, new Color(223, 62, 35));
            ORANGE.AddMapping(GREYSCALE_135.color, new Color(223, 62, 35));
            ORANGE.AddMapping(GREYSCALE_105.color, new Color(180, 32, 42));
            ORANGE.AddMapping(GREYSCALE_75.color, new Color(115, 23, 45));
            ORANGE.AddMapping(GREYSCALE_45.color, new Color(115, 23, 45));

            PINK.AddMapping(GREYSCALE_255.color, new Color(250, 214, 184));
            PINK.AddMapping(GREYSCALE_225.color, new Color(245, 160, 151));
            PINK.AddMapping(GREYSCALE_195.color, new Color(232, 106, 115));
            PINK.AddMapping(GREYSCALE_165.color, new Color(188, 74, 155));
            PINK.AddMapping(GREYSCALE_135.color, new Color(188, 74, 155));
            PINK.AddMapping(GREYSCALE_105.color, new Color(121, 58, 128));
            PINK.AddMapping(GREYSCALE_75.color, new Color(121, 58, 128));
            PINK.AddMapping(GREYSCALE_45.color, new Color(121, 58, 128));

            PURPLE.AddMapping(GREYSCALE_255.color, new Color(188, 74, 155));
            PURPLE.AddMapping(GREYSCALE_225.color, new Color(121, 58, 128));
            PURPLE.AddMapping(GREYSCALE_195.color, new Color(64, 51, 83));
            PURPLE.AddMapping(GREYSCALE_165.color, new Color(64, 51, 83));
            PURPLE.AddMapping(GREYSCALE_135.color, new Color(36, 34, 52));
            PURPLE.AddMapping(GREYSCALE_105.color, new Color(36, 34, 52));
            PURPLE.AddMapping(GREYSCALE_75.color, new Color(36, 34, 52));
            PURPLE.AddMapping(GREYSCALE_45.color, new Color(34, 28, 26));

            RED.AddMapping(GREYSCALE_255.color, new Color(223, 62, 35));
            RED.AddMapping(GREYSCALE_225.color, new Color(180, 32, 42));
            RED.AddMapping(GREYSCALE_195.color, new Color(115, 23, 45));
            RED.AddMapping(GREYSCALE_165.color, new Color(115, 23, 45));
            RED.AddMapping(GREYSCALE_135.color, new Color(59, 23, 37));
            RED.AddMapping(GREYSCALE_105.color, new Color(59, 23, 37));
            RED.AddMapping(GREYSCALE_75.color, new Color(20, 16, 19));
            RED.AddMapping(GREYSCALE_45.color, new Color(20, 16, 19));

            YELLOW.AddMapping(GREYSCALE_255.color, new Color(255, 252, 64));
            YELLOW.AddMapping(GREYSCALE_225.color, new Color(255, 213, 65));
            YELLOW.AddMapping(GREYSCALE_195.color, new Color(249, 163, 27));
            YELLOW.AddMapping(GREYSCALE_165.color, new Color(250, 106, 10));
            YELLOW.AddMapping(GREYSCALE_135.color, new Color(250, 106, 10));
            YELLOW.AddMapping(GREYSCALE_105.color, new Color(250, 106, 10));
            YELLOW.AddMapping(GREYSCALE_75.color, new Color(250, 106, 10));
            YELLOW.AddMapping(GREYSCALE_45.color, new Color(223, 62, 35));

            WHITE.AddMapping(GREYSCALE_255.color, new Color(255, 255, 255));
            WHITE.AddMapping(GREYSCALE_225.color, new Color(254, 243, 192));
            WHITE.AddMapping(GREYSCALE_195.color, new Color(250, 214, 184));
            WHITE.AddMapping(GREYSCALE_165.color, new Color(250, 214, 184));
            WHITE.AddMapping(GREYSCALE_135.color, new Color(250, 214, 184));
            WHITE.AddMapping(GREYSCALE_105.color, new Color(245, 160, 151));
            WHITE.AddMapping(GREYSCALE_75.color, new Color(245, 160, 151));
            WHITE.AddMapping(GREYSCALE_45.color, new Color(232, 106, 115));

            HAIR_TREEBARK_BROWN.AddMapping(GREYSCALE_255.color, new Color(146, 86, 63));
            HAIR_TREEBARK_BROWN.AddMapping(GREYSCALE_225.color, new Color(118, 65, 50));
            HAIR_CHARCOAL_BLACK.AddMapping(GREYSCALE_255.color, new Color(34, 28, 26));
            HAIR_CHARCOAL_BLACK.AddMapping(GREYSCALE_225.color, new Color(20, 16, 19));
            HAIR_SNOW_WHITE.AddMapping(GREYSCALE_255.color, new Color(255, 255, 255));
            HAIR_SNOW_WHITE.AddMapping(GREYSCALE_225.color, new Color(254, 243, 192));
            HAIR_SOLAR_BLOND.AddMapping(GREYSCALE_255.color, new Color(249, 163, 27));
            HAIR_SOLAR_BLOND.AddMapping(GREYSCALE_225.color, new Color(255, 213, 65));
            HAIR_FLARE_GINGER.AddMapping(GREYSCALE_255.color, new Color(250, 106, 10));
            HAIR_FLARE_GINGER.AddMapping(GREYSCALE_225.color, new Color(223, 62, 35));
            HAIR_CHERRYWOOD_BROWN.AddMapping(GREYSCALE_255.color, new Color(91, 49, 56));
            HAIR_CHERRYWOOD_BROWN.AddMapping(GREYSCALE_225.color, new Color(66, 36, 51));
            HAIR_SAND_BLOND.AddMapping(GREYSCALE_255.color, new Color(228, 210, 170));
            HAIR_SAND_BLOND.AddMapping(GREYSCALE_225.color, new Color(199, 176, 139));
            HAIR_DUST_BLOND.AddMapping(GREYSCALE_255.color, new Color(244, 210, 156));
            HAIR_DUST_BLOND.AddMapping(GREYSCALE_225.color, new Color(219, 164, 99));
            HAIR_ANTIQUE_SILVER.AddMapping(GREYSCALE_255.color, new Color(179, 185, 209));
            HAIR_ANTIQUE_SILVER.AddMapping(GREYSCALE_225.color, new Color(139, 147, 175));
            HAIR_RUSTED_GREY.AddMapping(GREYSCALE_255.color, new Color(74, 84, 98));
            HAIR_RUSTED_GREY.AddMapping(GREYSCALE_225.color, new Color(51, 57, 65));
            HAIR_MAGMA_RED.AddMapping(GREYSCALE_255.color, new Color(180, 32, 42));
            HAIR_MAGMA_RED.AddMapping(GREYSCALE_225.color, new Color(115, 23, 45));
            HAIR_FOREST_GREEN.AddMapping(GREYSCALE_255.color, new Color(26, 122, 62));
            HAIR_FOREST_GREEN.AddMapping(GREYSCALE_225.color, new Color(36, 82, 59));
            HAIR_NEON_GREEN.AddMapping(GREYSCALE_255.color, new Color(156, 219, 67));
            HAIR_NEON_GREEN.AddMapping(GREYSCALE_225.color, new Color(89, 193, 53));
            HAIR_ABYSS_NAVY.AddMapping(GREYSCALE_255.color, new Color(20, 52, 100));
            HAIR_ABYSS_NAVY.AddMapping(GREYSCALE_225.color, new Color(18, 32, 32));
            HAIR_CHRYSALIS_BLUE.AddMapping(GREYSCALE_255.color, new Color(32, 214, 199));
            HAIR_CHRYSALIS_BLUE.AddMapping(GREYSCALE_225.color, new Color(36, 159, 222));
            HAIR_SWEET_PINK.AddMapping(GREYSCALE_255.color, new Color(245, 160, 151));
            HAIR_SWEET_PINK.AddMapping(GREYSCALE_225.color, new Color(232, 106, 115));
            HAIR_MOTH_PURPLE.AddMapping(GREYSCALE_255.color, new Color(121, 58, 128));
            HAIR_MOTH_PURPLE.AddMapping(GREYSCALE_225.color, new Color(64, 51, 83));
            HAIR_GREY_FROST.AddMapping(GREYSCALE_255.color, new Color(185, 191, 251));
            HAIR_GREY_FROST.AddMapping(GREYSCALE_225.color, new Color(132, 155, 228));
            HAIR_WINTER_MINT.AddMapping(GREYSCALE_255.color, new Color(93, 175, 141));
            HAIR_WINTER_MINT.AddMapping(GREYSCALE_225.color, new Color(50, 132, 100));
            HAIR_VINYL_BROWN.AddMapping(GREYSCALE_255.color, new Color(90, 78, 68));
            HAIR_VINYL_BROWN.AddMapping(GREYSCALE_225.color, new Color(66, 57, 52));
        }

        public static void UnloadContent()
        {
            debugDrawRect.Dispose();
        }

        public static float DistanceBetweenVec2(Vector2 p1, Vector2 p2)
        {
            return (float)Math.Sqrt( Math.Pow((p2.X-p1.X), 2) + Math.Pow((p2.Y-p1.Y), 2));
        }

        public static float[] CreateAndFillArray(int size, float fillValue)
        {
            float[] arr = new float[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = fillValue;
            }
            return arr;
        }

        public static Vector2 Vec2FloatToInt(Vector2 floatVec)
        {
            return new Vector2((int)floatVec.X, (int)floatVec.Y);
        }

        public static Vector2 CenterTextureOnPoint(Vector2 center, int texWidth, int texHeight)
        {
            return new Vector2(center.X - (texWidth / 2), center.Y - (texHeight / 2));
        }

        public static float AdjustTowards(float valueCurrent, float valueAdjustedTowards, float adjustmentSpeed)
        {
            if (valueCurrent < valueAdjustedTowards)
            {
                valueCurrent += adjustmentSpeed;
                if (valueCurrent > valueAdjustedTowards)
                {
                    valueCurrent = valueAdjustedTowards;
                }
            } else if (valueCurrent > valueAdjustedTowards)
            {
                valueCurrent -= adjustmentSpeed;
                if (valueCurrent < valueAdjustedTowards)
                {
                    valueCurrent = valueAdjustedTowards;
                }
            }
            return valueCurrent;
        }

        public static int AdjustTowards(int valueCurrent, int valueAdjustedTowards, int adjustmentSpeed)
        {
            if (valueCurrent < valueAdjustedTowards)
            {
                valueCurrent += adjustmentSpeed;
                if (valueCurrent > valueAdjustedTowards)
                {
                    valueCurrent = valueAdjustedTowards;
                }
            }
            else if (valueCurrent > valueAdjustedTowards)
            {
                valueCurrent -= adjustmentSpeed;
                if (valueCurrent < valueAdjustedTowards)
                {
                    valueCurrent = valueAdjustedTowards;
                }
            }
            return valueCurrent;
        }

        public static int ValueFromComponents(float multiplier = 1.0f, int baseBonus = 0, params Item[] comps)
        {
            int total = baseBonus;

            foreach(Item item in comps)
            {
                total += item.GetValue();
            }

            return (int)(total * multiplier);
        }

        public static Vector2 ConvertFromAbsoluteToCameraVector(RectangleF cameraBoundingBox, Vector2 positions)
        {
            Vector2 convertedPos = new Vector2(cameraBoundingBox.Left, cameraBoundingBox.Top);
            convertedPos.X += positions.X;
            convertedPos.Y += positions.Y;
            return convertedPos;
        }

        public static RectangleF ConvertFromAbsoluteToCameraVector(RectangleF cameraBoundingBox, RectangleF rectangle)
        {
            RectangleF convertedRec = new RectangleF(cameraBoundingBox.Left, cameraBoundingBox.Top, rectangle.Width, rectangle.Height);
            convertedRec.X += rectangle.X;
            convertedRec.Y += rectangle.Y;
            return convertedRec;
        }

        private static Random random = new Random();

        public static int RandInt(int minInc, int maxInc)
        {
            return random.Next(minInc, maxInc + 1);
        }

        public static float SALE_VALUE_MULT = 1.5f;
        public static int GetSaleValue(Item item)
        {
            return (int)(item.GetValue() * SALE_VALUE_MULT);
        }

        public enum RecolorAdjustment
        {
            LIGHTEN, EXTRA_DARKEN, DARKEN, NORMAL
        }

        private static int ADJUSTMENT_AMOUNT = 12;

        public static Texture2D GenerateRecolor(Texture2D greyscaleTexture, RecolorMap recolorMap, RecolorAdjustment adjustment = RecolorAdjustment.NORMAL)
        {
            Texture2D recoloredTexture = new Texture2D(Plateau.GRAPHICS.GraphicsDevice, greyscaleTexture.Width, greyscaleTexture.Height);
            Color[] recoloredColors = new Color[recoloredTexture.Width * recoloredTexture.Height];
            Color[] greyscaleColors = new Color[greyscaleTexture.Width * greyscaleTexture.Height];
            greyscaleTexture.GetData<Color>(greyscaleColors);
            for(int i = 0; i < recoloredColors.Length; i++)
            {
                Color baseRecolor = recolorMap.GetRecolorForGreyscale(greyscaleColors[i]);
                if (baseRecolor.A != 0)
                {
                    if (adjustment == RecolorAdjustment.DARKEN)
                    {
                        baseRecolor.R = (byte)Util.AdjustTowards(baseRecolor.R, 0, ADJUSTMENT_AMOUNT);
                        baseRecolor.G = (byte)Util.AdjustTowards(baseRecolor.G, 0, ADJUSTMENT_AMOUNT);
                        baseRecolor.B = (byte)Util.AdjustTowards(baseRecolor.B, 0, ADJUSTMENT_AMOUNT);
                    }
                    else if (adjustment == RecolorAdjustment.LIGHTEN)
                    {
                        baseRecolor.R = (byte)Util.AdjustTowards(baseRecolor.R, 255, ADJUSTMENT_AMOUNT);
                        baseRecolor.G = (byte)Util.AdjustTowards(baseRecolor.G, 255, ADJUSTMENT_AMOUNT);
                        baseRecolor.B = (byte)Util.AdjustTowards(baseRecolor.B, 255, ADJUSTMENT_AMOUNT);
                    }
                    else if (adjustment == RecolorAdjustment.EXTRA_DARKEN)
                    {
                        baseRecolor.R = (byte)Util.AdjustTowards(baseRecolor.R, 0, ADJUSTMENT_AMOUNT * 1.5f);
                        baseRecolor.G = (byte)Util.AdjustTowards(baseRecolor.G, 0, ADJUSTMENT_AMOUNT * 1.5f);
                        baseRecolor.B = (byte)Util.AdjustTowards(baseRecolor.B, 0, ADJUSTMENT_AMOUNT * 1.5f);
                    }
                }

                recoloredColors[i] = baseRecolor;
            }
            recoloredTexture.SetData<Color>(recoloredColors);
            return recoloredTexture;
        }

        public static Color BlendColors(Color baseColor, Color blendColor, float blendAmount)
        {
            float diffRed = blendColor.R - baseColor.R;
            float diffBlue = blendColor.B - baseColor.B;
            float diffGreen = blendColor.G - baseColor.G;

            int changeRed = (int)(diffRed * blendAmount);
            int changeBlue = (int)(diffBlue * blendAmount);
            int changeGreen = (int)(diffGreen * blendAmount);

            return new Color(baseColor.R + changeRed, baseColor.G + changeGreen, baseColor.B + changeBlue);
        } 

        public static bool ArrayContains(Item[] arr, Item val)
        {
            for(int i = 0; i < arr.Length; i++)
            {
                if(arr[i] == val)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ArrayContains(int[] arr, int val)
        {
            for(int i = 0; i < arr.Length; i++)
            {
                if(arr[i] == val)
                {
                    return true;
                }
            }
            return false;
        }

        public static List<E> ShuffleList<E>(List<E> inputList)
        {
            List<E> randomList = new List<E>();

            Random r = new Random();
            int randomIndex = 0;
            while (inputList.Count > 0)
            {
                randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return randomList; //return the new random list
        }

        public static List<EntityCharacter.ClothingSet> GenerateClothingSetList(params EntityCharacter.ClothingSet[] sets)
        {
            List<EntityCharacter.ClothingSet> clothingSets = new List<EntityCharacter.ClothingSet>();
            foreach (EntityCharacter.ClothingSet set in sets)
            {
                clothingSets.Add(set);
            }
            return clothingSets;
        }

        public static List<EntityCharacter.Schedule.Event> GenerateSchedule(params EntityCharacter.Schedule.Event[] events)
        {
            List<EntityCharacter.Schedule.Event> schedule = new List<EntityCharacter.Schedule.Event>();
            foreach (EntityCharacter.Schedule.Event sEvent in events)
            {
                schedule.Add(sEvent);
            }
            return schedule;
        }

        public static List<EntityCharacter.DialogueOption> GenerateDialogueList(params EntityCharacter.DialogueOption[] options)
        {
            List<EntityCharacter.DialogueOption> dialogueList = new List<EntityCharacter.DialogueOption>();
            foreach (EntityCharacter.DialogueOption dOption in options)
            {
                dialogueList.Add(dOption);
            }
            return dialogueList;
        }

        public static string GenerateAutomaticID(string entityType, Area area)
        {
            int instance = 1;
            if(idCounterDict.ContainsKey(entityType))
            {
                idCounterDict[entityType]++;
                instance = idCounterDict[entityType];
            } else
            {
                idCounterDict[entityType] = 1;
            }

            return "autogenid_" + area.GetName() + "_" + entityType + instance;
        }

        private interface DebugDrawElement
        {
            void Draw(SpriteBatch sb, float layerDepth);
        }

        public static void Draw(SpriteBatch sb, float layerDepth)
        {
            foreach(DebugDrawElement dde in debugDrawElements)
            {
                dde.Draw(sb, layerDepth);
            }
            debugDrawElements.Clear();
        }

        private class QueuedRectangle : DebugDrawElement
        {
            public Color color;
            public RectangleF rect;

            public QueuedRectangle(RectangleF rect, Color color)
            {
                this.rect = rect;
                this.color = color;
            }

            public void Draw(SpriteBatch sb, float layerDepth)
            {
                sb.Draw(debugDrawRect, rect.ToRectangle(), color);
            }
        }

        public static void DrawDebugRect(RectangleF rect, Color color)
        {
            debugDrawElements.Add(new QueuedRectangle(rect, color));
        }

        public static float CalculateArea(RectangleF rect)
        {
            return rect.Width * rect.Height;
        }
    }
}
