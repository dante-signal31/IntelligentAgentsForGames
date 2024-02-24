using System.ComponentModel;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EvadeSteeringBehavior))]
public class DrawPanicDistance : Editor
{
    private void OnSceneGUI()
    {
        EvadeSteeringBehavior evade = (EvadeSteeringBehavior)target;
        
        EditorGUI.BeginChangeCheck();
        Handles.color =Color.red;
        evade.PanicDistance =
            Handles.RadiusHandle(Quaternion.identity, evade.transform.position, evade.PanicDistance);
        // GUIStyle style = new GUIStyle();
        // style.normal.textColor = Color.red;
        // Handles.Label(evade.transform.position + (Vector3.up * evade.PanicDistance) + Vector3.up*0.05f, 
        //     $"Panic radius: {evade.PanicDistance}", 
        //     style);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(evade, "Changed panic distance.");
        }
    }
}