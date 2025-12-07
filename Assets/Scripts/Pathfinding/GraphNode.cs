using System;
using System.Collections.Generic;
using Sirenix.Serialization;
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
    public CustomUnityDictionaries.OrientationGraphEdgeDictionary edges;

    /// <summary>
    /// Adds an edge between the current node and a specified destination node with a
    /// given cost, orientation, and optionally bidirectional behavior.
    /// </summary>
    /// <param name="endNode">The destination node to which the edge should point.</param>
    /// <param name="cost">The cost or weight associated with traversing the edge.</param>
    /// <param name="orientation">The orientation of the edge connecting the two
    /// nodes.</param>
    /// <param name="bidirectional">Indicates whether the edge should be bidirectional.
    /// If true, a corresponding opposite-direction edge is added to the destination
    /// node.</param>
    public void AddEdge(
        GraphNode endNode, 
        float cost, 
        Orientation orientation, 
        bool bidirectional = true)
    {
        if (edges == null) edges = new ();
        GraphEdge edge = new();
        edge.endNode = endNode;
        edge.cost = cost;
        edges[orientation] = edge;

        if (!bidirectional) return;

        // In my example the paths between nodes are bidirectional, so we need to add
        // an edge in both directions. To do this, we update the other node's edge to
        // point to us.
        switch (orientation)
        {
            // bidirectional argument must be false in this call to avoid infinite
            // recursion.
            case Orientation.North: 
                edge.endNode.AddEdge(this, cost, Orientation.South, false); 
                break;
            case Orientation.East:
                edge.endNode.AddEdge(this, cost, Orientation.West, false); 
                break;
            case Orientation.South:
                edge.endNode.AddEdge(this, cost, Orientation.North, false);
                break;
            case Orientation.West:
                edge.endNode.AddEdge(this, cost, Orientation.East, false);
                break;
        }
    }
}
}

