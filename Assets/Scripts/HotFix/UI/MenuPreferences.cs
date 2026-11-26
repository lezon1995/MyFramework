using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace MoreMountains;

/// <summary>桌面设置独立保存，只有应用/确认后才写入；不改变游戏存档槽。</summary>
public static class MenuPreferences
{
    const string StorageKey = "MoreMountains.DesktopSettings.v1";
    public static bool IsChinese => LocalizationSettings.SelectedLocale == null ||
        LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("zh", StringComparison.OrdinalIgnoreCase);

    [Serializable]
    public sealed class Data
    {
        public int width = 1920;
        public int height = 1080;
        public int mode = (int)FullScreenMode.FullScreenWindow;
        public int quality;
        public int fps = 60;
        public bool vsync = true;
        public float master = .5f;
        public float music = .5f;
        public float sound = .5f;
        public bool ambience = true;
        public bool muteBackground = true;
        public bool runBackground;
        public bool screenShake = true;
        public string locale = "zh-Hans";
        public int confirmKey = (int)KeyCode.KeypadEnter;
        public int cancelKey = (int)KeyCode.Escape;
        public Data Copy() => (Data)MemberwiseClone();
    }

    public static Data Current { get; private set; }

    public static Data Defaults() => new()
    {
        width = Mathf.Max(800, Screen.currentResolution.width),
        height = Mathf.Max(600, Screen.currentResolution.height),
        quality = Mathf.Min(2, QualitySettings.names.Length - 1),
        locale = LocalizationSettings.SelectedLocale?.Identifier.Code ?? "zh-Hans"
    };

    public static void Initialize()
    {
        var data = Defaults();
        data.width = Screen.width;
        data.height = Screen.height;
        data.mode = (int)Screen.fullScreenMode;
        data.quality = QualitySettings.GetQualityLevel();
        data.master = Settings.MASTER_VOLUME;
        data.music = Settings.MUSIC_VOLUME;
        data.sound = Settings.SOUND_VOLUME;
        data.ambience = Settings.AMBIANCE_ON;
        data.muteBackground = Game.MUTE_IF_BG;
        data.screenShake = Settings.SCREEN_SHAKE;
        if (PlayerPrefs.HasKey(StorageKey))
        {
            try { JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(StorageKey), data); }
            catch (Exception exception) { Debug.LogWarning("Desktop settings reset: " + exception.Message); }
        }
        Sanitize(data);
        Current = data;
        ApplyRuntime(data);
        if (PlayerPrefs.HasKey(StorageKey))
            ApplyDisplay(data);
    }

    public static void Sanitize(Data data)
    {
        data.width = Mathf.Clamp(data.width, 800, 16384);
        data.height = Mathf.Clamp(data.height, 600, 8640);
        if (data.mode != (int)FullScreenMode.Windowed && data.mode != (int)FullScreenMode.FullScreenWindow &&
            data.mode != (int)FullScreenMode.ExclusiveFullScreen)
            data.mode = (int)FullScreenMode.FullScreenWindow;
        if (Application.platform != RuntimePlatform.WindowsPlayer && data.mode == (int)FullScreenMode.ExclusiveFullScreen)
            data.mode = (int)FullScreenMode.FullScreenWindow;
        data.quality = Mathf.Clamp(data.quality, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
        if (data.fps != -1) data.fps = Mathf.Clamp(data.fps, 30, 360);
        data.master = float.IsNaN(data.master) ? .5f : Mathf.Clamp01(data.master);
        data.music = float.IsNaN(data.music) ? .5f : Mathf.Clamp01(data.music);
        data.sound = float.IsNaN(data.sound) ? .5f : Mathf.Clamp01(data.sound);
        if (!ValidKey(data.confirmKey)) data.confirmKey = (int)KeyCode.KeypadEnter;
        if (!ValidKey(data.cancelKey) || data.cancelKey == data.confirmKey) data.cancelKey = (int)KeyCode.Escape;
    }

    public static bool ValidKey(int key) => Enum.IsDefined(typeof(KeyCode), key) &&
        key > (int)KeyCode.None && key < (int)KeyCode.Mouse0;

    public static void ApplyDisplay(Data data)
    {
        if (!Application.isEditor)
            Screen.SetResolution(data.width, data.height, (FullScreenMode)data.mode);
    }

    public static void Commit(Data data)
    {
        Sanitize(data);
        ApplyRuntime(data);
        Current = data.Copy();
        PlayerPrefs.SetString(StorageKey, JsonUtility.ToJson(Current));
        PlayerPrefs.Save();
    }

    static void ApplyRuntime(Data data)
    {
        QualitySettings.SetQualityLevel(data.quality, true);
        QualitySettings.vSyncCount = data.vsync ? 1 : 0;
        Application.targetFrameRate = data.fps;
        Application.runInBackground = data.runBackground;
        Settings.MASTER_VOLUME = data.master;
        Settings.MUSIC_VOLUME = data.music;
        Settings.SOUND_VOLUME = data.sound;
        Settings.AMBIANCE_ON = data.ambience;
        Settings.SCREEN_SHAKE = data.screenShake;
        Game.MUTE_IF_BG = data.muteBackground;
        music?.updateVolume();
        Game.mainMenuScreen?.updateAmbienceVolume();
        InputActionSet.confirm?.remap((KeyCode)data.confirmKey);
        InputActionSet.cancel?.remap((KeyCode)data.cancelKey);
        var locale = LocalizationSettings.AvailableLocales.GetLocale(data.locale);
        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
            Settings.setLanguage(locale.Identifier.Code.StartsWith("zh") ? GameLanguage.ZHS : GameLanguage.ENG, false);
        }
        TickFocus();
    }

    public static void TickFocus()
    {
        Settings.isBackgrounded = !Application.isFocused;
        AudioListener.pause = Game.MUTE_IF_BG && Settings.isBackgrounded;
    }
}
