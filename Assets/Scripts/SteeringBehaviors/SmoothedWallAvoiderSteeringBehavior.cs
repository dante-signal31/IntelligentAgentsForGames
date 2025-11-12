using PropertyAttribute;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace SteeringBehaviors
{
/// <summary>
/// Steering behavior to avoid walls and obstacles using usher algorithm to smooth
/// movements.
/// </summary>
[RequireComponent(typeof(CustomTimer))]
public class SmoothedWallAvoiderSteeringBehavior: SteeringBehavior, ITargeter
{
    [Header("CONFIGURATION:")]
    [Tooltip("Target to go avoiding other agents.")]
    [SerializeField] private GameObject target;
    [Tooltip("Usher scene to spawn.")]
    [SerializeField] private GameObject usherPrefab;
    [Tooltip("How far should usher start ahead of current agent.")]
    [SerializeField] private float usherAdvantage;
    [Tooltip("Distance to usher to consider it reached.")]
    [SerializeField] private float reachingDistanceToUsher = 20.0f;
    [Tooltip("Time to wait after reaching usher to start chasing it again.")]
    [SerializeField] private float secondsToWaitAfterReachingUsher = 1.0f;
    
    [Header("WIRING:")]
    [Tooltip("Steering to chase the usher. Must comply with ITargeter interface.")]
    [InterfaceCompliant(typeof(ITargeter))]
    [SerializeField] private SteeringBehavior chaseToUsherSteeringBehavior;
    
    [Header("DEBUG:")]
    [Tooltip("Show gizmos to debug.")]
    [SerializeField] private bool showGizmos = true;
    [Tooltip("Colors for this object's gizmos.")] 
    [SerializeField] private Color gizmosColor;

    /// <summary>
    /// Target to go avoiding other agents.
    /// </summary>
    public GameObject Target
    {
        get => target;
        set
        {
            target = value;
            if (_usherTargeter == null) return;
            _usherTargeter.Target = value;
        }
    }

    /// <summary>
    /// Show gizmos.
    /// </summary>
    public bool ShowGizmos
    {
        get => showGizmos;
        set
        {
            showGizmos = value;
            if (_usherAgentSteeringBehaviorGizmos == null) return;
            _usherAgentSteeringBehaviorGizmos.ShowGizmos = value;
        }
    }

    /// <summary>
    /// Colors for this object's gizmos.
    /// </summary>
    public Color GizmosColor
    {
        get => gizmosColor; 
        set => gizmosColor = value;
    }
    
    
    private ITargeter _chaseToUsherTargeter;
    private AgentMover _currentAgent;
    private UsherWaiterAgent _usherAgent;
    private IGizmos _usherAgentSteeringBehaviorGizmos;
    private ITargeter _usherTargeter;
    private CustomTimer _advantageTimer;
    private bool _givingAdvantageToUsher;
    private bool _usherReached;
    private SteeringOutput _currentSteering;

    private void Awake()
    {
        _currentAgent = GetComponentInParent<AgentMover>();
        _advantageTimer = GetComponent<CustomTimer>();
        _chaseToUsherTargeter = (ITargeter)chaseToUsherSteeringBehavior;
        CreateUsher();
    }

    private void Start()
    {
        SetTimer();
        _currentSteering = new SteeringOutput();
        
        ConfigureUsher();
        
        // Prepare to follow the usher.
        _chaseToUsherTargeter.Target = _usherAgent.gameObject;
    }
    
    private void CreateUsher()
    {
        // Create usher to follow.
        _usherAgent = Instantiate(
            usherPrefab, 
            transform.position, 
            Quaternion.identity, 
            null).GetComponent<UsherWaiterAgent>();
        _usherAgentSteeringBehaviorGizmos = (IGizmos) _usherAgent.SteeringBehavior;
        _usherTargeter = (ITargeter)_usherAgent.SteeringBehavior;
    }

    private void OnEnable()
    {
        if (_usherAgent == null) return;
        _usherAgent.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (_usherAgent == null) return;
        _usherAgent.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_usherAgent == null) return;
        Destroy(_usherAgent.gameObject);
    }

    private void ConfigureUsher()
    {
        // Place usher ahead of our agent.
        _usherAgent.transform.position = _currentAgent.transform.position + 
                               usherAdvantage * (Vector3)_currentAgent.Forward;
        
        // Configure usher.
        _usherAgent.FollowingAgent = _currentAgent;
        _usherAgent.MaximumSpeed = _currentAgent.MaximumSpeed;
        _usherAgent.StopSpeed = _currentAgent.StopSpeed;
        _usherAgent.MaximumAcceleration = _currentAgent.MaximumAcceleration;
        _usherAgent.MaximumDeceleration = _currentAgent.MaximumDeceleration;
        _usherAgent.MaximumRotationalSpeed = _currentAgent.MaximumRotationalSpeed;
        
        // Give usher a place to go to.
        _usherTargeter.Target = target;
    }
    
    private void SetTimer()
    {
        _advantageTimer.waitTime = secondsToWaitAfterReachingUsher;
        _advantageTimer.oneShot = true;
        _advantageTimer.timeoutEvent.AddListener(OnTimerTimeout);
    }

    private void OnTimerTimeout()
    {
        _givingAdvantageToUsher = false;
    }
    
    private void StartTimer()
    {
        _givingAdvantageToUsher = true;
        _advantageTimer.StartTimer();
    }


    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (chaseToUsherSteeringBehavior == null || _usherAgent == null) 
            return SteeringOutput.Zero;

        float distanceToUsher = Vector2.Distance(
            _currentAgent.transform.position, 
            _usherAgent.transform.position);
        
        // To smooth our own movements, usher should have some advantage. So, if the usher
        // stops, and we reach it, then we wait to let it advance again and get advantage.
        if (distanceToUsher < reachingDistanceToUsher)
        {
            // If we are too near to usher, stay still.
            _usherReached = true;
            _currentSteering = new SteeringOutput(
                linear: Vector2.zero,
                angular: _currentSteering.Angular);
        } 
        else if (_usherReached && distanceToUsher > reachingDistanceToUsher)
        {
            // Usher is going away from us. Wait for a time to give him some advantage.
            _usherReached = false;
            _currentSteering = new SteeringOutput(
                linear: Vector2.zero,
                angular: _currentSteering.Angular);
            StartTimer();
        }
        else if (_givingAdvantageToUsher)
        {
            // If we are waiting to give advantage to usher, stay still.
            _currentSteering = new SteeringOutput(
                linear: Vector2.zero,
                angular: _currentSteering.Angular);
        }
        else
        {
            // If we are not giving advantage to usher, follow usher.
            _currentSteering = chaseToUsherSteeringBehavior.GetSteering(args);
        }
        
        return _currentSteering;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmosColor;
        
        if (Application.isPlaying)
        { // We are executing simulation.
            if (_usherAgent == null) return;
            
            Gizmos.DrawLine(
                _currentAgent.transform.position, 
                _usherAgent.transform.position);
            Gizmos.DrawWireSphere(_usherAgent.transform.position, 0.5f);
        }
        else
        { // We are in editor.
            if (_currentAgent == null) return;
            Gizmos.DrawLine(
                _currentAgent.transform.position, 
                _currentAgent.transform.position + 
                (Vector3)_currentAgent.Forward * usherAdvantage);
            Gizmos.DrawWireSphere(
                _currentAgent.transform.position + 
                                  (Vector3)_currentAgent.Forward * usherAdvantage, 
                0.5f);
        }
    }
#endif
}
}