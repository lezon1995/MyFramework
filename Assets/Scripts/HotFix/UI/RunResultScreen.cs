using System;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains;

/// <summary>胜负结算共用布局；只读取终局快照，动画不受战场暂停影响。</summary>
public abstract class RunResultScreen : LayoutScript
{
    TMP_Text title, overview, equipment, damage, status;
    Button restart, home;
    RectTransform panel, emblem;
    CanvasGroup celebration;
    float animationTime;
    bool victory;
    static string L(string zh, string en) => MenuPreferences.IsChinese ? zh : en;
    T Find<T>(string path) where T : Component => mTransform.Find("Panel/" + path).GetComponent<T>();

    public override void assignWindow()
    {
        panel = Find<RectTransform>("");
        title = Find<TMP_Text>("Title");
        overview = Find<TMP_Text>("Overview");
        equipment = Find<TMP_Text>("Equipment/Viewport/Body");
        damage = Find<TMP_Text>("Damage/Viewport/Body");
        status = Find<TMP_Text>("Status");
        restart = Find<Button>("Restart");
        home = Find<Button>("Home");
        emblem = Find<RectTransform>("Celebration");
        celebration = emblem.GetComponent<CanvasGroup>();
    }

    public override void init()
    {
        base.init();
        restart.onClick.AddListener(() => RunSettlement.Exit(true));
        home.onClick.AddListener(() => RunSettlement.Exit(false));
    }

    public void Open(RunSettlement.Summary summary)
    {
        victory = summary.Result == GameResult.Victory;
        animationTime = 0;
        title.text = victory ? L("游戏胜利", "VICTORY") : L("游戏失败", "DEFEAT");
        title.color = victory ? new Color(1, .84f, .4f) : new Color(1, .55f, .55f);
        emblem.gameObject.SetActive(victory);
        celebration.alpha = 0;
        emblem.GetComponent<TMP_Text>().text = L("-  胜 利 凯 旋  -", "-  TRIUMPH  -");
        var duration = TimeSpan.FromSeconds(Math.Max(0, summary.Seconds));
        overview.text = $"{summary.Character}    {summary.Level}\n" +
            L($"击杀 {summary.Kills:N0}    波次 {summary.Wave} / {summary.MaxWave}    难度：进阶 {summary.Ascension}    时长 {duration:hh\\:mm\\:ss}",
              $"Kills {summary.Kills:N0}    Wave {summary.Wave} / {summary.MaxWave}    Ascension {summary.Ascension}    Time {duration:hh\\:mm\\:ss}");
        equipment.text = L("<b>已装备的球</b>\n", "<b>Equipped balls</b>\n") +
            (summary.Balls.Length == 0 ? L("无", "None") : string.Join("\n", summary.Balls)) +
            L("\n\n<b>已装备的遗物</b>\n", "\n\n<b>Equipped relics</b>\n") +
            (summary.Relics.Length == 0 ? L("无", "None") : string.Join("\n", summary.Relics));
        var text = new StringBuilder(L("<b>累计伤害 / 伤害占比</b>\n", "<b>Total damage / Share</b>\n"));
        double total = summary.TotalDamage;
        foreach (var row in summary.Damage)
        {
            double share = total > 0 ? row.Damage / total : 0;
            text.AppendLine($"{row.Name}\n{row.Damage:N0}    <color=#F3CD77>{share:P1}</color>\n");
        }
        if (summary.Damage.Length == 0) text.AppendLine(L("本局尚未造成伤害。", "No damage dealt this run."));
        text.AppendLine(L($"合计：{total:N0}\n\n有效伤害，包含护盾伤害。\n同种球的不同等级合并统计。",
            $"Total: {total:N0}\n\nEffective damage, including shields.\nGrouped by ball type across all levels."));
        damage.text = text.ToString();
        status.text = "";
        restart.gameObject.SetActive(!victory);
        ((RectTransform)home.transform).anchoredPosition = new Vector2(victory ? 0 : 340, -430);
        Find<TMP_Text>("Restart/Text").text = L("重来", "Restart");
        Find<TMP_Text>("Home/Text").text = victory ? L("胜利凯旋", "Return in triumph") : L("退回大厅", "Return to lobby");
        foreach (var name in new[] { "Equipment", "Damage" }) Find<ScrollRect>(name).verticalNormalizedPosition = 1;
        if (victory) home.Select(); else restart.Select();
    }

    public void ShowError(string message) => status.text = message;
    public override bool onESCDown() => true;

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
        var size = mTransform.rect.size;
        panel.localScale = Vector3.one * Mathf.Min(size.x / 1920f, size.y / 1080f);
        if (!victory) return;
        animationTime += Time.unscaledDeltaTime;
        float intro = Mathf.Clamp01(animationTime / .7f);
        celebration.alpha = intro;
        emblem.localScale = Vector3.one * (Mathf.Lerp(.4f, 1f, 1 - Mathf.Pow(1 - intro, 3)) + .025f * Mathf.Sin(animationTime * 3));
        emblem.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(animationTime * 2) * 2);
    }
}

public sealed class DefeatScreen : RunResultScreen { }
public sealed class VictoryScreen : RunResultScreen { }
