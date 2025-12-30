using UnityEngine;

namespace Pathfinding
{
public interface IPathFinder
{
    public MapGraph Graph { get; set; }
    
    public PathData FindPath(Vector2 targetPosition);
}
}