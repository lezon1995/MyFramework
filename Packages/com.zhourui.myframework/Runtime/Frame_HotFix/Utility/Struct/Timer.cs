using System;

[Serializable]
public struct Timer
{
    public float duration;
    public float elapsed;
    public bool finished;
    public float remain => duration - elapsed;
    public float pct => duration.isZero() ? 0F : elapsed / duration;
    public bool unstarted => !duration.isZero() && elapsed.isZero();
    public bool isDone => finished;

    public bool update(float dt, bool canRepeatTrigger = false)
    {
        if (!this)
            return false;

        elapsed = (elapsed + dt).clamp(0F, duration);
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

[Serializable]
public class MTimer : ClassObject
{
    public float duration;
    public float elapsed;
    public bool finished;
    public float remain => duration - elapsed;
    public float pct => duration.isZero() ? 0F : elapsed / duration;
    public bool unstarted => !duration.isZero() && elapsed.isZero();
    public bool isDone => finished;

    public override void resetProperty()
    {
        base.resetProperty();
        duration = 0;
        elapsed = 0;
        finished = false;
    }

    public bool update(float dt, bool canRepeatTrigger = false)
    {
        elapsed = (elapsed + dt).clamp(0F, duration);
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

    public static implicit operator bool(MTimer timer)
    {
        return timer.duration > 0 && timer.elapsed < timer.duration;
    }

    public static implicit operator float(MTimer timer) => timer.remain;

    public static implicit operator MTimer(float duration)
    {
        FrameUtility.CLASS(out MTimer timer);
        timer.duration = duration;
        timer.elapsed = 0F;
        timer.finished = false;
        return timer;
    }

    public void release() => FrameUtility.UN_CLASS(this);
}