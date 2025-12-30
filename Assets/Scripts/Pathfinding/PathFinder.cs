using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Classes implementing inheriting this class are responsible for navigating a graph
/// structure to find a path to a target position.
/// </summary>
public abstract class PathFinder<T>: MonoBehaviour where T: NodeRecord, new()
{
     /// <summary>
    /// A collection of nodes with priority-based access for use in pathfinding
    /// algorithms like Dijkstra.
    /// </summary>
    /// <remarks>
    /// This class maintains a set of node along with their associated costs for
    /// traversing a graph. It provides functionality to add and remove nodes, check for
    /// node existence, and retrieve the node with the lowest cost value.
    /// </remarks>
    protected abstract class PrioritizedNodeSet
    {
        // Needed to keep ordered by cost the NodeRecords of the node pending to be
        // explored.
        // Initially, I planned to use a PriorityQueue<GraphNode, float>, but I found that
        // the Unity .NET API doesn't support it, because that collection was added in
        // .NET 6, while my Unity version is .NET Framework 4.7.1.
        protected readonly SortedSet<T> _prioritySet;
        // Needed to keep track of the nodes still pending to be explored and to quickly
        // get their respective records.
        protected readonly Dictionary<GraphNode, T> _nodeRecordDict = new ();
    
        public int Count => _nodeRecordDict.Count;
        public bool Contains(GraphNode node) => _nodeRecordDict.ContainsKey(node);

        public PrioritizedNodeSet(IComparer<T> comparer)
        {
            _prioritySet = new SortedSet<T>(comparer);
        }

        public void Add(T record)
        {
            // If the node already exists, we must remove it first because SortedSet
            // doesn't update positions automatically if an existing node costSoFar
            // value changes.
            if (_nodeRecordDict.ContainsKey(record.Node))
            {
                _prioritySet.Remove(_nodeRecordDict[record.Node]);
            }
            
            _prioritySet.Add(record);
            _nodeRecordDict[record.Node] = record;
        }
        
        public void Remove(T record)
        {
            if (_nodeRecordDict.ContainsKey(record.Node))
            {
                _prioritySet.Remove(_nodeRecordDict[record.Node]);
                _nodeRecordDict.Remove(record.Node);
            }
        }
    
        public T this[GraphNode node]
        {
            get => _nodeRecordDict[node];
            set => _nodeRecordDict[node] = value;
        }

        /// <summary>
        /// Extracts and removes the node record with the lowest cost-so-far value
        /// from the prioritized set. 
        /// </summary>
        /// <returns>
        /// The node record with the lowest cost-so-far value or a null if there are no
        /// valid records available in the set.
        /// </returns>
        public T ExtractLowestCostNodeRecord()
        {
            if (_prioritySet.Count == 0) return null;

            // In SortedSet, Min is the element with the lowest priority/cost.
            T lowest = _prioritySet.Min;
            _prioritySet.Remove(lowest);
            _nodeRecordDict.Remove(lowest.Node);
        
            return lowest;
        }
    }
    
    [Header("DEBUG:")]
    public bool showGizmos;
    public float exploredNodeGizmoRadius = 0.1f;
    [SerializeField] private Color exploredNodeColor = Color.black;
    [SerializeField] private float pathNodeGizmoRadius = 0.1f;
    [SerializeField] private Color pathNodeColor = Color.greenYellow;
    public Vector2 gizmoTextOffset = new(0.1f, 0.1f);
    public Color textColor = Color.white;
    
    public MapGraph Graph { get; set; }
    // closedDict and CurrentStartMode must be made public to allow DrawDijkstraPathFinder
    // draw gizmos.
    public readonly Dictionary<GraphNode, T> closedDict = new();
    public GraphNode CurrentStartNode { get; protected set; }
    protected PathData _foundPath;
    
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
    protected PathData BuildPath(GraphNode startNode, GraphNode targetNode)
    {
        List<GraphConnection> path = new();
        T pointer = closedDict[targetNode];

        // Traverse the closedDict backwards to build the path from target to start.
        while (pointer.Node != startNode)
        {
            path.Add(pointer.Connection);
            GraphNode endA = Graph.Nodes[pointer.Connection.startNodeKey];
            pointer = closedDict[endA];
        }

        // As Connections have been stored from target to start order, we must reverse
        // the list to get the path from start to target.
        path.Reverse();
    
        // Now that the Connection list is in correct order, we can build the Path
        // following Connections and taking note of their EndNode positions.
        _foundPath = new PathData
        {
            loop = false
        };
        foreach (GraphConnection connection in path)
        {
            GraphNode endB = Graph.Nodes[connection.endNodeKey];
            _foundPath.positions.Add(endB.position);
        }
    
        return _foundPath;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
    
        // Draw explored nodes.
        foreach (GraphNode exploredNode in closedDict.Keys)  
        {
            Gizmos.color = exploredNodeColor;
            Gizmos.DrawSphere(exploredNode.position, exploredNodeGizmoRadius);
        }
    
        if (_foundPath == null) return;
    
        // Draw found path.
        foreach (Vector2 position in _foundPath.positions)
        {
            Gizmos.color = pathNodeColor;
            Gizmos.DrawSphere(position, pathNodeGizmoRadius);
        }
    }
#endif
}
}