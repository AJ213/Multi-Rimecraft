using System;
using System.Collections.Generic;
using System.Text;

namespace RimecraftServer
{
    public class Lode
    {
        public ushort blockID;
        public int minHeight = 5;
        public int maxHeight = 0;
        public float scale;
        public float threshold;
        public float noiseOffset;
    }
}