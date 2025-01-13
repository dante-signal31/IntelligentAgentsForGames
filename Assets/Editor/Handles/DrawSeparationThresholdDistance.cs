using UnityEditor;
using Editor.Tools;

namespace Editor
{
    [CustomEditor(typeof(SeparationSteeringBehavior))]
    public class DrawSeparationThresholdDistance : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var separation = (SeparationSteeringBehavior) target;
        
            EditorGUI.BeginChangeCheck();
            Handles.color = separation.MarkerColor;
            float newSeparationThreshold =
                InteractiveRanges.CircularRange(separation.transform.position, separation.SeparationThreshold);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(separation, "Changed separation threshold.");
                separation.SeparationThreshold = newSeparationThreshold;
            }
        }
    }
}