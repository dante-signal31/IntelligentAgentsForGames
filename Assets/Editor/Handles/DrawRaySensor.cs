using Sensors;
using UnityEditor;
using UnityEngine;

namespace Editor
{
[CustomEditor(typeof(RaySensor))]
public class DrawRaySensor : UnityEditor.Editor
{
    private const float HandleSize = 0.6f;
    
    private void OnSceneGUI()
    {
        var sensor = (RaySensor)target;

        EditorGUI.BeginChangeCheck();
        
        // Draw start point handle.
        Vector3 newStartPosition = Handles.PositionHandle(
            sensor.GlobalStartPosition,
            Quaternion.identity);
        
        // Draw end point handle.
        Vector3 newEndPosition = Handles.PositionHandle(
            sensor.GlobalEndPosition,
            Quaternion.identity);
        
        // Arrow head pointing from start to end.
        Vector3 dir = newEndPosition - newStartPosition;
        if (dir.sqrMagnitude > 0.0001f)
        {
            dir.Normalize();
            Handles.color = sensor.gizmoColor;
            Handles.ArrowHandleCap(
                0,
                newEndPosition - dir * (HandleSize * 1.1f),
                Quaternion.LookRotation(dir, Vector3.forward),
                HandleSize,
                EventType.Repaint);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(sensor, $"Changed sensor ends data.");
            sensor.GlobalStartPosition = newStartPosition;
            sensor.GlobalEndPosition = newEndPosition;
        }
    }
}
}