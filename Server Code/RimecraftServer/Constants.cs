using System;
using System.Collections.Generic;
using System.Text;

namespace RimecraftServer
{
    internal class Constants
    {
        public const int TICKS_PER_SEC = 30;
        public const double MS_PER_TICK = 1000 / (double)TICKS_PER_SEC;

        public const int CHUNKSIZE = 16;

        public const int ChunkVolume = 4096; // CHUNKSIZE^3
        public const int ChunkSurfaceVolume = 256; // CHUNKSIZE^2
    }
}