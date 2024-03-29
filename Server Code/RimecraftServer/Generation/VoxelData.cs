﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace RimecraftServer
{
    public class VoxelData
    {
        public static readonly int TextureAtlasSizeInBlocks = 4;

        public static float NoramlizedBlockTextureSize
        { get { return 1 / (float)TextureAtlasSizeInBlocks; } }

        public static readonly Vector3[] faceChecks = new Vector3[6]
        {
            new Vector3(0, 0, -1),
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0),
            new Vector3(-1, 0, 0),
            new Vector3(1, 0, 0)
        };

        public static readonly int[] revFaceCheckIndex = new int[6] { 1, 0, 3, 2, 5, 4 };

        public static readonly int[,] voxelTris = new int[6, 4]
        {
        // Back, Front, Top, Bottom, Left, Right 0 1 2 2 1 3
        {0, 3, 1, 2}, // Back Face
        {5, 6, 4, 7}, // Front Face
        {3, 7, 2, 6}, // Top Face
        {1, 5, 0, 4}, // Bottom Face
        {4, 7, 0, 3}, // Left Face
        {1, 2, 5, 6}  // Right Face
        };
    }
}