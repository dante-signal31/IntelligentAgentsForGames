using Editor.Tools;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Editor
{
[CustomEditor(typeof(SectorRange))]
public class DrawSectorRange : UnityEditor.Editor
{
    private void OnSceneGUI()
    {
        var sector = (SectorRange)target;

        EditorGUI.BeginChangeCheck();
        (float newAheadSemiConeDegrees, 
            float newRange, 
            float minimumRange) = DrawAheadSector(sector);
    
        // Draw lines to highlight handles.
        Vector2 handlePosition = Quaternion.AngleAxis(
            newAheadSemiConeDegrees,
            Vector3.forward) * (Vector3.up * sector.Range);
        Vector2 globalHandlePosition = sector.transform.TransformPoint(
            handlePosition);
        Vector2 minimumRangeHandlePosition = Quaternion.AngleAxis(
            newAheadSemiConeDegrees,
            Vector3.forward) * (Vector3.up * sector.MinimumRange);
        Handles.color = Color.green;
        Handles.DrawLine(sector.transform.position, globalHandlePosition);
        Handles.DrawWireDisc(
            globalHandlePosition, 
            Vector3.forward, 
            0.1f);
        Handles.DrawWireDisc(
            minimumRangeHandlePosition, 
            Vector3.forward, 
            0.1f);
    
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(sector, $"Changed sector data.");
            sector.SemiConeDegrees = newAheadSemiConeDegrees;
            sector.Range = newRange;
            sector.MinimumRange = minimumRange;
        }
    }

    private static (float, float, float) DrawAheadSector(SectorRange sector)
    {
        return InteractiveRanges.SectorRange(
            sector.transform, 
            sector.SemiConeDegrees,
            sector.LocalForward, 
            sector.Normal, 
            sector.Color, 
            sector.Range,
            sector.MinimumRange,
            !sector.FixedRange);
    }
}
}