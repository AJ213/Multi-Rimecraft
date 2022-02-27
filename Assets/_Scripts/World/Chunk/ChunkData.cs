using UnityEngine;
using Unity.Mathematics;

[System.Serializable]
public class ChunkData
{
    private int3 coord;
    public readonly ushort[] blockMap = new ushort[Constants.CHUNK_VOLUME];

    public int3 Coord
    {
        get => coord;
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

    // Returns true if worked
    public void ModifyVoxel(int3 localPosition, ushort id)
    {
        if (GetVoxel(localPosition) == id)
        {
            return;
        }

        SetVoxel(localPosition, id);
        WorldData.UpdateSorroundingVoxels(WorldHelper.GetVoxelGlobalPositionFromChunk(localPosition, this.coord));
    }

    public ushort this[int x, int y, int z]
    {
        get => blockMap[Constants.COORD_TO_INT(x, y, z)];
        set => blockMap[Constants.COORD_TO_INT(x, y, z)] = value;
    }

    public ushort GetVoxel(int3 localPosition) => this[localPosition.x, localPosition.y, localPosition.z];

    private ushort SetVoxel(int3 localPosition, ushort voxel) => this[localPosition.x, localPosition.y, localPosition.z] = voxel;
}