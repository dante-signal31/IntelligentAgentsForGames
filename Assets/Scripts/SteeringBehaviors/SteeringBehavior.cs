using UnityEngine;

/// <summary>
/// Basic template for a steering behavior.
///
/// It could be an interface, but Unity does not allow to use interfaces in
/// inspector fields.
/// </summary>
public abstract class SteeringBehavior: MonoBehaviour
{
    /// <summary>
    /// Get new steering as a tuple of linear velocity and angular one.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public abstract SteeringOutput GetSteering(SteeringBehaviorArgs args);
}
