using System;
using UnityEngine;

/// <summary>
/// Monobehaviour to offer a velocity match steering behaviour.
/// </summary>
public class VelocityMatchingSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to match its velocity.")]
    public GameObject target;
    [Tooltip("Time to match velocity.")] 
    [SerializeField] private float timeToMatch; 

    private GameObject _currentTarget;
    private Vector2 _targetVelocity;
    private Rigidbody2D _rigidBody;

    private void Awake()
    {
        _rigidBody = target.GetComponentInChildren<Rigidbody2D>();
    }

    /// <summary>
    /// Load target data.
    /// </summary>
    private void UpdateTargetData()
    {
        if (_currentTarget != target)
        {
            _rigidBody = target.GetComponentInChildren<Rigidbody2D>();
            _currentTarget = target;
        }
        _targetVelocity = _rigidBody.velocity;
    }
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        UpdateTargetData();
        Vector2 currentVelocity = args.CurrentVelocity;
        float maximumAcceleration = args.MaximumAcceleration;
        float deltaTime = args.DeltaTime;

        Vector2 neededAcceleration = (_targetVelocity - currentVelocity) / timeToMatch;
        if (neededAcceleration.magnitude > maximumAcceleration)
        {
            neededAcceleration = neededAcceleration.normalized * maximumAcceleration;
        }

        Vector2 newVelocity = currentVelocity + neededAcceleration * deltaTime;
        
        return new SteeringOutput(newVelocity, 0);
    }
}