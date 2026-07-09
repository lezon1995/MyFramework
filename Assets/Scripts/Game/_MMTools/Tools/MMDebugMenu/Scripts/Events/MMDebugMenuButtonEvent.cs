using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// An event used to broadcast button events from a MMDebugMenu
    /// </summary>
    public struct MMDebugMenuButtonEvent
    {
        public enum EventModes
        {
            FromButton,
            SetButton
        }

        static event Delegate OnEvent;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialization()
        {
            OnEvent = null;
        }

        public static void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        public static void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        public delegate void Delegate(string buttonEventName, bool active = true, EventModes eventMode = EventModes.FromButton);

        public static void Trigger(string buttonEventName, bool active = true, EventModes eventMode = EventModes.FromButton)
        {
            OnEvent?.Invoke(buttonEventName, active, eventMode);
        }
    }
}