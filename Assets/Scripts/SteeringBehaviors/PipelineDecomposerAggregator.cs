using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// Pass the targeter goal sequentially through a series of decomposers. Every decomposer
/// will be evaluated in turn. The subgoal generated from a decomposer will be used as
/// input for the next decomposer. This way, the given gih level goal can be decomposed
/// into a smaller, and easily achievable, subgoal.
/// </summary>
/// <remarks>
/// The <c>PipelineDecomposerAggregator</c> works with child nodes that implement
/// the <c>IPipelineDecomposer</c> interface.
/// </remarks>
public class PipelineDecomposerAggregator: MonoBehaviour
{
    private IPipelineDecomposer[] _pipelineDecomposers;

    private void Start()
    {
        _pipelineDecomposers = GetComponentsInChildren<IPipelineDecomposer>();
    }

    public PipelineGoal DecomposeGoal(PipelineGoal goal, SteeringBehaviorArgs args)
    {
        // goal is passed by reference, so I allocate a new one to not overwrite the
        // global goal.
        PipelineGoal partialGoal = goal.GetGoalCopy();
        
        foreach (IPipelineDecomposer decomposer in _pipelineDecomposers)
        {
            partialGoal = decomposer.Decompose(partialGoal, args);
        }
        
        return partialGoal;
    }
}
}