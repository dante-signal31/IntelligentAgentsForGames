using Editor.Tools;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Editor
{
[CustomEditor(typeof(ConeRange))]
public class DrawConeRange : UnityEditor.Editor
{
    private void OnSceneGUI()
    {
        var cone = (ConeRange)target;

        EditorGUI.BeginChangeCheck();
        (float newAheadSemiConeDegrees, float newRange) = DrawAheadCone(cone);
        
        // Draw lines to see visual aids in the scene tab.
        Vector2 handlePosition = Quaternion.AngleAxis(
            newAheadSemiConeDegrees,
            Vector3.forward) * (Vector3.up * cone.Range);
        Vector2 globalHandlePosition = cone.transform.TransformPoint(handlePosition);
        Handles.color = Color.green;
        Handles.DrawLine(cone.transform.position, globalHandlePosition);
        Handles.DrawWireDisc(globalHandlePosition, Vector3.forward, 0.1f);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(cone, $"Changed cone data.");
            cone.SemiConeDegrees = newAheadSemiConeDegrees;
            cone.Range = newRange;
        }
    }

    private static (float, float) DrawAheadCone(ConeRange cone)
    {
        return InteractiveRanges.ConeRange(
            cone.transform, 
            cone.SemiConeDegrees,
            cone.LocalForward, 
            cone.Normal, 
            cone.Color, 
            cone.Range, 
            !cone.FixedRange);
    }
}
}

