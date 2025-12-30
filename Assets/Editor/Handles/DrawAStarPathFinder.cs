using Pathfinding;
using UnityEditor;
using UnityEngine;

namespace Editor
{
/// <summary>
/// Draw costs to reach every node explored by the AStar algorithm.
/// </summary>
[CustomEditor(typeof(AStarPathFinder))]
public class DrawAStarPathFinder : UnityEditor.Editor
{
    private void OnSceneGUI()
    {
        var pathFinder = (AStarPathFinder)target;

        if (!pathFinder.showGizmos) return;

        var textStyle = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = pathFinder.textColor }
        };

        foreach (GraphNode exploredNode in pathFinder.closedDict.Keys)
        {
            // Mark every node with the smallest cost to get there from the start node
            // and the local orientation of the connection to get there.
            Vector2 gizmoBorder = new Vector2(
                pathFinder.exploredNodeGizmoRadius,
                pathFinder.exploredNodeGizmoRadius);
            Vector2 textPosition = exploredNode.position - gizmoBorder -
                                   pathFinder.gizmoTextOffset;
            if (exploredNode == pathFinder.CurrentStartNode)
            {
                // The initial node has no cost nor comes from any Connection, so we
                // just mark it with the word "Start".
                Handles.Label(textPosition, "Start", textStyle);
            }
            else
            {
                Vector2Int fromNodeKey =
                    pathFinder.closedDict[exploredNode].connection.startNodeKey;
                GraphNode fromNode = pathFinder.Graph.Nodes[fromNodeKey];
                Vector2 relativePosition = exploredNode.position - fromNode.position;
                // Connection orientation from the receiving node perspective (the
                // explored node).
                string connectionOrientation;
                if (Mathf.Approximately(relativePosition.x, 0f))
                {
                    connectionOrientation = relativePosition.y > 0f ? "S" : "N";
                }
                else
                {
                    connectionOrientation = relativePosition.x > 0f ? "W" : "E";
                }

                string nodeInfoText =
                    $"{connectionOrientation}" +
                    $"{pathFinder.closedDict[exploredNode].costSoFar}";
                Handles.Label(textPosition, nodeInfoText, textStyle);
            }
        }
    }
}
}