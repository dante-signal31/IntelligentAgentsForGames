using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Inspectors
{
    [CustomEditor(typeof(BoxRangeManager))]
    public class BoxRangeManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // Add some space before the button
            EditorGUILayout.Space(10);

            // Get the BoxRangeManager component reference
            BoxRangeManager boxManager = (BoxRangeManager)target;

            // Create and handle the reset button
            if (GUILayout.Button("Reset Box Manager", GUILayout.Height(30)))
            {
                boxManager.ResetBoxManager();
                EditorUtility.SetDirty(target);
            }
        }
    }
}