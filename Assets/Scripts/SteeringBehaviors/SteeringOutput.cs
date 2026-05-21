using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// Velocities to move agent.
/// </summary>
public class SteeringOutput
{
    /// <summary>
    /// Zero steering output.
    /// </summary>
    public static readonly SteeringOutput zero = new();
    
    /// <summary>
    /// Velocity isn't set.
    /// </summary>
    public static readonly Vector2? linearUnset = null;
    
    /// <summary>
    /// Angular speed isn't set.
    /// </summary>
    public static readonly float angularUnset = Mathf.NegativeInfinity;

    /// <summary>
    /// Linear velocity vector.
    /// </summary>
    public Vector2 Linear { get; }

    /// <summary>
    /// True if the linear velocity has been explicitly set.
    /// </summary>
    public bool IsLinearSet { get; }

    /// <summary>
    /// Angular rotation speed in degrees. In Unity's 2D, a positive rotation means an
    /// anticlockwise rotation.
    /// </summary>
    public float Angular { get; }

    /// <summary>
    /// True if the angular velocity has been explicitly set.
    /// </summary>
    public bool IsAngularSet { get; }

    /// <summary>
    /// Default constructor for a zero steering output.
    /// </summary>
    public SteeringOutput()
    {
       IsLinearSet = false;
       Linear = Vector2.negativeInfinity;
       IsAngularSet = false;
       Angular = Mathf.NegativeInfinity;
    }
    
    /// <summary>
    /// Constructor for a not zero steering output.
    /// </summary>
    /// <param name="linear">Linear velocity.</param>
    /// <param name="angular">Angular speed.</param>
    public SteeringOutput(Vector2? linear = null, float angular=Mathf.NegativeInfinity)
    {
        IsLinearSet = linear != null;
        Linear = linear ?? Vector2.negativeInfinity;
        IsAngularSet = angular > Mathf.NegativeInfinity;
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
        Vector2? linear = linearUnset;
        if (a.IsLinearSet && b.IsLinearSet) 
            linear = a.Linear + b.Linear;
        if (a.IsLinearSet && !b.IsLinearSet) 
            linear = a.Linear;
        if (!a.IsLinearSet && b.IsLinearSet) 
            linear = b.Linear;
        
        float angular = angularUnset;
        if (a.IsAngularSet && b.IsAngularSet) 
            angular = a.Angular + b.Angular;
        if (a.IsAngularSet && !b.IsAngularSet) 
            angular = a.Angular;
        if (!a.IsAngularSet && b.IsAngularSet) 
            angular = b.Angular;
        
        return new SteeringOutput(linear, angular);
    }

    /// <summary>
    /// Operator for multiplying a SteeringOutput object and a float.
    /// </summary>
    /// <param name="a">One steering output.</param>
    /// <param name="b">A float to multiply.</param>
    /// <returns>Resulting steering output.</returns>
    public static SteeringOutput operator *(SteeringOutput a, float b)
    {
        Vector2? linear = linearUnset;
        if (a.IsLinearSet) linear = a.Linear * b;
        
        float angular = angularUnset;
        if (a.IsAngularSet) angular = a.Angular * b;
        
        return new SteeringOutput(linear, angular);
    }

    /// <summary>
    /// Equality comparison with another object.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool Equals(object other)
    {
        if (other is SteeringOutput output)
        {
            bool linearEquality = IsLinearSet && output.IsLinearSet? // Both set?
                                  Linear == output.Linear: // then compare.
                                  IsLinearSet == output.IsLinearSet; // Otherwise, if both
                                                                     // are unset, then
                                                                     // they are equal.
            bool angularEquality = IsAngularSet && output.IsAngularSet? 
                                   Mathf.Approximately(Angular, output.Angular):
                                   IsAngularSet == output.IsAngularSet;
            return linearEquality && angularEquality;
        }

        return false;
    }
    
    /// <summary>
    /// Equality operator for comparing two SteeringOutput objects.
    /// </summary>
    /// <param name="a">The first steering output.</param>
    /// <param name="b">The second steering output.</param>
    /// <returns>True if both steering outputs are equal.</returns>
    public static bool operator ==(SteeringOutput a, SteeringOutput b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    /// <summary>
    /// Inequality operator for comparing two SteeringOutput objects.
    /// </summary>
    /// <param name="a">The first steering output.</param>
    /// <param name="b">The second steering output.</param>
    /// <returns>True if both steering outputs are not equal.</returns>
    public static bool operator !=(SteeringOutput a, SteeringOutput b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return Linear.GetHashCode() ^ Angular.GetHashCode();
    }
}
}
