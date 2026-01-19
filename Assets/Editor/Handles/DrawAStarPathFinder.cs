using Pathfinding;
using UnityEditor;
using UnityEngine;

namespace Editor
{
/// <summary>
/// Draw costs to reach every node explored by the AStar algorithm.
/// </summary>
[CustomEditor(typeof(GraphPathFinder<AStarNodeRecord>), true)]
public class DrawAStarPathFinder : DrawPathFinder<AStarNodeRecord> { }
}