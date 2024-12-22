using UnityEngine;

/// <summary>
/// <p>Basic template for a steering behavior.</p>
///
/// <p>It should be an interface, but Unity does not allow to use interfaces in
/// inspector fields. So, I've used an abstract class to be able to pass its
/// implementations through inspector fields.</p>
/// </summary>
public abstract class SteeringBehavior: MonoBehaviour
{
    /// <summary>
    /// Get new steering as an object with linear velocity and rotation.
    /// </summary>
    /// <param name="args">Current agent state.</param>
    /// <returns>An object with new linear velocity and rotation as properties.</returns>
    public abstract SteeringOutput GetSteering(SteeringBehaviorArgs args);
}
