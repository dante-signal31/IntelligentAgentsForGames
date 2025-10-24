
using UnityEngine;

namespace Tools
{
/// <summary>
/// Tool to measure distances in the editor.
/// </summary>
public class MeasureTape : MonoBehaviour
{
    public Vector3 positionA;
    public Vector3 positionB; 
    
    [Header("CONFIGURATION:")]
    [Tooltip("Color for this tool.")]
    public Color color = Color.orange;
    [Tooltip("Line thickness.")]
    public float thickness = 0.1f;
    [Tooltip("Width for the perpendicular ends of the line.")]
    public float endWidth = 0.5f;
    [Range(-1.0f, 1.0f)]
    [Tooltip("End bias.")]
    public float endAlignment = 0.0f;
    [Tooltip("Text size for the distance.")]
    public int textSize = 12;
    [Tooltip("Distance between the text and the line.")]
    public float textDistance = 0.5f;
}
}

