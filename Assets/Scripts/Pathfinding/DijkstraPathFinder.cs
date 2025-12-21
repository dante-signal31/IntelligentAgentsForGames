using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
public class DijkstraPathFinder : MonoBehaviour, IPathFinder
{
    /// <summary>
    /// Structure needed for the Dijkstra algorithm to keep track of the calculations
    /// to reach every node.
    /// </summary>
    public struct NodeRecord : IEquatable<NodeRecord>
    {
        public GraphNode node;
        public GraphConnection connection;
        public float costSoFar;
    
        public bool Equals(NodeRecord other)
        {
            return Equals(node, other.node) && 
                   Equals(connection, other.connection) && 
                   costSoFar.Equals(other.costSoFar);
        }

        public override bool Equals(object obj)
        {
            return obj is NodeRecord other && Equals(other);
        }
    
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = node != null ? node.GetHashCode() : 0;
                hash = (hash * 397) ^ (connection != null ? connection.GetHashCode() : 0);
                hash = (hash * 397) ^ costSoFar.GetHashCode();
                return hash;
            }
        }
    
        public static bool operator ==(NodeRecord left, NodeRecord right)
        {
            return left.Equals(right);
        }
    
        public static bool operator !=(NodeRecord left, NodeRecord right)
        {
            return !left.Equals(right);
        }
    }

    private static readonly NodeRecord NodeRecordNull = new NodeRecord
    {
        node = null,
        connection = null,
        costSoFar = 0
    };

    /// <summary>
    /// A collection of nodes with priority-based access for use in pathfinding
    /// algorithms like Dijkstra.
    /// </summary>
    /// <remarks>
    /// This class maintains a set of node along with their associated costs for
    /// traversing a graph. It provides functionality to add and remove nodes, check for
    /// node existence, and retrieve the node with the lowest cost value.
    /// </remarks>
    private class PrioritizedNodeSet
    {
        // Comparer to keep the SortedSet ordered by CostSoFar
        private class NodeRecordComparer : IComparer<NodeRecord>
        {
            public int Compare(NodeRecord x, NodeRecord y)
            {
                int result = x.costSoFar.CompareTo(y.costSoFar);
                // If costs are equal, we must not return 0, otherwise SortedSet 
                // thinks they are the same element and won't add the new one.
                if (result == 0 && x.node != y.node)
                    return x.GetHashCode().CompareTo(y.GetHashCode());
                return result;
            }
        }
    
        // Needed to keep ordered by cost the NodeRecords of the node pending to be
        // explored.
        // Initially, I planned to use a PriorityQueue<GraphNode, float>, but I found that
        // the Unity .NET API doesn't support it, because that collection was added in
        // .NET 6, while my Unity version is .NET Framework 4.7.1.
        private readonly SortedSet<NodeRecord> _prioritySet = 
            new(new NodeRecordComparer());
        // Needed to keep track of the nodes still pending to be explored and to quickly
        // get their respective records.
        private readonly Dictionary<GraphNode, NodeRecord> _nodeRecordDict = new ();
    
        public int Count => _nodeRecordDict.Count;
        public bool Contains(GraphNode node) => _nodeRecordDict.ContainsKey(node);
    
        public static PrioritizedNodeSet operator +(
            PrioritizedNodeSet set, 
            NodeRecord record)
        {
            // If the node already exists, we must remove it first because SortedSet
            // doesn't update positions automatically if an existing node costSoFar
            // value changes.
            if (set._nodeRecordDict.ContainsKey(record.node))
            {
                set._prioritySet.Remove(set._nodeRecordDict[record.node]);
            }
        
            set._prioritySet.Add(record);
            set._nodeRecordDict[record.node] = record;
            return set;
        }

        public static PrioritizedNodeSet operator -(
            PrioritizedNodeSet set, 
            NodeRecord record)
        {
            if (set._nodeRecordDict.ContainsKey(record.node))
            {
                set._prioritySet.Remove(set._nodeRecordDict[record.node]);
                set._nodeRecordDict.Remove(record.node);
            }
            return set;
        }
    
        public NodeRecord this[GraphNode node]
        {
            get => _nodeRecordDict[node];
            set => _nodeRecordDict[node] = value;
        }

        /// <summary>
        /// Extracts and removes the node record with the lowest cost-so-far value
        /// from the prioritized set. 
        /// </summary>
        /// <returns>
        /// The node record with the lowest cost-so-far value or a null-equivalent record
        /// (NodeRecordNull) if there are no valid records available in the set.
        /// </returns>
        public NodeRecord ExtractLowestCostNodeRecord()
        {
            if (_prioritySet.Count == 0) return NodeRecordNull;

            // In SortedSet, Min is the element with the lowest priority/cost.
            NodeRecord lowest = _prioritySet.Min;
            _prioritySet.Remove(lowest);
            _nodeRecordDict.Remove(lowest.node);
        
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
    public readonly Dictionary<GraphNode, NodeRecord> closedDict = new();
    public GraphNode CurrentStartNode { get; private set; }
    
    private PathData _foundPath;

    // private void Awake()
    // {
    //     _closedDict = new Dictionary<GraphNode, NodeRecord>();
    // }

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
    public PathData FindPath(Vector2 targetPosition)
    {
        // Nodes not fully explored yet, ordered by the cost to get them from the
        // start node.
        PrioritizedNodeSet openSet = new ();
        // Nodes already fully explored. We use a dictionary to keep track of the
        // information gathered from each node, including the connection to get there,
        // while exploring the graph.
        closedDict.Clear();
    
        // Get graph nodes associated with the start and target positions. 
        CurrentStartNode = Graph.GetNodeAtPosition(transform.position);
        GraphNode targetNode = Graph.GetNodeAtPosition(targetPosition);
    
        // You get to the start node from nowhere (null) and at no cost (0).
        openSet += new NodeRecord {
            node = CurrentStartNode,
            connection = null,
            costSoFar = 0,
        };

        // Loop until we reach the target node or no more nodes are available to explore.
        NodeRecord current = NodeRecordNull;
        while (openSet.Count > 0)
        {
            // Explore prioritizing the node with the lowest cost to be reached.
            current = openSet.ExtractLowestCostNodeRecord();
            if (current == NodeRecordNull) break;

            // If we reached the end node, then our exploration is complete.
            if (current.node == targetNode)
            {
                closedDict[current.node] = current;
                break;
            }

            // Get all the connections of the current node and take note of the nodes
            // those connections lead to into the openSet to explore those nodes later.
            foreach (GraphConnection graphConnection in current.node.connections.Values)
            {
                // Where does that connection lead us?
                GraphNode endNode = Graph.Nodes[graphConnection.endNodeKey];
                // If that connection leads to a node fully explored, skip it.
                if (closedDict.ContainsKey(endNode)) continue;
                // Calculate the cost to reach the end node from the current node.
                float endNodeCost = current.costSoFar + graphConnection.cost;

                NodeRecord endNodeRecord;
                if (openSet.Contains(endNode))
                {
                    endNodeRecord = openSet[endNode];
                    // If the end node is already in the open set, but with a lower cost,
                    // it means that we are NOT found a better path to get to it. So skip
                    // it.
                    if (endNodeRecord.costSoFar <= endNodeCost) continue;
                    // Otherwise, update the record with the lower cost and the connection
                    // to get there with that lower cost.
                    endNodeRecord.costSoFar = endNodeCost;
                    endNodeRecord.connection = graphConnection;
                }
                else
                {
                    // If the open set does not contain that node, it means we have
                    // discovered a new node. So include it in the open set to explore it 
                    // further later.
                    endNodeRecord = new NodeRecord
                    {
                        node = endNode,
                        connection = graphConnection,
                        costSoFar = endNodeCost,
                    };
                    openSet += endNodeRecord;
                }
            }
        
            // As we've finished looking at the connections of the current node, mark it
            // as fully explored, including it in the closed list.
            closedDict[current.node] = current;
        }
    
        // If we get here and the current record does not point to the targetNode, then
        // we've fully explored the graph without finding a valid path to get the target.
        if (current.node != targetNode) return null;
    
        // As we've got the target node, analyze the closedDict to follow back connections
        // from the target node to start node to build the path.
        PathData foundPath = BuildPath(CurrentStartNode, targetNode);
        return foundPath;
    }

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
    private PathData BuildPath(GraphNode startNode, GraphNode targetNode)
    {
        List<GraphConnection> path = new();
        NodeRecord pointer = closedDict[targetNode];

        // Traverse the closedDict backwards to build the path from target to start.
        while (pointer.node != startNode)
        {
            path.Add(pointer.connection);
            GraphNode endA = Graph.Nodes[pointer.connection.startNodeKey];
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

