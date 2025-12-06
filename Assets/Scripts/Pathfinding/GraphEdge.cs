using System;
using OdinSerializer;

namespace Pathfinding
{
/// <summary>
/// Represents an edge in a graph, connecting two nodes with an associated cost.
/// </summary>
/// <remarks>
/// A graph edge serves as a connection between two <see cref="GraphNode"/> objects,
/// defining the destination node (end node) and the cost associated with traversing the
/// edge.
/// </remarks>
[Serializable]
public class GraphEdge
{
    public float Cost;
    [OdinSerialize] public GraphNode EndNode;
}
}

