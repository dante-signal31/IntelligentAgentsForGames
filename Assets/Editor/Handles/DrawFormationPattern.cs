using System.Collections.Generic;
using Groups;
using SteeringBehaviors;
using UnityEditor;
using UnityEngine;

namespace Editor
{ 
[CustomEditor(typeof(FormationPattern))]
public class DrawFormationPattern : UnityEditor.Editor
{
    private List<Vector2> positionHandles = new();
    
    private void OnSceneGUI()
    {
        var pattern = (FormationPattern)target;
        
        if (pattern.positions == null || pattern.positions.Offsets.Length < 1) 
            return;
        
        EditorGUI.BeginChangeCheck();
        
        positionHandles.Clear();
        for (int i=0; i < pattern.positions.Offsets.Length; i++)
        {
            Handles.color = pattern.GizmosColor;
            // Draw handle to place the offset marker visually.
            positionHandles.Add(Handles.PositionHandle(
                pattern.transform.TransformPoint(pattern.positions.Offsets[i]),
                Quaternion.identity));
            // Highlight the handle with a circle.
            Handles.DrawWireDisc(
                positionHandles[i], 
                Vector3.forward, 
                0.1f);
            // Show the handle number.
            Vector2 textPosition = pattern.transform.TransformPoint(
                pattern.positions.Offsets[i] + 
                pattern.GizmoTextPosition);
            var greenStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = pattern.GizmosColor }
            };
            Handles.Label(textPosition, $"{i}", greenStyle);
        }
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(pattern, "Changed formation pattern.");
            // Update the offsets with the changes in the handles.
            for (int i = 0; i < positionHandles.Count; i++)
            {
                pattern.positions.Offsets[i] = 
                    pattern.transform.InverseTransformPoint(positionHandles[i]);
            }
            // Force inspector update.
            EditorUtility.SetDirty(pattern);
        }
    }
    
}
}