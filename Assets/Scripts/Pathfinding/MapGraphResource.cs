using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using Tools;
using UnityEngine;

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
    public CustomUnityDictionaries.Vector2IntGraphNodeDictionary nodes;
}
}