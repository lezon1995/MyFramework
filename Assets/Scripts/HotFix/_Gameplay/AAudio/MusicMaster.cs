using System.Collections.Generic;

namespace MarbleHero;

public class MusicMaster
{
    List<MainMusic> mainTrack = new();
    List<TempMusic> tempTrack = new();

    public MusicMaster()
    {
        Settings.MASTER_VOLUME = Settings.soundPref.getFloat("Master Volume", 0.5F);
        Settings.MUSIC_VOLUME = Settings.soundPref.getFloat("Music Volume", 0.5F);
        log("Music Volume: " + Settings.MUSIC_VOLUME);
    }

    public void update(float dt)
    {
        updateBGM(dt);
        updateTempBGM(dt);
    }

    public void updateVolume()
    {
        foreach (var m in mainTrack)
            m.updateVolume();

        foreach (var m in tempTrack)
            m.updateVolume();
    }

    void updateBGM(float dt)
    {
        for (var i = mainTrack.Count - 1; i >= 0; i--)
        {
            var e = mainTrack[i];
            e.update(dt);
            if (e.isDone)
                mainTrack.RemoveAt(i);
        }
    }

    void updateTempBGM(float dt)
    {
        for (var i = tempTrack.Count - 1; i >= 0; i--)
        {
            var e = tempTrack[i];
            e.update(dt);
            if (e.isDone)
                tempTrack.RemoveAt(i);
        }
    }

    public void fadeOutTempBGM()
    {
        foreach (var m in tempTrack)
        {
            if (!m.isFadingOut)
                m.fadeOut();
        }

        foreach (var m in mainTrack)
            m.unsilence();
    }

    public void justFadeOutTempBGM()
    {
        foreach (var m in tempTrack)
        {
            if (!m.isFadingOut)
                m.fadeOut();
        }
    }

    public void playTempBGM(string key)
    {
        if (key != null)
        {
            log("Playing " + key);
            tempTrack.Add(new TempMusic(key, false));
            foreach (var m in mainTrack)
                m.silence();
        }
    }

    public void playTempBgmInstantly(string key)
    {
        if (key != null)
        {
            log("Playing " + key);
            tempTrack.Add(new TempMusic(key, true));
            foreach (var m in mainTrack)
                m.silenceInstantly();
        }
    }

    public void precacheTempBgm(string key)
    {
        if (key != null)
        {
            log("Pre-caching " + key);
            tempTrack.Add(new TempMusic(key, true, true, true));
        }
    }

    public void playPrecachedTempBgm()
    {
        if (tempTrack.Count > 0)
        {
            tempTrack[0].playPrecached();
            foreach (var m in mainTrack)
                m.silenceInstantly();
        }
    }

    public void playTempBgmInstantly(string key, bool loop)
    {
        if (key != null)
        {
            log("Playing " + key);
            tempTrack.Add(new TempMusic(key, true, loop));
            foreach (var m in mainTrack)
                m.silenceInstantly();
        }
    }

    public void changeBGM(string key)
    {
        mainTrack.Add(new MainMusic(key));
    }

    public void fadeOutBGM()
    {
        foreach (var m in mainTrack)
        {
            if (!m.isFadingOut)
                m.fadeOut();
        }
    }

    public void silenceBGM()
    {
        foreach (var m in mainTrack)
            m.silence();
    }

    public void silenceBGMInstantly()
    {
        foreach (var m in mainTrack)
            m.silenceInstantly();
    }

    public void unsilenceBGM()
    {
        foreach (var m in mainTrack)
            m.unsilence();
    }

    public void silenceTempBgmInstantly()
    {
        foreach (var m in tempTrack)
        {
            m.silenceInstantly();
            m.isFadingOut = true;
        }
    }

    public void dispose()
    {
        foreach (var m in mainTrack)
            m.kill();
        foreach (var m in tempTrack)
            m.kill();
    }

    public void fadeAll()
    {
        foreach (var m in mainTrack)
            m.fadeOut();
        foreach (var m in tempTrack)
            m.fadeOut();
    }
}