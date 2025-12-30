using UnityEngine;

namespace Pathfinding
{
public class SquareDistanceHeuristic: MonoBehaviour, IAStarHeuristic
{
    /// <summary>
    /// Estimates the cost to reach a target position from a start position using
    /// the square of the distance. This implementation is computationally efficient
    /// as it avoids calculating the square root.
    /// </summary>
    /// <param name="startPosition">The starting position from which the cost is
    /// calculated.</param>
    /// <param name="targetPosition">The target position for which the cost is
    /// estimated.</param>
    /// <returns>The squared distance between the start position and the target
    /// position.</returns>
    public float EstimateCostToTarget(Vector2 startPosition, Vector2 targetPosition)
    {
        return Vector2.SqrMagnitude(targetPosition - startPosition);
    }
}
}