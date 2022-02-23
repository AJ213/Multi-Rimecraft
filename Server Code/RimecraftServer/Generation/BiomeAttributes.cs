﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RimecraftServer
{
    public class BiomeAttributes
    {
        public int offset;
        public float scale;

        public int terrainHeight;
        public float terrainScale;

        public ushort surfaceBlock;
        public ushort subSurfaceBlock;

        public int octaves;
        public float persistence;
        //[Header("Major Flora")]
        /*public float majorFloraZoneScale = 1.3f;

        public int majorFloraIndex;

        //[Range(0.1f, 1f)]
        public float majorFloraZoneThreshold = 0.6f;

        public float majorFloraPlacementScale = 15f;

        //[Range(0.1f, 1f)]
        public float majorFloraPlacementThreshold = 0.6f;

        public bool placeMajorFlora = true;*/
        public int maxHeight = 12;
        public int minHeight = 5;

        public Lode[] lodes;
    }
}