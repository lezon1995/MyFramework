using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Obfuz;
using UnityEngine;
using UnityEngine.Localization;

namespace MoreMountains;

[ObfuzIgnore(ObfuzScope.TypeName)]
public sealed class EncyclopediaScreen : MainMenuPage
{
    readonly List<string> entries = new();
    int category;
    int index;

    public override void init()
    {
        base.init();
        Click("Panel/Category1", () => ChangeCategory(0));
        Click("Panel/Category2", () => ChangeCategory(1));
        Click("Panel/Category3", () => ChangeCategory(2));
        Click("Panel/Previous", () => { index = Mathf.Max(0, index - 1); Render(); });
        Click("Panel/Next", () => { index = Mathf.Min(entries.Count - 1, index + 1); Render(); });
    }

    protected override void Refresh()
    {
        title.text = L("百科大全", "Encyclopedia");
        Label("Panel/Category1/Text", L("角色", "Characters"));
        Label("Panel/Category2/Text", L("球", "Balls"));
        Label("Panel/Category3/Text", L("遗物", "Relics"));
        Label("Panel/Previous/Text", L("上一项", "Previous"));
        Label("Panel/Next/Text", L("下一项", "Next"));
        ChangeCategory(category);
    }

    static string Localized(LocalizedString value, string fallback) =>
        value == null || value.IsEmpty ? fallback : value.GetLocalizedString();

    void ChangeCategory(int value)
    {
        category = value;
        index = 0;
        entries.Clear();
        if (category == 0 && characterManager != null)
            foreach (var def in characterManager.getDefs().Where(x => x != null).OrderBy(x => x.Type))
                entries.Add($"{def.DisplayName}\n\n{def.DisplayDesc}\n\n{def.DisplayStats}");
        if (category == 1)
            foreach (var def in BallDefLibrary.Instance.All.OrderBy(x => x.Type))
                entries.Add($"{Localized(def.DisplayName, def.Type.ToString())}\n\n" +
                    $"{Localized(def.DisplayDescription, L("暂无描述", "No description yet"))}\n\n" +
                    $"{L("稀有度", "Rarity")}: {def.Rarity}\n{L("基础价格", "Base price")}: {def.BasePrice}\n" +
                    $"{L("最高等级", "Max level")}: {def.maxLevel}");
        if (category == 2 && relicManager != null)
            foreach (var def in relicManager.getDefs().Where(x => x != null).OrderBy(x => x.Type))
                entries.Add($"{Localized(def.DisplayName, def.Type.ToString())}\n\n" +
                    $"{Localized(def.DisplayDescription, L("暂无描述", "No description yet"))}\n\n" +
                    $"{L("稀有度", "Rarity")}: {def.Rarity}\n{L("基础价格", "Base price")}: {def.BasePrice}");
        Render();
    }

    void Render()
    {
        Label("Panel/Scroll/Viewport/Body", entries.Count == 0
            ? L("当前分类尚无内容。", "No entries in this category yet.") : entries[index]);
        status.text = entries.Count == 0 ? "0 / 0" : $"{index + 1} / {entries.Count}";
        Find<UnityEngine.UI.Button>("Panel/Previous").interactable = index > 0;
        Find<UnityEngine.UI.Button>("Panel/Next").interactable = index + 1 < entries.Count;
        Find<UnityEngine.UI.ScrollRect>("Panel/Scroll").verticalNormalizedPosition = 1;
    }
}

[ObfuzIgnore(ObfuzScope.TypeName)]
public sealed class StatisticsScreen : MainMenuPage
{
    protected override void Refresh()
    {
        title.text = L("游戏统计", "Statistics");
        // 当前唯一实现的角色 Ironclad 使用 DataVagabond；不构造需要活动玩家的 CharStat。
        var prefs = SaveHelper.getPrefs("DataVagabond");
        var builder = new StringBuilder();
        void Row(string zh, string en, string key) =>
            builder.AppendLine($"{L(zh, en)}    {prefs.getInteger(key, 0)}");
        Row("胜利次数", "Victories", CharStat.WIN_COUNT);
        Row("失败次数", "Defeats", CharStat.LOSE_COUNT);
        Row("最高分数", "Highest score", CharStat.HIGHEST_SCORE);
        Row("最高层数", "Highest floor", CharStat.HIGHEST_FLOOR);
        Row("累计层数", "Total floors", CharStat.TOTAL_FLOORS);
        Row("敌人击败数", "Enemies defeated", CharStat.ENEMY_KILL);
        Row("首领击败数", "Bosses defeated", CharStat.BOSS_KILL);
        Row("最佳连胜", "Best win streak", CharStat.BEST_WIN_STREAK);
        var time = TimeSpan.FromSeconds(Math.Max(0, prefs.getLong(CharStat.PLAYTIME, 0)));
        builder.AppendLine($"{L("累计游玩时间", "Total playtime")}    {(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}");
        Label("Panel/Scroll/Viewport/Body", builder.ToString());
        status.text = L("当前存档槽的已记录数据；尚无记录的项目显示为 0。", "Recorded data for this save slot; unrecorded values are 0.");
    }
}

[ObfuzIgnore(ObfuzScope.TypeName)]
public sealed class PatchNotesScreen : MainMenuPage
{
    protected override void Refresh()
    {
        title.text = L("补丁说明", "Patch notes") + " · " + Application.version;
        var notes = Resources.Load<TextAsset>(MenuPreferences.IsChinese ? "Menu/PatchNotes_zh" : "Menu/PatchNotes_en");
        Label("Panel/Scroll/Viewport/Body", notes != null ? notes.text : L("暂无补丁说明。", "No patch notes available."));
        status.text = L("本地版本说明", "Notes bundled with this build");
    }
}

[ObfuzIgnore(ObfuzScope.TypeName)]
public sealed class MainMenuConfirmScreen : MainMenuPage
{
    Action confirmed;
    public override void init()
    {
        base.init();
        Click("Panel/Confirm", () => confirmed?.Invoke());
    }

    protected override void Refresh() { status.text = ""; }

    public void Configure(string heading, string body, Action action)
    {
        title.text = heading;
        Label("Panel/Scroll/Viewport/Body", body);
        Label("Panel/Confirm/Text", L("确认", "Confirm"));
        Label("Panel/Back/Text", L("取消", "Cancel"));
        confirmed = action;
        back.Select();
    }

    public void ShowError(string message) => status.text = message;
    public override void onHide() { confirmed = null; base.onHide(); }
}
