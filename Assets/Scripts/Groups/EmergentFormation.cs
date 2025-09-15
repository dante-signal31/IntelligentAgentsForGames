
using System;
using System.Collections.Generic;
using System.Linq;
using SteeringBehaviors;
using Tools;
using UnityEngine;

namespace Groups
{
public class EmergentFormation : MonoBehaviour
{
    [Header("CONFIGURATION:")]
    [Tooltip("Name of the formation group.")]
    [SerializeField] private string formationGroupName = "EmergentFormation";
    [Tooltip("Layers with obstacles for this agent placement.")]
    [SerializeField] private LayerMask notCleanLayers;
    [Tooltip("Radius to check for clean areas.")]
    [SerializeField] private float cleanAreaRadius = 0.55f;
    [Tooltip("Possible offsets to try with a selected formation partner.")]
    [SerializeField] private OffsetList offsets;
    [Tooltip("Maximum number of attempts to find a valid formation partner.")]
    [SerializeField] private int maxFormationAttempts = 30;
    [Tooltip("Time delay before attempting to find a formation partner again.")]
    [SerializeField] private float newAttemptDelay = 1.0f;
    [Tooltip("Time before checking again if we are in a loop.")]
    [SerializeField] private float loopDetectionCooldown = 1.0f;
    
    [Header("WIRING:")]
    [Tooltip("Steering behavior to follow the selected formation partner.")]
    [SerializeField] private OffsetFollowSteeringBehavior offsetFollowSteeringBehavior;

    /// <summary>
    /// Indicates whether we have a suitable formation partner.
    /// </summary>
    public bool NoSuitableFormationPartner => Partner == null;
    
    /// <summary>
    /// Indicates whether all attempts to search for a suitable formation partner
    /// have been exhausted.
    /// </summary>
    public bool SearchForSuitablePartnerGivenUp => 
        _formationAttempts >= maxFormationAttempts;
    
    private Queue<GameObject> _groupMembers = new();
    // private OffsetFollowSteeringBehavior _offsetFollowSteeringBehavior;
    private CleanAreaChecker _cleanAreaChecker;
    private int _formationAttempts;
    private System.Timers.Timer _newAttemptTimer;
    private System.Timers.Timer _loopDetectionCooldownTimer;
    private bool _waitingForNewAttemptTimeout;
    private bool _waitingForLoopDetectionCooldownTimeout;
    private bool _loopDetected;
    private AgentMover _ownAgent;
    private EmergentFormation _partnerEmergentFormation;
    private HashSet<GameObject> _loopMembers = new();
    
    private GameObject _partner;
    /// <summary>
    /// Specifies the partnered agent to synchronize movements or behaviors within the
    /// formation.
    /// </summary>
    public GameObject Partner
    {
        get => _partner;
        private set
        {
            _partner = value;
            offsetFollowSteeringBehavior.Target = value;
            _partnerEmergentFormation = value == null? 
                null: 
                value.GetComponentInChildren<EmergentFormation>();
        }
    }
    
    private Vector2 _partnerOffset;
    /// <summary>
    /// Specifies the offset position of the partnered agent to synchronize movements or
    /// behaviors within the formation.
    /// </summary>
    public  Vector2 PartnerOffset
    {
        get => _partnerOffset;
        private set
        {
            _partnerOffset = value;
            if (offsetFollowSteeringBehavior == null) return;
            offsetFollowSteeringBehavior.offsetFromTarget = value;
        }
    }
    
    /// <summary>
    /// Whether the current partner is the leader of the formation.
    /// </summary>
    private bool PartnerIsLeader => _partnerEmergentFormation == null;

    private void Awake()
    {
        SetNewAttemptTimer();
        SetLoopDetectionCooldownTimer();
        _ownAgent = GetComponentInParent<AgentMover>();
    }
    
    private void SetNewAttemptTimer()
    {
        _newAttemptTimer = new System.Timers.Timer(newAttemptDelay * 1000);
        _newAttemptTimer.AutoReset = false;
        _newAttemptTimer.Elapsed += OnNewAttemptTimerTimeout;
    }
    
    private void SetLoopDetectionCooldownTimer()
    {
        _loopDetectionCooldownTimer = new System.Timers.Timer(loopDetectionCooldown * 1000);
        _loopDetectionCooldownTimer.AutoReset = false;
        _loopDetectionCooldownTimer.Elapsed += OnLoopDetectionCooldownTimerTimeout;
    }
    
    private void OnNewAttemptTimerTimeout(object sender, System.Timers.ElapsedEventArgs e)
    {
        _waitingForNewAttemptTimeout = false;
    }
    
    private void OnLoopDetectionCooldownTimerTimeout(
        object sender, 
        System.Timers.ElapsedEventArgs e)
    {
        _waitingForLoopDetectionCooldownTimeout = false;
    }

    private void StartNewAttemptTimer()
    {
        _waitingForNewAttemptTimeout = true;
        _newAttemptTimer.Stop();
        _newAttemptTimer.Start();
    }
    
    private void StartLoopDetectionCooldownTimer()
    {
        _waitingForLoopDetectionCooldownTimeout = true;
        _loopDetectionCooldownTimer.Stop();
        _loopDetectionCooldownTimer.Start();
    }

