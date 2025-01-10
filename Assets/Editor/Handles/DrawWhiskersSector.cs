using UnityEditor;
using UnityEngine.UIElements;
using Editor.Tools;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(Whiskers))]
    public class DrawWhiskersSector : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var whiskers = (Whiskers)target;
        
            EditorGUI.BeginChangeCheck();
            (float newAheadSemiConeDegrees,
                float newRange,
                float newMinimumRange) = DrawSector(whiskers);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(whiskers, "Changed ahead semicone degrees and ranges.");
                whiskers.SemiConeDegrees = newAheadSemiConeDegrees;
                whiskers.Range = newRange;
                whiskers.MinimumRange = newMinimumRange;
            }
        }

        public static (float, float, float) DrawSector(Whiskers whiskers)
        {
            return InteractiveRanges.SectorRange(
                whiskers.transform,
                whiskers.SemiConeDegrees,
                Vector3.up,
                Vector3.forward,
                new Color(0f, 1f, 0f, 1f),
                whiskers.Range,
                whiskers.MinimumRange,
                true);
        }
    }
}