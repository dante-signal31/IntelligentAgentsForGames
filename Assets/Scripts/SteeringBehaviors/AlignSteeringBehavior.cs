using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// <p> MonoBehaviour to offer an Align steering behavior. </p>
///
/// <p> Align steering behavior makes the agent look in the same direction as
/// a target GameObject. </p>
/// </summary>
public class AlignSteeringBehavior : SteeringBehavior, ITargeter
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to align with.")]
    [SerializeField] private GameObject target;
    [Tooltip("Rotation to start to slow down (degrees).")]
    [SerializeField] private float decelerationRadius;
    [Tooltip("Deceleration curve.")] 
    [SerializeField] private AnimationCurve decelerationCurve;
    [Tooltip("At this rotation start angle will be at full speed (degrees).")]
    [SerializeField] private float accelerationRadius;
    [Tooltip("Acceleration curve.")] 
    [SerializeField] private AnimationCurve accelerationCurve;

    /// <summary>
    /// Target to align with.
    /// </summary>
    public GameObject Target
    {
        get => target;
        set => target = value;
    }

    /// <summary>
    /// Rotation to start to slow down (degrees).
    /// </summary>
    public float DecelerationRadius
    {
        get => decelerationRadius;
        set => decelerationRadius = value;
    }

    /// <summary>
    /// Deceleration curve.
    /// </summary>
    public AnimationCurve DecelerationCurve
    {
        get => decelerationCurve;
        set => decelerationCurve = value;
    }

    /// <summary>
    /// At this rotation the start angle will be at full speed (degrees).
    /// </summary>
    public float AccelerationRadius
    {
        get => accelerationRadius;
        set => accelerationRadius = value;
    }

    /// <summary>
    /// Acceleration curve.
    /// </summary>
    public AnimationCurve AccelerationCurve
    {
        get => accelerationCurve;
        set => accelerationCurve = value;
    }

    private float _startOrientation;
    private float _rotationFromStartAbs;
    private bool _idle = true;

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {   // I want smooth rotations, so I will use the same approach as in
        // ArriveSteeringBehavior.
        if (Target == null) return new SteeringOutput(Vector2.zero);
    
        float targetOrientation = Target.transform.rotation.eulerAngles.z;
        float currentOrientation = args.Orientation;
        float maximumRotationalSpeed = args.MaximumRotationalSpeed;
        float arrivingMargin = args.StopRotationThreshold;
    
        float toTargetRotation = Mathf.DeltaAngle(currentOrientation, targetOrientation);
        int rotationSide = (toTargetRotation < 0) ? -1 : 1;
        float toTargetRotationAbs = Mathf.Abs(toTargetRotation);
    
        float newRotationalSpeed;

        if (_idle && toTargetRotationAbs < arrivingMargin)
        {   // If you are stopped, and you are close enough to target rotation, you are
            // done. Just stay there.
            return new SteeringOutput(Vector2.zero);
        }
    
        if (_idle && _rotationFromStartAbs > 0)
        {   // If you are stopped, and you are not close enough to target rotation, you
            // need to start rotating. But first, you need to reset your rotation counter.
            _rotationFromStartAbs = 0;
        }
    
        if (toTargetRotationAbs >= arrivingMargin && 
            _rotationFromStartAbs < AccelerationRadius)
        {   // Acceleration phase.
            if (_idle)
            {
                _startOrientation = currentOrientation;
                _idle = false;
            }
            _rotationFromStartAbs = Mathf.Abs(
                Mathf.DeltaAngle(currentOrientation, _startOrientation));
            // Acceleration curve should start at more than 0 or agent will not
            // start to move.
            float accelerationProgress = Mathf.InverseLerp(
                0, 
                AccelerationRadius, 
                _rotationFromStartAbs);
            newRotationalSpeed = maximumRotationalSpeed * 
                                 accelerationCurve.Evaluate(accelerationProgress) * 
                                 rotationSide;
        }
        else if (toTargetRotationAbs < DecelerationRadius && 
                 toTargetRotationAbs >= arrivingMargin)
        { // Deceleration phase.
            float decelerationProgress = Mathf.InverseLerp(
                DecelerationRadius, 
                0, 
                toTargetRotationAbs);
            newRotationalSpeed = maximumRotationalSpeed * 
                                 decelerationCurve.Evaluate(decelerationProgress) * 
                                 rotationSide;
        }
        else if (toTargetRotationAbs < arrivingMargin)
        { // Stop phase.
            newRotationalSpeed = 0;
            _idle = true;
        }
        else
        { // Cruise speed phase.
            newRotationalSpeed = maximumRotationalSpeed * rotationSide;
        }
    
        return new SteeringOutput(angular: newRotationalSpeed);
    }
}
}