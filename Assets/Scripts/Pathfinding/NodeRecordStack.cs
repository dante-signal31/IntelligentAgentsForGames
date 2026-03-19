using System.Collections.Generic;

namespace Pathfinding
{
/// <summary>
/// A stack-based collection used for managing NodeRecord objects during
/// pathfinding operations. This class ensures that nodes are only added
/// once and supports operations for adding, retrieving, and checking nodes.
/// </summary>
public class NodeRecordStack: INodeRecordCollection<NodeRecord>
{
    // Needed to keep ordered the NodeRecords pending to be explored. The order is
    // last-found-first-to-be-explored.
    private readonly Stack<NodeRecord> _stack = new();

    // Needed to keep track of the nodes already discovered.
    private readonly Dictionary<IPositionNode, NodeRecord> _nodeRecordDict = new();

    public int Count => _stack.Count;

    public void Clear()
    {
        _stack.Clear();
        _nodeRecordDict.Clear();
    }

    public bool Contains(IPositionNode node) => _nodeRecordDict.ContainsKey(node);

    public void Add(NodeRecord record)
    {
        // If the node was already discovered before, then do nothing. If you stack
        // the discovered nodes again, you would end up with loops.
        if (_nodeRecordDict.ContainsKey(record.node)) return;

        // Standard case.
        _stack.Push(record);
        _nodeRecordDict[record.node] = record;
    }

    public NodeRecord this[IPositionNode node]
    {
        get => _nodeRecordDict[node];
        set => _nodeRecordDict[node] = value;
    }

    /// <summary>
    /// Retrieves and removes the top NodeRecord from the stack.
    /// This method returns the NodeRecord instance currently
    /// at the top of the stack while preserving its reference in the
    /// associated dictionary used for tracking discovered nodes.
    /// If the stack is empty, it returns null.
    /// </summary>
    /// <returns>
    /// The NodeRecord at the top of the stack, or null if the stack is empty.
    /// </returns>
    public NodeRecord Get()
    {
        if (_stack.Count == 0) return null;
        NodeRecord recoveredNodeRecord = _stack.Pop();
        // Do not remove the node from the dictionary. That dictionary is used to keep
        // track of the nodes already discovered, to avoid loops.
        return recoveredNodeRecord;
    }
}
}