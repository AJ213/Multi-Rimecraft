﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure 
{
    public static void MakeTree(Vector3 position, Queue<VoxelMod> queue, int minTrunkHeight, int maxTrunkHeight)
    {
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 222, 3));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for(int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 4));
        }

        queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + height, position.z), 6));
        for (int y = 1; y < 3; y++)
        {
            for (int x = -3; x < 3; x++)
            {
                for(int z = -y; z < 3; z++)
                {
                    queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height - y, position.z + z), 6));
                }
            }
        }
        
    }
}