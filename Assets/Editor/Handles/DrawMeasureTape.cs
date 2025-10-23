using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
[CustomEditor(typeof(MeasureTape))]
public class DrawMeasureTape : UnityEditor.Editor
{
    
    private void OnSceneGUI()
    {
        var tape = (MeasureTape)target;

        EditorGUI.BeginChangeCheck();
        
        // Place handles to locate ends.
        Vector3 positionAHandle = Handles.PositionHandle(
            tape.positionA, 
            Quaternion.identity);
        Vector3 positionBHandle = Handles.PositionHandle(
            tape.positionB, 
            Quaternion.identity);
    
        // Draw a line between handles.
        Handles.color = tape.color;
        Handles.DrawLine(positionAHandle, positionBHandle, tape.thickness);
        
        // Draw lines to highlight handles.
        Handles.DrawWireDisc(
            positionAHandle, 
            Vector3.forward, 
            0.1f, 
            tape.thickness);
        Handles.DrawWireDisc(
            positionBHandle, 
            Vector3.forward, 
            0.1f, 
            tape.thickness);
        
        float distance = Vector3.Distance(positionAHandle, positionBHandle);
        Vector3 direction = (positionBHandle - positionAHandle).normalized;
        Vector3 normalVector = new Vector3(-direction.y, direction.x, direction.z);
        
        // Draw a label to show the distance between handles.
        Vector3 middlePosition = (positionAHandle + positionBHandle) / 2;
        GUIStyle style = new GUIStyle();
        style.normal.textColor = tape.color;
        style.fontSize = tape.textSize;
        Vector3 labelPosition = middlePosition + normalVector * tape.textDistance;
        Handles.Label(labelPosition, distance.ToString("F2"), style);
        
        // Draw tape ends.
        Handles.DrawLine(
            positionAHandle - (normalVector/2) + (normalVector/2) * tape.endAlignment, 
            positionAHandle + (normalVector/2) + (normalVector/2) * tape.endAlignment);
        Handles.DrawLine(
            positionBHandle - (normalVector/2) + (normalVector/2) * tape.endAlignment,
            positionBHandle + (normalVector/2) + (normalVector/2) * tape.endAlignment);
    
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(tape, $"Changed tape ends.");
            tape.positionA = positionAHandle;
            tape.positionB = positionBHandle;
        }
    }
}
}