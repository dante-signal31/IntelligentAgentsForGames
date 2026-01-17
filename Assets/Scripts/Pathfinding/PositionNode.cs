using System;
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
public class PositionNode: GraphNode
{
    public Vector2 position;
    
    public PositionNode(Vector2 position)
    {
        this.position = position;
    }
}
}

