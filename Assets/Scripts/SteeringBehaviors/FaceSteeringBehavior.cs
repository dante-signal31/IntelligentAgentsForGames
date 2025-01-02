using UnityEngine;

/// <summary>
/// <p>Monobehaviour to offer a face to a target steering behaviour.</p>
///
/// <p>This behavior makes its agent look at its target.</p>
/// </summary>
[RequireComponent(typeof(AlignSteeringBehavior))]
public class FaceMatchingSteeringBehavior : SteeringBehavior, ITargeter
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to face to.")]
    [SerializeField] private GameObject target;
    
    // TODO: Make this reference assignment automatic.
    [Header("WIRING:")] 
    [SerializeField] private AlignSteeringBehavior alignSteeringBehavior;

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
        if (target != null) _targetPosition = target.transform.position;
        // We use an align steering behavior to make the agent update its rotation. But
        // align behavior copies another GameObject rotation, so we need a dummy
        // GameObject to rotate it in the direction to look at. That dummy GameObject
        // will be passed to align steering behavior, to give it something to copy.
        _marker = new GameObject("MarkerForAlignSteeringBehavior");
        // Make the align steering behavior to copy the dummy GameObject rotation.
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
        if (target == null) return new SteeringOutput(Vector2.zero, 0);
        
        UpdateTargetData();
        Vector2 currentPosition = args.Position;

        Vector2 direction = _targetPosition - currentPosition;
        
        // Rotate the dummy GameObject in the direction we want to look at. Remember
        // that dummy GameObject is the align steering behavior target since Awake().
        _marker.transform.up = direction;
        return alignSteeringBehavior.GetSteering(args);
    }
}