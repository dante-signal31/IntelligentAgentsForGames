using PropertyAttribute;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Implements the A* (A-Star) pathfinding algorithm, which is used to calculate
/// the shortest path between a start and target position in a graph. It calculates the
/// least-cost path from a starting position to a target position by exploring nodes
/// systematically based on their estimated cost to get the goal from them.
/// </summary>
public class AStarGraphPathFinder: 
    HeuristicGraphPathFinder<AStarNodeRecord, AStarPrioritizedNodeRecordSet>
{
    [Header("WIRING")]
    [Tooltip("Heuristic to use for estimating the cost to reach the target.")]
    [InterfaceCompliant(typeof(IAStarHeuristic))]
    [SerializeField] private MonoBehaviour heuristic;
    
    private IAStarHeuristic _heuristic;

    private void Awake()
    {
        _heuristic = (IAStarHeuristic) heuristic;
        currentNodeRecord = AStarNodeRecord.aStarNodeRecordNull;
    }
    
    public override void CalculateCosts(
        IPositionNode startNode,
        EndCondition endCondition)
    {
        // Nodes not fully explored yet, ordered by the estimated cost to get the target
        // through them.
        _openRecordSet.Clear();
        
        // Nodes already fully explored. We use a dictionary to keep track of the
        // information gathered from each node, including the connection to get there,
        // while exploring the graph.
        closedDict.Clear();
        
        // You get to the start node from nowhere (null) and at no cost (0).
        AStarNodeRecord startNodeRecord = new()
        {
            node = CurrentStartNode,
            connection = null,
            costSoFar = 0,
            // For A* to guarantee the shortest path, the heuristic must never
            // overestimate the actual cost (it must be "admissible"). E.g., the heuristic
            // to the target should not be higher than the actual cost to get there.
            //
            // For instance, If you are using Euclidean Distance as a heuristic while your
            // connection costs (graphConnection.Cost) are significantly lower (e.g.,
            // smaller than the pixel distance), the heuristic becomes too "aggressive."
            // Consequence: The algorithm relies so heavily on the direct distance to
            // the target (h) that it ignores terrain costs (g), because the h-value
            // outweighs the accumulated cost.
            //
            // Solution: Ensure graph connection costs are on the same scale as the
            // used heuristic (Euclidean distance in the example).
            totalEstimatedCostToTarget = _heuristic.EstimateCostToTarget(
                CurrentStartNode.Position,
                CurrentTargetNode.Position)
        };
        _openRecordSet.Add(startNodeRecord);

        // Loop until we reach the target node or no more nodes are available to explore.
        while (_openRecordSet.Count > 0)
        {
            // Explore prioritizing the node with the lowest total estimated cost to get
            // the target.
            currentNodeRecord = _openRecordSet.Get();
            if (currentNodeRecord == null) break;

            // If we reached the end node, then our exploration is complete.
            if (endCondition())
            {
                closedDict[currentNodeRecord.node] = currentNodeRecord;
                break;
            }

            // Get all the connections of the current node and take note of the nodes
            // those connections lead to into the _openRecordSet to explore those nodes later.
            foreach (GraphConnection graphConnection in 
                     currentNodeRecord.node.Connections.Values)
            {
                // Where does that connection lead us?
                IPositionNode endNode = Graph.GetNodeById(graphConnection.endNodeId);
                
                // Calculate the cost to reach the end node from the current node.
                float endNodeCost = currentNodeRecord.costSoFar + graphConnection.cost;

                // If that connection leaded to a node fully explored, in Dijkstra we
                // skipped it because no better path could be found to any closed node.
                // Whereas in A* you can have closed a node under a wrong estimation.
                // That's why, in A*, you must check if you've just found a better path to
                // an already closed node.
                if (closedDict.TryGetValue(endNode, out AStarNodeRecord endNodeRecord))
                {
                    // No better path to this node, so skip it.
                    if (endNodeCost >= endNodeRecord.costSoFar) continue;

                    // Otherwise, we must asses this node again to check if it's more 
                    // promising than the previous time. So, we must remove it from 
                    // the ClosedDict and add it back to the OpenSet.
                    closedDict.Remove(endNode);

                    // We could call the heuristic again, but it will return the same
                    // value as the last time. What has changed is the CostSoFar part,
                    // so we remove the old CostSoFar from the total to add the new value.
                    float estimatedCostToTarget =
                        endNodeRecord.totalEstimatedCostToTarget -
                        endNodeRecord.costSoFar;
                    endNodeRecord.costSoFar = endNodeCost;
                    endNodeRecord.totalEstimatedCostToTarget =
                        estimatedCostToTarget + endNodeCost;
                    endNodeRecord.connection = graphConnection;
                }
                // OK, we've just found a node that is still being assessed in the open
                // list.
                else if (_openRecordSet.Contains(endNode))
                {
                    endNodeRecord = _openRecordSet[endNode];
                    // If the end node is already in the open set, but with a lower cost,
                    // it means that we have NOT found a better path to get to it. So skip
                    // it.
                    if (endNodeCost >= endNodeRecord.costSoFar) continue;
                    
                    // Otherwise, update the record with the lower cost and the connection
                    // to get there with that lower cost.
                    //
                    // First, remove the record from the existing set to avoid
                    // corrupting it by editing its values.
                    _openRecordSet.Remove(endNodeRecord);
                    // Now, you can safely edit the record values.
                    // We could call the heuristic again, but it will return the same
                    // value as the last time. What has changed is the CostSoFar part,
                    // so we remove the old CostSoFar from the total to add the new value.
                    float estimatedCostToTarget =
                        endNodeRecord.totalEstimatedCostToTarget -
                        endNodeRecord.costSoFar;
                    endNodeRecord.totalEstimatedCostToTarget = estimatedCostToTarget +
                        endNodeCost;
                    endNodeRecord.costSoFar = endNodeCost;
                    endNodeRecord.connection = graphConnection;
                }
                // If the open set does not contain that node, it means we have
                // discovered a new node. So include it in the open set to explore it 
                // further later.
                else
                {
                    endNodeRecord = new AStarNodeRecord
                    {
                        node = endNode,
                        connection = graphConnection,
                        costSoFar = endNodeCost,
                        totalEstimatedCostToTarget =
                            endNodeCost + _heuristic.EstimateCostToTarget(
                                endNode.Position, 
                                CurrentTargetNode.Position)
                    };
                }
                // Add the node to the _openRecordSet to assess it fully again.
                _openRecordSet.Add(endNodeRecord);
            }
            // As we've finished looking at the connections of the current node, mark it
            // as fully explored, including it in the closed list.
            closedDict[currentNodeRecord.node] = currentNodeRecord;
        }
    }
}
}