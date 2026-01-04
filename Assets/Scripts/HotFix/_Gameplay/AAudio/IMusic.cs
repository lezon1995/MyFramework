using System;

namespace MarbleHero;

public interface IMusic : IDisposable
{
    void play();
    void pause();
    void stop();
    bool isPlaying();
    void setLooping(bool paramBoolean);
    bool isLooping();
    void setVolume(float paramFloat);
    float getVolume();
    void setPan(float paramFloat1, float paramFloat2);
    void setPosition(float paramFloat);
    float getPosition();
    void setOnCompletionListener(OnCompletionListener paramOnCompletionListener);
    public interface OnCompletionListener
    {
        void onCompletion(IMusic param1Music);
    }
}

