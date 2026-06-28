namespace PropertyAttribute
{
/// <summary>
/// <p>Decorate an inspector field to show an alert message bar if the decorated property
/// does not comply with any of the given interfaces.</p>
/// </summary>
public class InterfaceCompliant: UnityEngine.PropertyAttribute
{
    public readonly System.Type[] interfaceTypes;

    public InterfaceCompliant(params System.Type[] interfaceTypes)
    {
        this.interfaceTypes = interfaceTypes;
    }
}
}