namespace Sensors
{
/// <summary>
/// An example of a sensor that can detect smell signal like modality.
/// </summary>
public partial class RegionSenseSmellSensor: 
    RegionSenseSensor<SmellModality, FEMSenseManager>
{ }
}