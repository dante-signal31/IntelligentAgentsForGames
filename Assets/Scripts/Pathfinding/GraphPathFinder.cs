
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
public abstract class GraphPathFinder<T> : MonoBehaviour, IGraphPathFinder 
    where T: NodeRecord, new()
{
    [Header("DEBUG:")]
    public bool showGizmos;
    public float exploredNodeGizmoRadius = 0.1f;
    [SerializeField] private Color exploredNodeColor = Color.black;
    [SerializeField] private float pathNodeGizmoRadius = 0.1f;
    [SerializeField] private Color pathNodeColor = Color.greenYellow;
    public Vector2 gizmoTextOffset = new(0.1f, 0.1f);
    public Color textColor = Color.white;
    
    /// <summary>
    /// Graph modeling the environment.
    /// </summary>
    public MapGraph Graph { get; set; }
    
    /// <summary>
    /// Dictionary containing nodes and their corresponding recorded data after the
    /// exploration process.
    /// </summary>
    // closedDict and CurrentStartMode must be made public to allow DrawDijkstraPathFinder
    // draw gizmos. --> TODO: Try another visibility qualifier to avoid public.
    public readonly Dictionary<PositionNode, T> closedDict = new();
    
    /// <summary>
    /// Agent starting node.
    /// </summary>
    public PositionNode CurrentStartNode { get; protected set; }
    
    /// <summary>
    /// The currently found path across the graph to the target node.
    /// </summary>
    private PathData foundPath;
    
    /// <summary>
    /// Finds a path from the current position to the specified target position
    /// within the provided graph using Dijkstra's algorithm.
    /// </summary>
    /// <param name="targetPosition">
    /// The target position on the map to which the path is to be found.
    /// </param>
    /// <returns>
    /// A path object representing the sequence of nodes from the start position
    /// to the target position. Returns null if no valid path exists to the target.
    /// </returns>
    public abstract PathData FindPath(Vector2 targetPosition);
    
    /// <summary>
    /// Constructs a path from the start node to the target node by traversing
    /// the closed dictionary in reverse and building a sequence of connections.
    /// </summary>
    /// <param name="startNode">
    /// The initial node where the pathfinding process starts.
    /// </param>
    /// <param name="targetNode">
    /// The final node where the pathfinding process ends.
    /// </param>
    /// <returns>
    /// A Path object representing the ordered sequence of positions
    /// from the start node to the target node.
    /// </returns>
    protected PathData BuildPath(PositionNode startNode, PositionNode targetNode)
    {
        List<GraphConnection> path = new();
        T pointer = closedDict[targetNode];

        // Traverse the closedDict backwards to build the path from target to start.
        while (pointer.node != startNode)
        {
            path.Add(pointer.connection);
            PositionNode endA = Graph.GetNodeById(pointer.connection.startNodeId);
            pointer = closedDict[endA];
        }

        // As Connections have been stored from target to start order, we must reverse
        // the list to get the path from start to target.
        path.Reverse();
    
        // Now that the Connection list is in correct order, we can build the Path
        // following Connections and taking note of their EndNode positions.
        foundPath = new PathData
        {
            loop = false,
        };
        foreach (GraphConnection connection in path)
        {
            PositionNode endB = Graph.GetNodeById(connection.endNodeId);
            foundPath.positions.Add(endB.position);
        }
    
        return foundPath;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
    
        // Draw explored nodes.
        foreach (PositionNode exploredNode in closedDict.Keys)  
        {
            Gizmos.color = exploredNodeColor;
            Gizmos.DrawSphere(exploredNode.position, exploredNodeGizmoRadius);
        }
    
        if (foundPath == null) return;
    
        // Draw found path.
        foreach (Vector2 position in foundPath.positions)
        {
            Gizmos.color = pathNodeColor;
            Gizmos.DrawSphere(position, pathNodeGizmoRadius);
        }
    }
#endif
}
}

