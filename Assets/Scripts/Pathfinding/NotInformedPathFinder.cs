using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Represents a base implementation for pathfinding algorithms
/// that do not rely on heuristic information to guide the search.
/// </summary>
public abstract class NotInformedPathFinder: PathFinder<NodeRecord>
{
    /// <summary>
    /// Finds and returns a path to the specified target position.
    /// Depending on the implementation, this uses a specific node collection
    /// type to determine the sequence of nodes to explore during pathfinding.
    /// </summary>
    /// <typeparam name="TN">The type of node collection to use for pathfinding.
    /// Must implement INodeCollection.</typeparam>
    /// <param name="targetPosition">The target position to find a path to.</param>
    /// <returns>A Path object representing the found path from the start position
    /// to the target position, or null if no valid path exists.</returns>
    protected PathData FindPath<TN>(Vector2 targetPosition) 
        where TN: INodeCollection<NodeRecord>, new()
    {
        // Nodes not fully explored yet, ordered as they were found.
        TN openQueue = new();
        
        // Nodes already fully explored. We use a dictionary to keep track of the
        // information gathered from each node, including the connection to get there,
        // while exploring the graph.
        closedDict.Clear();
        
        // Get graph nodes associated with the start and target positions. 
        CurrentStartNode = Graph.GetNodeAtPosition(transform.position);
        GraphNode targetNode = Graph.GetNodeAtPosition(targetPosition);
        
        // You get to the start node from nowhere (null) and at no cost (0).
        var startRecord = new NodeRecord
        {
            node = CurrentStartNode,
            connection = null,
            costSoFar = 0,
        };
        openQueue.Add(startRecord);
        
        // Loop until we reach the target node or no more nodes are available to explore.
        NodeRecord current = NodeRecord.nodeRecordNull;
        while (openQueue.Count > 0)
        {
            // Explore the pending node that was first discovered.
            current = openQueue.Get();
            if (current == null) break;

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
                if (openQueue.Contains(endNode))
                {
                    endNodeRecord = openQueue[endNode];
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
                    openQueue.Add(endNodeRecord);
                }
            }
            
            // As we've finished looking at the connections of the current node, mark it
            // as fully explored, including it in the closed list.
            closedDict[current.node] = current;
        }
        
        // If we get here and the current record does not point to the targetNode, then
        // we've fully explored the graph without finding a valid path to get the target.
        if (current?.node == null || current.node != targetNode) 
            return null;
        
        // As we've got the target node, analyze the closedDict to follow back connections
        // from the target node to start node to build the path.
        foundPath = BuildPath(CurrentStartNode, targetNode);
        return foundPath;
    }
}
}