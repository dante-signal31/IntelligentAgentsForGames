namespace PropertyAttribute
{
/// <summary>
/// <p>Decorate an inspector field to show an alert message bar if the decorated property
/// does not comply with any of the given interfaces.</p>
/// </summary>
public class InterfaceCompliantAttribute: UnityEngine.PropertyAttribute
{
    public readonly System.Type[] InterfaceTypes;

    public InterfaceCompliantAttribute(params System.Type[] interfaceTypes)
    {
        InterfaceTypes = interfaceTypes;
    }
}
}