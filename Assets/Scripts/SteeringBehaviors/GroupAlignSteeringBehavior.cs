using System.Collections.Generic;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// <p> Monobehaviour to offer an Align steering behaviour. </p>
/// <p> Group align steering behaviour makes the agent look at the same direction than
/// the average orientation of a group of target nodes. </p>
/// </summary>
[RequireComponent(typeof(AlignSteeringBehavior))]
public class GroupAlignSteeringBehavior: SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("List of agents to align with averaging their orientations.")]
    [SerializeField] private List<GameObject> targets = new List<GameObject>();
    [Tooltip("Rotation to start to slow down (degress).")]
    [SerializeField] private float decelerationRadius = 30f;
    [Tooltip("Deceleration curve.")]
    [SerializeField] private AnimationCurve decelerationCurve;
    [Tooltip("At this rotation start angle will be at full speed (degress).")]
    [SerializeField] private float accelerationRadius = 30f;
    [Tooltip("Acceleration curve.")]
    [SerializeField] private AnimationCurve accelerationCurve;

    [Header("DEBUG")]
    [Tooltip("Make orientation gizmos visible.")]
    [SerializeField] private bool orientationGizmosVisible;
    [Tooltip("Color for other's orientation gizmos.")]
    [SerializeField] private Color otherOrientationGizmosColor;
    [Tooltip("Length for other's orientation gizmos.")]
    [SerializeField] private float otherOrientationGizmosLength;
    [Tooltip("Agent's own orientation gizmos color.")]
    [SerializeField] private Color ownOrientationGizmosColor;
    [Tooltip("Agent's own orientation gizmos length.")]
    [SerializeField] private float ownOrientationGizmosLength;

    /// <summary>
    /// List of agents to align with averaging their orientations.
    /// </summary>
    public List<GameObject> Targets { get => targets; set => targets = value; }

    /// <summary>
    /// Rotation to start to slow down (degress).
    /// </summary>
    public float DecelerationRadius
    {
        get => decelerationRadius;
        set
        {
            decelerationRadius = value;
            if (_alignSteeringBehavior != null)
                _alignSteeringBehavior.DecelerationRadius = value;
        }
    }

    /// <summary>
    /// Deceleration curve.
    /// </summary>
    public AnimationCurve DecelerationCurve
    {
        get => decelerationCurve;
        set
        {
            decelerationCurve = value;
            if (_alignSteeringBehavior != null)
                _alignSteeringBehavior.DecelerationCurve = value;
        }
    }

    /// <summary>
    /// At this rotation start angle will be at full speed (degress).
    /// </summary>
    public float AccelerationRadius
    {
        get => accelerationRadius;
        set
        {
            accelerationRadius = value;
            if (_alignSteeringBehavior != null)
                _alignSteeringBehavior.AccelerationRadius = value;
        }
    }

    /// <summary>
    /// Acceleration curve.
    /// </summary>
    public AnimationCurve AccelerationCurve
    {
        get => accelerationCurve;
        set
        {
            accelerationCurve = value;
            if (_alignSteeringBehavior != null)
                _alignSteeringBehavior.AccelerationCurve = value;
        }
    }

    /// <summary>
    /// <p>Average orientation counting every agent's targets.</p>
    /// </summary>
    public float AverageOrientation{ get; private set; }

    private GameObject _orientationMarker;
    private AlignSteeringBehavior _alignSteeringBehavior;

    private void Awake()
    {
        _orientationMarker = new GameObject("OrientationMarker");
        _alignSteeringBehavior = GetComponent<AlignSteeringBehavior>();
        _alignSteeringBehavior.Target = _orientationMarker;
        _alignSteeringBehavior.AccelerationRadius = AccelerationRadius;
        _alignSteeringBehavior.DecelerationRadius = DecelerationRadius;
        _alignSteeringBehavior.AccelerationCurve = AccelerationCurve;
        _alignSteeringBehavior.DecelerationCurve = DecelerationCurve;
    }

    private void OnDestroy()
    {
        Destroy(_orientationMarker);
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (Targets == null || Targets.Count == 0 || _alignSteeringBehavior == null) 
            return new SteeringOutput(Vector2.zero, 0);

        // Let's average heading counting every agent's targets. You'd better get an
        // average vector from heading vectors than average their angle rotation values.
        // This way you can be sure that resulting average is in the inner angle between
        // every target vector pair.
        Vector2 headingSum = new();
        foreach (GameObject target in Targets)
        {
            // Remember that, for our agents, forward direction point upwards, i.e. Y
            // axis. So, their respective transform.up vectors are actually their heading
            // vectors.
            headingSum += (Vector2) target.transform.up;
        }
        Vector2 averageHeading = (headingSum / Targets.Count);
    
        // Rotate our marker to point at the average heading.
        _orientationMarker.transform.up = averageHeading;
    
        // Store resulting orientation.
        AverageOrientation =
            _orientationMarker.transform.rotation.eulerAngles.z;
    
        return _alignSteeringBehavior.GetSteering(args);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!orientationGizmosVisible || Targets == null) return;

        // Draw other orientation markers.
        Gizmos.color = otherOrientationGizmosColor;
        foreach (GameObject target in Targets)
        {
            Vector2 targetOrientation = target.transform.rotation * 
                                        Vector2.up * 
                                        otherOrientationGizmosLength;
            Gizmos.DrawLine(
                transform.position, 
                transform.position + (Vector3) targetOrientation);
        }
    
        // Draw resulting average orientation.
        Gizmos.color = ownOrientationGizmosColor;
    
        Vector2 ownOrientationDirection = Quaternion.AngleAxis(
            AverageOrientation, 
            Vector3.forward) * Vector2.up * ownOrientationGizmosLength;
        Gizmos.DrawLine(
            transform.position, 
            transform.position + (Vector3) ownOrientationDirection);
    }
#endif
}
}