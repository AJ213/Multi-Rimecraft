using UnityEngine;
using Unity.Mathematics;

[System.Serializable]
public class ChunkData
{
    private int3 coord;

    public int3 Coord
    {
        get { return coord; }
        set
        {
            coord.x = value.x;
            coord.y = value.y;
            coord.z = value.z;
        }
    }

    public ChunkData(int3 pos)
    {
        Coord = pos;
    }

    [HideInInspector]
    public ushort[,,] map = new ushort[Constants.CHUNKSIZE, Constants.CHUNKSIZE, Constants.CHUNKSIZE];

    public static void Populate(ChunkData chunk)
    {
        for (int y = 0; y < Constants.CHUNKSIZE; y++)
        {
            for (int x = 0; x < Constants.CHUNKSIZE; x++)
            {
                for (int z = 0; z < Constants.CHUNKSIZE; z++)
                {
                    int3 localPosition = new int3(x, y, z);
                    chunk.map[x, y, z] = GenerateBlock.SamplePosition(WorldHelper.GetVoxelGlobalPositionFromChunk(localPosition, chunk.Coord), RimecraftWorld.Instance.biomes);
                }
            }
        }
    }

    public void ModifyVoxel(int3 localPosition, ushort id, bool updateSurrounding = false)
    {
        if (map[localPosition.x, localPosition.y, localPosition.z] == id)
        {
            return;
        }

        map[localPosition.x, localPosition.y, localPosition.z] = id;

        RimecraftWorld.Instance.AddChunkToUpdate(coord, true);
        if (updateSurrounding)
        {
            int3 globalPosition = WorldHelper.GetVoxelGlobalPositionFromChunk(localPosition, coord);
            UpdateSorroundingVoxels(globalPosition);
        }
    }

    private void UpdateSorroundingVoxels(int3 globalPosition)
    {
        for (int p = 0; p < 6; p++)
        {
            int3 currentVoxel = globalPosition + VoxelData.faceChecks[p];

            if (!WorldHelper.IsVoxelGlobalPositionInChunk(currentVoxel, coord))
            {
                int3 coord = WorldHelper.GetChunkCoordFromPosition(currentVoxel);
                if (ChunkMeshManager.Instance.chunkMeshes.ContainsKey(coord))
                {
                    RimecraftWorld.Instance.AddChunkToUpdate(coord, true);
                }
            }
        }
    }

    public ushort VoxelFromPosition(int3 localPosition)
    {
        return map[localPosition.x, localPosition.y, localPosition.z];
    }
}