using System;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Represents a connection between two nodes in a graph.
/// </summary>
[Serializable]
public class GraphConnection
{
    public float cost;
    // In my first approach I used direct references to Start and End nodes. But it
    // happened that it didn't serialize well. Randomly, some GraphConnections serialized 
    // their EndNode fields to null. After some investigation, I found out that
    // serialization algorithms don't like cyclic graphs. My graph implemented cycles:
    // MapGraphResource -> GraphNode A -> GraphConnection -> EndNode (GraphNode B) ->
    // Connections -> ... -> Return to GraphNode A. So I had to break cycles. That's why
    // now I used Vector2I keys to serialize GraphConnections and not direct references
    // instead.
    public Vector2Int startNodeKey;
    public Vector2Int endNodeKey;
    
    public GraphConnection(Vector2Int startNodeKey, Vector2Int endNodeKey, float cost)
    {
        this.startNodeKey = startNodeKey;
        this.endNodeKey = endNodeKey;
        this.cost = cost;
    }
}
}