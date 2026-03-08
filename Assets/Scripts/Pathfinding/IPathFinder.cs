using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Interface for pathfinding algorithms.
/// </summary>
public interface IPathFinder
{
    /// <summary>
    /// Graph modeling the environment this pathfinder goes through.
    /// </summary>
    public IPositionGraph Graph { get; set; }
    
    /// <summary>
    /// Find a path to the target position.
    /// </summary>
    /// <param name="targetPosition">Position to get to.</param>
    /// <param name="fromPosition">
    /// The position on to map from where the path should start. If not set, the current
    /// agent position will be used.</param>
    /// <returns>Path to reach the target position.</returns>
    public PathData FindPath(Vector2 targetPosition, Vector2 fromPosition=default);
}
}