using UnityEngine;
using UnityEngine.Tilemaps;

namespace Levels.Tiles
{
[CreateAssetMenu(
    fileName = "CourtyardTile", 
    menuName = "Scriptable Objects/CourtyardTile")]
public class CourtyardTile : Tile
{
    [Header("CONFIGURATION:")]
    [Tooltip("Navigation cost of this tile.")]
    [SerializeField] private float cost;
    
    /// <summary>
    /// Navigation cost of this tile.
    /// </summary>
    public float Cost => cost;
}
}
