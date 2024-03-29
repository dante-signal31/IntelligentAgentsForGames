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
    
    /// <summary>
    /// Operator for adding two SteeringOutput objects.
    /// </summary>
    /// <param name="a">One steering output.</param>
    /// <param name="b">Another steering output.</param>
    /// <returns>Resulting steering output.</returns>
    public static SteeringOutput operator +(SteeringOutput a, SteeringOutput b)
    {
        return new SteeringOutput(a.Linear + b.Linear, a.Angular + b.Angular);
    }
    
    /// <summary>
    /// Operator for multiplying a SteeringOutput object and a float.
    /// </summary>
    /// <param name="a">One steering output.</param>
    /// <param name="b">A float to multiply.</param>
    /// <returns>Resulting steering output.</returns>
    public static SteeringOutput operator *(SteeringOutput a, float b)
    {
        return new SteeringOutput(a.Linear * b, a.Angular * b);
    }
}
