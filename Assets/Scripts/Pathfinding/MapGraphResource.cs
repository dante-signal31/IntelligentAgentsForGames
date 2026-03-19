using System;
using Tools;

namespace Pathfinding
{
/// <summary>
/// Represents a serialized container for graph node data in a grid-based map system.
/// This class serves as a lightweight storage mechanism, holding a dictionary of nodes
/// that can be used in pathfinding algorithms or graph analyses.
/// </summary>
[Serializable]
public class MapGraphResource
{
    /// <summary>
    /// Dictionary graph array positions to their corresponding position nodes.
    /// </summary>
    public CustomUnityDictionaries.Vector2IntPositionNodeDictionary arrayPositionsToNodes 
        = new();
    
    /// <summary>
    /// Dictionary mapping nodes, identified by their ID, to their corresponding
    /// graph array positions.
    /// </summary>
    public CustomUnityDictionaries.UintVector2IntDictionary nodeIdsToArrayPositions = 
        new();
}
}