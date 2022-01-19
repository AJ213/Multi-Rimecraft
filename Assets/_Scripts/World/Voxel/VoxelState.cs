using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class VoxelState
{
    public ushort id;
    [System.NonSerialized] public int3 position;

    public VoxelState(ushort id, int3 position)
    {
        this.id = id;
        this.position = position;
    }

    public BlockType Properties => RimecraftWorld.Instance.blockTypes[id];
}