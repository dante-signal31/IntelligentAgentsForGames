using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// Pipeline targeter to make the agent look at a target.
/// </summary>
public class PipelineLookAtTargeter : MonoBehaviour, IPipelineTargeter
{
    [Header("CONFIGURATION:")]
    [SerializeField] public GameObject target;
    
    [Header("WIRING:")]
    [SerializeField] private LookAtSteeringBehavior lookAtSteeringBehavior;

    private void Start()
    {
        lookAtSteeringBehavior.Target = target;
    }

    public PipelineGoal GetGoal(SteeringBehaviorArgs args)
    {
        SteeringOutput steeringOutput = lookAtSteeringBehavior.GetSteering(args);
        
        PipelineGoal lookAtGoal = new()
        {
            Rotation = steeringOutput.Angular,
        };
        
        return lookAtGoal;
    }
}
}

