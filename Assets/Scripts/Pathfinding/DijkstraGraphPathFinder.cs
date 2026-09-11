namespace Pathfinding
{
/// <summary>
/// Implements a pathfinding algorithm based on Dijkstra's algorithm to find the shortest
/// path between nodes in a graph. It calculates the least-cost path from a starting
/// position to a target position by exploring nodes systematically based on their cost
/// to be reached from the starting position.
/// </summary>
public class DijkstraGraphPathFinder : 
    HeuristicGraphPathFinder<NodeRecord, DijkstraPrioritizedNodeRecordSet>
{
    private void Awake()
    {
        currentNodeRecord = NodeRecord.nodeRecordNull;
    }

    protected override void ExploreGraph(IPositionNode startNode, EndCondition endCondition)
    {
        CalculateCosts(startNode, endCondition);
    }

    public void CalculateCosts(IPositionNode startNode, EndCondition endCondition)
    {
        // Nodes not fully explored yet, ordered by the cost to get them from the
        // start node.
        openRecordSet.Clear();
        
        // Nodes already fully explored. We use a dictionary to keep track of the
        // information gathered from each node, including the connection to get there,
        // while exploring the graph.
        closedDict.Clear();
        
        // You get to the start node from nowhere (null) and at no cost (0).
        NodeRecord startNodeRecord = new()
        {
            node = startNode,
            connection = null,
            costSoFar = 0,
        };
        openRecordSet.Add(startNodeRecord);

        // Loop until we reach the target node or no more nodes are available to explore.
        while (openRecordSet.Count > 0)
        {
            // Explore prioritizing the node with the lowest cost to be reached.
            currentNodeRecord = openRecordSet.Get();
            if (currentNodeRecord == null) break;

            // If we comply with end condition, then our exploration is complete.
            if (endCondition())
            {
                closedDict[currentNodeRecord.node] = currentNodeRecord;
                break;
            }

            // Get all the connections of the current node and take note of the nodes
            // those connections lead to into the openRecordSet to explore those nodes
            // later.
            foreach (GraphConnection graphConnection in 
                     currentNodeRecord.node.Connections.Values)
            {
                // Where does that connection lead us?
                IPositionNode endNode = Graph.GetNodeById(graphConnection.endNodeId);
                
                // If that connection leads to a node fully explored, skip it.
                if (closedDict.ContainsKey(endNode)) continue;
                
                // Calculate the cost to reach the end node from the current node.
                float endNodeCost = currentNodeRecord.costSoFar + graphConnection.cost;

                NodeRecord endNodeRecord;
                if (openRecordSet.Contains(endNode))
                {
                    endNodeRecord = openRecordSet[endNode];
                    // If the end node is already in the open set, but with a lower cost,
                    // it means that we are NOT found a better path to get to it. So skip
                    // it.
                    if (endNodeRecord.costSoFar <= endNodeCost) continue;
                    // Otherwise, update the record with the lower cost and the connection
                    // to get there with that lower cost.
                    //
                    // First, remove the record from the existing set to avoid
                    // corrupting it by editing its values.
                    openRecordSet.Remove(endNodeRecord);
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
                openRecordSet.Add(endNodeRecord);
            }

            // As we've finished looking at the connections of the current node, mark it
            // as fully explored, including it in the closed list.
            closedDict[currentNodeRecord.node] = currentNodeRecord;
        }
    }
}
}

