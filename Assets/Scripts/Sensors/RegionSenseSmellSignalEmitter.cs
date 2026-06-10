using UnityEngine;

namespace Sensors
{
/// <summary>
/// Represents a signal emitter that uses the smell sense modality.
/// </summary>
public class RegionSenseSmellSignalEmitter: 
    RegionSenseSignalEmitter<SmellModality, FEMSenseManager>
{
    protected override SmellModality GenerateModality()
    {
        return new SmellModality(
            ModalityMaximumRange, 
            ModalityAttenuation, 
            ModalityInverseTransmissionSpeed);
    }
}
}