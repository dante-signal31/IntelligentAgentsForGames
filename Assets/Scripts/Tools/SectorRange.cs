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

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        // OnValidate was being called while playing and that is not supposed to happen.
        if (Application.isPlaying) return; 
        
        // Call properties to force event emission.
        base.OnValidate();
        MinimumRange = minimumRange;   
    }
#endif
}
}

