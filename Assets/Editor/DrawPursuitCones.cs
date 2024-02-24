using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.VirtualTexturing;

[CustomEditor(typeof(PursuitSteeringBehavior))]
public class DrawPursuitCones : Editor
{
    private void OnSceneGUI()
    {
        PursuitSteeringBehavior pursuer = (PursuitSteeringBehavior)target;
        
        EditorGUI.BeginChangeCheck();
        drawAheadCone(pursuer);
        drawComingToUsCone(pursuer);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(pursuer, "Changed ahead semicone degrees.");
        }
    }

    private static void drawAheadCone(PursuitSteeringBehavior pursuer)
    {
        Handles.color =Color.blue;
        Vector3 pursuerPosition = pursuer.transform.position;
        float aheadSemiConeDegrees = Mathf.Rad2Deg * pursuer.AheadSemiConeRadians;
        Vector3 forward = pursuer.transform.up;
        Vector3 rotatedVector = pursuerPosition + Quaternion.Euler(0, 0, aheadSemiConeDegrees) * forward;
        Vector3 aheadSemiConePosition = Handles.PositionHandle(rotatedVector,
            Quaternion.identity);
        pursuer.AheadSemiConeRadians = Mathf.Deg2Rad * Vector2.Angle(forward, 
            pursuer.transform.InverseTransformPoint(aheadSemiConePosition));
        Handles.DrawSolidArc(pursuerPosition, Vector3.forward, 
            forward, aheadSemiConeDegrees, 1);
        Handles.DrawSolidArc(pursuerPosition, Vector3.forward, 
            forward, -1 * aheadSemiConeDegrees, 1);
    }
    
    private static void drawComingToUsCone(PursuitSteeringBehavior pursuer)
    {
        Handles.color =Color.yellow;
        Vector3 pursuerPosition = pursuer.transform.position;
        float comingToUsConeDegrees = Mathf.Rad2Deg * pursuer.ComingToUsSemiConeRadians;
        Vector3 forward = pursuer.transform.up;
        Vector3 rotatedVector = pursuerPosition + Quaternion.Euler(0, 0, comingToUsConeDegrees) * forward;
        Vector3 comingToUsSemiConePosition = Handles.PositionHandle(rotatedVector,
            Quaternion.identity);
        pursuer.ComingToUsSemiConeRadians = Mathf.Deg2Rad * Vector2.Angle(forward, 
            pursuer.transform.InverseTransformPoint(comingToUsSemiConePosition));
        Handles.DrawSolidArc(pursuerPosition, Vector3.forward, 
            -forward, (180 - comingToUsConeDegrees), 1);
        Handles.DrawSolidArc(pursuerPosition, Vector3.forward, 
            -forward, -1 * (180 - comingToUsConeDegrees), 1);
    }
}
