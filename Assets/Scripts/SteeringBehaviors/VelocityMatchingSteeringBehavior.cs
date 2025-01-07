﻿using System.Diagnostics;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Debug = UnityEngine.Debug;

/// <summary>
/// Monobehaviour to offer a velocity match steering behaviour.
/// </summary>
public class VelocityMatchingSteeringBehavior : SteeringBehavior, ITargeter
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to match its velocity.")]
    [SerializeField] private GameObject target;
    [Tooltip("Time to match velocity.")] 
    [SerializeField] private float timeToMatch; 

    // TODO: Some of this variables can be made local to methods.
    private GameObject _currentTarget;
    private Rigidbody2D _targetRigidBody;
    private Vector2 _targetVelocity;
    private Vector2 _currentVelocity;
    private Vector2 _currentAcceleration;
    private bool _currentAccelerationUpdateIsNeeded;

    /// <summary>
    /// Target to match its velocity.
    /// </summary>
    public GameObject Target
    {
        get => target;
        set => target = value;
    }
    
    /// <summary>
    /// Time to match velocity.
    /// </summary>
    public float TimeToMatch
    {
        get => timeToMatch;
        set => timeToMatch = value;
    }

    private void Awake()
    {
        // TODO: Don't need a reference to target RigidBody2D. You need target AgentMover offers a Velocity property.
        if (Target != null) 
            _targetRigidBody = Target.GetComponentInChildren<Rigidbody2D>();
    }

    /// <summary>
    /// Load target data.
    /// </summary>
    private void UpdateTargetData()
    {
        if (_currentTarget != Target)
        {
            _targetRigidBody = Target.GetComponentInChildren<Rigidbody2D>();
            _currentTarget = Target;
        }

        // Acceleration should change in two cases:
        // 1. Target velocity has changed.
        // 2. Target velocity and current velocity are the same. So we should
        //    stop accelerating.
        if ((_targetVelocity != _targetRigidBody.linearVelocity) ||
            Mathf.Approximately(_targetVelocity.magnitude, _targetRigidBody.linearVelocity.magnitude))
        {
            _targetVelocity = _targetRigidBody.linearVelocity;
            _currentAccelerationUpdateIsNeeded = true;
        }
    }
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (Target == null) return new SteeringOutput(Vector2.zero, 0);

        UpdateTargetData();
        
        _currentVelocity = args.CurrentVelocity;
        float stopSpeed = args.StopSpeed;
        
        float deltaTime = args.DeltaTime;

        if (_currentAccelerationUpdateIsNeeded)
        {
            // Millington executes this code section in every frame, but I
            // think that is an error. Doing that way targets velocity is never
            // reached because current gap between target and current velocity
            // is always divided by timeToMatch. So, my version is executing this
            // code section only when acceleration needs to really update
            // (target has changed its velocity or target velocity has been
            // reached and we no longer need an acceleration).
            float maximumAcceleration = args.MaximumAcceleration;
            float maximumDeceleration = args.MaximumDeceleration;
            Vector2 neededAcceleration = (_targetVelocity - _currentVelocity) / TimeToMatch;
            
            // if braking, then target velocity is zero or the opposite direction than current.
            bool braking = _targetVelocity == Vector2.zero || 
                           Vector2.Dot(_currentVelocity, _targetVelocity) < 0;
            
            // Make sure velocity change is not greater than its maximum values.
            if (!braking && neededAcceleration.magnitude > maximumAcceleration)
            {
                Debug.LogWarning($"[{gameObject.name} - VelocityMatchingSteeringBehavior]: neededAcceleration magnitude {neededAcceleration.magnitude} is greater than maximumAcceleration {maximumAcceleration}. Clamping to maximumAcceleration.");
                neededAcceleration = neededAcceleration.normalized * maximumAcceleration;
            }
            else if (braking && _currentVelocity.magnitude <= stopSpeed)
            {
                return new SteeringOutput(Vector2.zero, 0);
            }
            else if (braking && neededAcceleration.magnitude > maximumDeceleration)
            {
                Debug.LogWarning($"[{gameObject.name} - VelocityMatchingSteeringBehavior]: neededAcceleration magnitude {neededAcceleration.magnitude} for braking is greater than maximumDeceleration {maximumDeceleration}. Clamping to maximumDeceleration.");
                neededAcceleration = neededAcceleration.normalized * maximumDeceleration;
            }
            
            _currentAcceleration = neededAcceleration;
            _currentAccelerationUpdateIsNeeded = false;
        }
        
        Vector2 newVelocity = _currentVelocity + _currentAcceleration * deltaTime;
        Debug.Log($"[{gameObject.name} - VelocityMatchingSteeringBehavior] New velocity: {newVelocity} Speed: {newVelocity.magnitude} Target speed: {_targetVelocity.magnitude}");    
        return new SteeringOutput(newVelocity, 0);
    }
}