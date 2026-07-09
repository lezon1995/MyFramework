using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// CoroutineHandle helpers
    /// </summary>
    public static class MMFeedbacksCoroutine
    {
        /// <summary>
        /// Waits for the specified amount of frames
        /// use : yield return MMCoroutine.WaitFor(1);
        /// </summary>
        /// <param name="frameCount"></param>
        /// <returns></returns>
        public static IEnumerator<float> WaitForFrames(int frameCount)
        {
            while (frameCount > 0)
            {
                frameCount--;
                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// Waits for the specified amount of seconds (using regular time)
        /// use : yield return MMCoroutine.WaitFor(1f);
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static IEnumerator<float> WaitFor(float seconds)
        {
            yield return Timing.WaitForSeconds(seconds);
        }

        /// <summary>
        /// Waits for the specified amount of seconds (using unscaled time)
        /// use : yield return MMCoroutine.WaitForUnscaled(1f);
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static IEnumerator<float> WaitForUnscaled(float seconds)
        {
            yield return Timing.WaitForSeconds(seconds);
        }
    }
}