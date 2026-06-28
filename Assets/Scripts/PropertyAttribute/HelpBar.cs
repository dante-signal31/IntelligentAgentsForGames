namespace PropertyAttribute
{
/// <summary>
/// Decorate an inspector field with a help bar with a given message as text.
/// The help bar is preceded by an icon depending on the message type.
/// </summary>
public class HelpBar : UnityEngine.PropertyAttribute
{
    public string message;
    public MessageTypes.MessageType messageType;

    public HelpBar(string message, MessageTypes.MessageType type)
    {
        this.message = message;
        messageType = type;
    }
}
}