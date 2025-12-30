using UnityEngine;

namespace Pathfinding
{
public class ManhattanDistanceHeuristic: MonoBehaviour, IAStarHeuristic
{
    /// <summary>
    /// Quick heuristic to estimate the cost to reach the target position. It's commonly
    /// used in square grids. It just uses sums and substractions, so it's very
    /// performant.
    /// </summary>
    /// <param name="startPosition">Start position.</param>
    /// <param name="targetPosition">Target position.</param>
    /// <returns>Estimated cost.</returns>
    public float EstimateCostToTarget(Vector2 startPosition, Vector2 targetPosition)
    {
        float distanceX = Mathf.Abs(targetPosition.x - startPosition.x);
        float distanceY = Mathf.Abs(targetPosition.y - startPosition.y);
        float manhattanDistance = distanceX + distanceY;
        return manhattanDistance;
    }
}
}