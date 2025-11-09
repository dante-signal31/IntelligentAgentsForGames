using System.Collections.Generic;
using System.Linq;
using System.Timers;
using PropertyAttribute;
using SteeringBehaviors;
using Tools;
using UnityEngine;

namespace Groups
{
/// <summary>
/// The FormationObstacleManager class handles formation movement in the presence of
/// large obstacles, addressing issues where formation agents become stuck attempting
/// to follow ushers trapped within obstacles. It uses logic to redirect agents to
/// the main formation target or ushers, circumventing obstacles
/// and enabling smoother and more realistic navigation.
/// </summary>
public class FormationObstacleManager: MonoBehaviour
{
    [Header("CONFIGURATION:")]
    [Tooltip("Our agents radius.")]
    [SerializeField] private float obstacleDetectionRadius = 50.0f;
    [Tooltip("Layer where our obstacles are.")]
    [SerializeField] private LayerMask obstaclesLayers;
    [Tooltip("Time to let pass between checks for obstacles.")]
    [SerializeField] public float detectionCooldown = 0.5f;
    
    [Header("WIRING:")]
    [Tooltip("Steering behavior to go to target. It should be iTargeter compliant.")]
    [InterfaceCompliant(typeof(ITargeter))]
    [SerializeField] private SteeringBehavior iTargeterBehaviour;
    [Tooltip("Formation that generates and holds members.")]
    [InterfaceCompliant(typeof(IFormation))]
    [SerializeField] private MonoBehaviour iFormation;
    [Tooltip("Formation that generates and holds ushers.")]
    [InterfaceCompliant(typeof(IFormation))]
    [SerializeField] private MonoBehaviour iFormationUshers;
    
    private ITargeter _targeter;
    private IFormation _formation;
    private IFormation _formationUshers;
    private CleanAreaChecker _areaChecker;
    private Timer _detectionCooldownTimer;
    private bool _waitingForDetectionCooldownTimeout;
    HashSet<int> _formationPositionsInsideObstacles = new();

    private void Awake()
    {
        _areaChecker =
            new CleanAreaChecker(obstacleDetectionRadius, obstaclesLayers);
        SetTimer();
        _targeter = (ITargeter) iTargeterBehaviour;
        _formation = (IFormation) iFormation;
        _formationUshers = (IFormation) iFormationUshers;
    }

    private void SetTimer()
    {
        _detectionCooldownTimer = 
            new Timer(detectionCooldown * 1000);
        _detectionCooldownTimer.AutoReset = false;
        _detectionCooldownTimer.Elapsed += OnTimerTimeout;
    }
    
    private void StartDetectionCooldownTimer()
    {
        _waitingForDetectionCooldownTimeout = true;
        _detectionCooldownTimer.Stop();
        _detectionCooldownTimer.Start();
    }
    
    private void StopDetectionCooldownTimer()
    {
        _waitingForDetectionCooldownTimeout = false;
        _detectionCooldownTimer.Stop();
    }

    private void OnEnable()
    {
        _detectionCooldownTimer.Elapsed += OnTimerTimeout;
    }

    private void OnDisable()
    {
        _detectionCooldownTimer.Elapsed -= OnTimerTimeout;
    }

    private void OnTimerTimeout(object sender, ElapsedEventArgs e)
    {
        _waitingForDetectionCooldownTimeout = false;
    }

    /// <summary>
    /// Identifies the positions of the formation pattern offsets that are currently
    /// located inside obstacles. This method checks the spatial location of each
    /// formation pattern offset against detected obstacles in the environment and
    /// returns a collection of indices representing the offsets currently obstructed.
    /// </summary>
    /// <returns>
    /// A HashSet of integers where each integer corresponds to the index of a formation
    /// offset that is currently inside an obstacle.
    /// </returns>
    private HashSet<int> GetFormationPositionsInsideObstacles()
    {
        HashSet<int> formationPositionsInsideObstacles = new();
        for (int i = 0; i < _formationUshers.MemberPositions.Count; i++)
        {
            if (!_areaChecker.IsCleanArea(
                    transform.TransformPoint(
                        _formationUshers.MemberPositions[i])))
            {
                formationPositionsInsideObstacles.Add(i);
            }
        }
        return formationPositionsInsideObstacles;
    }

    /// <summary>
    /// Redirects the specified formation agents to target the main formation target.
    /// This method assigns the main formation target to the agents indicated by the
    /// provided indices, enabling them to navigate towards it while bypassing
    /// an obstructed path or ushers located inside obstacles.
    /// </summary>
    /// <param name="agentsToRedirect">
    /// A HashSet of integers where each integer corresponds to the index of a formation
    /// member that needs its target redirected to the main formation target.
    /// </param>
    private void RedirectAgentsToFormationTarget(HashSet<int> agentsToRedirect)
    {
        foreach (int agentIndex in agentsToRedirect)
        {
            ITargeter agentTargeter = 
                _formation.Members[agentIndex].GetComponentInChildren<ITargeter>();
            if (agentTargeter != null)
            {
                agentTargeter.Target = _targeter.Target;
            }
        }
    }

    /// <summary>
    /// Redirects specified agents in the formation to their respective ushers. This
    /// method updates the target of each agent in the provided set, directing them
    /// towards their designated usher, facilitating coordinated movement in scenarios
    /// where ushers become unobstructed and usual navigation can resume.
    /// </summary>
    /// <param name="agentsToRedirect">
    /// A collection of integers representing the indices of agents within the formation
    /// that need to be redirected to their respective ushers.
    /// </param>
    private void RedirectAgentsToUshers(HashSet<int> agentsToRedirect)
    {
        foreach (int agentIndex in agentsToRedirect)
        {
            ITargeter agentTargeter = 
                _formation.Members[agentIndex].GetComponentInChildren<ITargeter>();
            if (agentTargeter != null)
            {
                agentTargeter.Target = _formationUshers.Members[agentIndex];
            }
        }
    }

    private void FixedUpdate()
    {
        // We are using a shape cast to check for obstacles, and that is a heavy load. So,
        // we'd better not do it every frame. 
        if (_waitingForDetectionCooldownTimeout) return;
        
        // To avoid members trying to follow ushers while they are inside obstacles.
        // Those members are redirected to the main formation target. As soon as their 
        // respective ushers are outside the obstacle, they are redirected to ushers
        // again.
        HashSet<int> positionsInsideObstacles = GetFormationPositionsInsideObstacles();
        
        HashSet<int> positionsJustEnteredObstacles = 
            positionsInsideObstacles
                .Except(_formationPositionsInsideObstacles)
                .ToHashSet();
        HashSet<int> positionJustLeftObstacles =
            _formationPositionsInsideObstacles
                .Except(positionsInsideObstacles)
                .ToHashSet();
        
        _formationPositionsInsideObstacles = positionsInsideObstacles;
        
        if (positionsJustEnteredObstacles.Count > 0)
        {
            RedirectAgentsToFormationTarget(positionsJustEnteredObstacles);
        }
        
        if (positionJustLeftObstacles.Count > 0)
        {
            RedirectAgentsToUshers(positionJustLeftObstacles);
        }
        
        StartDetectionCooldownTimer();
    }
}
}