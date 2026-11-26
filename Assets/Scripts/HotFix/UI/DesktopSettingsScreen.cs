using System;
using System.Collections.Generic;
using System.Linq;
using Obfuz;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

namespace MoreMountains;

[ObfuzIgnore(ObfuzScope.TypeName)]
public sealed class DesktopSettingsScreen : MainMenuPage
{
    MenuPreferences.Data draft;
    MenuPreferences.Data previousDisplay;
    readonly List<Vector2Int> resolutions = new();
    readonly int[] frameRates = { 30, 60, 90, 120, 144, 165, 240, -1 };
    int tab;
    int capturingKey = -1;
    int captureStartedFrame;
    float confirmDeadline;
    bool awaitingDisplay;
    readonly Action[] actions = new Action[7];

    public override void init()
    {
        base.init();
        for (int i = 0; i < 4; i++)
        {
            int selected = i;
            Click($"Panel/Tab{i}", () => { if (!awaitingDisplay) { tab = selected; capturingKey = -1; Render(); } });
        }
        for (int i = 0; i < 7; i++)
        {
            int row = i;
            Click($"Panel/Rows/Row{i}/Value", () => { if (!awaitingDisplay) actions[row]?.Invoke(); });
            Find<Slider>($"Panel/Rows/Row{i}/Slider").onValueChanged.AddListener(value =>
            {
                if (draft == null || tab != 1 || row > 2) return;
                if (row == 0) draft.master = value;
                if (row == 1) draft.music = value;
                if (row == 2) draft.sound = value;
                Label($"Panel/Rows/Row{row}/Value/Text", $"{value:P0}");
            });
        }
        Click("Panel/Apply", Apply);
        Click("Panel/Defaults", () =>
        {
            if (awaitingDisplay) return;
            draft = MenuPreferences.Defaults();
            AddResolution(new Vector2Int(draft.width, draft.height));
            Render();
            status.text = L("默认设置已载入，点击应用保存。", "Defaults loaded. Apply to save.");
        });
        Click("Panel/Keep", () =>
        {
            if (!awaitingDisplay) return;
            awaitingDisplay = false;
            Save();
        });
        Click("Panel/Revert", RevertDisplay);
    }

    protected override void Refresh()
    {
        draft = MenuPreferences.Current.Copy();
        resolutions.Clear();
        foreach (var resolution in Screen.resolutions)
            if (resolution.width >= 800 && resolution.height >= 600)
                AddResolution(new Vector2Int(resolution.width, resolution.height));
        AddResolution(new Vector2Int(draft.width, draft.height));
        resolutions.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
        capturingKey = -1;
        Render();
    }

    void AddResolution(Vector2Int value)
    {
        if (!resolutions.Contains(value)) resolutions.Add(value);
    }

    void Row(int index, string label, string value, Action action, float? slider = null)
    {
        var path = $"Panel/Rows/Row{index}";
        mTransform.Find(path).gameObject.SetActive(true);
        Label(path + "/Label", label);
        Label(path + "/Value/Text", value);
        actions[index] = action;
        Find<Button>(path + "/Value").interactable = action != null;
        var control = Find<Slider>(path + "/Slider");
        control.gameObject.SetActive(slider.HasValue);
        if (slider.HasValue) control.SetValueWithoutNotify(slider.Value);
    }

    void Toggle(int index, string zh, string en, bool value, Action change) =>
        Row(index, L(zh, en), value ? L("开启", "On") : L("关闭", "Off"), () => { change(); Render(); });

