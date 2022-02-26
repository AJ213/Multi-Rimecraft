using System;
using System.Collections.Generic;
using System.Numerics;

namespace RimecraftServer
{
    public class ChunkData
    {
        private Vector3 coord;
        public readonly ushort[] blockMap = new ushort[Constants.CHUNK_VOLUME];
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

        public static void Populate(ChunkData chunk)
        {
            for (int y = 0; y < Constants.CHUNK_SIZE; y++)
            {
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                    {
                        Vector3 localPosition = new Vector3(x, y, z);
                        Vector3 globalPosition = WorldHelper.GetVoxelGlobalPositionFromChunk(localPosition, chunk.Coord);
                        chunk[x, y, z] = GenerateBlock.SamplePosition(globalPosition, Program.worldData.biomes, Program.worldData.NoiseGeneration);
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

        public ushort this[int x, int y, int z]
        {
            get => blockMap[Constants.COORD_TO_INT(x, y, z)];
            set => blockMap[Constants.COORD_TO_INT(x, y, z)] = value;
        }

        public ushort VoxelFromPosition(Vector3 localPosition)
        {
            return this[(int)localPosition.X, (int)localPosition.Y, (int)localPosition.Z];
        }

        public void SetVoxelFromPosition(Vector3 localPosition, ushort voxel)
        {
            this[(int)localPosition.X, (int)localPosition.Y, (int)localPosition.Z] = voxel;
        }
    }
}