using SteeringBehaviors;
using UnityEditor;
using UnityEngine;

namespace Editor
{
[CustomEditor(typeof(VoAgentAvoiderBehavior))]
public class DrawVoAgentAvoider : UnityEditor.Editor
{
    private void OnSceneGUI()
    {
        var voAgent = (VoAgentAvoiderBehavior)target;

        if (!voAgent.showGizmos) return;
        
        // Draw candidate velocities cloud.
        // 
        // First, no collision candidates.
        foreach (Vector2 velocity in voAgent.noCollisionCandidateVelocities)
        {
            Handles.color = voAgent.noCollisionVelocitiesColor;
            Handles.DrawSolidDisc(
                voAgent.transform.position + (Vector3) velocity, 
                Vector3.forward, 
                voAgent.gizmoRadius);
        }
        // Next, collision candidates.
        foreach (Vector2 velocity in voAgent.collisionCandidateVelocities)
        {
            Handles.color = voAgent.collisionVelocitiesColor;
            Handles.DrawSolidDisc(
                voAgent.transform.position + (Vector3) velocity, 
                Vector3.forward, 
                voAgent.gizmoRadius);
        }
    }
}
}