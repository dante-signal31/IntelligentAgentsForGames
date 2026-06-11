using UnityEngine;

namespace Sensors
{
/// <summary>
/// A signal is a message that indicated that something has occurred in the game level.
/// E.g., a sound has been emitted.
/// </summary>
public struct RegionSenseSignal
{
    /// <summary>
    /// Signal strength.
    /// </summary>
    public float strength;
    
    /// <summary>
    /// Position of the signal emission.
    /// </summary>
    public Vector2 emissionPosition;
    
    /// <summary>
    /// Signal source.
    /// </summary>
    public GameObject source;
    
    /// <summary>
    /// Unique identifier of the modality this signal is based on.
    /// </summary>
    public RegionSenseModality modality;
}
}