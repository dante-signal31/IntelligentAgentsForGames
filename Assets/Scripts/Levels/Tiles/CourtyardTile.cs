using UnityEngine;
using UnityEngine.Tilemaps;

namespace Levels.Tiles
{
[CreateAssetMenu(fileName = "CourtyardTile", menuName = "Scriptable Objects/CourtyardTile")]
public class CourtyardTile : Tile
{
    [Header("CONFIGURATION:")]
    [SerializeField] private bool isObstacle;
}
}
