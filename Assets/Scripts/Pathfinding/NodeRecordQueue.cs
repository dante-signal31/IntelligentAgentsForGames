using System.Collections.Generic;

namespace Pathfinding
{
/// <summary>
/// Represents a queue-based data structure for managing a collection
/// of <see cref="NodeRecord"/> objects used in pathfinding algorithms. The queue ensures
/// nodes are processed in a first-in, first-out (FIFO) order while maintaining a
/// dictionary for quick lookups.
/// </summary>
/// <remarks>
/// This class is designed to prevent duplicate entries of nodes by tracking discovered
/// nodes in an internal dictionary. It is commonly used in breadth-first search
/// (BFS) or other pathfinding strategies where nodes need to be processed sequentially.
/// </remarks>
public class NodeRecordQueue : INodeRecordCollection<NodeRecord>
{
    // Needed to keep ordered the NodeRecords pending to be explored. The order is
    // first-found-first-to-be-explored.
    private readonly Queue<NodeRecord> _queue = new();

    // Needed to keep track of the nodes already discovered.
    private readonly Dictionary<IPositionNode, NodeRecord> _nodeRecordDict = new();

    public int Count => _queue.Count;

    public void Clear()
    {
        _queue.Clear();
        _nodeRecordDict.Clear();
    }

    public bool Contains(IPositionNode node) => _nodeRecordDict.ContainsKey(node);

    public void Add(NodeRecord record)
    {
        // If the node was already discovered before, then do nothing. If you enqueue
        // the discovered nodes again, you would end up with loops.
        if (_nodeRecordDict.ContainsKey(record.node)) return;

        // Standard case.
        _queue.Enqueue(record);
        _nodeRecordDict[record.node] = record;
    }

    public NodeRecord this[IPositionNode node]
    {
        get => _nodeRecordDict[node];
        set => _nodeRecordDict[node] = value;
    }

    /// <summary>
    /// Retrieves the next NodeRecord from the queue. If the queue is empty, it returns
    /// null.
    /// </summary>
    /// <remarks>
    /// Once got the NodeRecord, it is removed from the queue.
    /// </remarks>
    /// <returns>
    /// The next NodeRecord in the queue, or null if the queue is empty.
    /// </returns>
    public NodeRecord Get()
    {
        if (_queue.Count == 0) return null;
        NodeRecord recoveredNodeRecord = _queue.Dequeue();
        // Do not remove the node from the dictionary. That dictionary is used to keep
        // track of the nodes already discovered, to avoid loops.
        return recoveredNodeRecord;
    }
}
}