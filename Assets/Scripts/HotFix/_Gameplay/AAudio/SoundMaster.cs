using System.Collections.Generic;

namespace MoreMountains
{
    public class SoundMaster
    {
        const string SFX_DIR = $"{GAMEPLAY_PATH}/Audio/Sounds/";
        Dictionary<string, Sfx> map = new();
        List<SoundInfo> fadeOutList = new();

        public SoundMaster()
        {
            long startTime = TimeUtility.getNowTimeStampMS();
            Settings.SOUND_VOLUME = Settings.soundPref.getFloat("Sound Volume", 0.5F);
            map.Add(SoundDefine.BALL_HIT_BRICK_COMMON, load("ball_hit_brick_common.wav"));
            map.Add(SoundDefine.BALL_HIT_BORDER_COMMON, load("ball_hit_border.wav"));
            map.Add(SoundDefine.LASER_BEAM, load("laser_beam.wav"));
            map.Add(SoundDefine.LIGHTNING_STRIKE, load("lightning_strike.wav"));
            map.Add(SoundDefine.ELECTRICITY_STRIKE, load("electricity_strike.wav"));

            log("Sound Effect Volume: " + Settings.SOUND_VOLUME);
            log("Loaded " + map.Count + " Sound Effects");
            log("SFX load time: " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms");
        }

        Sfx load(string filename) => load(filename, true);
        Sfx load(string filename, bool preload) => new(SFX_DIR + filename, preload);

        public void update(float dt)
        {
            for (var i = fadeOutList.Count - 1; i >= 0; i--)
            {
                var e = fadeOutList[i];
                e.update(dt);
                if (map.TryGetValue(e.name, out var sfx))
                {
                    if (e.isDone)
                    {
                        sfx.stop(e.id);
                        fadeOutList.RemoveAt(i);
                        continue;
                    }

                    sfx.setVolume(e.id, Settings.SOUND_VOLUME * Settings.MASTER_VOLUME * e.volumeMultiplier);
                }
            }
        }

        public void preload(string key)
        {
            if (map.ContainsKey(key))
            {
                log("Preloading: " + key);
                int id = map[key].play(0.0F);
                map[key].stop(id);
            }
            else
            {
                log("Missing: " + key);
            }
        }

        public int play(string key, bool useBgmVolume)
        {
            if (Game.MUTE_IF_BG && Settings.isBackgrounded)
                return 0;

            if (map.ContainsKey(key))
            {
                if (useBgmVolume)
                    return map[key].play(Settings.MUSIC_VOLUME * Settings.MASTER_VOLUME);

                return map[key].play(Settings.SOUND_VOLUME * Settings.MASTER_VOLUME);
            }

            log("Missing: " + key);
            return 0;
        }

        public int play(string key)
        {
            if (Game.MUTE_IF_BG && Settings.isBackgrounded)
                return 0;

            return play(key, false);
        }

        public int play(string key, float pitchVariation)
        {
            if (Game.MUTE_IF_BG && Settings.isBackgrounded)
                return 0;

            if (map.ContainsKey(key))
                return map[key].play(Settings.SOUND_VOLUME * Settings.MASTER_VOLUME, 1.0F + MathUtils.random(-pitchVariation, pitchVariation), 0.0F);

            log("Missing: " + key);
            return 0;
        }

        public int playA(string key, float pitchAdjust)
        {
            if (Game.MUTE_IF_BG && Settings.isBackgrounded)
                return 0;

            if (map.ContainsKey(key))
                return map[key].play(Settings.SOUND_VOLUME * Settings.MASTER_VOLUME, 1.0F + pitchAdjust, 0.0F);

            log("Missing: " + key);
            return 0;
        }

        public int playV(string key, float volumeMod)
        {
            if (Game.MUTE_IF_BG && Settings.isBackgrounded)
                return 0;

            if (map.ContainsKey(key))
                return map[key].play(Settings.SOUND_VOLUME * Settings.MASTER_VOLUME * volumeMod, 1.0F, 0.0F);

            log("Missing: " + key);
            return 0;
        }

        public int playAV(string key, float pitchAdjust, float volumeMod)
        {
            if (Game.MUTE_IF_BG && Settings.isBackgrounded)
                return 0;

            if (map.ContainsKey(key))
                return map[key].play(Settings.SOUND_VOLUME * Settings.MASTER_VOLUME * volumeMod, 1.0F + pitchAdjust, 0.0F);

            log("Missing: " + key);
            return 0;
        }

        public int playAndLoop(string key)
        {
            if (map.ContainsKey(key))
                return map[key].loop(Settings.SOUND_VOLUME * Settings.MASTER_VOLUME);

            log("Missing: " + key);
            return 0;
        }

        public int playAndLoop(string key, float volume)
        {
            if (map.ContainsKey(key))
                return map[key].loop(volume);

            log("Missing: " + key);
            return 0;
        }

        public void adjustVolume(string key, int id, float volume)
        {
            map[key].setVolume(id, volume);
        }

        public void adjustVolume(string key, int id)
        {
            map[key].setVolume(id, Settings.SOUND_VOLUME * Settings.MASTER_VOLUME);
        }

        public void fadeOut(string key, int id)
        {
            fadeOutList.Add(new SoundInfo(key, id));
        }

        public void stop(string key, int id)
        {
            map[key].stop(id);
        }

        public void stop(string key)
        {
            if (map[key] != null)
                map[key].stop();
        }
    }
}