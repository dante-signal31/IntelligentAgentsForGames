using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Tools
{
/// <summary>
/// Represents a 2D cone range primarily used for visualization and calculations in an
/// Unity scene.
/// The cone range is defined by its length, angular width, and an associated color
/// for visual representation.
/// </summary>
[ExecuteAlways]
public class ConeRange : MonoBehaviour
{
    [Header("CONFIGURATION OF CONE RANGE:")]
    [Tooltip("Length of this cone.")]
    [SerializeField] private float range;
    [Tooltip("If true, the range is fixed and cannot be changed with the visual handle.")]
    [SerializeField] private bool fixedRange;
    [Tooltip("Half angular width in degrees for this cone.")]
    [Range(0, 90)]
    [SerializeField] private float semiConeDegrees;
    [FormerlySerializedAs("coneColor")]
    [Tooltip("Color to display this cone in editor.")]
    [SerializeField] private Color color = new Color(1.0f, 0,0, 0.5f);
    [Space]
    [Tooltip("Event to trigger when the cone is updated.")]
    public UnityEvent Updated;

    /// <summary>
    /// Length of this cone.
    /// </summary>
    public float Range
    {
        get => range;
        set
        {
            range = value;
            if (Updated != null) Updated.Invoke();
        }
    }
    
    /// <summary>
    /// If true, the range is fixed and cannot be changed with the visual handle.
    /// </summary>
    public bool FixedRange => fixedRange;

    /// <summary>
    /// Half angular width in degrees for this cone.
    /// </summary>
    public float SemiConeDegrees
    {
        get => semiConeDegrees;
        set
        {
            semiConeDegrees = value;
            if (Updated != null) Updated.Invoke();
        }
    }
    /// <summary>
    /// Color to display this cone in editor.
    /// </summary>
    public Color Color => color;
    
    // <summary>
    /// <p>This node local Forward vector.</p>
    /// </summary>
    public Vector3 Forward => Vector3.up;

    // <summary>
    /// <p>This node local Normal vector.</p>
    /// </summary>
    public Vector3 Normal => Vector3.forward;


    /// <summary>
    /// Initializes the ConeRange2D instance with specified parameters.
    /// </summary>
    /// <param name="range">The length of the cone.</param>
    /// <param name="semiConeDegrees">The half angular width in degrees for the
    /// cone.</param>
    /// <param name="fixedRange">Specifies whether the range is fixed and cannot be
    /// changed with the visual handle. Default is false.</param>
    /// <param name="coneColor">The color to display the cone in the editor. Default
    /// is Color.black.</param>
    // public void Initialize(
    //     float range,
    //     float semiConeDegrees,
    //     bool fixedRange = false
    //     )
    // {
    //     this.range = range;
    //     this.semiConeDegrees = semiConeDegrees;
    //     this.fixedRange = fixedRange;
    // }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        // Call properties to force event emission.
        Range = range;
        SemiConeDegrees = semiConeDegrees;
    }
#endif
}
}

