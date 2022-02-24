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

    public ushort VoxelFromPosition(int3 localPosition)
    {
        return map[localPosition.x, localPosition.y, localPosition.z];
    }
}