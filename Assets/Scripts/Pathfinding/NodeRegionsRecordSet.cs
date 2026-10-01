using System.Collections.Generic;

namespace Pathfinding
{
/// <summary>
/// This collection manages nodes to be explored in priority order
/// based on their accumulated path cost, ensuring that the lowest-cost nodes
/// are processed first.
/// </summary>
class NodeRegionsRecordSet : PrioritizedNodeRecordSet<RegionNodeRecord>
{
    // Comparer to keep the SortedSet ordered by TotalEstimatedCostToTarget
    private class NodeRecordComparer : IComparer<RegionNodeRecord>
    {
        public int Compare(RegionNodeRecord x, RegionNodeRecord y)
        {
            int result = x.costSoFar.CompareTo(y.costSoFar);
            // If costs are equal, we must not return 0, otherwise SortedSet 
            // thinks they are the same element and won't add the new one.
            if (result == 0 && x.node != y.node)
                return x.node.Id.CompareTo(y.node.Id);
            return result;
        }
    }

    public NodeRegionsRecordSet() : base(new NodeRecordComparer())
    {
    }
}
}