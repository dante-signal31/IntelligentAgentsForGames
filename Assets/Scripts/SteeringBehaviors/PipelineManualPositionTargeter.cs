using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// Pipeline targeter to set the position channel as a manual position set with a Target
/// node.
/// </summary>
public class PipelineManualPositionTargeter: MonoBehaviour, IPipelineTargeter
{
    [Header("CONFIGURATION:")]
    [SerializeField] public GameObject target;
    
    public PipelineGoal GetGoal(SteeringBehaviorArgs args)
    {
        if (target == null) return null;
        PipelineGoal goal = new()
        {
            Position = target.transform.position,
        };
        return goal;
    }
}
}