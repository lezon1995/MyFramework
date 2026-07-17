namespace MoreMountains;

public class MockMusic : IMusic
{
    public void play()
    {
    }

    public void pause()
    {
    }

    public void stop()
    {
    }

    public bool isPlaying()
    {
        return false;
    }

    public void setLooping(bool isLooping)
    {
    }

    public bool isLooping()
    {
        return false;
    }

    public void setVolume(float volume)
    {
    }

    public float getVolume()
    {
        return 0.0F;
    }

    public void setPan(float pan, float volume)
    {
    }

    public void setPosition(float position)
    {
    }

    public float getPosition()
    {
        return 0.0F;
    }

    public void Dispose()
    {
    }

    public void setOnCompletionListener(IMusic.OnCompletionListener listener)
    {
    }
}