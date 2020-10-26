﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Voxel Mesh Data", menuName = "Rimecraft/Voxel Mesh Data")]
public class VoxelMeshData : ScriptableObject
{
    public string blockName;
    public FaceMeshData[] faces;
}

[System.Serializable]
public class VertData
{
    public Vector3 position;
    public Vector2 uv;

    public VertData(Vector3 position, Vector2 uv)
    {
        this.position = position;
        this.uv = uv;
    }
}

[System.Serializable]
public class FaceMeshData
{
    public string direction;
    public Vector3 normal;
    public VertData[] vertData;
    public int[] triangles;
}