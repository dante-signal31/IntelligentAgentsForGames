using Tools;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Interface for every graph node that has a spatial meaning.
/// </summary>
public interface IPositionNode
{
    /// <summary>
    /// Unique identifier of the node.
    /// </summary>
    public uint Id { get; }
    
    /// <summary>
    /// Global position of the node.
    /// </summary>
    public Vector2 Position { get; set; }
    
    /// <summary>
    /// Connections from this node to other nodes.
    /// </summary>
    public CustomUnityDictionaries.UintGraphConnectionDictionary Connections { get; }
}
}