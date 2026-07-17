namespace MoreMountains;

public class TempMusic
{
    IMusic music;
    public string key;
    static string DIR = "Audio/Music/";
    static string SHOP_BGM = "STS_Merchant_NewMix_v1.ogg";
    static string SHRINE_BGM = "STS_Shrine_NewMix_v1.ogg";
    static string MINDBLOOM_BGM = "STS_Boss1MindBloom_v1.ogg";
    static string LEVEL_1_BOSS_BGM = "STS_Boss1_NewMix_v1.ogg";
    static string LEVEL_2_BOSS_BGM = "STS_Boss2_NewMix_v1.ogg";
    static string LEVEL_3_BOSS_BGM = "STS_Boss3_NewMix_v1.ogg";
    static string LEVEL_4_BOSS_BGM = "STS_Boss4_v6.ogg";
    static string ELITE_BGM = "STS_EliteBoss_NewMix_v1.ogg";
    static string CREDITS = "STS_Credits_v5.ogg";
    public bool isSilenced;
    float silenceTimer;
    float silenceTime;
    static float FAST_SILENCE_TIME = 0.25F;
    float silenceStartVolume;
    static float FADE_IN_TIME = 4.0F;
    static float FAST_FADE_IN_TIME = 0.25F;
    static float FADE_OUT_TIME = 4.0F;
    float fadeTimer;
    float fadeTime;
    public bool isFadingOut;
    float fadeOutStartVolume;
    public bool isDone;

    public TempMusic(string key, bool isFast) : this(key, isFast, true)
    {
    }

    public TempMusic(string key, bool isFast, bool loop)
    {
        this.key = key;
        music = getSong(key);
        if (isFast)
        {
            fadeTimer = 0.25F;
            fadeTime = 0.25F;
        }
        else
        {
            fadeTimer = 4.0F;
            fadeTime = 4.0F;
        }

        music.setLooping(loop);
        music.play();
        music.setVolume(0.0F);
    }

    public TempMusic(string key, bool isFast, bool loop, bool precache)
    {
        this.key = key;
        music = getSong(key);
        if (isFast)
        {
            fadeTimer = 0.25F;
            fadeTime = 0.25F;
        }
        else
        {
            fadeTimer = 4.0F;
            fadeTime = 4.0F;
        }

        music.setLooping(loop);
        music.setVolume(0.0F);
    }

    public void playPrecached()
    {
        if (!music.isPlaying())
        {
            music.play();
        }
        else
        {
            log("[WARNING] Attempted to play music that is already playing.");
        }
    }

    IMusic getSong(string key)
    {
        return key switch
        {
            "SHOP" => MainMusic.newMusic(DIR + "STS_Merchant_NewMix_v1.ogg"),
            "SHRINE" => MainMusic.newMusic(DIR + "STS_Shrine_NewMix_v1.ogg"),
            "MINDBLOOM" => MainMusic.newMusic(DIR + "STS_Boss1MindBloom_v1.ogg"),
            "BOSS_BOTTOM" => MainMusic.newMusic(DIR + "STS_Boss1_NewMix_v1.ogg"),
            "BOSS_CITY" => MainMusic.newMusic(DIR + "STS_Boss2_NewMix_v1.ogg"),
            "BOSS_BEYOND" => MainMusic.newMusic(DIR + "STS_Boss3_NewMix_v1.ogg"),
            "BOSS_ENDING" => MainMusic.newMusic(DIR + "STS_Boss4_v6.ogg"),
            "ELITE" => MainMusic.newMusic(DIR + "STS_EliteBoss_NewMix_v1.ogg"),
            "CREDITS" => MainMusic.newMusic(DIR + "STS_Credits_v5.ogg"),
            _ => MainMusic.newMusic(DIR + key)
        };
    }

    public void fadeOut()
    {
        isFadingOut = true;
        fadeOutStartVolume = music.getVolume();
        fadeTimer = 4.0F;
    }

    public void silenceInstantly()
    {
        isSilenced = true;
        silenceTimer = 0.25F;
        silenceTime = 0.25F;
        silenceStartVolume = music.getVolume();
    }

    public void kill()
    {
        log("Disposing TempMusic: " + key);
        music.Dispose();
        isDone = true;
    }

    public void update(float dt)
    {
        if (music.isPlaying())
        {
            if (!isFadingOut)
            {
                updateFadeIn(dt);
            }
            else
            {
                updateFadeOut(dt);
            }
        }
        else if (isFadingOut)
        {
            kill();
        }
    }

    void updateFadeIn(float dt)
    {
        if (!isSilenced)
        {
            fadeTimer -= dt;
            if (fadeTimer < 0.0F)
            {
                fadeTimer = 0.0F;
                if (!Settings.isBackgrounded)
                {
                    music.setVolume(MMLerp.fade.apply(0.0F, 1.0F, 1.0F - fadeTimer / fadeTime) * Settings.MUSIC_VOLUME * Settings.MASTER_VOLUME);
                }
                else
                {
                    music.setVolume(MathHelper.slowColorLerpSnap(music.getVolume(), 0.0F, dt));
                }
            }
            else if (!Settings.isBackgrounded)
            {
                music.setVolume(MMLerp.fade.apply(0.0F, 1.0F, 1.0F - fadeTimer / fadeTime) * Settings.MUSIC_VOLUME * Settings.MASTER_VOLUME);
            }
            else
            {
                music.setVolume(MathHelper.slowColorLerpSnap(music.getVolume(), 0.0F, dt));
            }
        }
        else
        {
            silenceTimer -= dt;
            if (silenceTimer < 0.0F)
                silenceTimer = 0.0F;
            if (!Settings.isBackgrounded)
            {
                music.setVolume(MMLerp.fade.apply(silenceStartVolume, 0.0F, 1.0F - silenceTimer / silenceTime));
            }
            else
            {
                music.setVolume(MathHelper.slowColorLerpSnap(music.getVolume(), 0.0F, dt));
            }
        }
    }

    void updateFadeOut(float dt)
    {
        fadeTimer -= dt;
        if (fadeTimer < 0.0F)
        {
            fadeTimer = 0.0F;
            isDone = true;
            log("Disposing TempMusic: " + key);
            music.Dispose();
        }
        else
        {
            music.setVolume(MMLerp.fade.apply(fadeOutStartVolume, 0.0F, 1.0F - fadeTimer / 4.0F));
        }
    }

    public void updateVolume()
    {
        if (!isFadingOut && !isSilenced)
            music.setVolume(Settings.MUSIC_VOLUME * Settings.MASTER_VOLUME);
    }
}