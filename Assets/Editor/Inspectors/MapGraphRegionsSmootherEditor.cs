using Pathfinding;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Inspectors
{
[CustomEditor(typeof(MapGraphRegionsSmoother))]
public class MapGraphRegionsSmootherEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Add some space before the button
        EditorGUILayout.Space(10);

        // Get the component reference
        MapGraphRegionsSmoother mapGraphRegionsSmoother = (MapGraphRegionsSmoother)target;

        // Create and handle the reset button
        if (GUILayout.Button("Bake Smoothed Regions", GUILayout.Height(30)))
        {
            mapGraphRegionsSmoother.SmoothRegions();
            EditorUtility.SetDirty(mapGraphRegionsSmoother.mapGraphRegions);
        }
    }
}
}