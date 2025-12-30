using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Interface that must comply with every class to be used to estimate costs to reach
/// targets in the A* algorithm.
/// </summary>
public interface IAStarHeuristic
{
    /// <summary>
    /// Get estimate cost to reach targets in the A* algorithm.
    /// </summary>
    /// <param name="startPosition">Start position.</param>
    /// <param name="targetPosition">Current position to get to.</param>
    /// <returns>An estimated cost.</returns>
    public float EstimateCostToTarget(Vector2 startPosition, Vector2 targetPosition);
}
}