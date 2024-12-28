using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// <p> Monobehaviour to offer an Align steering behaviour. </p>
///
/// <p> Align steering behaviour makes the agent look at the same direction than
/// a target GameObject. </p>
/// </summary>
public class AlignSteeringBehavior : SteeringBehavior, ITargeter
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to align with.")]
    [SerializeField] private GameObject target;
    [Tooltip("Rotation to start to slow down (degress).")]
    [SerializeField] private float decelerationRadius;
    [Tooltip("Deceleration curve.")] 
    [SerializeField] private AnimationCurve decelerationCurve;
    [Tooltip("At this rotation start angle will be at full speed (degress).")]
    [SerializeField] private float accelerationRadius;
    [Tooltip("Acceleration curve.")] 
    [SerializeField] private AnimationCurve accelerationCurve;

    private float _startOrientation;
    private float _rotationFromStartAbs;
    private bool _idle = true;
    
    private float _targetOrientation;

    public GameObject Target
    {
        get => target;
        set => target = value;
    }
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    { // I want smooth rotations, so I will use the same approach than in
      // ArriveSteeringBehavior.
        if (target == null) return new SteeringOutput(Vector2.zero, 0);
        
        _targetOrientation = target.transform.rotation.eulerAngles.z;
        float currentOrientation = args.Orientation;
        float maximumRotationalSpeed = args.MaximumRotationalSpeed;
        float arrivingMargin = args.StopRotationThreshold;
        
        float toTargetRotation = Mathf.DeltaAngle(currentOrientation, _targetOrientation);
        int rotationSide = (toTargetRotation < 0) ? -1 : 1;
        float toTargetRotationAbs = Mathf.Abs(toTargetRotation);
        

        float newRotationalSpeed = 0.0f;

        if (_idle && toTargetRotationAbs < arrivingMargin)
        { // If you are stopped and you are close enough to target rotation, you are done.
          // Just stay there.
            return new SteeringOutput(Vector2.zero, 0);
        }
        
        if (_idle && _rotationFromStartAbs > 0)
        { // If you are stopped and you are not close enough to target rotation, you need
          // to start rotating. But first, you need to reset your rotation counter.
            _rotationFromStartAbs = 0;
        }
        
        if (toTargetRotationAbs >= arrivingMargin && 
            _rotationFromStartAbs < accelerationRadius)
        { // Acceleration phase.
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
                accelerationRadius, 
                _rotationFromStartAbs);
            newRotationalSpeed = maximumRotationalSpeed * 
                                 accelerationCurve.Evaluate(accelerationProgress) * 
                                 rotationSide;
        }
        else if (toTargetRotationAbs < decelerationRadius && 
                 toTargetRotationAbs >= arrivingMargin)
        { // Deceleration phase.
            float decelerationProgress = Mathf.InverseLerp(
                decelerationRadius, 
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
        
        return new SteeringOutput(Vector2.zero, newRotationalSpeed);
    }
}