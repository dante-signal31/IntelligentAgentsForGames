using UnityEditor;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Editor.Tools;

namespace Editor
{
    [CustomEditor(typeof(PursuitSteeringBehavior))]
    public class DrawPursuitCones : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var pursuer = (PursuitSteeringBehavior)target;
        
            EditorGUI.BeginChangeCheck();
            (float newAheadSemiConeDegrees, float _) = DrawAheadCone(pursuer);
            (float newComingToUsSemiConeDegrees, float _) = DrawComingToUsCone(pursuer);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(pursuer, "Changed ahead semicone degrees.");
                pursuer.AheadSemiConeDegrees = newAheadSemiConeDegrees;
                pursuer.ComingToUsSemiConeDegrees = newComingToUsSemiConeDegrees;
            }
        }

        private static (float, float) DrawAheadCone(PursuitSteeringBehavior pursuer)
        {
            return InteractiveRanges.RangeCone(pursuer.transform, pursuer.AheadSemiConeDegrees,
                Vector3.up, Vector3.forward, 
                new Color(0f, 0f, 1f, 0.1f), 1.0f, false);
        }
    
        private static (float, float) DrawComingToUsCone(PursuitSteeringBehavior pursuer)
        {
            return InteractiveRanges.RangeCone(pursuer.transform, pursuer.ComingToUsSemiConeDegrees,
                -Vector3.up, Vector3.forward, 
                new Color(1, 0.92f, 0.016f, 0.3f), 1.0f, false);
        }
    }
}
