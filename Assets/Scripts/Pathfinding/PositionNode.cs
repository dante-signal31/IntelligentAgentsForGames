using System;
using System.Collections.Generic;
using Tools;
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
    
    public bool HasConnection(Orientation orientation)
    {
        return Connections.ContainsKey((uint)orientation);
    }

    public GraphConnection GetConnection(Orientation orientation)
    {
        return HasConnection(orientation) ? Connections[(uint)orientation]: null;
    }

    public Dictionary<Orientation, GraphConnection> GetConnections()
    {
        Dictionary<Orientation, GraphConnection> currentConnections = new();
        foreach (KeyValuePair<uint, GraphConnection> connection in Connections)
        {
            currentConnections[(Orientation)connection.Key] = connection.Value;
        }
        return currentConnections;
    }
    
    public void AddConnection(
        uint endNodeId, 
        float cost, 
        Orientation orientation)
    {
        base.AddConnection(endNodeId, cost, (uint)orientation);
    }
}
}

