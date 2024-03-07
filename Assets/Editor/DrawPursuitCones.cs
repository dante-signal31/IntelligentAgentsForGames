﻿using System.Numerics;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
            // float newAheadSemiConeRadians = DrawAheadCone(pursuer);
            (float newComingToUsSemiConeDegrees, float _) = DrawComingToUsCone(pursuer);
            // float newComingToUsSemiConeRadians = DrawComingToUsCone(pursuer);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(pursuer, "Changed ahead semicone degrees.");
                pursuer.AheadSemiConeDegrees = newAheadSemiConeDegrees;
                pursuer.ComingToUsSemiConeDegrees = newComingToUsSemiConeDegrees;
            }
        }
        /// <summary>
        /// Show a range cone with handle to set its angular width and its range.
        /// </summary>
        /// <param name="parentTransform">Transform of the GameObject this cone is attached to.</param>
        /// <param name="semiConeDegrees">Half angular width in degrees for this cone.</param>
        /// <param name="localConeForward">Forward direction for this cone in local space.</param>
        /// <param name="coneNormal">Normal vector for this cone plane.</param>
        /// <param name="coneColor">Color for this cone.</param>
        /// <param name="range">Length of this cone.</param>
        /// <param name="takeRange">Return the updated range after moving handle</param>
        /// <returns>Tuple of (new semiConeDegrees, new range)</returns>
        private static (float, float) RangeCone(Transform parentTransform, float semiConeDegrees, 
            Vector3 localConeForward, Vector3 coneNormal, Color coneColor,
            float range=1.0f, bool takeNewRange=false)
        {
            Handles.color = coneColor;
            Vector3 position = parentTransform.position;
            Vector3 globalForward = parentTransform.TransformDirection(localConeForward);
            // Rotate in local space.
            // I don't know why rotated vector is only 0.5 length so I have to multiply by two to get a 1 unit length vector.
            Vector3 rotatedVector = Quaternion.AngleAxis(semiConeDegrees, coneNormal) * localConeForward * 2;
            // Handle is placed in world space so rotated vector must be taken back to world space.
            Vector3 semiConePosition = Handles.PositionHandle(parentTransform.TransformPoint(rotatedVector * range) , 
                Quaternion.identity);
            range = takeNewRange ? Vector3.Distance(position, semiConePosition) : range;
            Handles.DrawSolidArc(position, coneNormal, 
                globalForward, semiConeDegrees, range);
            Handles.DrawSolidArc(position, coneNormal,
                globalForward, -1 * semiConeDegrees, range);
            // Again, we rotate in local space, so aheadSemiConePosition is taken back to local space.
            float newSemiConeDegrees =  Vector2.Angle(localConeForward, 
                parentTransform.InverseTransformPoint(semiConePosition));
            return (newSemiConeDegrees, range);
        }

        private static (float, float) DrawAheadCone(PursuitSteeringBehavior pursuer)
        // private static float DrawAheadCone(PursuitSteeringBehavior pursuer)
        {
            // Handles.color = new Color(0f, 0f, 1f, 0.1f);
            // Vector3 pursuerPosition = pursuer.transform.position;
            // float aheadSemiConeDegrees = Mathf.Rad2Deg * pursuer.AheadSemiConeRadians;
            // Vector3 forward = pursuer.transform.up;
            // // Rotate in local space.
            // // I don't know why rotated vector is only 0.5 length so I have to multiply by two to get a 1 unit length vector.
            // Vector3 rotatedVector = Quaternion.AngleAxis(aheadSemiConeDegrees, Vector3.forward) * Vector3.up * 2;
            // // Handle is placed in world space so rotated vector must be taken back to world space.
            // Vector3 aheadSemiConePosition = Handles.PositionHandle(pursuer.transform.TransformPoint(rotatedVector), 
            //     Quaternion.identity);
            // Handles.DrawSolidArc(pursuerPosition, Vector3.forward, 
            //     forward, aheadSemiConeDegrees, 1);
            // Handles.DrawSolidArc(pursuerPosition, Vector3.forward,
            //     forward, -1 * aheadSemiConeDegrees, 1);
            // // Again, we rotate in local space, so aheadSemiConePosition is taken back to local space.
            // float newAheadSemiConeRadians =  Mathf.Deg2Rad * Vector2.Angle(Vector3.up, 
            //     pursuer.transform.InverseTransformPoint(aheadSemiConePosition));
            // return newAheadSemiConeRadians;
            return RangeCone(pursuer.transform, pursuer.AheadSemiConeDegrees,
                Vector3.up, Vector3.forward, 
                new Color(0f, 0f, 1f, 0.1f), 1.0f, false);
        }
    
        private static (float, float) DrawComingToUsCone(PursuitSteeringBehavior pursuer)
        // private static float DrawComingToUsCone(PursuitSteeringBehavior pursuer)
        {
            // Handles.color = new Color(1, 0.92f, 0.016f, 0.3f);
            // Vector3 pursuerPosition = pursuer.transform.position;
            // float comingToUsConeDegrees = Mathf.Rad2Deg * pursuer.ComingToUsSemiConeRadians;
            // Vector3 forward = pursuer.transform.up;
            // Vector3 rotatedVector = Quaternion.AngleAxis(comingToUsConeDegrees, Vector3.forward) * Vector3.up * 2;
            // Vector3 comingToUsSemiConePosition = Handles.PositionHandle(pursuer.transform.TransformPoint(rotatedVector),
            //     Quaternion.identity);
            // Handles.DrawSolidArc(pursuerPosition, Vector3.forward, 
            //     -forward, (180 - comingToUsConeDegrees), 1);
            // Handles.DrawSolidArc(pursuerPosition, Vector3.forward, 
            //     -forward, -1 * (180 - comingToUsConeDegrees), 1);
            // float newComingToUsSemiConeRadians = Mathf.Deg2Rad * Vector2.Angle(Vector3.up, 
            //     pursuer.transform.InverseTransformPoint(comingToUsSemiConePosition));
            // return newComingToUsSemiConeRadians;
            return RangeCone(pursuer.transform, pursuer.ComingToUsSemiConeDegrees,
                -Vector3.up, Vector3.forward, 
                new Color(1, 0.92f, 0.016f, 0.3f), 1.0f, false);
        }
    }
}