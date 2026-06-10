using System.Collections.Generic;
using Pathfinding;
using Sensors;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspectors
{
[CustomEditor(typeof(FEMSenseManager))]
public class DrawFEMSenseManager : UnityEditor.Editor
{
    private void OnSceneGUI()
    {
        var femSenseManager = (FEMSenseManager)target;
    
        if (femSenseManager == null) return;
        
        if (!femSenseManager.showGizmos) return;
        
        Vector2 cellSize = femSenseManager.MapGraph.CellSize;
        
        // Get the maximum intensity.
        float maxIntensity = 0f;
        foreach (KeyValuePair<uint, float> nodeIntensity in 
                 femSenseManager.NodeIntensities)
        {
            if (nodeIntensity.Value > maxIntensity)
            {
                maxIntensity = nodeIntensity.Value;
            }
        }
        
        // Now give color to every cell.
        foreach (KeyValuePair<uint, float> nodeIntensity in 
                 femSenseManager.NodeIntensities)
        {
            IPositionNode node = femSenseManager.MapGraph.GetNodeById(nodeIntensity.Key);
            Vector2 position = node.Position;
            float intensity = nodeIntensity.Value;
            
            // Don't draw any cell whose intensity is under minimum.
            if (intensity < femSenseManager.minimumDisseminationIntensity) continue;
            
            // Debug color saturation is proportional to normalized intensity.
            float normalizedIntensity = intensity / maxIntensity;
            Color.RGBToHSV(
                femSenseManager.dissipationColor, 
                out float hue, 
                out _, 
                out float _);
            float saturation = normalizedIntensity;
            float value = 1.0f;
            Color intensityColor = Color.HSVToRGB(hue, saturation, value);
            intensityColor.a = femSenseManager.gizmoAlpha;
            
            // Draw a rectangle with the intensity color covering the node cell.
            Vector2 halfSize = cellSize / 2;
            Rect rect = new Rect(position - halfSize, cellSize);
            Handles.DrawSolidRectangleWithOutline(
                rect, intensityColor, 
                femSenseManager.MapGraph.GridColor);
        }
    }
}
}