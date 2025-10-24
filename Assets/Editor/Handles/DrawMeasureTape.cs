using Tools;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(MeasureTape))]
// Leave this script under the default namespace. I don't know why, but Gizmos are not
// drawn if this script is under Editor namespace.
public class DrawMeasureTape : UnityEditor.Editor
{
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    static void DrawTapeGizmos(MeasureTape tape, GizmoType gizmoType)
    {
        // Draw a line between handles.
        Handles.color = tape.color;
        Handles.DrawLine(tape.PositionA, tape.PositionB, tape.thickness);
        
        // Draw lines to highlight handles.
        Handles.DrawWireDisc(
            tape.PositionA,  
            Vector3.forward, 
            0.1f, 
            tape.thickness);
        Handles.DrawWireDisc(
            tape.PositionB, 
            Vector3.forward, 
            0.1f, 
            tape.thickness);
        
        float distance = Vector3.Distance(tape.PositionA, tape.PositionB);
        Vector3 direction = (tape.PositionB - tape.PositionA).normalized;
        Vector3 normalVector = new Vector3(-direction.y, direction.x, direction.z);
        
        // Draw a label to show the distance between handles.
        Vector3 middlePosition = (tape.PositionA + tape.PositionB) / 2;
        GUIStyle style = new GUIStyle();
        style.normal.textColor = tape.color;
        style.fontSize = tape.textSize;
        Vector3 labelPosition = middlePosition + normalVector * tape.textDistance;
        Handles.Label(labelPosition, distance.ToString("F2"), style);
        
        // Draw tape ends.
        // TODO: Include endWidth.
        Vector3 semiEnd = normalVector * tape.endWidth / 2;
        Handles.DrawLine(
            tape.PositionA - (semiEnd) + (semiEnd) * tape.endAlignment, 
            tape.PositionA + (semiEnd) + (semiEnd) * tape.endAlignment);
        Handles.DrawLine(
            tape.PositionB - (semiEnd) + (semiEnd) * tape.endAlignment,
            tape.PositionB + (semiEnd) + (semiEnd) * tape.endAlignment);
    }
    
    private void OnSceneGUI()
    {
        var tape = (MeasureTape)target;
        
        EditorGUI.BeginChangeCheck();
        
        // Place handles to locate ends.
        Vector3 positionAHandle = Handles.PositionHandle(
            tape.PositionA, 
            Quaternion.identity);
        Vector3 positionBHandle = Handles.PositionHandle(
            tape.PositionB, 
            Quaternion.identity);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(tape, $"Changed tape ends.");
            tape.PositionA = positionAHandle;
            tape.PositionB = positionBHandle;
        }
    }
}
