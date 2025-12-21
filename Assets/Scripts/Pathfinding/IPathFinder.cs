using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Defines the contract for a pathfinding interface. Classes implementing this interface
/// are responsible for navigating a graph structure to find a path to a target position.
/// </summary>
public interface IPathFinder
{
    /// <summary>
    /// Graph representation of the scene.
    /// </summary>
    public MapGraph Graph { get; set; }

    /// <summary>
    /// Find a path to the target position from the current position.
    /// </summary>
    /// <param name="targetPosition">Position to get to.</param>
    /// <returns>A Path to get the target position avoiding scene obstacles.</returns>
    public PathData FindPath(Vector2 targetPosition);
}
}