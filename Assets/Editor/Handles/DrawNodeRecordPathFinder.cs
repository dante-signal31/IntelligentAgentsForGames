using Pathfinding;
using UnityEditor;

namespace Editor
{ 
/// <summary>
/// Draw costs to reach every node explored by a pathfinder algorithm based on
/// NodeRecords.
/// </summary>
[CustomEditor(typeof(PathFinder<NodeRecord>), true)]
public class DrawNodeRecordPathFinder : DrawPathFinder<NodeRecord> { }
}

