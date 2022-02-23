﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace RimecraftServer
{
    public class ChunkData
    {
        private Vector3 coord;

        public Vector3 Coord
        {
            get { return coord; }
            set
            {
                coord = value;
            }
        }

        public ChunkData(int x, int y, int z)
        {
            Coord = new Vector3(x, y, z);
        }

        public ChunkData(Vector3 coord)
        {
            this.coord = coord;
        }

        public ushort[,,] map = new ushort[Constants.CHUNKSIZE, Constants.CHUNKSIZE, Constants.CHUNKSIZE];

        public static void Populate(ChunkData chunk)
        {
            for (int y = 0; y < Constants.CHUNKSIZE; y++)
            {
                for (int x = 0; x < Constants.CHUNKSIZE; x++)
                {
                    for (int z = 0; z < Constants.CHUNKSIZE; z++)
                    {
                        chunk.map[x, y, z] = GenerateBlock.GenerateFlatGround(WorldHelper.GetVoxelGlobalPositionFromChunk(new Vector3(x, y, z), chunk.Coord));
                    }
                }
            }
        }

        public void ModifyVoxel(Vector3 localPosition, ushort id)
        {
            if (VoxelFromPosition(localPosition) == id)
            {
                return;
            }

            SetVoxelFromPosition(localPosition, id);

            ServerSend.SendChunkToAll(this);
        }

        public ushort VoxelFromPosition(Vector3 localPosition)
        {
            return map[(int)localPosition.X, (int)localPosition.Y, (int)localPosition.Z];
        }

        public void SetVoxelFromPosition(Vector3 localPosition, ushort voxel)
        {
            map[(int)localPosition.X, (int)localPosition.Y, (int)localPosition.Z] = voxel;
        }
    }
}