using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Constants
{
    public const int CHUNK_SIZE = 16;
    public const int CHUNK_VOLUME = 4096;
    public const int CHUNK_SURFACE_VOLUME = 256;

    public static int COORD_TO_INT(int x, int y, int z) => x + (z << 4) + (y << 8);
}