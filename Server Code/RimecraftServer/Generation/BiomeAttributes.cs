using System;
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

        public int maxHeight = 12;
        public int minHeight = 5;

        public Lode[] lodes;
    }
}