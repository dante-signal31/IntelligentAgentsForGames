using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(FleeSteeringBehavior))]
    public class DrawFleePanicDistance : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var evade = (FleeSteeringBehavior)target;
        
            EditorGUI.BeginChangeCheck();
            Handles.color =Color.red;
            float newPanicDistance =
                Handles.RadiusHandle(Quaternion.identity, evade.transform.position, evade.PanicDistance);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(evade, "Changed panic distance.");
                evade.PanicDistance = newPanicDistance;
            }
        }
    }
}