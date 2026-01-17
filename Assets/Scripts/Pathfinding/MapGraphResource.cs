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
    public CustomUnityDictionaries.Vector2IntGraphNodeDictionary nodes = new();
    
    // Nodes store GraphNodes indexed by their array position in the spatial grid.
    // We need something to map node ids to array positions. That's what this
    // dictionary does.
    public CustomUnityDictionaries.UintVector2IntDictionary nodeArrayPositionsById = 
        new();
}
}