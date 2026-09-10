using System.Collections.Generic;

namespace Pathfinding
{
    /// <summary>
    /// A specialized collection of node records used in the Dijkstra pathfinding
    /// algorithm. This collection manages nodes to be explored in priority order
    /// based on their accumulated path cost, ensuring that the lowest-cost nodes
    /// are processed first.
    /// </summary>
    public class DijkstraPrioritizedNodeRecordSet: PrioritizedNodeRecordSet<NodeRecord>
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
}