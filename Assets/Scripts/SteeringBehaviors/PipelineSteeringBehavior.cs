using System.Timers;
using PropertyAttribute;
using UnityEngine;
using UnityEngine.Serialization;

namespace SteeringBehaviors
{
/// <summary>
/// Represents a pipeline-based steering behavior that combines multiple
/// elements to compute a SteeringOutput. The behavior involves targeting,
/// goal decomposition, constraint enforcement, and goal execution through an actuator.
/// </summary>
public class PipelineSteeringBehavior : SteeringBehavior
{
    [FormerlySerializedAs("AvoidanceTimeout")]
    [Header("CONFIGURATION:")] 
    [SerializeField] public float avoidanceTimeout = 0.5f;

    [Header("WIRING:")]
    [SerializeField] private PipelineTargeterAggregator targeterAggregator;
    [SerializeField] private PipelineDecomposerAggregator decomposerAggregator;
    [SerializeField] private PipelineConstraintsAggregator constraintsAggregator;
    [InterfaceCompliant(typeof(IPipelineActuator))]
    [SerializeField] private MonoBehaviour actuatorBehavior;
    [SerializeField] private SteeringBehavior deadlockSteeringBehavior;

    private IPipelineActuator actuator;
    private Timer _cooldownTimer;
    private bool _waitingForCooldownTimeout;
    private SteeringOutput _currentSteering;

    private void Start()
    {
        actuator = (IPipelineActuator) actuatorBehavior;
        
        _cooldownTimer = new Timer(avoidanceTimeout * 1000);
        _cooldownTimer.AutoReset = false;
        _cooldownTimer.Elapsed += OnCooldownTimeout;
    }

    private void OnCooldownTimeout(object sender, ElapsedEventArgs elapsedEventArgs)
    {
        _waitingForCooldownTimeout = false;
    }

    private void StartCooldownTimer()
    {
        _cooldownTimer.Stop();
        _cooldownTimer.Start();
        _waitingForCooldownTimeout = true;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (_waitingForCooldownTimeout) return _currentSteering;
    
        // First, we ask the targeters to get a high-level goal.
        PipelineGoal targeterGoals = targeterAggregator.GetAggregatedGoal(args);
    
        // Divide and win. We ask the decomposer to decompose the high-level goal into
        // smaller, simpler to get goals.
        PipelineGoal partialGoal = decomposerAggregator.DecomposeGoal(
            targeterGoals, 
            args);
    
        // Check if the subgoal violates any constraint. If that's the case, the
        // constraints will try to calculate a new subgoal, the similar as possible to
        // the original one, but that does not violate the constraint. If no new
        // subgoal could be found that complies with every constraint, then we return
        // a null goal.
        PipelineGoal constrainedGoal = constraintsAggregator.ApplyConstraints(
            partialGoal,
            args,
            actuator);
    
        if (constrainedGoal == null)
            // If we get here, then it means no meaningful goal was found.
            // So, we fall back to a deadlock-steering behavior.
            return deadlockSteeringBehavior.GetSteering(args);
    
        // If a subgoal could be calculated, then we ask the actuator to get the best
        // steering output to achieve the goal.
        _currentSteering = actuator.GetOutput(constrainedGoal, args);
        StartCooldownTimer();
        return _currentSteering;
    }
}
}

