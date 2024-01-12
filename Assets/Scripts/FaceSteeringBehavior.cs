using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Monobehaviour to offer a face to a target steering behaviour.
/// </summary>
[RequireComponent(typeof(AlignSteeringBehavior))]
public class FaceMatchingSteeringBehavior : SteeringBehavior
{
    [Header("WIRING:")] 
    [SerializeField] private AlignSteeringBehavior alignSteeringBehavior;
    
    [Header("CONFIGURATION:")]
    [Tooltip("Target to face to.")]
    public GameObject target;
    [Tooltip("Prefab used to give orientation to underlying Align behavior.")]
    [SerializeField] private GameObject orientationMarker;
    [Tooltip("Make visible position marker.")] 
    [SerializeField] private bool orientationMarkerVisible = true;
    
    private Vector2 _targetPosition;
    private GameObject _marker;

    private void Awake()
    {
        _targetPosition = target.transform.position;
        _marker = Instantiate(orientationMarker, Vector2.zero, Quaternion.identity);
        _marker.GetComponentInChildren<SpriteRenderer>().enabled = orientationMarkerVisible;
        alignSteeringBehavior.target = _marker;
    }

    private void OnDestroy()
    {
        Destroy(_marker);
    }

    /// <summary>
    /// Load target data.
    /// </summary>
    private void UpdateTargetData()
    {
        _targetPosition = target.transform.position;
    }
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        UpdateTargetData();
        Vector2 currentPosition = args.Position;

        Vector2 direction = _targetPosition - currentPosition;

        if (direction == Vector2.zero)
        {
            return new SteeringOutput(Vector2.zero, 0);
        }
        else
        {
            _marker.transform.up = direction;
            return alignSteeringBehavior.GetSteering(args);
        }
    }
}