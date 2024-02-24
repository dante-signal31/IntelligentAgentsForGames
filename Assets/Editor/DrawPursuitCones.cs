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
        // Rotate in local space.
        // I don't know why rotated vector is only 0.5 length so I have to multiply by two to get a 1 unit length vector.
        Vector3 rotatedVector = Quaternion.AngleAxis(aheadSemiConeDegrees, Vector3.forward) * Vector3.up * 2;
        // Handle is placed in world space so rotated vector must be taken back to world space.
        Vector3 aheadSemiConePosition = Handles.PositionHandle(pursuer.transform.TransformPoint(rotatedVector), 
            Quaternion.identity);
        // Again, we rotate in local space, so aheadSemiConePosition is taken back to local space.
        pursuer.AheadSemiConeRadians = Mathf.Deg2Rad * Vector2.Angle(Vector3.up, 
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
        Vector3 rotatedVector = Quaternion.AngleAxis(comingToUsConeDegrees, Vector3.forward) * Vector3.up * 2;
        Vector3 comingToUsSemiConePosition = Handles.PositionHandle(pursuer.transform.TransformPoint(rotatedVector),
            Quaternion.identity);
        pursuer.ComingToUsSemiConeRadians = Mathf.Deg2Rad * Vector2.Angle(Vector3.up, 
            pursuer.transform.InverseTransformPoint(comingToUsSemiConePosition));
        Handles.DrawSolidArc(pursuerPosition, Vector3.forward, 
            -forward, (180 - comingToUsConeDegrees), 1);
        Handles.DrawSolidArc(pursuerPosition, Vector3.forward, 
            -forward, -1 * (180 - comingToUsConeDegrees), 1);
    }
}
