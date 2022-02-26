using System;
using System.Collections.Generic;
using System.Text;

namespace RimecraftServer
{
    internal class Constants
    {
        public const int TICKS_PER_SEC = 30;
        public const double MS_PER_TICK = 1000 / (double)TICKS_PER_SEC;

        public const int CHUNK_SIZE = 16;
        public const int CHUNK_SURFACE_VOLUME = 256; // CHUNKSIZE^2
        public const int CHUNK_VOLUME = 4096; // CHUNKSIZE^3

        public static int COORD_TO_INT(int x, int y, int z) => x + (z << 4) + (y << 8);
    }
}