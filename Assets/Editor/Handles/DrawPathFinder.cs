using Pathfinding;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
/// <summary>
/// Draw costs to reach every node explored by a pathfinder algorithm.
/// </summary>
public class DrawPathFinder<T> : UnityEditor.Editor where T: NodeRecord, new()
{
    protected void OnSceneGUI()
    {
        var pathFinder = (GraphPathFinder<T>)target;

        if (!pathFinder.showGizmos) return;

        var textStyle = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = pathFinder.textColor }
        };

        foreach (PositionNode exploredNode in pathFinder.closedDict.Keys)
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
                uint fromNodeId =
                    pathFinder.closedDict[exploredNode].connection.startNodeId;
                PositionNode fromNode = pathFinder.Graph.GetNodeById(fromNodeId);
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