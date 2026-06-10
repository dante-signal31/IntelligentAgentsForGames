using System.Collections.Generic;

namespace Sensors
{
/// <summary>
/// Interface for those sensor capable of detecting signals through a sense manager.
/// </summary>
public interface ISignalSensor
{
    /// <summary>
    /// Priority queue of detected signals, sorted by signal strength. The strongest
    /// signal, the first.
    /// </summary>
    public SortedSet<DetectedSignal> DetectedSignals { get; }
}
}