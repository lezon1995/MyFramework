using UnityEngine;

namespace MoreMountains.Feedbacks
{
    public static class FeedbackExtensions
    {
        public static void Initialize(this MMFeedbacks feedbacks)
        {
            if (feedbacks == null)
                return;

            feedbacks.Initialization();
        }

        public static void Initialize(this MMFeedbacks feedbacks, GameObject owner)
        {
            if (feedbacks == null)
                return;

            feedbacks.Initialization(owner);
        }

        public static void Play(this MMFeedbacks feedbacks)
        {
            if (feedbacks == null)
                return;

            feedbacks.PlayFeedbacks();
        }

        public static void Play(this MMFeedbacks feedbacks, Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (feedbacks == null)
                return;

            feedbacks.PlayFeedbacks(position, feedbacksIntensity);
        }

        public static void Stop(this MMFeedbacks feedbacks)
        {
            if (feedbacks == null)
                return;

            feedbacks.StopFeedbacks();
        }

        public static void Stop(this MMFeedbacks feedbacks, Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (feedbacks == null)
                return;

            feedbacks.StopFeedbacks(position, feedbacksIntensity);
        }
    }
}