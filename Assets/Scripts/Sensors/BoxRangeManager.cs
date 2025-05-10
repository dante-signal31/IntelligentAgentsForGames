using System;
using UnityEngine;
using UnityEngine.Events;

namespace Sensors
{
/// <summary>
/// <p>This script allows to resize a box collider in realtime.<p>
///
/// <p>WARNING! when using this component, collider offset should be set only by this
/// script. So let collider component offset property alone and do not edit it through
/// the inspector.<p>
/// </summary>
[ExecuteAlways]
public class BoxRangeManager : MonoBehaviour
{
    private enum GrowDirection
    {   
        Symmetric,
        Up,
        Down,
        Left,
        Right  
    }

    [Header("CONFIGURATION:")]
    [SerializeField] private Vector2 initialOffset;
    [Tooltip("Length for this sensor.")]
    [SerializeField] private float range;
    [Tooltip("Width for this sensor.")]
    [SerializeField] private float width;
    [Tooltip("Grow direction for this sensor when width or range is change")]
    [SerializeField] private GrowDirection growDirection;

    [Header("WIRING:")]
    [SerializeField] private BoxCollider2D boxCollider;

    public float Range
    {
        get => range;
        set
        {
            range = value;
            SetBoxSize(width, value);
        }
    }

    public float Width {
        get => width;
        set
        {
            width = value;
            SetBoxSize(value, range);
        }
    }

    /// <summary>
    /// Managed box collider.
    /// </summary>
    public BoxCollider2D BoxCollider => boxCollider;

    private Vector2 _currentSize;
    private GrowDirection _currentGrowDirection;
    private const float OffsetBias = 0.5f;

    private Vector2 GetGrowOffsetVector()
    {
        switch (growDirection)
        {
            case GrowDirection.Symmetric:
                return new Vector2(0, 0);
            case GrowDirection.Up:
                return new Vector2(0, OffsetBias);
            case GrowDirection.Down:
                return new Vector2(0, -OffsetBias);
            case GrowDirection.Left:
                return new Vector2(-OffsetBias, 0);
            case GrowDirection.Right:
                return new Vector2(OffsetBias, 0);
            default:
                return new Vector2(0, 0);
        }
    }

    private Vector2 GetGrowVector(Vector2 currentSize, Vector2 newSize)
    {
        return newSize - currentSize;
    }

    private void Awake()
    {
        _currentSize = new Vector2(width, range);
    }

    private void Start()
    {
        RefreshBoxSize();
    }

    private void RefreshBoxSize()
    {
        SetBoxSize(width, range);
    }

    private void SetBoxSize(float newWidth, float newRange)
    {
        Vector2 newSize = new Vector2(newWidth, newRange);
        boxCollider.offset = Vector2.zero;
        boxCollider.size = Vector2.one;
        Vector2 growOffsetVector = GetGrowOffsetVector();
        Vector2 growVector = GetGrowVector(boxCollider.size, newSize);
        boxCollider.size = newSize;
        boxCollider.offset = initialOffset + growVector * growOffsetVector;
    }

    [ContextMenu("Reset Box Collider")]
    public void ResetBoxCollider()
    {
        if (boxCollider == null) return;
        boxCollider.offset = Vector2.zero;
        boxCollider.size = Vector2.one;
        RefreshBoxSize();
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (growDirection != _currentGrowDirection)
        {
            ResetBoxCollider();
            _currentSize = Vector2.zero;
            _currentGrowDirection = growDirection;
        }
        RefreshBoxSize();
    }
#endif

}
}