    private void Start()
    {
        GameObject[] groupNodes = GameObject.FindGameObjectsWithTag(formationGroupName);
        _groupMembers = new Queue<GameObject>(groupNodes.ToArray());
        _cleanAreaChecker = new CleanAreaChecker(
            cleanAreaRadius, 
            notCleanLayers);
    }
    
    /// <summary>
    /// Resets the number of formation attempts made for finding a suitable partner
    /// in a group formation context.
    /// </summary>
    /// <remarks>
    /// This method is typically used to retry the partner search process from the
    /// beginning, ensuring that the number of attempts is set back to zero. It is
    /// useful in situations where a previously failed attempt at finding a suitable
    /// formation partner needs to be entirely reattempted.
    /// </remarks>
    public void RetryFormationPartnerSearch()
    {
        _formationAttempts = 0;
    }
    
    /// <summary>
    /// <p>Vanilla algorithm has a weak point. There is a small chance that three members 
    /// (or more) partner to follow each other, forming a loop apart from the main
    /// formation. So, the main formation can go away while these other members chase
    /// each other endlessly. This problem is inherent to a simple behavior where a
    /// formation member just follows another member.</p>
    /// 
    /// <p>A more complex behavior could be used to avoid this problem, running graph
    /// searches to check that following Partner references up in the 
    /// graph until you eventually reach the formation leader. If a formation leader
    /// cannot be reached, then a member can assume that it is in a loop and can run
    /// a new search for a new partner to break the loop and join the main formation.</p>
    ///
    /// <p>This method runs a recursive check to see if we are in a loop.</p>
    /// <param name="currentLoopDetectionCalls">Current number of steps in the
    /// recursive callstack.</param>
    /// <returns>True if we are in a loop detached from the leader.
    /// False instead.</returns>
    /// </summary>
    private bool WeAreInALoop(
        ref HashSet<GameObject> loopMembers, 
        int currentLoopDetectionCalls = 0)
    {
        // Recursive calls are very expensive. So we should avoid use them in every frame.
        // Instead, we should use a cooldown timer to check if we are in a loop only every
        // _loopDetectionCooldown seconds.
        // So, if we are in a cooldown, then we return the cached result.
        if (_waitingForLoopDetectionCooldownTimeout) return _loopDetected;
        
        // If we don't have a partner, then we are not in a loop.
        if (Partner == null) return false;
        
        // If our current partner is the leader, then we are not in a loop.
        if (PartnerIsLeader) return false;
        
        // Maximum recursion deep would be If we were in a loop composed of all group
        // members, except the leader (extremely odd situation, but theoretically
        // possible). This would end in endless recursion calls, so we break from it.
        if (currentLoopDetectionCalls > _groupMembers.Count - 1) return true;
        
        // If our partner is not the leader, and we have not reached maximum recursion
        // depth, then we continue checking with from our current partner.
        loopMembers.Add(_ownAgent.gameObject);
        _loopDetected = 
            _partnerEmergentFormation.WeAreInALoop(
                ref loopMembers, 
                ++currentLoopDetectionCalls);
        
        // After our check, start a cooldown timer to wait until the new check.
        StartLoopDetectionCooldownTimer();
        
        return _loopDetected;
    }

    private void FixedUpdate()
    {
        if (_partner != null)
        {
            _loopMembers.Clear();
            // If we have a suitable formation partner, then check we can still use the
            // selected offset position.
            Vector2 offsetGlobalPosition = 
                Partner.transform.TransformPoint(PartnerOffset);
            // If we are within the offset area, then there is no need to check anything.
            if ((Vector2.Distance(transform.position, offsetGlobalPosition) <= 
                 2 * cleanAreaRadius ||
                // If we are outside the clean area, then we need to check if we can still
                // use the offset position.
                _cleanAreaChecker.IsCleanArea(offsetGlobalPosition)) &&
                // If we were in a loop, then we should look for a new partner.
                !WeAreInALoop(ref _loopMembers))
                return;
            Partner = null;
            PartnerOffset = Vector2.zero;
        }
        
        // If we have tried too many times to find a new partner, then we must give up
        // and wait to try again.
        if ((_formationAttempts >= maxFormationAttempts)
            && !_waitingForNewAttemptTimeout)
        {
            _formationAttempts = 0;
            StartNewAttemptTimer();
        }
        
        if (_waitingForNewAttemptTimeout) return;
        
        // If we get here, it means we have no suitable formation partner. So, we must
        // find one.
        foreach (GameObject member in _groupMembers)
        {
            // Don't try to partner with our own agent.
            if (member == _ownAgent.gameObject) continue;
            
            // Don't try to partner with agents that are using us as partners.
            EmergentFormation memberEmergentFormation = 
                member.GetComponentInChildren<EmergentFormation>();
            if (memberEmergentFormation != null &&
                memberEmergentFormation.Partner == _ownAgent.gameObject)
                continue;
            
            // Don't try to partner with agents that are already in our loop.
            if (_loopMembers.Contains(member)) continue;
            
            foreach (Vector2 offset in offsets.Offsets)
            {
                Vector2 offsetGlobalPosition = 
                    member.transform.TransformPoint(offset);
                if (_cleanAreaChecker.IsCleanArea(offsetGlobalPosition))
                {
                    Partner = member;
                    PartnerOffset = offset;
                    _formationAttempts = 0;
                    return;
                }
            }
        }

        _formationAttempts++;
    }
}
}

