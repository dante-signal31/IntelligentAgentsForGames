using System;
using Pathfinding;
using UnityEngine;

namespace Tools
{
/// <summary>
/// <p>Because Unity does not serialize generic types, it is necessary to make a concrete
/// Dictionary type for every use by inheriting from the UnitySerializedDictionary.</p>
/// <p>See <see cref="UnitySerializedDictionary{TKey,TValue}"/> for more details.</p>
/// </summary>
public static class CustomUnityDictionaries
{
    [Serializable]
    public class OrientationGraphEdgeDictionary : 
        UnitySerializedDictionary<Orientation, GraphConnection>
    { }

    [Serializable]
    public class Vector2IntGraphNodeDictionary : 
        UnitySerializedDictionary<Vector2Int, GraphNode>
    { }
}
}