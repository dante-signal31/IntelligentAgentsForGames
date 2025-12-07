using System;
using UnityEngine;

namespace Tools
{
public class CleanAreaChecker : IDisposable
{
    /// <summary>
    /// Gets the radius of the clean area checker.
    /// </summary>
    /// <returns>Radius of the clean area checker.</returns>
    public float Radius { get; set; }

    /// <summary>
    /// Gets the detection layers of the clean area checker.
    /// </summary>
    /// <returns>Detection layers of the clean area checker.</returns>
    public LayerMask DetectionLayers { get; set; }

    /// <summary>
    /// Collider detected by the clean area checker.
    /// </summary>
    public Collider2D DetectedCollider { get; private set; }

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <param name="radius">Radius of the clean area checker.</param>
    /// <param name="detectionLayers">Detection layers of the clean area checker.</param>
    public CleanAreaChecker(
        float radius,
        LayerMask detectionLayers)
    {
        Radius = radius;
        DetectionLayers = detectionLayers;
    }

    /// <summary>
    /// Checks if a given position is within a clean area, meaning it is not colliding
    /// with any objects in the specified detection layers.
    /// </summary>
    /// <param name="position">The global position to check for cleanliness.</param>
    /// <returns>
    /// True if the area at the specified position is clean (not colliding with any
    /// objects in the detection layers), otherwise false.
    /// </returns>
    public bool IsCleanArea(Vector2 position)
    {
        DetectedCollider = Physics2D.OverlapCircle(
            position,
            Radius,
            DetectionLayers);
        return DetectedCollider == null;
    }
    
    /// <summary>
    /// Disposes of the CleanAreaChecker resources.
    /// </summary>
    public void Dispose()
    {
        DetectedCollider = null;
    }
}
}