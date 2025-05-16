using UnityEngine;

namespace Tools
{
/// <summary>
/// <p>This script allows resizing a box collider in realtime.</p>
///
/// <p>WARNING! when using this component, collider offset should be set only by this
/// script. So let the collider component offset the property alone and do not edit it
/// through the inspector.</p>
/// </summary>
public class BoxRangeManager : MonoBehaviour
{
    /// <summary>
    /// <p>Possible grow directions for the box collider:</p>
    /// <list type="bullet">
    /// <item>
    /// <b>Symmetric</b>: grow in every direction. If you change the range, then UP and
    /// DOWN grow. If you change width, then LEFT and RIGHT grow.
    /// </item>
    /// <item>
    /// <b>Up</b>: grow in the UP direction if you change range. If you change width,
    /// then LEFT and RIGHT grow.
    /// </item>
    /// <item>
    /// <b>Down</b>: grow in the DOWN direction if you change range. If you change width,
    /// then LEFT and RIGHT grow.
    /// </item>
    /// <item>
    /// <b>Left</b>: grow in the LEFT direction if you change width. If you change the
    /// range, then UP and DOWN grow.
    /// </item>
    /// <item>
    /// <b>Right</b>: grow in the RIGHT direction if you change width. If you change
    /// the range, then UP and DOWN grow.
    /// </item>
    /// </list>
    /// </summary>
    private enum GrowDirection
    {   
        Symmetric,
        Up,
        Down,
        Left,
        Right  
    }

    [Header("CONFIGURATION:")]
    [Tooltip("Offset of this sensor at its (1,1) dimensions.")]
    [SerializeField] private Vector2 initialOffset;
    [Tooltip("Length for this sensor. It moves UP and DOWN of the box.")]
    [SerializeField] private float range;
    [Tooltip("Width for this sensor. It moves LEFT and RIGHT of the box.")]
    [SerializeField] private float width;
    [Tooltip("Grow direction for this sensor when width or range is change")]
    [SerializeField] private GrowDirection growDirection;

    [Header("WIRING:")]
    [Tooltip("Managed box collider.")]
    [SerializeField] private BoxCollider2D boxCollider;

    /// <summary>
    /// Length for this sensor. It moves UP and DOWN of the box.
    /// </summary>
    public float Range
    {
        get => range;
        set
        {
            range = value;
            SetBoxSize(width, value);
        }
    }

    /// <summary>
    /// Width for this sensor. It moves LEFT and RIGHT of the box.
    /// </summary>
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

    // private Vector2 _currentSize;
    private GrowDirection _currentGrowDirection;
    private const float OffsetBias = 0.5f;

    /// <summary>
    /// Get offset vector needed to keep the box collider in the same position as before
    /// after changing the size.
    /// </summary>
    /// <returns>New offset vector.</returns>
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

    /// <summary>
    /// Get the vector needed to grow the box collider to the new size.
    /// </summary>
    /// <param name="currentSize">Current box dimensions.</param>
    /// <param name="newSize">New box dimensions.</param>
    /// <returns>Vector with dimensions changes.</returns>
    private Vector2 GetGrowVector(Vector2 currentSize, Vector2 newSize)
    {
        return newSize - currentSize;
    }

    private void Start()
    {
        RefreshBoxSize();
    }

    /// <summary>
    /// Update the box with its new dimensions.
    /// </summary>
    private void RefreshBoxSize()
    {
        SetBoxSize(width, range);
    }

    /// <summary>
    /// Sets the size of the box collider and adjusts its offset accordingly.
    /// </summary>
    /// <param name="newWidth">The new width of the box collider.</param>
    /// <param name="newRange">The new range (height) of the box collider.</param>
    private void SetBoxSize(float newWidth, float newRange)
    {
        if (boxCollider == null) return;
        Vector2 newSize = new Vector2(newWidth, newRange);
        boxCollider.offset = Vector2.zero;
        boxCollider.size = Vector2.one;
        Vector2 growOffsetVector = GetGrowOffsetVector();
        Vector2 growVector = GetGrowVector(boxCollider.size, newSize);
        boxCollider.size = newSize;
        boxCollider.offset = initialOffset + growVector * growOffsetVector;
    }
    
    [ContextMenu("Reset Box Manager")]
    public void ResetBoxManager()
    {
        initialOffset = new Vector2(0, 0);
        range = 1;
        width = 1;
        growDirection = GrowDirection.Symmetric;
        ResetBoxCollider();
        RefreshBoxSize();
    }
    
    /// <summary>
    /// Resets the box collider to its default settings, including offset and size.
    /// Updates the box dimensions to align with the specified width and range values.
    /// </summary>
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
            _currentGrowDirection = growDirection;
        }
        RefreshBoxSize();
    }
#endif

}
}
