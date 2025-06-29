using UnityEngine;

namespace Tools
{
/// <summary>
/// <p>Sector cone gizmo to configure ranges for 2D games.</p>
/// <p>It's like a cone range, but this one has a minimum range.</p>
/// </summary>
[ExecuteAlways]
public class SectorRange : ConeRange
{
    [Header("CONFIGURATION OF SECTOR RANGE:")]
    [Tooltip("Minimum range for this sector.")]
    [SerializeField] private float minimumRange;

    /// <summary>
    /// Minimum range for this cone.
    /// </summary>   
    public float MinimumRange
    {
        get => minimumRange;
        set
        {
            minimumRange = value;
            if (Updated != null) Updated.Invoke();
        }   
    }
    
    /// <summary>
    /// Initializes the SectorRange instance with specified parameters.
    /// </summary>
    /// <param name="range">The length of the cone.</param>
    /// <param name="minimumRange">Minimum range for this sector.</param>
    /// <param name="semiConeDegrees">The half angular width in degrees for the
    /// cone.</param>
    /// <param name="fixedRange">Specifies whether the range is fixed and cannot be
    /// changed with the visual handle. Default is false.</param>
    /// <param name="coneColor">The color to display the cone in the editor. Default
    /// is Color.black.</param>
    // public void Initialize(
    //     float range,
    //     float minimumRange,
    //     float semiConeDegrees,
    //     bool fixedRange = false
    //     )
    // {
    //     base.Initialize(range, semiConeDegrees, fixedRange);
    //     this.minimumRange = minimumRange;
    // }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        // Call properties to force event emission.
        base.OnValidate();
        MinimumRange = minimumRange;   
    }
#endif
}
}