    void Render()
    {
        title.text = L("设置", "Settings");
        string[] names = { L("显示", "Display"), L("声音", "Audio"), L("游戏", "Game"), L("操作", "Controls") };
        for (int i = 0; i < names.Length; i++) Label($"Panel/Tab{i}/Text", (i == tab ? "● " : "") + names[i]);
        Label("Panel/Apply/Text", L("应用", "Apply"));
        Label("Panel/Defaults/Text", L("恢复默认", "Defaults"));
        Label("Panel/Keep/Text", L("保留设置", "Keep"));
        Label("Panel/Revert/Text", L("恢复显示", "Revert"));
        Label("Panel/Back/Text", L("返回", "Back"));
        mTransform.Find("Panel/Keep").gameObject.SetActive(awaitingDisplay);
        mTransform.Find("Panel/Revert").gameObject.SetActive(awaitingDisplay);
        Find<Button>("Panel/Apply").interactable = !awaitingDisplay;
        Find<Button>("Panel/Defaults").interactable = !awaitingDisplay;
        Find<CanvasGroup>("Panel/Rows").interactable = !awaitingDisplay;
        for (int i = 0; i < 7; i++) { mTransform.Find($"Panel/Rows/Row{i}").gameObject.SetActive(false); actions[i] = null; }
        status.text = L("修改后点击应用保存；返回会丢弃未应用的修改。", "Apply to save changes. Back discards unapplied changes.");
        if (tab == 0)
        {
            var modes = Application.platform == RuntimePlatform.WindowsPlayer
                ? new[] { FullScreenMode.Windowed, FullScreenMode.FullScreenWindow, FullScreenMode.ExclusiveFullScreen }
                : new[] { FullScreenMode.Windowed, FullScreenMode.FullScreenWindow };
            string ModeName() => (FullScreenMode)draft.mode == FullScreenMode.Windowed ? L("窗口", "Windowed")
                : (FullScreenMode)draft.mode == FullScreenMode.ExclusiveFullScreen ? L("独占全屏", "Exclusive fullscreen") : L("无边框全屏", "Borderless fullscreen");
            Row(0, L("显示模式", "Display mode"), ModeName(), () =>
            { draft.mode = (int)modes[(Array.IndexOf(modes, (FullScreenMode)draft.mode) + 1) % modes.Length]; Render(); });
            Row(1, L("分辨率", "Resolution"), $"{draft.width} × {draft.height}", () =>
            {
                var size = resolutions[(resolutions.IndexOf(new Vector2Int(draft.width, draft.height)) + 1) % resolutions.Count];
                draft.width = size.x; draft.height = size.y; Render();
            });
            Row(2, L("画质", "Quality"), QualitySettings.names[draft.quality], () =>
            { draft.quality = (draft.quality + 1) % QualitySettings.names.Length; Render(); });
            Toggle(3, "垂直同步", "VSync", draft.vsync, () => draft.vsync = !draft.vsync);
            Row(4, L("帧率上限", "Frame limit"), draft.vsync ? L("由垂直同步控制", "Controlled by VSync")
                : draft.fps < 0 ? L("不限", "Unlimited") : draft.fps.ToString(), draft.vsync ? null : () =>
            { draft.fps = frameRates[(Array.IndexOf(frameRates, draft.fps) + 1) % frameRates.Length]; Render(); });
            Toggle(5, "后台继续运行", "Run in background", draft.runBackground, () => draft.runBackground = !draft.runBackground);
            if (Application.isEditor) status.text = L("编辑器内不改变窗口分辨率；请在独立构建中验证显示切换。", "Resolution switching is available in standalone builds.");
        }
        if (tab == 1)
        {
            Row(0, L("主音量", "Master volume"), $"{draft.master:P0}", null, draft.master);
            Row(1, L("音乐音量", "Music volume"), $"{draft.music:P0}", null, draft.music);
            Row(2, L("音效音量", "Effects volume"), $"{draft.sound:P0}", null, draft.sound);
            Toggle(3, "环境音", "Ambience", draft.ambience, () => draft.ambience = !draft.ambience);
            Toggle(4, "失去焦点时静音", "Mute when unfocused", draft.muteBackground, () => draft.muteBackground = !draft.muteBackground);
        }
        if (tab == 2)
        {
            var locales = LocalizationSettings.AvailableLocales.Locales
                .Where(x => x.Identifier.Code == "en" || x.Identifier.Code == "zh-Hans").ToList();
            Row(0, L("语言", "Language"), draft.locale.StartsWith("zh") ? "简体中文" : "English", locales.Count == 0 ? null : () =>
            {
                int current = locales.FindIndex(x => x.Identifier.Code == draft.locale);
                draft.locale = locales[(current + 1) % locales.Count].Identifier.Code; Render();
            });
            Toggle(1, "屏幕震动", "Screen shake", draft.screenShake, () => draft.screenShake = !draft.screenShake);
        }
        if (tab == 3)
        {
            Row(0, L("事件确认键", "Event confirm key"), ((KeyCode)draft.confirmKey).ToString(), () => Capture(0));
            Row(1, L("游戏取消键", "Game cancel key"), ((KeyCode)draft.cancelKey).ToString(), () => Capture(1));
            Row(2, L("菜单导航", "Menu navigation"), L("方向键 / 手柄方向键", "Arrow keys / gamepad"), null);
            Row(3, L("菜单确认", "Menu submit"), L("Enter / 手柄 A", "Enter / gamepad A"), null);
            Row(4, L("菜单返回", "Menu back"), L("Esc / 手柄 B", "Esc / gamepad B"), null);
            status.text = L("选择按键后按新键；Esc 取消绑定。菜单导航保留标准按键。", "Select a binding, then press a key. Esc cancels capture. Menu controls remain standard.");
        }
    }

