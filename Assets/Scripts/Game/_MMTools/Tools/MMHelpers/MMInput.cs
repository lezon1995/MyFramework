using System;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Input helpers
    /// </summary>
    public class MMInput : MonoBehaviour
    {
        /// <summary>
        /// All possible states for a button. Can be used in a state machine.
        /// </summary>
        public enum States
        {
            Off,
            Down,
            Pressed,
            Up
        }

        public enum AxisTypes
        {
            Positive,
            Negative
        }

        /// <summary>
        /// Takes an axis and returns a ButtonState depending on whether the axis is pressed or not (useful for xbox triggers for example),
        /// and when you need to use an axis/trigger as a binary thing
        /// </summary>
        /// <returns>The axis as button.</returns>
        /// <param name="axisName">Axis name.</param>
        /// <param name="threshold">Threshold value below which the button is off or released.</param>
        /// <param name="currentState">Current state of the axis.</param>
        /// <param name="axisType"></param>
        public static States ProcessAxisAsButton(string axisName, float threshold, States currentState, AxisTypes axisType = AxisTypes.Positive)
        {
            float axisValue = Input.GetAxis(axisName);
            States returnState;
            var comparison = axisType == AxisTypes.Positive ? axisValue < threshold : axisValue > threshold;

            if (comparison)
                returnState = currentState == States.Pressed ? States.Up : States.Off;
            else
                returnState = currentState == States.Off ? States.Down : States.Pressed;

            return returnState;
        }

        /// <summary>
        /// IM button, short for InputManager button, a class used to handle button states, whether mobile or actual keys
        /// </summary>
        public sealed class IMButton
        {
            /// a state machine used to store button states
            public MMStateMachine<States> State { get; }

            /// the unique ID of this button
            public string ButtonID;

            public event Action OnDown;
            public event Action OnPressed;
            public event Action OnUp;

            // returns the time (in unscaled seconds) since the last time the button was pressed down
            public float TimeSinceLastButtonDown => Time.unscaledTime - _lastButtonDownAt;

            // returns the time (in unscaled seconds) since the last time the button was released
            public float TimeSinceLastButtonUp => Time.unscaledTime - _lastButtonUpAt;

            // returns true if this button was pressed down within the time (in unscaled seconds) passed in parameters
            public bool ButtonDownRecently(float time) => TimeSinceLastButtonDown <= time;

            // returns true if this button was released within the time (in unscaled seconds) passed in parameters
            public bool ButtonUpRecently(float time) => TimeSinceLastButtonUp <= time;

            float _lastButtonDownAt;
            float _lastButtonUpAt;

            public IMButton(string playerID, string buttonID, Action onDown = null, Action onPressed = null, Action onUp = null)
                : this($"{playerID}_{buttonID}", onDown, onPressed, onUp)
            {
            }

            public IMButton(string buttonID, Action onDown = null, Action onPressed = null, Action onUp = null)
            {
                ButtonID = buttonID;
                OnDown = onDown;
                OnPressed = onPressed;
                OnUp = onUp;
                State = new();
                State.ChangeState(States.Off);
            }

            public bool IsPressed() => State.Is(States.Pressed);
            public bool IsDown() => State.Is(States.Down);
            public bool IsUp() => State.Is(States.Up);
            public bool IsOff() => State.Is(States.Off);

            /// <summary>
            /// Presses the button for the first time, putting it in ButtonDown state
            /// </summary>
            public void TriggerButtonDown()
            {
                _lastButtonDownAt = Time.unscaledTime;
                if (OnDown == null)
                    State.ChangeState(States.Down);
                else
                    OnDown?.Invoke();
            }

            /// <summary>
            /// Puts the button in the Pressed state, potentially bypassing the Down state
            /// </summary>
            public void TriggerButtonPressed()
            {
                if (OnPressed == null)
                    State.ChangeState(States.Pressed);
                else
                    OnPressed?.Invoke();
            }

            /// <summary>
            /// Puts the button in the Up state
            /// </summary>
            public void TriggerButtonUp()
            {
                _lastButtonUpAt = Time.unscaledTime;
                if (OnUp == null)
                    State.ChangeState(States.Up);
                else
                    OnUp?.Invoke();
            }
        }
    }

    public static class MMInputExtensions
    {
        public static bool IsOff(this MMInput.States states) => states == MMInput.States.Off;
        public static bool IsDown(this MMInput.States states) => states == MMInput.States.Down;
        public static bool IsPressed(this MMInput.States states) => states == MMInput.States.Pressed;
        public static bool IsUp(this MMInput.States states) => states == MMInput.States.Up;
    }
}