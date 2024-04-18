using UnityEngine;

public static class GizmoTools
{
    /// <summary>
    /// Draw an arrow in the scene
    /// </summary>
    /// <param name="position">Start point for tha arrow.</param>
    /// <param name="direction">Direction and length vector for the arrow.</param>
    /// <param name="arrowHeadLength">Size of the arrow head.</param>
    /// <param name="arrowHeadAngle">Angle of the arrow head.</param>
    /// <param name="arrowColor">Color for the arrow.</param>
    public static void DrawArrow(Vector3 position, Vector3 direction, 
        float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Vector2 right = Quaternion.AngleAxis(arrowHeadAngle+180, Vector3.forward) * direction.normalized;
        Vector2 left = Quaternion.AngleAxis(-arrowHeadAngle+180, Vector3.forward) * direction.normalized;
        Gizmos.DrawRay(position, direction);
        Gizmos.DrawRay(position+direction, right * arrowHeadLength);
        Gizmos.DrawRay(position+direction, left * arrowHeadLength);
    }
}
