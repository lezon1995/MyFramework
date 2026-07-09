using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// A base class, meant to be extended, defining a Feedback. A Feedback is an action triggered by a MMFeedbacks, usually in reaction to the player's input or actions,
    /// to help communicate both emotion and legibility, improving game feel.
    /// To create a new feedback, extend this class and override its Custom methods, declared at the end of this class. You can look at the many examples for reference.
    /// </summary>
    
    [System.Serializable]
    [ExecuteAlways]
    public abstract class MMFeedback : MonoBehaviour
    {
        /// whether or not this feedback is active
        [Tooltip("whether or not this feedback is active")]
        public bool Active = true;

        /// the name of this feedback to display in the inspector
        [Tooltip("the name of this feedback to display in the inspector")]
        public string Label = "MMFeedback";

        /// the chance of this feedback happening (in percent : 100 : happens all the time, 0 : never happens, 50 : happens once every two calls, etc)
        [Tooltip("the chance of this feedback happening (in percent : 100 : happens all the time, 0 : never happens, 50 : happens once every two calls, etc)")]
        [Range(0, 100)]
        public float Chance = 100f;

        /// a number of timing-related values (delay, repeat, etc)
        [FormerlySerializedAs("Timing")]
        [Tooltip("a number of timing-related values (delay, repeat, etc)")]
        public MMFeedbackTiming Time;

        /// the Owner of the feedback, as defined when calling the Initialization method
        public GameObject Owner { get; set; }

        [HideInInspector]
        /// whether or not this feedback is in debug mode
        public bool DebugActive;

        /// set this to true if your feedback should pause the execution of the feedback sequence
        public virtual IEnumerator<float> Pause
        {
            get { return null; }
        }

        /// if this is true, this feedback will wait until all previous feedbacks have run
        public virtual bool HoldingPause
        {
            get { return false; }
        }

        /// if this is true, this feedback will wait until all previous feedbacks have run, then run all previous feedbacks again
        public virtual bool LooperPause
        {
            get { return false; }
        }

        /// if this is true, this feedback will pause and wait until Resume() is called on its parent MMFeedbacks to resume execution
        public virtual bool ScriptDrivenPause { get; set; }

        /// if this is a positive value, the feedback will auto resume after that duration if it hasn't been resumed via script already
        public virtual float ScriptDrivenPauseAutoResume { get; set; }

        /// if this is true, this feedback will wait until all previous feedbacks have run, then run all previous feedbacks again
        public virtual bool LooperStart
        {
            get { return false; }
        }

        /// an overridable color for your feedback, that can be redefined per feedback. White is the only reserved color, and the feedback will revert to 
        /// normal (light or dark skin) when left to White
#if UNITY_EDITOR
        public virtual Color FeedbackColor
        {
            get { return Color.white; }
        }
#endif
        /// returns true if this feedback is in cooldown at this time (and thus can't play), false otherwise
        public virtual bool InCooldown
        {
            get { return (Time.CooldownDuration > 0f) && (FeedbackTime - _lastPlayTimestamp < Time.CooldownDuration); }
        }

        /// if this is true, this feedback is currently playing
        public virtual bool IsPlaying { get; set; }

        /// the time (or unscaled time) based on the selected Timing settings
        public float FeedbackTime
        {
            get
            {
                if (Time.TimescaleMode == TimescaleModes.Scaled)
                {
                    return UnityEngine.Time.time;
                }
                else
                {
                    return UnityEngine.Time.unscaledTime;
                }
            }
        }

        /// the delta time (or unscaled delta time) based on the selected Timing settings
        public float FeedbackDeltaTime
        {
            get
            {
                if (Time.TimescaleMode == TimescaleModes.Scaled)
                {
                    return UnityEngine.Time.deltaTime;
                }
                else
                {
                    return UnityEngine.Time.unscaledDeltaTime;
                }
            }
        }


        /// <summary>
        /// The total duration of this feedback :
        /// total = initial delay + duration * (number of repeats + delay between repeats)  
        /// </summary>
        public float TotalDuration
        {
            get
            {
                if ((Time != null) && (!Time.ContributeToTotalDuration))
                {
                    return 0f;
                }

                float totalTime = 0f;

                if (Time == null)
                {
                    return 0f;
                }

                if (Time.InitialDelay != 0)
                {
                    totalTime += ApplyTimeMultiplier(Time.InitialDelay);
                }

                totalTime += FeedbackDuration;

                if (Time.NumberOfRepeats > 0)
                {
                    float delayBetweenRepeats = ApplyTimeMultiplier(Time.DelayBetweenRepeats);

                    totalTime += (Time.NumberOfRepeats * FeedbackDuration) + (Time.NumberOfRepeats * delayBetweenRepeats);
                }

                return totalTime;
            }
        }

        // the timestamp at which this feedback was last played
        public virtual float FeedbackStartedAt
        {
            get { return _lastPlayTimestamp; }
        }

        // the perceived duration of the feedback, to be used to display its progress bar, meant to be overridden with meaningful data by each feedback
        public virtual float FeedbackDuration
        {
            get { return 0f; }
            set { }
        }

        /// whether or not this feedback is playing right now
        public virtual bool FeedbackPlaying
        {
            get { return ((FeedbackStartedAt > 0f) && (UnityEngine.Time.time - FeedbackStartedAt < FeedbackDuration)); }
        }

        public virtual MMChannelData ChannelData(int channel) => _channelData.Set(MMChannelModes.Int, channel, null);

        protected float _lastPlayTimestamp = -1f;
        protected int _playsLeft;
        protected bool _initialized;
        protected CoroutineHandle _playCoroutine;
        protected CoroutineHandle _infinitePlayCoroutine;
        protected CoroutineHandle _sequenceCoroutine;
        protected CoroutineHandle _repeatedPlayCoroutine;
        protected int _sequenceTrackID;
        protected MMFeedbacks _hostMMFeedbacks;

        protected float _beatInterval;
        protected bool BeatThisFrame;
        protected int LastBeatIndex;
        protected int CurrentSequenceIndex;
        protected float LastBeatTimestamp;
        protected bool _isHostMMFeedbacksNotNull;
        protected MMChannelData _channelData;

        protected virtual void OnEnable()
        {
            _hostMMFeedbacks = this.gameObject.GetComponent<MMFeedbacks>();
            _isHostMMFeedbacksNotNull = _hostMMFeedbacks != null;
        }

        /// <summary>
        /// Initializes the feedback and its timing related variables
        /// </summary>
        /// <param name="owner"></param>
        public virtual void Initialization(GameObject owner)
        {
            _initialized = true;
            Owner = owner;
            _playsLeft = Time.NumberOfRepeats + 1;
            _hostMMFeedbacks = this.gameObject.GetComponent<MMFeedbacks>();
            _channelData = new MMChannelData(MMChannelModes.Int, 0, null);

            SetInitialDelay(Time.InitialDelay);
            SetDelayBetweenRepeats(Time.DelayBetweenRepeats);
            SetSequence(Time.Sequence);

            CustomInitialization(owner);
        }

        /// <summary>
        /// Plays the feedback
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        public virtual void Play(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active)
            {
                return;
            }

            if (!_initialized)
            {
                Debug.LogWarning("The " + this + " feedback is being played without having been initialized. Call Initialization() first.");
            }

            // we check the cooldown
            if (InCooldown)
            {
                return;
            }

            if (Time.InitialDelay > 0f)
            {
                _playCoroutine = Timing.RunCoroutine(PlayCoroutine(position, feedbacksIntensity));
            }
            else
            {
                _lastPlayTimestamp = FeedbackTime;
                RegularPlay(position, feedbacksIntensity);
            }
        }

        /// <summary>
        /// An internal coroutine delaying the initial play of the feedback
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <returns></returns>
        protected virtual IEnumerator<float> PlayCoroutine(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Time.TimescaleMode == TimescaleModes.Scaled)
            {
                yield return Timing.WaitForSeconds(Time.InitialDelay);
            }
            else
            {
                yield return Timing.WaitUntilDone(MMFeedbacksCoroutine.WaitForUnscaled(Time.InitialDelay), Segment.RealtimeUpdate);
            }

            _lastPlayTimestamp = FeedbackTime;
            RegularPlay(position, feedbacksIntensity);
        }

        /// <summary>
        /// Triggers delaying coroutines if needed
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected virtual void RegularPlay(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Chance == 0f)
            {
                return;
            }

            if (Chance != 100f)
            {
                // determine the odds
                float random = Random.Range(0f, 100f);
                if (random > Chance)
                {
                    return;
                }
            }

            if (Time.UseIntensityInterval)
            {
                if ((feedbacksIntensity < Time.IntensityIntervalMin) || (feedbacksIntensity >= Time.IntensityIntervalMax))
                {
                    return;
                }
            }

            if (Time.RepeatForever)
            {
                _infinitePlayCoroutine = Timing.RunCoroutine(InfinitePlay(position, feedbacksIntensity));
                return;
            }

            if (Time.NumberOfRepeats > 0)
            {
                _repeatedPlayCoroutine = Timing.RunCoroutine(RepeatedPlay(position, feedbacksIntensity));
                return;
            }

            if (Time.Sequence == null)
            {
                CustomPlayFeedback(position, feedbacksIntensity);
            }
            else
            {
                _sequenceCoroutine = Timing.RunCoroutine(SequenceCoroutine(position, feedbacksIntensity));
            }
        }

        /// <summary>
        /// Internal coroutine used for repeated play without end
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <returns></returns>
        protected virtual IEnumerator<float> InfinitePlay(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            while (true)
            {
                _lastPlayTimestamp = FeedbackTime;
                if (Time.Sequence == null)
                {
                    CustomPlayFeedback(position, feedbacksIntensity);
                    if (Time.TimescaleMode == TimescaleModes.Scaled)
                    {
                        yield return Timing.WaitForSeconds(Time.DelayBetweenRepeats);
                    }
                    else
                    {
                        yield return Timing.WaitUntilDone(MMFeedbacksCoroutine.WaitForUnscaled(Time.DelayBetweenRepeats), Segment.RealtimeUpdate);
                    }
                }
                else
                {
                    _sequenceCoroutine = Timing.RunCoroutine(SequenceCoroutine(position, feedbacksIntensity));

                    float delay = ApplyTimeMultiplier(Time.DelayBetweenRepeats) + Time.Sequence.Length;
                    if (Time.TimescaleMode == TimescaleModes.Scaled)
                    {
                        yield return Timing.WaitForSeconds(delay);
                    }
                    else
                    {
                        yield return Timing.WaitUntilDone(MMFeedbacksCoroutine.WaitForUnscaled(delay), Segment.RealtimeUpdate);
                    }
                }
            }
        }

        /// <summary>
        /// Internal coroutine used for repeated play
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <returns></returns>
        protected virtual IEnumerator<float> RepeatedPlay(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            while (_playsLeft > 0)
            {
                _lastPlayTimestamp = FeedbackTime;
                _playsLeft--;
                if (Time.Sequence == null)
                {
                    CustomPlayFeedback(position, feedbacksIntensity);

                    if (Time.TimescaleMode == TimescaleModes.Scaled)
                    {
                        yield return Timing.WaitForSeconds(Time.DelayBetweenRepeats);
                    }
                    else
                    {
                        yield return Timing.WaitUntilDone(MMFeedbacksCoroutine.WaitForUnscaled(Time.DelayBetweenRepeats), Segment.RealtimeUpdate);
                    }
                }
                else
                {
                    _sequenceCoroutine = Timing.RunCoroutine(SequenceCoroutine(position, feedbacksIntensity));

                    float delay = ApplyTimeMultiplier(Time.DelayBetweenRepeats) + Time.Sequence.Length;
                    if (Time.TimescaleMode == TimescaleModes.Scaled)
                    {
                        yield return Timing.WaitForSeconds(delay);
                    }
                    else
                    {
                        yield return Timing.WaitUntilDone(MMFeedbacksCoroutine.WaitForUnscaled(delay), Segment.RealtimeUpdate);
                    }
                }
            }

            _playsLeft = Time.NumberOfRepeats + 1;
        }

        /// <summary>
        /// A coroutine used to play this feedback on a sequence
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <returns></returns>
        protected virtual IEnumerator<float> SequenceCoroutine(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            yield return Timing.WaitForOneFrame;
            float timeStartedAt = FeedbackTime;
            float lastFrame = FeedbackTime;

            BeatThisFrame = false;
            LastBeatIndex = 0;
            CurrentSequenceIndex = 0;
            LastBeatTimestamp = 0f;

            if (Time.Quantized)
            {
                while (CurrentSequenceIndex < Time.Sequence.QuantizedSequence[0].Line.Count)
                {
                    _beatInterval = 60f / Time.TargetBPM;

                    if ((FeedbackTime - LastBeatTimestamp >= _beatInterval) || (LastBeatTimestamp == 0f))
                    {
                        BeatThisFrame = true;
                        LastBeatIndex = CurrentSequenceIndex;
                        LastBeatTimestamp = FeedbackTime;

                        for (int i = 0; i < Time.Sequence.SequenceTracks.Count; i++)
                        {
                            if (Time.Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID == Time.TrackID)
                            {
                                CustomPlayFeedback(position, feedbacksIntensity);
                            }
                        }

                        CurrentSequenceIndex++;
                    }

                    yield return Timing.WaitForOneFrame;
                }
            }
            else
            {
                while (FeedbackTime - timeStartedAt < Time.Sequence.Length)
                {
                    foreach (MMSequenceNote item in Time.Sequence.OriginalSequence.Line)
                    {
                        if ((item.ID == Time.TrackID) && (item.Timestamp >= lastFrame) && (item.Timestamp <= FeedbackTime - timeStartedAt))
                        {
                            CustomPlayFeedback(position, feedbacksIntensity);
                        }
                    }

                    lastFrame = FeedbackTime - timeStartedAt;
                    yield return Timing.WaitForOneFrame;
                }
            }
        }

        /// <summary>
        /// Stops all feedbacks from playing. Will stop repeating feedbacks, and call custom stop implementations
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        public virtual void Stop(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (_playCoroutine != default)
            {
                Timing.KillCoroutines(_playCoroutine);
            }

            if (_infinitePlayCoroutine != default)
            {
                Timing.KillCoroutines(_infinitePlayCoroutine);
            }

            if (_repeatedPlayCoroutine != default)
            {
                Timing.KillCoroutines(_repeatedPlayCoroutine);
            }

            if (_sequenceCoroutine != default)
            {
                Timing.KillCoroutines(_sequenceCoroutine);
            }

            _lastPlayTimestamp = 0f;
            _playsLeft = Time.NumberOfRepeats + 1;
            if (Time.InterruptsOnStop)
            {
                CustomStopFeedback(position, feedbacksIntensity);
            }
        }

        /// <summary>
        /// Calls this feedback's custom reset 
        /// </summary>
        public virtual void ResetFeedback()
        {
            _playsLeft = Time.NumberOfRepeats + 1;
            CustomReset();
        }

        /// <summary>
        /// Use this method to change this feedback's sequence at runtime
        /// </summary>
        /// <param name="newSequence"></param>
        public virtual void SetSequence(MMSequence newSequence)
        {
            Time.Sequence = newSequence;
            if (Time.Sequence != null)
            {
                for (int i = 0; i < Time.Sequence.SequenceTracks.Count; i++)
                {
                    if (Time.Sequence.SequenceTracks[i].ID == Time.TrackID)
                    {
                        _sequenceTrackID = i;
                    }
                }
            }
        }

        /// <summary>
        /// Use this method to specify a new delay between repeats at runtime
        /// </summary>
        /// <param name="delay"></param>
        public virtual void SetDelayBetweenRepeats(float delay)
        {
            Time.DelayBetweenRepeats = delay;
        }

        /// <summary>
        /// Use this method to specify a new initial delay at runtime
        /// </summary>
        /// <param name="delay"></param>
        public virtual void SetInitialDelay(float delay)
        {
            Time.InitialDelay = delay;
        }

        /// <summary>
        /// Returns a new value of the normalized time based on the current play direction of this feedback
        /// </summary>
        /// <param name="normalizedTime"></param>
        /// <returns></returns>
        protected virtual float ApplyDirection(float normalizedTime)
        {
            return NormalPlayDirection ? normalizedTime : 1 - normalizedTime;
        }

        /// <summary>
        /// Returns true if this feedback should play normally, or false if it should play in rewind
        /// </summary>
        public virtual bool NormalPlayDirection
        {
            get
            {
                switch (Time.PlayDirection)
                {
                    case MMFeedbackTiming.PlayDirections.FollowMMFeedbacksDirection:
                        return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
                    case MMFeedbackTiming.PlayDirections.AlwaysNormal:
                        return true;
                    case MMFeedbackTiming.PlayDirections.AlwaysRewind:
                        return false;
                    case MMFeedbackTiming.PlayDirections.OppositeMMFeedbacksDirection:
                        return !(_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
                }

                return true;
            }
        }

        /// <summary>
        /// Returns true if this feedback should play in the current parent MMFeedbacks direction, according to its MMFeedbacksDirectionCondition setting
        /// </summary>
        public virtual bool ShouldPlayInThisSequenceDirection
        {
            get
            {
                switch (Time.MMFeedbacksDirectionCondition)
                {
                    case MMFeedbackTiming.MMFeedbacksDirectionConditions.Always:
                        return true;
                    case MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards:
                        return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
                    case MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards:
                        return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.BottomToTop);
                }

                return true;
            }
        }

        /// <summary>
        /// Returns the t value at which to evaluate a curve at the end of this feedback's play time
        /// </summary>
        protected virtual float FinalNormalizedTime
        {
            get { return NormalPlayDirection ? 1f : 0f; }
        }

        /// <summary>
        /// Applies the host MMFeedbacks' time multiplier to this feedback
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        protected virtual float ApplyTimeMultiplier(float duration)
        {
            if (_isHostMMFeedbacksNotNull)
            {
                return _hostMMFeedbacks.ApplyTimeMultiplier(duration);
            }

            return duration;
        }

        /// <summary>
        /// This method describes all custom initialization processes the feedback requires, in addition to the main Initialization method
        /// </summary>
        /// <param name="owner"></param>
        protected virtual void CustomInitialization(GameObject owner)
        {
        }

        /// <summary>
        /// This method describes what happens when the feedback gets played
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected abstract void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f);

        /// <summary>
        /// This method describes what happens when the feedback gets stopped
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected virtual void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
        }

        /// <summary>
        /// This method describes what happens when the feedback gets reset
        /// </summary>
        protected virtual void CustomReset()
        {
        }
    }
}