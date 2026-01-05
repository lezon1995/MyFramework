using UnityEngine;

namespace MarbleHero
{
    public class Sfx
    {
        static int soundIdGenerator;
        string url;
        int id;
        AudioClip clip;

        public Sfx(string _url, bool preload = false)
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
            if (clip)
            {
                // mAudioManager.loadAudio();
                // MMSfxEvent.Trigger(clip, id, volume);
            }

            return id;
        }

        public int play(float volume, float pitch, float z)
        {
            initSound(url);
            if (clip)
            {
                // MMSfxEvent.Trigger(clip, id, volume, pitch);
            }

            return id;
        }

        public int loop(float volume)
        {
            initSound(url);
            if (clip)
            {
                // return clip.loop(volume);
            }

            return id;
        }

        public void setVolume(int id, float volume)
        {
            if (clip)
            {
                // clip.setVolume(id, volume);
            }
        }

        public void stop()
        {
            if (clip)
            {
                // MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Stop, id);
            }
        }

        public void stop(int id)
        {
            if (clip)
            {
                // MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Stop, id);
            }
        }

        void initSound(string file)
        {
            if (clip == null)
            {
                // if (file != null)
                    // clip = mResourceManager.loadGameResource<AudioClip>(file);

                // if (clip == null)
                // {
                //     log("File: " + url + " was not found.");
                // }
            }
        }
    }
}