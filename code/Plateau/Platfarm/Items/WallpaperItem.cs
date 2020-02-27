﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platfarm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Items
{
    public class WallpaperItem : PlaceableItem
    {
        string placedTextureRecolorTopPath, placedTextureTopPath;
        string placedTextureRecolorBottomPath, placedTextureBottomPath;
        Texture2D placedTextureRecolorTop, placedTextureTop;
        Texture2D placedTextureRecolorBottom, placedTextureBottom;
        private Util.RecolorMap recolorMap;

        public WallpaperItem(string name, string texturePath, string textureRecolorPath,
            string placedTextureRecolorPath, string placedTexturePath,
            string placedTextureRecolorTopPath, string placedTextureTopPath,
            string placedTextureRecolorBottomPath, string placedTextureBottomPath,
            int placeableHeight, int placeableWidth, int stackCapacity, string description, int value, Entities.EntityType placedType, PlacementType placementType, 
            Util.RecolorMap recolor, params Tag[] tags) : base(name, texturePath, textureRecolorPath, placedTextureRecolorPath, placedTexturePath, placeableWidth, placeableHeight, stackCapacity, description, value, placedType, placementType, recolor, tags)
        {
            this.placedTextureRecolorTopPath = placedTextureRecolorTopPath;
            this.placedTextureTopPath = placedTextureTopPath;
            this.placedTextureRecolorBottomPath = placedTextureRecolorBottomPath;
            this.placedTextureBottomPath = placedTextureBottomPath;
            this.recolorMap = recolor;
        }

        public override void Load()
        {
            base.Load();

            //load top
            placedTextureTop = Plateau.CONTENT.Load<Texture2D>(placedTextureTopPath);
            placedTextureRecolorTop = Plateau.CONTENT.Load<Texture2D>(placedTextureRecolorTopPath);
            //load bottom
            placedTextureBottom = Plateau.CONTENT.Load<Texture2D>(placedTextureBottomPath);
            placedTextureRecolorBottom = Plateau.CONTENT.Load<Texture2D>(placedTextureRecolorBottomPath);

            if(recolorMap != null)
            {
                placedTextureRecolorTop = Util.GenerateRecolor(placedTextureRecolorTop, recolorMap);
                placedTextureRecolorBottom = Util.GenerateRecolor(placedTextureRecolorBottom, recolorMap);
            }
        }

        public string GetPlacedTextureBottomPath()
        {
            return this.placedTextureBottomPath;
        }

        public string GetPlacedTextureTopPath()
        {
            return this.placedTextureTopPath;
        }

        public Texture2D GetPlacedTextureRecolorBottom()
        {
            if (!this.IsLoaded())
            {
                this.Load();
            }
            return this.placedTextureRecolorBottom;
        }

        public Texture2D GetPlacedTextureRecolorTop()
        {
            if (!this.IsLoaded())
            {
                this.Load();
            }
            return this.placedTextureRecolorTop;
        }

        public Texture2D GetPlacedTextureTop()
        {
            if (!this.IsLoaded())
            {
                this.Load();
            }
            return this.placedTextureTop;
        }

        public Texture2D GetPlacedTextureBottom()
        {
            if (!this.IsLoaded())
            {
                this.Load();
            }
            return this.placedTextureBottom;
        }
    }
}
