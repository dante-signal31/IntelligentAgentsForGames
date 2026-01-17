using System.Collections.Generic;

namespace Pathfinding
{
    public class NodeRecordStack: INodeRecordCollection<NodeRecord>
{
    // Needed to keep ordered the NodeRecords pending to be explored. The order is
    // last-found-first-to-be-explored.
    private readonly Stack<NodeRecord> _stack = new();

    // Needed to keep track of the nodes already discovered.
    private readonly Dictionary<PositionNode, NodeRecord> _nodeRecordDict = new();

    public int Count => _nodeRecordDict.Count;

    public void Clear()
    {
        _stack.Clear();
        _nodeRecordDict.Clear();
    }

    public bool Contains(PositionNode node) => _nodeRecordDict.ContainsKey(node);

    public void Add(NodeRecord record)
    {
        // If the node was already discovered before, then do nothing. If you stack
        // the discovered nodes again, you would end up with loops.
        if (_nodeRecordDict.ContainsKey(record.node)) return;

        // Standard case.
        _stack.Push(record);
        _nodeRecordDict[record.node] = record;
    }

    public NodeRecord this[PositionNode node]
    {
        get => _nodeRecordDict[node];
        set => _nodeRecordDict[node] = value;
    }

    public NodeRecord Get()
    {
        if (_stack.Count == 0) return null;
        NodeRecord recoveredNodeRecord = _stack.Pop();
        return recoveredNodeRecord;
    }
}
}