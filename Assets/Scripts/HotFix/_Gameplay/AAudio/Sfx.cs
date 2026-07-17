using UnityEngine;

namespace MoreMountains
{
    public class Sfx
    {
        static int soundIdGenerator;
        string url;
        int id;
        bool initialized;
        AudioClip clip;

        public Sfx(string _url, bool preload = true)
        {
            id = ++soundIdGenerator;
            if (preload)
                initSound(_url);
            else
                url = _url;
        }

        public int play(float volume)
        {
            initSound(url);
            if (initialized)
            {
                AT.SOUND_2D(id, volume);
                // MMSfxEvent.Trigger(clip, id, volume);
            }

            return id;
        }

        public int play(float volume, float pitch, float z)
        {
            initSound(url);
            if (initialized)
            {
                AT.SOUND_2D(id, volume);
                // MMSfxEvent.Trigger(clip, id, volume, pitch);
            }

            return id;
        }

        public int loop(float volume)
        {
            initSound(url);
            if (initialized)
            {
                AT.SOUND_2D(id, true, volume);
            }

            return id;
        }

        public void setVolume(int id, float volume)
        {
            if (initialized)
            {
                // clip.setVolume(id, volume);
            }
        }

        public void stop()
        {
            if (initialized)
            {
                // MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Stop, id);
            }
        }

        public void stop(int id)
        {
            if (initialized)
            {
                // MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Stop, id);
            }
        }

        void initSound(string file)
        {
            if (initialized)
                return;

            mAudioManager.registeSoundDefine(id, file);
            initialized = true;
            /*if (clip == null)
            {
                if (!file.isEmpty())
                    clip = res.loadGameResource<AudioClip>(file);

                if (clip == null)
                {
                    log("File: " + url + " was not found.");
                }
            }*/
        }
    }
}