using Pathfinding;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspectors
{
[CustomEditor(typeof(MapGraph))]
public class MapGraphEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Add some space before the button
        EditorGUILayout.Space(10);

        // Get the MapGraph component reference
        MapGraph mapGraph = (MapGraph)target;

        // Create and handle the reset button
        if (GUILayout.Button("Generate Graph", GUILayout.Height(30)))
        {
            mapGraph.GenerateGraph();
            EditorUtility.SetDirty(target);
        }
    }
}
}