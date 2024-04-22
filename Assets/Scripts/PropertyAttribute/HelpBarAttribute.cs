using UnityEngine;

namespace PropertyAttribute
{
    /// <summary>
    /// Decorate an inspector field with a help bar with given message as text.
    /// The help bar is preceded by an icon depending on the message type.
    /// </summary>
    public class HelpBarAttribute : UnityEngine.PropertyAttribute
    {
        public string Message;
        public MessageTypes.MessageType MessageType;

        public HelpBarAttribute(string message, MessageTypes.MessageType type)
        {
            Message = message;
            MessageType = type;
        }
        
    }
}