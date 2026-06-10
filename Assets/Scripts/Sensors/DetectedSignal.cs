using System;

namespace Sensors
{
/// <summary>
/// Represents a signal detected by a sensor through a sense manager.
/// </summary>
public struct DetectedSignal
{
    public RegionSenseSignal signal;
    public DateTimeOffset detectionTimeStamp;
}
}