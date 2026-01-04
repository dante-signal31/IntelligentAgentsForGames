using System.Collections.Generic;

namespace Pathfinding
{
/// <summary>
/// Classes implementing inheriting this class are responsible for navigating a graph
/// structure to find a path to a target position.
/// <remarks>
/// The heuristic pathfinders are informed searchers that use heuristics to
/// estimate which graph branches are more promising to get the goal so they can be
/// explored first.
/// </remarks> 
/// </summary>
public abstract class HeuristicPathFinder<T>: PathFinder<T>
    where T: NodeRecord, new()
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
    protected abstract class PrioritizedNodeSet: INodeCollection<T>
    {
        // Needed to keep ordered by cost the NodeRecords of the node pending to be
        // explored.
        // Initially, I planned to use a PriorityQueue<GraphNode, float>, but I found that
        // the Unity .NET API doesn't support it, because that collection was added in
        // .NET 6, while my Unity version is .NET Framework 4.7.1.
        private readonly SortedSet<T> prioritySet;
        // Needed to keep track of the nodes still pending to be explored and to quickly
        // get their respective records.
        private readonly Dictionary<GraphNode, T> nodeRecordDict = new ();
    
        public int Count => nodeRecordDict.Count;
        public bool Contains(GraphNode node) => nodeRecordDict.ContainsKey(node);

        protected PrioritizedNodeSet(IComparer<T> comparer)
        {
            prioritySet = new SortedSet<T>(comparer);
        }

        public void Add(T record)
        {
            // If the node already exists, we must remove it first because SortedSet
            // doesn't update positions automatically if an existing node costSoFar
            // value changes.
            if (nodeRecordDict.TryGetValue(record.node, out var value))
            {
                prioritySet.Remove(value);
            }
            
            prioritySet.Add(record);
            nodeRecordDict[record.node] = record;
        }
        
        public void Remove(T record)
        {
            if (nodeRecordDict.ContainsKey(record.node))
            {
                prioritySet.Remove(nodeRecordDict[record.node]);
                nodeRecordDict.Remove(record.node);
            }
        }

        /// <summary>
        /// Provides indexed access to the node records using a GraphNode as the key.
        /// </summary>
        public T this[GraphNode node]
        {
            get => nodeRecordDict[node];
            set => nodeRecordDict[node] = value;
        }

        /// <summary>
        /// Extracts and removes the node record with the lowest cost-so-far value
        /// from the prioritized set. 
        /// </summary>
        /// <returns>
        /// The node record with the lowest cost-so-far value or a null if there are no
        /// valid records available in the set.
        /// </returns>
        public T Get()
        {
            if (prioritySet.Count == 0) return null;

            // In SortedSet, Min is the element with the lowest priority/cost.
            T lowest = prioritySet.Min;
            prioritySet.Remove(lowest);
            nodeRecordDict.Remove(lowest.node);
        
            return lowest;
        }
    }
}
}