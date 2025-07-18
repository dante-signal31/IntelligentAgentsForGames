using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// Interface for components that can show gizmos.
/// </summary>
public interface IGizmos
{
    /// <summary>
    /// Show gizmos.
    /// </summary>
    public bool ShowGizmos { get; set; }
    
    /// <summary>
    /// Color for this component's gizmos.
    /// </summary>
    public Color GizmosColor { get; set; }
}
}