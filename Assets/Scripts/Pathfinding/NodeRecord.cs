namespace Pathfinding
{
/// <summary>
/// Structure needed for the algorithm to keep track of the calculations
/// to reach every node.
/// </summary>
public class NodeRecord
{
    /// <summary>
    /// Node this record refers to.
    /// </summary>
    public PositionNode node;
    
    /// <summary>
    /// Best local connection so far to reach this node.
    /// </summary>
    public GraphConnection connection;
    
    /// <summary>
    /// Best cost so far to reach this node.
    /// </summary>
    public float costSoFar;
    
    public static readonly NodeRecord nodeRecordNull = new()
    {
        node = null,
        connection = null,
        costSoFar = 0
    };
}
}