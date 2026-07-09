using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// An event used to broadcast slider events from a MMDebugMenu
    /// </summary>
    public struct MMDebugMenuSliderEvent
    {
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

        public enum EventModes
        {
            FromSlider,
            SetSlider
        }

        public delegate void Delegate(string sliderEventName, float value, EventModes eventMode = EventModes.FromSlider);

        public static void Trigger(string sliderEventName, float value, EventModes eventMode = EventModes.FromSlider)
        {
            OnEvent?.Invoke(sliderEventName, value, eventMode);
        }
    }
}