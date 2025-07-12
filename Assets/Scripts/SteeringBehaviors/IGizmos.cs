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
    /// Colors for this agents's gizmos.
    /// </summary>
    public Color GizmosColor { get; set; }
}
}