using System.Collections.Generic;
using System.ComponentModel.Composition;
using Tools;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Node representing a region. A region is a group of position nodes.
/// </summary>
public class RegionNode: GraphNode, IPositionNode
{
    [SerializeField] private Vector2 position;

    /// <summary>
    /// Position of the region center.
    /// </summary>
    public Vector2 Position
    {
        get  => position;
        set => position = value;
    }
   
    /// <summary>
    /// Region nodes that border another region.
    /// </summary>
    /// <remarks>
    /// Key is the region ID of the neighbor region.
    /// Value is an array of position node IDs that border the neighbor region.
    /// </remarks>
    public CustomUnityDictionaries.UintListUintDictionary boundaryNodes = new();
}
}