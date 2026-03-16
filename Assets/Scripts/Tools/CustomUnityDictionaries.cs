using System;
using System.Collections.Generic;
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
    public class UintUintDictionary : 
        UnitySerializedDictionary<uint, uint>
    { }
    
    [Serializable]
    public class UintGraphConnectionDictionary : 
        UnitySerializedDictionary<uint, GraphConnection>
    { }

    [Serializable]
    public class Vector2IntPositionNodeDictionary : 
        UnitySerializedDictionary<Vector2Int, PositionNode>
    { }
    
    [Serializable]
    public class UintRegionNodeDictionary : 
        UnitySerializedDictionary<uint, RegionNode>
    { }
    
    [Serializable]
    public class Vector2RegionNodeDictionary : 
        UnitySerializedDictionary<Vector2, RegionNode>
    { }
    
    [Serializable]
    public class LongInterRegionPathDictionary : 
        UnitySerializedDictionary<long, InterRegionPath>
    { }
    
    [Serializable]
    public class UintVector2IntDictionary : 
        UnitySerializedDictionary<uint, Vector2Int>
    { }
    
    [Serializable]
    public class UintListUintDictionary : 
        UnitySerializedDictionary<uint, UintList>
    { }
}
}