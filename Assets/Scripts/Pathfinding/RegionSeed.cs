using System;
using UnityEngine;

namespace Pathfinding
{ 
/// <summary>
/// Represents an influence point within the map graph regions, defining a specific
/// position, influence magnitude, and a visual representation color for debugging
/// purposes.
/// </summary>
[Serializable]
public class RegionSeed
{
    public Vector2 position = Vector2.zero;
    public float influence = 1.0f;
    public Color gizmoColor = Color.white;
}
}