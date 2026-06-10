namespace Sensors
{
/// <summary>
/// An example of a class that can emit a sound signal like modality.
/// </summary>  
public class RegionSenseSoundSignalEmitter: 
    RegionSenseSignalEmitter<SoundModality, RegionSenseManager>
{
    protected override SoundModality GenerateModality()
    {
        return new SoundModality(
            ModalityMaximumRange, 
            ModalityAttenuation, 
            ModalityInverseTransmissionSpeed);
    }
}
}