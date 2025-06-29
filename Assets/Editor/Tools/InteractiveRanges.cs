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
    /// <param name="parentTransform">Transform of the GameObject this cone is
    /// attached to.</param>
    /// <param name="semiConeDegrees">Half angular width in degrees for this
    /// cone.</param>
    /// <param name="localConeForward">Forward direction for this cone in local
    /// space.</param>
    /// <param name="coneNormal">Normal vector for this cone plane.</param>
    /// <param name="coneColor">Color for this cone.</param>
    /// <param name="range">Length of this cone.</param>
    /// <param name="takeRange">Return the updated range after moving handle</param>
    /// <returns>Tuple of (new semiConeDegrees, new range)</returns>
    public static (float, float) ConeRange(
        Transform parentTransform, 
        float semiConeDegrees, 
        Vector3 localConeForward, 
        Vector3 coneNormal, 
        Color coneColor,
        float range=1.0f, 
        bool takeNewRange=false)
    {
        Handles.color = coneColor;
        Vector3 position = parentTransform.position;
        Vector3 globalForward = parentTransform.TransformDirection(localConeForward);
        // Rotate in local space.
        Vector3 rotatedVector = (Quaternion.AngleAxis(semiConeDegrees, coneNormal) * 
                                localConeForward).normalized;
        // Handle is placed in world space, so the rotated vector must be taken back to
        // world space.
        Vector3 newHandlePosition = Handles.PositionHandle(
            parentTransform.TransformPoint(rotatedVector * range), 
            Quaternion.identity);
        range = takeNewRange ? Vector3.Distance(position, newHandlePosition) : range;
        Handles.DrawSolidArc(position, coneNormal, 
            globalForward, semiConeDegrees, range);
        Handles.DrawSolidArc(position, coneNormal,
            globalForward, -1 * semiConeDegrees, range);
        // Again, we rotate in local space, so aheadSemiConePosition is taken back
        // to local space.
        float newSemiConeDegrees =  Vector2.Angle(localConeForward, 
            parentTransform.InverseTransformPoint(newHandlePosition));
        return (newSemiConeDegrees, range);
    }

    /// <summary>
    /// Show a range sector with handle to set its angular width and its range.
    ///
    /// Whereas a cone laterals go from center to its range, a range laterals go
    /// from a minimum range to its range. 
    /// </summary>
    /// <param name="parentTransform">Transform of the GameObject this cone is
    /// attached to.</param>
    /// <param name="semiConeDegrees">Half angular width in degrees for this
    /// cone.</param>
    /// <param name="globalConeForward">Forward direction for this cone.</param>
    /// <param name="coneNormal">Normal vector for this cone plane.</param>
    /// <param name="coneColor">Color for this cone.</param>
    /// <param name="range">Length of this cone.</param>
    /// <param name="minimumRange">Minimum length of this cone.</param>
    /// <param name="takeNewRange">Return the updated range after moving
    /// handle</param>
    /// <returns>Tuple of (new semiConeDegrees, new range,
    /// new minimumRange)</returns>
    public static (float, float, float) SectorRange(
        Transform parentTransform,
        float semiConeDegrees,
        Vector3 localConeForward, 
        Vector3 coneNormal, 
        Color coneColor,
        float range = 0.5f, 
        float minimumRange = 0.2f, 
        bool takeNewRange = false)
    {
        Handles.color = coneColor;
        Vector3 position = parentTransform.position;
        Vector3 globalConeForward = parentTransform.TransformDirection(localConeForward);
        // Rotate in local space.
        Vector3 rotatedVector = (Quaternion.AngleAxis(semiConeDegrees, coneNormal) * 
                                localConeForward).normalized;
        
        // Place handles.
        // Handle is placed in world space, so the rotated vector must be taken back to
        // world space.
        Vector3 handlePosition = parentTransform.TransformPoint(
            rotatedVector * range);
        Vector3 newHandlePosition = Handles.PositionHandle(
            handlePosition, 
            Quaternion.identity);
        Vector3 minimumRangeHandlePosition = parentTransform.TransformPoint(
            rotatedVector * minimumRange);
        Vector3 newMinimumRangeHandlePosition = Handles.PositionHandle(
            minimumRangeHandlePosition,
            Quaternion.identity);
        // Process handles output.
        range = takeNewRange ? Vector3.Distance(position, newHandlePosition) : range;
        minimumRange = takeNewRange ? 
            Vector3.Distance(position, newMinimumRangeHandlePosition) : 
            minimumRange;
        
        // Draw external arc.
        Handles.DrawWireArc(
            position, 
            coneNormal, 
            globalConeForward, 
            semiConeDegrees, 
            range);
        Handles.DrawWireArc(
            position, 
            coneNormal,
            globalConeForward, 
            -1 * semiConeDegrees, 
            range);
        
        // Draw inner arc.
        Handles.DrawWireArc(
            position, 
            coneNormal, 
            globalConeForward, 
            semiConeDegrees, 
            minimumRange);
        Handles.DrawWireArc(
            position, 
            coneNormal,
            globalConeForward, 
            -1 * semiConeDegrees, 
            minimumRange);
        
        // Draw lateral lines.
        Vector3 side = globalConeForward * range;
        Vector3 minimumSide = globalConeForward * minimumRange;
        Handles.DrawLine(
            Quaternion.AngleAxis(-semiConeDegrees, coneNormal) * minimumSide + position,
            Quaternion.AngleAxis(-semiConeDegrees, coneNormal) * side + position);
        Handles.DrawLine(
            Quaternion.AngleAxis(semiConeDegrees, coneNormal) * minimumSide + position,
            Quaternion.AngleAxis(semiConeDegrees, coneNormal) * side + position);
        
        // Again, we rotate in local space, so aheadSemiConePosition is taken back to
        // local space.
        float newSemiConeDegrees =  Vector2.Angle(localConeForward, 
            parentTransform.InverseTransformPoint(newHandlePosition));
        return (newSemiConeDegrees, range, minimumRange);
    }
}
}