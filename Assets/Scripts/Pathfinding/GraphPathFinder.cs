
using System.Collections.Generic;
using PropertyAttribute;
using UnityEngine;

namespace Pathfinding
{
public abstract class GraphPathFinder<T> : MonoBehaviour, IGraphPathFinder 
    where T: NodeRecord, new()
{
    [Header("CONFIGURATION:")]
    [Tooltip("Graph modeling the environment.")]
    [InterfaceCompliant(typeof(IPositionGraph))]
    [SerializeField] private MonoBehaviour mapGraph;
    
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
    public IPositionGraph Graph
    {
        get => mapGraph as IPositionGraph; 
        set => mapGraph = value as MonoBehaviour;
    }
    
    /// <summary>
    /// Dictionary containing nodes and their corresponding recorded data after the
    /// exploration process.
    /// </summary>
    public readonly Dictionary<IPositionNode, T> closedDict = new();
    
    /// <summary>
    /// Exposes the explored nodes for debugging purposes (e.g., Editor handles) but
    /// avoids modifications of the dictionary from editor code.
    /// </summary>
    public IReadOnlyDictionary<IPositionNode, T> ExploredNodes => closedDict;
    
    /// <summary>
    /// Agent starting node.
    /// </summary>
    public IPositionNode CurrentStartNode { get; protected set; }
    
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
    /// <param name="fromPosition">
    /// The position on to map from where the path should start. If not set, the current
    /// agent position will be used.</param>
    /// <returns>
    /// A path object representing the sequence of nodes from the start position
    /// to the target position. Returns null if no valid path exists to the target.
    /// </returns>
    public abstract PathData FindPath(
        Vector2 targetPosition, 
        Vector2 fromPosition=default);

    /// <summary>
    /// Constructs a path from the start node to the target node by traversing
    /// the closed dictionary in reverse and building a sequence of connections.
    /// </summary>
    /// <param name="currentClosedDict">
    /// A dictionary containing nodes and their corresponding recorded data,
    /// used to trace the path from the target node to the start node.
    /// </param>
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
    public PathData BuildPath(
        Dictionary<IPositionNode, T> currentClosedDict,
        IPositionNode startNode, 
        IPositionNode targetNode)
    {
        List<GraphConnection> path = new();
        T pointer = currentClosedDict[targetNode];

        // Traverse the closedDict backwards to build the path from target to start.
        while (pointer.node != startNode)
        {
            path.Add(pointer.connection);
            IPositionNode endA = Graph.GetNodeById(pointer.connection.startNodeId);
            pointer = currentClosedDict[endA];
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
            IPositionNode endB = Graph.GetNodeById(connection.endNodeId);
            foundPath.positions.Add(endB.Position);
        }
    
        return foundPath;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
    
        // Draw explored nodes.
        foreach (IPositionNode exploredNode in closedDict.Keys)  
        {
            Gizmos.color = exploredNodeColor;
            Gizmos.DrawSphere(exploredNode.Position, exploredNodeGizmoRadius);
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

