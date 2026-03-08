using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Interface provided by every position graph implementation to be used by the
/// pathfinding algorithm.
/// </summary>
public interface IPositionGraph
{
    /// <summary>
    /// Get a graph node instance by its ID.
    /// </summary>
    /// <param name="nodeId">ID of the node to retrieve.</param>
    /// <returns>Node instance.</returns>
    IPositionNode GetNodeById(uint nodeId);
    
    /// <summary>
    /// Get a graph node instance by its position.
    /// </summary>
    /// <param name="position">Position of the node to retrieve.</param>
    /// <returns>Node instance.</returns>
    IPositionNode GetNodeAtPosition(Vector2 position);
}
}