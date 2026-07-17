namespace MoreMountains;

public class SoundInfo
{
    public string name;
    public int id;
    public bool isDone;
    static float FADE_OUT_DURATION = 5.0F;
    float fadeDuration = FADE_OUT_DURATION;
    public float volumeMultiplier = 1.0F;

    public SoundInfo(string _name, int _id)
    {
        name = _name;
        id = _id;
    }

    public void update(float dt)
    {
        if (fadeDuration != 0.0F)
        {
            fadeDuration -= dt;
            volumeMultiplier = MMLerp.fade.apply(1.0F, 0.0F, 1.0F - fadeDuration / FADE_OUT_DURATION);
            if (fadeDuration < 0.0F)
            {
                isDone = true;
                fadeDuration = 0.0F;
            }
        }
    }
}