using System.Collections.Generic;
using Pathfinding;
using UnityEditor;
using UnityEngine;

namespace Editor
{
[CustomEditor(typeof(Path))]
public class DrawPath : UnityEditor.Editor
{
    private readonly List<Vector2> positionHandles = new();

    private void OnSceneGUI()
    {
        var path = (Path)target;

        if (path.positions == null || path.positions.Count < 1)
            return;

        EditorGUI.BeginChangeCheck();

        positionHandles.Clear();
        for (int i = 0; i < path.positions.Count; i++)
        {
            Handles.color = path.GizmosColor;
            // Draw handle to place the offset marker visually.
            positionHandles.Add(Handles.PositionHandle(
                path.transform.TransformPoint(path.positions[i]),
                Quaternion.identity));
            // Highlight the handle with a circle.
            Handles.DrawWireDisc(
                positionHandles[i],
                Vector3.forward,
                0.1f);
            // Show the handle number.
            Vector2 textPosition = path.transform.TransformPoint(
                path.positions[i] +
                path.GizmoTextPosition);
            var textStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = path.GizmosColor }
            };
            Handles.Label(textPosition, $"{path.name}-{i}", textStyle);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(path, "Changed path target positions.");
            // Update the offsets with the changes in the handles.
            for (int i = 0; i < positionHandles.Count; i++)
            {
                path.positions[i] =
                    path.transform.InverseTransformPoint(positionHandles[i]);
            }

            // Force inspector update.
            EditorUtility.SetDirty(path);
        }
    }
}
}

