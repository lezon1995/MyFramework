using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    public class MMInformationAttribute : PropertyAttribute
    {
        public enum InformationType
        {
            Error,
            Info,
            None,
            Warning
        }

#if UNITY_EDITOR
        public string Message;
        public MessageType Type;
        public bool MessageAfterProperty;

        public MMInformationAttribute(string message, InformationType type = InformationType.Info, bool messageAfterProperty = false)
        {
            Message = message;
            Type = type switch
            {
                InformationType.Error => MessageType.Error,
                InformationType.Info => MessageType.Info,
                InformationType.Warning => MessageType.Warning,
                InformationType.None => MessageType.None,
                _ => Type
            };

            MessageAfterProperty = messageAfterProperty;
        }
#else
		public MMInformationAttribute(string message, InformationType type, bool messageAfterProperty)
		{

		}
#endif
    }
}