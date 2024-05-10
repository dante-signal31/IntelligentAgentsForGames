using UnityEngine;

/// <summary>
/// Monobehaviour to offer a face to a target steering behaviour.
/// </summary>
[RequireComponent(typeof(AlignSteeringBehavior))]
public class FaceMatchingSteeringBehavior : SteeringBehavior, ITargeter
{
    [Header("WIRING:")] 
    [SerializeField] private AlignSteeringBehavior alignSteeringBehavior;
    
    [Header("CONFIGURATION:")]
    [Tooltip("Target to face to.")]
    [SerializeField] private GameObject target;

    /// <summary>
    /// Target to look to.
    /// </summary>
    public GameObject Target
    {
        get => target;
        set => target = value;
    }
    
    private Vector2 _targetPosition;
    private GameObject _marker;

    private void Awake()
    {
        _targetPosition = target.transform.position;
        _marker = new GameObject("MarkerForAlignSteeringBehavior");
        alignSteeringBehavior.Target = _marker;
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