namespace Sensors
{
/// <summary>
/// Modality for smell signals.
/// </summary>
public class SmellModality: RegionSenseModality
{
    
    public SmellModality(
        float maximumRange,
        float attenuation,
        float inverseTransmissionSpeed) :
        base(maximumRange, attenuation, inverseTransmissionSpeed)
    { }

    public override bool ExtraChecks(RegionSenseSignal signal, IRegionSenseSensor sensor)
    {
        // We don't need to do any extra checks for smell modality.
        return true;
    }
}
}