using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Concurrent;

public class ChunkMeshManager : MonoBehaviourSingleton<ChunkMeshManager>
{
    public Material material = null;
    public Material transparentMaterial = null;
    public Material shinyMaterial = null;
    public AllBlockTypes blockTypes = null;

    [HideInInspector] public ConcurrentStack<ChunkMesh> chunksToDraw = new ConcurrentStack<ChunkMesh>();
    [HideInInspector] public ConcurrentDictionary<int3, ChunkMesh> chunkMeshes = new ConcurrentDictionary<int3, ChunkMesh>();

    private void FixedUpdate()
    {
        if (chunksToDraw.Count > 0)
        {
            ChunkMesh result;
            chunksToDraw.TryPop(out result);

            result.CreateMesh();
        }
    }
}