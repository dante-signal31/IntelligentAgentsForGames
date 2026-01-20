using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Implements a pathfinding algorithm based on Dijkstra's algorithm to find the shortest
/// path between nodes in a graph. It calculates the least-cost path from a starting
/// position to a target position by exploring nodes systematically based on their cost
/// to be reached from the starting position.
/// </summary>
public class DijkstraGraphPathFinder : HeuristicGraphPathFinder<NodeRecord>
{
    /// <summary>
    /// A specialized collection of node records used in the Dijkstra pathfinding
    /// algorithm. This collection manages nodes to be explored in priority order
    /// based on their accumulated path cost, ensuring that the lowest-cost nodes
    /// are processed first.
    /// </summary>
    protected class DijkstraPrioritizedNodeRecordSet: PrioritizedNodeRecordSet
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
                    return x.node.Id.CompareTo(y.node.Id);
                return result;
            }
        }
        
        public DijkstraPrioritizedNodeRecordSet() : base(new NodeRecordComparer()) {}
    }

    private readonly DijkstraPrioritizedNodeRecordSet _openRecordSet = new ();
    
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
    public override PathData FindPath(Vector2 targetPosition)
    {
        // Nodes not fully explored yet, ordered by the cost to get them from the
        // start node.
        _openRecordSet.Clear();
        
        // Nodes already fully explored. We use a dictionary to keep track of the
        // information gathered from each node, including the connection to get there,
        // while exploring the graph.
        closedDict.Clear();
    
        // Get graph nodes associated with the start and target positions. 
        CurrentStartNode = Graph.GetNodeAtPosition(transform.position);
        PositionNode targetNode = Graph.GetNodeAtPosition(targetPosition);
    
        // You get to the start node from nowhere (null) and at no cost (0).
        NodeRecord startNodeRecord = new()
        {
            node = CurrentStartNode,
            connection = null,
            costSoFar = 0,
        };
        _openRecordSet.Add(startNodeRecord);

        // Loop until we reach the target node or no more nodes are available to explore.
        NodeRecord current = NodeRecord.nodeRecordNull;
        while (_openRecordSet.Count > 0)
        {
            // Explore prioritizing the node with the lowest cost to be reached.
            current = _openRecordSet.Get();
            if (current == null) break;

            // If we reached the end node, then our exploration is complete.
            if (current.node == targetNode)
            {
                closedDict[current.node] = current;
                break;
            }

            // Get all the connections of the current node and take note of the nodes
            // those connections lead to into the _openRecordSet to explore those nodes later.
            foreach (GraphConnection graphConnection in current.node.connections.Values)
            {
                // Where does that connection lead us?
                PositionNode endNode = Graph.GetNodeById(graphConnection.endNodeId);
                // If that connection leads to a node fully explored, skip it.
                if (closedDict.ContainsKey(endNode)) continue;
                // Calculate the cost to reach the end node from the current node.
                float endNodeCost = current.costSoFar + graphConnection.cost;

                NodeRecord endNodeRecord;
                if (_openRecordSet.Contains(endNode))
                {
                    endNodeRecord = _openRecordSet[endNode];
                    // If the end node is already in the open set, but with a lower cost,
                    // it means that we are NOT found a better path to get to it. So skip
                    // it.
                    if (endNodeRecord.costSoFar <= endNodeCost) continue;
                    // Otherwise, update the record with the lower cost and the connection
                    // to get there with that lower cost.
                    //
                    // First, remove the record from the existing set to avoid
                    // corrupting it by editing its values.
                    _openRecordSet.Remove(endNodeRecord);
                    // Now, you can safely edit the record values.
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
                }
                // Add the node to the openSet to assess it fully again.
                _openRecordSet.Add(endNodeRecord);
            }
            // As we've finished looking at the connections of the current node, mark it
            // as fully explored, including it in the closed list.
            closedDict[current.node] = current;
        }
    
        // If we get here and the current record does not point to the targetNode, then
        // we've fully explored the graph without finding a valid path to get the target.
        if (current?.node == null || current.node != targetNode) return null;
    
        // As we've got the target node, analyze the closedDict to follow back connections
        // from the target node to start node to build the path.
        PathData calculatedPath = BuildPath(CurrentStartNode, targetNode);
        return calculatedPath;
    }
}
}

