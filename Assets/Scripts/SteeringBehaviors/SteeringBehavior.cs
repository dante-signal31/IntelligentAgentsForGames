using UnityEngine;

public abstract class SteeringBehavior: MonoBehaviour
{
    /// <summary>
    /// Get new steering as a tuple of linear acceleration and angular one.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public abstract SteeringOutput GetSteering(SteeringBehaviorArgs args);
}
