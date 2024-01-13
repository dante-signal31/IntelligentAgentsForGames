using UnityEngine;

/// <summary>
/// Accelerations to move agent.
/// </summary>
public class SteeringOutput
{
    /// <summary>
    /// Linear acceleration vector.
    /// </summary>
    public Vector2 Linear { get; }
    
    /// <summary>
    /// Angular rotation.
    /// </summary>
    public float Angular { get; }

    public SteeringOutput(Vector2 linear=new Vector2(), float angular=0)
    {
        Linear = linear;
        Angular = angular;
    }
}
