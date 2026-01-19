using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Interface for pathfinding algorithms.
/// </summary>
public interface IPathFinder
{
    /// <summary>
    /// Find a path to the target position.
    /// </summary>
    /// <param name="targetPosition">Position to get to.</param>
    /// <returns>Path to reach the target position.</returns>
    public PathData FindPath(Vector2 targetPosition);
}
}