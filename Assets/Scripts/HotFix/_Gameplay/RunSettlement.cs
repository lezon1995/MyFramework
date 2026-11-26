using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MoreMountains;

/// <summary>一次性的终局锁和结算快照。结算不杀死剩余实体，不产生清场奖励。</summary>
public static class RunSettlement
{
    public sealed class DamageEntry
    {
        public int Id;
        public string Name;
        public double Damage;
    }

    public sealed class Summary
    {
        public GameResult Result;
        public int Kills, Wave, MaxWave, Ascension;
        public float Seconds;
        public string Character, Level;
        public string[] Balls, Relics;
        public DamageEntry[] Damage;
        public double TotalDamage => Damage.Sum(row => row.Damage);
    }

    static readonly Dictionary<int, DamageEntry> damage = new();
    static WaveGameMode mode;
    static RunResultScreen screen;
    static bool presented;
    static string savePath;
    public static bool IsFinished { get; private set; }
    public static Summary Current { get; private set; }

    public static void Begin(WaveGameMode currentMode)
    {
        Reset();
        mode = currentMode;
    }

    public static void Reset()
    {
        screen?.close();
        screen = null;
        mode = null;
        damage.Clear();
        Current = null;
        IsFinished = presented = false;
        savePath = null;
    }

    // 聚合同一种球各等级的伤害。无球来源的遗物、环境及持续伤害单列，避免误归因。
    public static void RecordDamage(BallDef def, double actualDamage)
    {
        if (IsFinished || actualDamage <= 0 || double.IsNaN(actualDamage) || double.IsInfinity(actualDamage)) return;
        int id = def != null ? def.BallDefId : -1;
        if (!damage.TryGetValue(id, out var row))
        {
            row = new DamageEntry { Id = id, Name = def != null ? def.DisplayName.GetLocalizedString() : "其他来源 / Other sources" };
            damage.Add(id, row);
        }
        row.Damage += actualDamage;
    }

    public static void RecordAppliedDamage(BallDef def, int rawDamage, int healthDamage, int healthBefore)
    {
        int shieldDamage = Math.Max(0, rawDamage - healthDamage);
        RecordDamage(def, (double)shieldDamage + Math.Min(Math.Max(0, healthBefore), Math.Max(0, healthDamage)));
    }

    public static BallDef ResolveBall(GameObject instigator)
    {
        if (instigator == null) return null;
        var ball = instigator.GetComponentInParent<Ball>();
        if (ball != null) return ball.Def;
        var projectile = instigator.GetComponentInParent<Projectile>();
        return projectile != null && projectile.SourceWeapon is BallWeapon weapon ? weapon.BallDef : null;
    }

    public static void Finish(GameResult result)
    {
        if (IsFinished || Game.RunExitPending || Game.mode != Game.GameMode.GAMEPLAY ||
            result == GameResult.None || GBR.player == null) return;
        IsFinished = true; // 在任何回调前锁定，防止同帧多次阵亡/击杀覆盖结果。
        var player = GBR.player;
        var inventory = player.Inventory;
        if (inventory?.BallBag != null)
            foreach (var slot in inventory.BallBag.SlotList)
                if (slot.Item != null && !damage.ContainsKey(slot.Item.ItemId))
                    damage.Add(slot.Item.ItemId, new DamageEntry { Id = slot.Item.ItemId, Name = slot.Item.Def.DisplayName.GetLocalizedString() });
        Current = new Summary
        {
            Result = result, Kills = mode != null ? mode.TotalKills : 0,
            Wave = mode != null ? mode.CurWave : 0, MaxWave = mode != null ? mode.MaxWave : 0,
            Seconds = mode != null ? mode.TotalPlayTime : 0, Ascension = ADungeon.ascensionLevel,
            Character = player.chosenClass.ToString(), Level = mode != null ? mode.CurrentLevelConfig?.levelName : "",
            Balls = inventory?.BallBag?.SlotList.Where(s => s.Item != null).Select(s => s.Item.DisplayName).ToArray() ?? Array.Empty<string>(),
            Relics = inventory?.RelicBag?.SlotList.Where(s => s.Item != null).Select(s => s.Item.DisplayName).ToArray() ?? Array.Empty<string>(),
            Damage = damage.Values.Select(r => new DamageEntry { Id = r.Id, Name = r.Name, Damage = r.Damage }).OrderByDescending(r => r.Damage).ToArray()
        };
        savePath = SaveAndContinue.getPlayerSavePath(Current.Character);
        EscPanelService.Instance.Close();
        OperationPanelService.Instance.Close();
        OverlayMenuService.Instance.Close();
        foreach (var weapon in player.mainHandleWeapons) weapon.ForceStop();
        foreach (var weapon in player.ballMetaHandleWeapons.Values) weapon.ForceStop();
        foreach (var weapon in player.relicMetaHandleWeapons.Values) weapon.ForceStop();
        if (GBR.brickManager != null)
            foreach (var enemy in GBR.brickManager.getActiveBrickList().ToArray())
                if (enemy != null && enemy.CharacterBrain != null) enemy.CharacterBrain.enabled = false;
        Game.stopClock = true;
        if (GameManager.Instance != null) GameManager.Instance.Pause(unpauseIfPaused: false);
        Time.timeScale = 0;
    }

    /// <summary>在 Game 帧边界打开 UI，避开 Health、波次事件的当前调用栈。</summary>
    public static void Present()
    {
        if (!IsFinished) return;
        Time.timeScale = 0;
        if (GameManager.Instance != null) GameManager.Instance.Paused = true;
        if (presented) return;
        screen = Current.Result == GameResult.Victory ? LT.LOAD_TOP<VictoryScreen>() : LT.LOAD_TOP<DefeatScreen>();
        screen.Open(Current);
        presented = true;
        TryDeleteSave();
    }

    static bool TryDeleteSave()
    {
        try
        {
            SaveAndContinue.deleteSave(savePath);
            if (File.Exists(savePath) || File.Exists(savePath + ".backUp")) throw new IOException("Save still exists.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Settlement] Cannot remove finished run save: " + e.Message);
            screen?.ShowError("无法删除本局存档，请检查文件权限后重试。 / Could not remove run save. Check file permissions and retry.");
            return false;
        }
    }

    public static void Exit(bool restart)
    {
        if (!IsFinished || Game.RunExitPending || !TryDeleteSave()) return;
        Game.RequestRunExit(restart);
    }
}
