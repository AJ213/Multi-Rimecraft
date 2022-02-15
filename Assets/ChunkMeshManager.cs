using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class ChunkMeshManager : MonoBehaviourSingleton<ChunkMeshManager>
{
    public Material material = null;
    public Material transparentMaterial = null;
    public Material shinyMaterial = null;
    public AllBlockTypes blockTypes = null;

    [HideInInspector] public Queue<ChunkMesh> chunksToDraw = new Queue<ChunkMesh>();
    [HideInInspector] public Dictionary<int3, ChunkMesh> chunkMeshes = new Dictionary<int3, ChunkMesh>();

    private void FixedUpdate()
    {
        if (chunksToDraw.Count > 0)
        {
            chunksToDraw.Dequeue().CreateMesh();
        }
    }
}