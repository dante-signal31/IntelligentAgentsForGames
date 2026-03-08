using Tools;

namespace Pathfinding
{
/// <summary>
/// Represents a serialized container for region graph node data in a grid-based map
/// system. This class serves as a lightweight storage mechanism, holding a dictionary
/// of nodes that can be used in pathfinding algorithms or graph analyses.
/// </summary>
public class RegionGraphResource
{
    /// <summary>
    /// Dictionary mapping region ID to region node.
    /// </summary>
    public CustomUnityDictionaries.UintRegionNodeDictionary regionIdToRegionNode 
        = new();
    
    /// <summary>
    /// Dictionary mapping position to region node.
    /// </summary>
    public CustomUnityDictionaries.Vector2RegionNodeDictionary positionToRegionNode 
        = new();
    
    /// <summary>
    /// Dictionary mapping an array of path positions for every node to any other region. 
    /// </summary>
    public CustomUnityDictionaries.LongInterRegionPathDictionary fromNodeToRegionPaths 
        = new();
    
    /// <summary>
    /// Chain two uint into a single ulong.
    /// <remarks>
    /// Godot can only serialize Dictionaries if their keys are simple variant types. My
    /// Unity implementation for serialized dictionaries does not have this limitation.
    /// Anyway, I want to keep the code simple. It's simpler converting two uint into
    /// a single ulong than creating an instance of a struct to use it as the key.
    /// </remarks>
    /// </summary>
    /// <param name="fromNodeId">Node ID we are starting from.</param>
    /// <param name="toRegionId">Region ID where we want to go.</param>
    /// <returns></returns>
    public static long GetFromNodeToRegionKey(uint fromNodeId, uint toRegionId)
    {
        return (long)fromNodeId << 32 | toRegionId;
    }

    /// <summary>
    /// Splits a combined key into its constituent parts: the node ID and the region ID.
    /// </summary>
    /// <param name="key">The combined key containing both the node ID and the
    /// region ID.</param>
    /// <param name="fromNodeId">The extracted node ID component of the key.</param>
    /// <param name="toRegionId">The extracted region ID component of the key.</param>
    public static void SplitKey(long key, out uint fromNodeId, out uint toRegionId)
    {
        fromNodeId = (uint)((ulong)key >> 32);
        toRegionId = (uint)key;
    }
}
}