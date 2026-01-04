namespace Pathfinding
{
/// <summary>
/// Specialized node record used in the A* pathfinding algorithm to store
/// detailed information on each node during the search process.
/// </summary>
/// <remarks>
/// This class extends the functionality of the base <see cref="NodeRecord"/>
/// by including an additional property for the total estimated cost to the target.
/// This property is essential in A* to prioritize nodes based on cost estimates
/// and guide the algorithm efficiently toward a solution.
/// </remarks>
public class AStarNodeRecord : NodeRecord
{
    public float totalEstimatedCostToTarget;
    
    public static readonly AStarNodeRecord aStarNodeRecordNull = new ()
    {
        node = null,
        connection = null,
        costSoFar = 0,
        totalEstimatedCostToTarget = float.MaxValue
    };
}
}