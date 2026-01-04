namespace MarbleHero;

public class MainMusic
{
    IMusic music;
    public string key;
    const string DIR = "Audio/Music/";
    static string TITLE_BGM = "STS_MenuTheme_NewMix_v1.ogg";
    static string LEVEL_1_1_BGM = "STS_Level1_NewMix_v1.ogg";
    static string LEVEL_1_2_BGM = "STS_Level1-2_v2.ogg";
    static string LEVEL_2_1_BGM = "STS_Level2_NewMix_v1.ogg";
    static string LEVEL_2_2_BGM = "STS_Level2-2_v2.ogg";
    static string LEVEL_3_1_BGM = "STS_Level3_v2.ogg";
    static string LEVEL_3_2_BGM = "STS_Level3-2_v2.ogg";
    static string LEVEL_4_1_BGM = "STS_Act4_BGM_v2.ogg";
    public bool isSilenced;
    float silenceTimer;
    float silenceTime;
    static float SILENCE_TIME = 4.0F;
    static float FAST_SILENCE_TIME = 0.25F;
    float silenceStartVolume;
    static float FADE_IN_TIME = 4.0F;
    static float FADE_OUT_TIME = 4.0F;
    float fadeTimer = 0.0F;
    public bool isFadingOut = false;
    float fadeOutStartVolume;
    public bool isDone = false;

    public MainMusic(string key)
    {
        this.key = key;
        music = getSong(key);
        fadeTimer = 4.0F;
        music.setLooping(true);
        music.play();
        music.setVolume(0.0F);
    }

    IMusic getSong(string key)
    {
        return key switch
        {
            "Exordium" => ADungeon.miscRng.random(1) switch
            {
                0 => newMusic(DIR + LEVEL_1_1_BGM),
                _ => newMusic(DIR + LEVEL_1_2_BGM)
            },
            "TheCity" => ADungeon.miscRng.random(1) switch
            {
                0 => newMusic(DIR + LEVEL_2_1_BGM),
                _ => newMusic(DIR + LEVEL_2_2_BGM)
            },
            "TheBeyond" => ADungeon.miscRng.random(1) switch
            {
                0 => newMusic(DIR + LEVEL_3_1_BGM),
                _ => newMusic(DIR + LEVEL_3_2_BGM)
            },
            "TheEnding" => newMusic(DIR + LEVEL_4_1_BGM),
            "MENU" => newMusic(DIR + TITLE_BGM),
            _ => newMusic(DIR + LEVEL_1_1_BGM)
        };
    }

    public static IMusic newMusic(string path)
    {
        // if (Gdx.audio == null)
        {
            log("WARNING: Gdx.audio is null so no Music instance can be initialized.");
            return new MockMusic();
        }

        // return Gdx.audio.newMusic(path);
    }

    public void updateVolume()
    {
        if (!isFadingOut && !isSilenced)
            music.setVolume(Settings.MUSIC_VOLUME * Settings.MASTER_VOLUME);
    }

    public void fadeOut()
    {
        isFadingOut = true;
        fadeOutStartVolume = music.getVolume();
        fadeTimer = 4.0F;
    }

    public void silence()
    {
        isSilenced = true;
        silenceTimer = 4.0F;
        silenceTime = 4.0F;
        silenceStartVolume = music.getVolume();
    }

    public void silenceInstantly()
    {
        isSilenced = true;
        silenceTimer = 0.25F;
        silenceTime = 0.25F;
        silenceStartVolume = music.getVolume();
    }

    public void unsilence()
    {
        if (isSilenced)
        {
            log("Unsilencing " + key);
            isSilenced = false;
            fadeTimer = 4.0F;
        }
    }

    public void kill()
    {
        music.Dispose();
        isDone = true;
    }

    public void update(float dt)
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

    void updateFadeIn(float dt)
    {
        if (!isSilenced)
        {
            fadeTimer -= dt;
            if (fadeTimer < 0.0F)
                fadeTimer = 0.0F;
            if (!Settings.isBackgrounded)
            {
                music.setVolume(MMLerp.fade.apply(0.0F, 1.0F, 1.0F - fadeTimer / 4.0F) * Settings.MUSIC_VOLUME * Settings.MASTER_VOLUME);
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
        if (!isSilenced)
        {
            fadeTimer -= dt;
            if (fadeTimer < 0.0F)
            {
                fadeTimer = 0.0F;
                isDone = true;
                log("Disposing MainMusic: " + key);
                music.Dispose();
            }
            else
            {
                music.setVolume(MMLerp.fade.apply(fadeOutStartVolume, 0.0F, 1.0F - fadeTimer / 4.0F));
            }
        }
        else
        {
            silenceTimer -= dt;
            if (silenceTimer < 0.0F)
                silenceTimer = 0.0F;
            music.setVolume(MMLerp.fade.apply(silenceStartVolume, 0.0F, 1.0F - silenceTimer / silenceTime));
        }
    }
}