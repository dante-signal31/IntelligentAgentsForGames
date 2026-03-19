using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Represents a node in a graph structure, defined by a position and connected edges.
/// </summary>
/// <remarks>
/// A node in a graph is typically used to represent a specific point within a 2D space
/// and stores information about its position and the edges that connect it to other
/// nodes.
/// </remarks>
[Serializable]
public class PositionNode: GraphNode, IPositionNode
{
    [SerializeField] private Vector2 position;

    /// <summary>
    /// This node global position.
    /// </summary>
    public Vector2 Position
    {
        get => position;
        set => position = value;
    }

    public PositionNode(Vector2 position)
    {
        this.Position = position;
    }

    /// <summary>
    /// Determines whether the node has a connection in the specified orientation.
    /// </summary>
    /// <param name="orientation">The orientation to check for a connection.</param>
    /// <returns>True if a connection exists in the given orientation; otherwise,
    /// false.</returns>
    public bool HasConnection(Orientation orientation)
    {
        return Connections.ContainsKey((uint)orientation);
    }

    /// <summary>
    /// Retrieves the connection associated with the specified orientation, if it exists.
    /// </summary>
    /// <param name="orientation">The orientation associated with the desired
    /// connection.</param>
    /// <returns>The connection object if a connection exists in the specified
    /// orientation; otherwise, null.</returns>
    public GraphConnection GetConnection(Orientation orientation)
    {
        return HasConnection(orientation) ? Connections[(uint)orientation]: null;
    }

    /// <summary>
    /// Retrieves all connections associated with the current node, organized by their
    /// orientation.
    /// </summary>
    /// <returns>A dictionary where the keys represent the orientation of the connections,
    /// and the values are the corresponding <see cref="GraphConnection"/> objects.
    /// </returns>
    public Dictionary<Orientation, GraphConnection> GetConnections()
    {
        Dictionary<Orientation, GraphConnection> currentConnections = new();
        foreach (KeyValuePair<uint, GraphConnection> connection in Connections)
        {
            currentConnections[(Orientation)connection.Key] = connection.Value;
        }
        return currentConnections;
    }

    /// <summary>
    /// Adds a connection to the node with the specified end node ID, cost,
    /// and orientation.
    /// </summary>
    /// <param name="endNodeId">The unique identifier of the end node to connect
    /// to.</param>
    /// <param name="cost">The cost associated with traversing the connection.</param>
    /// <param name="orientation">The orientation of the connection.</param>
    public void AddConnection(
        uint endNodeId, 
        float cost, 
        Orientation orientation)
    {
        base.AddConnection(endNodeId, cost, (uint)orientation);
    }
}
}