    void Capture(int which)
    {
        capturingKey = which;
        captureStartedFrame = Time.frameCount;
        status.text = L("请按新按键，Esc 取消。", "Press a new key. Esc cancels.");
    }

    void Apply()
    {
        if (awaitingDisplay || capturingKey >= 0) return;
        var old = MenuPreferences.Current;
        if (!Application.isEditor && (old.width != draft.width || old.height != draft.height || old.mode != draft.mode))
        {
            previousDisplay = old.Copy();
            // 回滚到实际窗口状态，兼容玩家此前通过系统快捷键改变显示模式。
            previousDisplay.width = Screen.width;
            previousDisplay.height = Screen.height;
            previousDisplay.mode = (int)Screen.fullScreenMode;
            awaitingDisplay = true;
            confirmDeadline = Time.realtimeSinceStartup + 15;
            MenuPreferences.ApplyDisplay(draft);
            Render();
            Find<Button>("Panel/Revert").Select();
            return;
        }
        Save();
    }

    void Save()
    {
        MenuPreferences.Commit(draft);
        draft = MenuPreferences.Current.Copy();
        Render();
        status.text = L("设置已保存。", "Settings saved.");
        Find<Button>("Panel/Apply").Select();
    }

    void RevertDisplay()
    {
        if (!awaitingDisplay) return;
        awaitingDisplay = false;
        MenuPreferences.ApplyDisplay(previousDisplay);
        draft.width = previousDisplay.width;
        draft.height = previousDisplay.height;
        draft.mode = previousDisplay.mode;
        AddResolution(new Vector2Int(draft.width, draft.height));
        Render();
        status.text = L("显示设置已恢复，其他修改尚未应用。", "Display reverted. Other changes have not been applied.");
        Find<Button>("Panel/Apply").Select();
    }

    public override bool onESCDown()
    {
        if (capturingKey >= 0) { capturingKey = -1; Render(); return true; }
        if (awaitingDisplay) { RevertDisplay(); return true; }
        return base.onESCDown();
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
        if (awaitingDisplay)
        {
            int seconds = Mathf.CeilToInt(confirmDeadline - Time.realtimeSinceStartup);
            if (seconds <= 0) RevertDisplay();
            else status.text = L($"保留显示设置？{seconds} 秒后自动恢复。", $"Keep display settings? Reverting in {seconds}s.");
        }
        if (capturingKey < 0 || Time.frameCount == captureStartedFrame) return;
        if (Input.GetKeyDown(KeyCode.Escape)) { capturingKey = -1; Render(); return; }
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (!MenuPreferences.ValidKey((int)key) || !Input.GetKeyDown(key)) continue;
            int other = capturingKey == 0 ? draft.cancelKey : draft.confirmKey;
            if ((int)key == other)
            {
                status.text = L("该按键已分配给另一操作，请选择其他按键。", "Key already assigned. Choose another key.");
                return;
            }
            if (capturingKey == 0) draft.confirmKey = (int)key; else draft.cancelKey = (int)key;
            capturingKey = -1; Render(); return;
        }
    }

    public override void onHide()
    {
        RevertDisplay();
        capturingKey = -1;
        base.onHide();
    }

    public override void destroy()
    {
        RevertDisplay();
        base.destroy();
    }
}
