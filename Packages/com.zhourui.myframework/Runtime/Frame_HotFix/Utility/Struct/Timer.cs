public struct Timer
{
    public float duration;
    public float elapsed;
    public bool finished;
    public float remain => duration - elapsed;
    public float pct => MathUtility.isFloatEqual(duration, 0F) ? 0F : elapsed / duration;
    public bool unstarted => !MathUtility.isFloatEqual(duration, 0F) && MathUtility.isFloatEqual(elapsed, 0F);
    public bool isDone => finished;

    public bool update(float dt, bool canRepeatTrigger = false)
    {
        elapsed = MathUtility.clamp(elapsed + dt, 0F, duration);
        var timeUp = elapsed >= duration;
        if (canRepeatTrigger)
            return timeUp;

        if (!finished && timeUp)
        {
            finished = true;
            return true;
        }

        return false;
    }

    public void kill()
    {
        duration = 0F;
        elapsed = 0F;
        finished = false;
    }

    public void reset()
    {
        elapsed = 0F;
        finished = false;
    }

    public static implicit operator bool(Timer timer)
    {
        return timer.duration > 0 && timer.elapsed < timer.duration;
    }

    public static implicit operator float(Timer timer) => timer.remain;

    public static implicit operator Timer(float duration) => new()
    {
        duration = duration,
        elapsed = 0F,
        finished = false
    };
}