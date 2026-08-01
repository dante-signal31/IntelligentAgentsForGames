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
    [SerializeField] private Vector2 position = new(1,1);

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
        Position = position;
    }
}
}

