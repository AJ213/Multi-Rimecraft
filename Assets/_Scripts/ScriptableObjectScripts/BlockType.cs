using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "BlockType", menuName = "Rimecraft/BlockType")]
public class BlockType : ScriptableObject
{
    [SerializeField] private string blockName;
    [SerializeField] private bool isSolid;
    [SerializeField] private VoxelMeshData meshData;
    [SerializeField] private bool renderNeighborFaces;
    [SerializeField] private Sprite icon;

    public string BlockName => blockName;
    public bool IsSolid => isSolid;
    public VoxelMeshData MeshData => meshData;
    public bool RenderNeighborFaces => renderNeighborFaces;
    public Sprite Icon => icon;

    [Header("Texture Values"), SerializeField] private int backFaceTexture;

    [SerializeField] private int frontFaceTexture;
    [SerializeField] private int topFaceTexture;
    [SerializeField] private int bottomFaceTexture;
    [SerializeField] private int leftFaceTexture;
    [SerializeField] private int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;

            case 1:
                return frontFaceTexture;

            case 2:
                return topFaceTexture;

            case 3:
                return bottomFaceTexture;

            case 4:
                return leftFaceTexture;

            case 5:
                return rightFaceTexture;

            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }
    }
}