using UnityEditor;
using UnityEngine;

namespace Editor.Tools
{
    public static class InteractiveRanges
    {
        /// <summary>
        /// Show a circular range handle.
        /// </summary>
        /// <param name="position">Center for the circular handle.</param>
        /// <param name="range">Radius for the circular range handle.</param>
        /// <returns>New range got from handle.</returns>
        public static float CircularRange(Vector3 position, float range)
        {
            return Handles.RadiusHandle(Quaternion.identity, position, range);
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
        public static (float, float) RangeCone(Transform parentTransform, float semiConeDegrees, 
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
            Vector3 newHandlePosition = Handles.PositionHandle(parentTransform.TransformPoint(rotatedVector * range) , 
                Quaternion.identity);
            range = takeNewRange ? Vector3.Distance(position, newHandlePosition) : range;
            Handles.DrawSolidArc(position, coneNormal, 
                globalForward, semiConeDegrees, range);
            Handles.DrawSolidArc(position, coneNormal,
                globalForward, -1 * semiConeDegrees, range);
            // Again, we rotate in local space, so aheadSemiConePosition is taken back to local space.
            float newSemiConeDegrees =  Vector2.Angle(localConeForward, 
                parentTransform.InverseTransformPoint(newHandlePosition));
            return (newSemiConeDegrees, range);
        }
    }
}