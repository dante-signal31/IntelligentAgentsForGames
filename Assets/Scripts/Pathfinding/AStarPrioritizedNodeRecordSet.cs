using System.Collections.Generic;

namespace Pathfinding
{
    /// <summary>
    /// Represents a prioritized set of nodes for the A* pathfinding algorithm.
    /// This class manages open nodes, ordering them based on their total estimated cost
    /// to reach the target, enabling efficient retrieval of the next node to explore.
    /// </summary>
    public class AStarPrioritizedNodeRecordSet: PrioritizedNodeRecordSet<AStarNodeRecord>
    {
        // Comparer to keep the SortedSet ordered by TotalEstimatedCostToTarget
        private class NodeRecordComparer : IComparer<AStarNodeRecord>
        {
            public int Compare(AStarNodeRecord x, AStarNodeRecord y)
            {
                int result = x.totalEstimatedCostToTarget.CompareTo(
                    y.totalEstimatedCostToTarget);
                // If costs are equal, we must not return 0, otherwise SortedSet 
                // thinks they are the same element and won't add the new one.
                if (result == 0 && x.node != y.node)
                    return x.costSoFar.CompareTo(y.costSoFar);
                return result;
            }
        }
        
        public AStarPrioritizedNodeRecordSet() : base(new NodeRecordComparer()) {}
    }
}