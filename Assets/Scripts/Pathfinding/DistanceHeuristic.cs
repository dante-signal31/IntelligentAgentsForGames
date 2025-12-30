using UnityEngine;

namespace Pathfinding
{
public class DistanceHeuristic: MonoBehaviour, IAStarHeuristic
{
    /// <summary>
    /// Calculates the estimated cost to reach the target position from the start position
    /// based on a Euclidean distance heuristic.
    /// </summary>
    /// <param name="startPosition">The starting position in the path.</param>
    /// <param name="targetPosition">The target position to reach.</param>
    /// <returns>A float representing the estimated cost to reach the target position.
    /// </returns>
    public float EstimateCostToTarget(Vector2 startPosition, Vector2 targetPosition)
    {
        return Vector2.Distance(startPosition, targetPosition);
    }
}
}