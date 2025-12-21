using System;
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
public class GraphNode
{
    public Vector2 position;
    public CustomUnityDictionaries.OrientationGraphEdgeDictionary connections = new ();
    
    public GraphNode(Vector2 position)
    {
        this.position = position;
    }

    /// <summary>
    /// Adds a connection between the current node and a specified destination node
    /// with a given cost, orientation, and optionally bidirectional behavior.
    /// </summary>
    /// <param name="thisNodeKey">The current node.</param>
    /// <param name="endNodeKey">The destination node to which the connection should
    /// point.</param>
    /// <param name="cost">The cost or weight associated with traversing the edge.</param>
    /// <param name="orientation">The orientation of the edge connecting the two
    /// nodes.</param>
    public void AddConnection(
        Vector2Int thisNodeKey,
        Vector2Int endNodeKey,
        float cost, 
        Orientation orientation)
    {
        GraphConnection graphConnection = new(thisNodeKey, endNodeKey, cost);
        connections[orientation] = graphConnection;
    }
}
}

