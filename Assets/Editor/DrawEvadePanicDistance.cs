using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(EvadeSteeringBehavior))]
    public class DrawEvadePanicDistance : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var evade = (EvadeSteeringBehavior)target;
        
            EditorGUI.BeginChangeCheck();
            Handles.color =Color.yellow;
            evade.PanicDistance =
                Handles.RadiusHandle(Quaternion.identity, evade.transform.position, evade.PanicDistance);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(evade, "Changed panic distance.");
            }
        }
    }
}