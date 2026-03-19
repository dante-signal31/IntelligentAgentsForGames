using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Path positions across a region to link two other regions.
/// </summary>
[Serializable]    
public class InterRegionPath
{
    public List<Vector2> pathPositions;
    public float cost;
}
}