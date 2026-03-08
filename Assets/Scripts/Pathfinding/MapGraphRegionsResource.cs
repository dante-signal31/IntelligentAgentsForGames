using System;
using Tools;

namespace Pathfinding
{
/// <summary>
/// This class serves as a resource for associating graph nodes with their respective
/// regions within a map. 
/// </summary>
[Serializable]
public class MapGraphRegionsResource
{
    /// <summary>
    /// Dictionary mapping node ID to region ID.
    /// </summary>
    public CustomUnityDictionaries.UintUintDictionary nodesIdToRegionsId = new();
}
}