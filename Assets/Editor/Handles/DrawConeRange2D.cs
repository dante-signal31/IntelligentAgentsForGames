using Editor.Tools;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Editor
{
[CustomEditor(typeof(ConeRange2D))]
public class DrawConeRange2D : UnityEditor.Editor
{
    private void OnSceneGUI()
    {
        var cone = (ConeRange2D)target;

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

    private static (float, float) DrawAheadCone(ConeRange2D cone)
    {
        return InteractiveRanges.ConeRange(
            cone.transform, 
            cone.SemiConeDegrees,
            Vector3.up, 
            Vector3.forward, 
            cone.ConeColor, 
            cone.Range, 
            !cone.FixedRange);
    }
}
}

