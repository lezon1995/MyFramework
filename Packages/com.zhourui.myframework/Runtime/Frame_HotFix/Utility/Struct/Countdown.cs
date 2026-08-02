public struct Countdown
{
    public int duration;
    public int elapsed;
    public bool finished;
    public int remain => duration - elapsed;
    public float pct => duration == 0 ? 0F : (float)elapsed / duration;
    public bool unstarted => duration != 0 && elapsed == 0;
    public bool isDone => finished;

    public bool update(int dt = 1, bool canRepeatTrigger = false)
    {
        elapsed = (elapsed + dt).clamp(0, duration);
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
        duration = 0;
        elapsed = 0;
        finished = false;
    }

    public void reset()
    {
        elapsed = 0;
        finished = false;
    }

    public static implicit operator bool(Countdown timer)
    {
        return timer.duration > 0 && timer.elapsed < timer.duration;
    }

    public static implicit operator int(Countdown timer) => timer.remain;

    public static implicit operator Countdown(int duration) => new()
    {
        duration = duration,
        elapsed = 0,
        finished = false
    };
}