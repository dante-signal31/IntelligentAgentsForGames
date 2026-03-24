using System;
using UnityEngine;

namespace Tools
{
/// <summary>
/// Struct to represent ray ends for every sensor in the prefab local space.
/// </summary>
[Serializable] 
public struct RayEnds
{
    public Vector3 start;
    public Vector3 end;

    public RayEnds(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
    }
}
}