using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Monobehaviour to offer an Align steering behaviour.
/// </summary>
public class AlignSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to align with.")]
    public GameObject target;
    [FormerlySerializedAs("brakingRadius")]
    [Tooltip("Rotation to start to slow down (degress).")]
    [SerializeField] private float decelerationRadius;
    [Tooltip("At this rotation from target angle will full stop (degress).")]
    [SerializeField] private float arrivingRadius;
    [Tooltip("Deceleration curve.")] 
    [SerializeField] private AnimationCurve decelerationCurve;
    [Tooltip("At this rotation start angle will be at full speed (degress).")]
    [SerializeField] private float accelerationRadius;
    [Tooltip("Acceleration curve.")] 
    [SerializeField] private AnimationCurve accelerationCurve;

    private float _startOrientation;
    private float _rotationFromStart;
    private bool _idle = true;

    private GameObject _currentTarget;
    private float _targetOrientation;

    /// <summary>
    /// Load target data.
    /// </summary>
    private void UpdateTargetData()
    {
        _currentTarget = target;
        _targetOrientation = target.transform.rotation.eulerAngles.z;
    }
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        UpdateTargetData();
        float currentOrientation = args.Orientation;
        float maximumRotationalSpeed = args.MaximumRotationalSpeed;
        
        
        float toTargetRotation = Mathf.DeltaAngle(currentOrientation, _targetOrientation);
        float toTargetRotationAbs = Mathf.Abs(toTargetRotation);
        int rotationSide = (toTargetRotation < 0) ? -1 : 1;

        float newRotationalSpeed = 0.0f;

        if (_idle && toTargetRotationAbs < arrivingRadius)
        {
            return new SteeringOutput(Vector2.zero, 0);
        }
        else if (_idle && _rotationFromStart > 0)
        {
            _rotationFromStart = 0;
        }
        
        if (toTargetRotationAbs >= arrivingRadius && Mathf.Abs(_rotationFromStart) < accelerationRadius)
        { // Acceleration phase.
            if (_idle)
            {
                _startOrientation = currentOrientation;
                _idle = false;
            }
            _rotationFromStart = Mathf.DeltaAngle(currentOrientation, _startOrientation);
            newRotationalSpeed = maximumRotationalSpeed * accelerationCurve.Evaluate(Mathf.Abs(_rotationFromStart) / accelerationRadius) * rotationSide;
        }
        else if (toTargetRotationAbs < decelerationRadius && toTargetRotationAbs >= arrivingRadius)
        { // Deceleration phase.
            newRotationalSpeed = maximumRotationalSpeed * decelerationCurve.Evaluate(toTargetRotationAbs / decelerationRadius) * rotationSide;
        }
        else if (toTargetRotationAbs < arrivingRadius)
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