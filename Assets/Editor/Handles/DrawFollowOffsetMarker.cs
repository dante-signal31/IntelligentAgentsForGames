using UnityEditor;
using UnityEngine;
using Editor.Tools;
using SteeringBehaviors;

namespace Editor
{
    [CustomEditor(typeof(OffsetFollowSteeringBehavior))]
    public class DrawFollowOffsetMarker : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var follower = (OffsetFollowSteeringBehavior)target;
            
            if (follower.Target == null) return;
        
            EditorGUI.BeginChangeCheck();
            
            // Draw handle to place offset marker visually.
            Vector2 newMarkerPosition = Handles.PositionHandle(
                follower.Target.transform.TransformPoint(follower.offsetFromTarget),
                Quaternion.identity);
            
            // Draw lines to see visual aids in the scene tab.
            Handles.color = Color.green;
            Handles.DrawLine(follower.transform.position, newMarkerPosition);
            Handles.DrawWireDisc(newMarkerPosition, Vector3.forward, 0.1f);
            Handles.color = Color.red;
            Handles.DrawLine(newMarkerPosition, follower.Target.transform.position);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(follower, "Changed follow offset.");
                follower.offsetFromTarget = 
                    follower.Target.transform.InverseTransformPoint(newMarkerPosition);
            }
        }
    }
}