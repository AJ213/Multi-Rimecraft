using UnityEngine;

[System.Serializable, CreateAssetMenu(fileName = "AllBlockTypes", menuName = "Rimecraft/AllBlockTypes")]
public class AllBlockTypes : ScriptableObject
{
    [SerializeField] private BlockType[] blockTypes;
    public BlockType this[int key] => blockTypes[key];
}