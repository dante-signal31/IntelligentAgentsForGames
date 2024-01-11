﻿using UnityEngine;

/// <summary>
/// Monobehaviour to offer a Seek steering behaviour.
/// </summary>
public class ArriveSteeringBehavior : SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("Point to arrive to.")]
    public GameObject target;
    [Tooltip("Radius to start to slow down.")]
    [SerializeField] private float brakingRadius;
    [Tooltip("At this distance from target agent will full stop.")]
    [SerializeField] private float arrivingRadius;
    [Tooltip("Deceleration curve.")] 
    [SerializeField] private AnimationCurve decelerationCurve;
    [Tooltip("At this distance from start point will be at full speed.")]
    [SerializeField] private float accelerationRadius;
    [Tooltip("Acceleration curve.")] 
    [SerializeField] private AnimationCurve accelerationCurve;

    private Vector2 _startPosition;
    private float _distanceFromStart;
    private bool _idle = true;

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        Vector2 targetPosition = target.transform.position;
        Vector2 currentPosition = args.Position;
        Vector2 currentVelocity = args.CurrentVelocity;
        float stopSpeed = args.StopSpeed;
        float maximumSpeed = args.MaximumSpeed;
        
        
        Vector2 toTarget = targetPosition - currentPosition;
        float distanceToTarget = toTarget.magnitude;

        float newSpeed = 0.0f;
        
        if (_idle && _distanceFromStart > 0) _distanceFromStart = 0;
        
        if (distanceToTarget >= arrivingRadius && _distanceFromStart < accelerationRadius)
        { // Acceleration phase.
            if (_idle)
            {
                _startPosition = currentPosition;
                _idle = false;
            }
            _distanceFromStart = (currentPosition - _startPosition).magnitude;
            newSpeed = maximumSpeed * accelerationCurve.Evaluate(_distanceFromStart / accelerationRadius);
        }
        else if (distanceToTarget < brakingRadius && distanceToTarget >= arrivingRadius)
        { // Deceleration phase.
            newSpeed = currentVelocity.magnitude > stopSpeed?
                maximumSpeed * decelerationCurve.Evaluate(distanceToTarget / brakingRadius):
                0;
        }
        else if (distanceToTarget < arrivingRadius)
        { // Stop phase.
            newSpeed = 0;
            _idle = true;
        }
        else
        { // Cruise speed phase.
            newSpeed = maximumSpeed;
        }
        
        Vector2 newVelocity = toTarget.normalized * newSpeed;
        
        return new SteeringOutput(newVelocity, 0);
    }
}