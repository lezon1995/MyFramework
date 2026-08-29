using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// A list of the methods available to change the current score
    /// </summary>
    public enum MMTimeScaleMethods
    {
        For,
        Reset,
        Unfreeze
    }

    /// <summary>
    /// The possible modes with which to interpolate time scale. Speed is a legacy mode, the recommended mode if you want to lerp timescale would be Duration, which offers the most options and accuracy
    /// </summary>
    public enum MMTimeScaleLerpModes
    {
        Speed,
        Duration,
        NoInterpolation
    }

    /// <summary>
    /// The different settings you can play with on a time scale event
    /// </summary>
    public struct TimeScaleProperties
    {
        public float TimeScale;
        public float Duration;
        public bool TimeScaleLerp;
        public float LerpSpeed;
        public bool Infinite;
        public MMTimeScaleLerpModes TimeScaleLerpMode;
        public MMTweenType TimeScaleLerpCurve;
        public float TimeScaleLerpDuration;
        public bool TimeScaleLerpOnReset;
        public MMTweenType TimeScaleLerpCurveOnReset;
        public float TimeScaleLerpDurationOnReset;
        public override string ToString() => $"REQUESTED ts={TimeScale} time={Duration} lerp={TimeScaleLerp} speed={LerpSpeed} keep={Infinite}";
    }

    public struct MMTimeScaleEvent
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

        public delegate void Delegate(MMTimeScaleMethods timeScaleMethod, float timeScale, float duration, bool lerp, float lerpSpeed, bool infinite,
            MMTimeScaleLerpModes timeScaleLerpMode = MMTimeScaleLerpModes.Speed, MMTweenType timeScaleLerpCurve = null, float timeScaleLerpDuration = 0.2f,
            bool timeScaleLerpOnReset = false, MMTweenType timeScaleLerpCurveOnReset = null, float timeScaleLerpDurationOnReset = 0.2f);

        public static void Trigger(MMTimeScaleMethods timeScaleMethod, float timeScale, float duration, bool lerp, float lerpSpeed, bool infinite,
            MMTimeScaleLerpModes timeScaleLerpMode = MMTimeScaleLerpModes.Speed, MMTweenType timeScaleLerpCurve = null, float timeScaleLerpDuration = 0.2f,
            bool timeScaleLerpOnReset = false, MMTweenType timeScaleLerpCurveOnReset = null, float timeScaleLerpDurationOnReset = 0.2f)
        {
            OnEvent?.Invoke(timeScaleMethod, timeScale, duration, lerp, lerpSpeed, infinite, timeScaleLerpMode, timeScaleLerpCurve, timeScaleLerpDuration, timeScaleLerpOnReset, timeScaleLerpCurveOnReset, timeScaleLerpDurationOnReset);
        }
    }

    public struct MMFreezeFrameEvent
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

        public delegate void Delegate(float duration);

        public static void Trigger(float duration)
        {
            OnEvent?.Invoke(duration);
        }
    }

    /// <summary>
    /// Put this component in your scene, and it'll catch MMFreezeFrameEvents and MMTimeScaleEvents, allowing you to control the flow of time.
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Various/MMTimeManager")]
    public class MMTimeManager : MMSingleton<MMTimeManager>
    {
        [Header("Default Values")]
        [Tooltip("The reference time scale, to which the system will go back to after all time is changed")]
        public float NormalTimeScale = 1f;

        [Header("Impacted Values")]
        [Tooltip("whether or not to update Time.timeScale when changing time scale")]
        public bool UpdateTimescale = true;

        [Tooltip("whether or not to update Time.fixedDeltaTime when changing time scale")]
        public bool UpdateFixedDeltaTime = true;

        [Tooltip("whether or not to update Time.maximumDeltaTime when changing time scale")]
        public bool UpdateMaximumDeltaTime = true;

        [Header("Debug")]
        [Tooltip("the current, real time, time scale")]
        [MMReadOnly]
        public float CurrentTimeScale = 1f;

        [Tooltip("the time scale the system is lerping towards")]
        [MMReadOnly]
        public float TargetTimeScale = 1f;

        [MMInspectorButton("TestButtonToSlowDownTime")]
        public bool TestButton;

        protected Stack<TimeScaleProperties> _timeScaleProperties = new();
        protected TimeScaleProperties _currentProperty;
        protected TimeScaleProperties _resetProperty;
        protected float _initialFixedDeltaTime;
        protected float _initialMaximumDeltaTime;
        protected float _startedAt;
        protected bool _lerpingBackToNormal;
        protected float _timeScaleLastTime = float.NegativeInfinity;

        /// <summary>
        /// Statics initialization to support enter play modes
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        protected static void InitializeStatics()
        {
            _instance = null;
        }

        /// <summary>
        /// A method used from the inspector to test the system
        /// </summary>
        protected virtual void TestButtonToSlowDownTime()
        {
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0.5f, 3f, true, 1f, false);
        }

        /// <summary>
        /// On start we initialize our stack 
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            PreInitialization();
        }

        /// <summary>
        /// We initialize our stack
        /// </summary>
        public virtual void PreInitialization()
        {
            TargetTimeScale = NormalTimeScale;
            _initialFixedDeltaTime = Time.fixedDeltaTime;
            _initialMaximumDeltaTime = Time.maximumDeltaTime;
            ApplyTimeScale(NormalTimeScale);
        }

        /// <summary>
        /// On Start we apply our timescale
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
        }

        /// <summary>
        /// On init we store our initial timescales and apply the normal timescale
        /// </summary>
        public virtual void Initialization()
        {
        }

        /// <summary>
        /// On Update, applies the time scale and resets it if needed
        /// </summary>
        protected virtual void Update()
        {
            // if we have things in our stack, we handle them, otherwise we reset to the normal time scale
            while (_timeScaleProperties.Count > 0)
            {
                _currentProperty = _timeScaleProperties.Peek();
                TargetTimeScale = _currentProperty.TimeScale;
                _currentProperty.Duration -= Time.unscaledDeltaTime;

                _timeScaleProperties.Pop();
                _timeScaleProperties.Push(_currentProperty);

                if (_currentProperty.Duration > 0f || _currentProperty.Infinite)
                    break; // keep current property values

                Unfreeze(); // pop current property
            }

            if (_timeScaleProperties.Count == 0)
            {
                TargetTimeScale = NormalTimeScale;
            }

            // we apply our time scale
            if (_currentProperty.TimeScaleLerp)
            {
                switch (_currentProperty.TimeScaleLerpMode)
                {
                    case MMTimeScaleLerpModes.Speed:
                        if (_currentProperty.LerpSpeed <= 0)
                        {
                            _currentProperty.LerpSpeed = 1;
                        }

                        ApplyTimeScale(Mathf.Lerp(Time.timeScale, TargetTimeScale, Time.unscaledDeltaTime * _currentProperty.LerpSpeed));
                        break;
                    case MMTimeScaleLerpModes.Duration:
                        float timeSinceStart = Time.unscaledTime - _startedAt;
                        float progress = MMMaths.Remap(timeSinceStart, 0f, _currentProperty.TimeScaleLerpDuration, 0f, 1f);
                        float delta = _currentProperty.TimeScaleLerpCurve.Evaluate(progress);
                        ApplyTimeScale(Mathf.Lerp(Time.timeScale, TargetTimeScale, delta));
                        if (timeSinceStart > _currentProperty.TimeScaleLerpDuration)
                        {
                            ApplyTimeScale(TargetTimeScale);
                            if (_lerpingBackToNormal)
                            {
                                _lerpingBackToNormal = false;
                                _timeScaleProperties.Pop();
                            }
                        }

                        break;
                }
            }
            else
            {
                ApplyTimeScale(TargetTimeScale);
            }
        }

        /// <summary>
        /// Modifies the timescale and time attributes to match the new timescale
        /// </summary>
        /// <param name="newValue"></param>
        protected virtual void ApplyTimeScale(float newValue)
        {
            // if the new timescale is the same as last time, we don't bother updating it
            if (newValue == _timeScaleLastTime)
                return;

            if (UpdateTimescale)
                Time.timeScale = newValue;

            if (UpdateFixedDeltaTime && newValue != 0)
                Time.fixedDeltaTime = _initialFixedDeltaTime * newValue;

            if (UpdateMaximumDeltaTime)
                Time.maximumDeltaTime = _initialMaximumDeltaTime * newValue;

            CurrentTimeScale = Time.timeScale;
            _timeScaleLastTime = CurrentTimeScale;
        }

        /// <summary>
        /// Resets all stacked time scale changes and simply sets the time scale, until further changes
        /// </summary>
        /// <param name="newTimeScale">New time scale.</param>
        protected virtual void SetTimeScale(float newTimeScale)
        {
            _timeScaleProperties.Clear();
            ApplyTimeScale(newTimeScale);
        }

        /// <summary>
        /// Sets the time scale for the specified properties (duration, time scale, lerp or not, and lerp speed)
        /// </summary>
        /// <param name="property">Time scale properties.</param>
        protected virtual void SetTimeScale(TimeScaleProperties property)
        {
            if (property is { TimeScaleLerp: true, TimeScaleLerpMode: MMTimeScaleLerpModes.Duration })
            {
                property.Duration = Mathf.Max(property.Duration, property.TimeScaleLerpDuration);
                property.Duration = Mathf.Max(property.Duration, property.TimeScaleLerpDurationOnReset);
            }

            _startedAt = Time.unscaledTime;
            _timeScaleProperties.Push(property);
        }

        /// <summary>
        /// Resets the time scale to the stored normal time scale
        /// </summary>
        protected virtual void ResetTimeScale()
        {
            SetTimeScale(NormalTimeScale);
        }

        /// <summary>
        /// Resets the time scale to the last saved time scale.
        /// </summary>
        protected virtual void Unfreeze()
        {
            if (_timeScaleProperties.Count > 0)
            {
                _resetProperty = _timeScaleProperties.Peek();
                _timeScaleProperties.Pop();
            }

            if (_timeScaleProperties.Count == 0)
            {
                if (_resetProperty.TimeScaleLerp && _resetProperty is
                    {
                        TimeScaleLerpMode: MMTimeScaleLerpModes.Duration, 
                        TimeScaleLerpOnReset: true
                    })
                {
                    _lerpingBackToNormal = true;
                    MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, NormalTimeScale, _resetProperty.TimeScaleLerpDuration, _resetProperty.TimeScaleLerp,
                        _resetProperty.LerpSpeed, true, MMTimeScaleLerpModes.Duration, _resetProperty.TimeScaleLerpCurveOnReset, _resetProperty.TimeScaleLerpDurationOnReset);
                }
                else
                {
                    ResetTimeScale();
                }
            }
        }

        /// <summary>
        /// Sets the time scale to the specified value, instantly
        /// </summary>
        /// <param name="newNormalTimeScale">New normal time scale.</param>
        public virtual void SetTimeScaleTo(float newNormalTimeScale)
        {
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, newNormalTimeScale, 0f, false, 0f, true);
        }

        /// <summary>
        /// Catches TimeScaleEvents and acts on them
        /// </summary>
        /// <param name="timeScaleEvent">MMTimeScaleEvent event.</param>
        public virtual void OnTimeScaleEvent(MMTimeScaleMethods timeScaleMethod, float timeScale, float duration, bool lerp, float lerpSpeed, bool infinite,
            MMTimeScaleLerpModes timeScaleLerpMode = MMTimeScaleLerpModes.Speed, MMTweenType timeScaleLerpCurve = null, float timeScaleLerpDuration = 0.2f,
            bool timeScaleLerpOnReset = false, MMTweenType timeScaleLerpCurveOnReset = null, float timeScaleLerpDurationOnReset = 0.2f)
        {
            var property = new TimeScaleProperties
            {
                TimeScale = timeScale,
                Duration = duration,
                TimeScaleLerp = lerp,
                LerpSpeed = lerpSpeed,
                Infinite = infinite,
                TimeScaleLerpOnReset = timeScaleLerpOnReset,
                TimeScaleLerpCurveOnReset = timeScaleLerpCurveOnReset,
                TimeScaleLerpDurationOnReset = timeScaleLerpDurationOnReset,
                TimeScaleLerpMode = timeScaleLerpMode,
                TimeScaleLerpCurve = timeScaleLerpCurve,
                TimeScaleLerpDuration = timeScaleLerpDuration
            };

            switch (timeScaleMethod)
            {
                case MMTimeScaleMethods.Reset:
                    ResetTimeScale();
                    break;

                case MMTimeScaleMethods.For:
                    SetTimeScale(property);
                    break;

                case MMTimeScaleMethods.Unfreeze:
                    Unfreeze();
                    break;
            }
        }

        /// <summary>
        /// When getting a freeze frame event we stop the time
        /// </summary>
        /// <param name="freezeFrameEvent">Freeze frame event.</param>
        public virtual void OnMMFreezeFrameEvent(float duration)
        {
            TimeScaleProperties properties = new TimeScaleProperties
            {
                Duration = duration,
                TimeScaleLerp = false,
                LerpSpeed = 0f,
                TimeScale = 0f
            };
            SetTimeScale(properties);
        }

        /// <summary>
        /// On enable, starts listening for FreezeFrame events
        /// </summary>
        void OnEnable()
        {
            MMFreezeFrameEvent.Register(OnMMFreezeFrameEvent);
            MMTimeScaleEvent.Register(OnTimeScaleEvent);
        }

        /// <summary>
        /// On disable, stops listening for FreezeFrame events
        /// </summary>
        void OnDisable()
        {
            MMFreezeFrameEvent.Unregister(OnMMFreezeFrameEvent);
            MMTimeScaleEvent.Unregister(OnTimeScaleEvent);
        }
    }
}