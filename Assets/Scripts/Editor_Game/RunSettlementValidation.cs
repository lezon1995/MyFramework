using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains;

/// <summary>隔离验证结算统计与实际预制体绑定，不启动战斗、不读写玩家存档。</summary>
public static class RunSettlementValidation
{
    [MenuItem("Tools/Game UI/Validate Run Settlement")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Run outside Play mode.");
        var runtime = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotFix");
        var settlement = runtime.GetType("MoreMountains.RunSettlement", true);
        if ((bool)settlement.GetProperty("IsFinished").GetValue(null)) throw new InvalidOperationException("A settlement is active.");
        var entries = settlement.GetField("damage", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as System.Collections.IDictionary;
        if (entries.Count != 0) throw new InvalidOperationException("A run is active.");
        var record = settlement.GetMethod("RecordDamage");
        var reset = settlement.GetMethod("Reset");
        try
        {
            record.Invoke(null, new object[] { null, 25d });
            record.Invoke(null, new object[] { null, 75d });
            record.Invoke(null, new object[] { null, 0d });
            record.Invoke(null, new object[] { null, -1d });
            record.Invoke(null, new object[] { null, double.NaN });
            record.Invoke(null, new object[] { null, double.PositiveInfinity });
            var rowType = runtime.GetType("MoreMountains.RunSettlement+DamageEntry", true);
            Require(entries.Count == 1 && (double)rowType.GetField("Damage").GetValue(entries[-1]) == 100d, "Damage accumulation or invalid-damage filter failed.");
            var applied = settlement.GetMethod("RecordAppliedDamage");
            applied.Invoke(null, new object[] { null, 100, 80, 30 }); // 20 护盾 + 30 生命，排除 50 过量。
            Require((double)rowType.GetField("Damage").GetValue(entries[-1]) == 150d, "Overkill cap or shield damage failed.");
            var terminal = settlement.GetField("<IsFinished>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
            terminal.SetValue(null, true);
            record.Invoke(null, new object[] { null, 500d });
            Require((double)rowType.GetField("Damage").GetValue(entries[-1]) == 150d, "Damage changed after terminal state.");
            terminal.SetValue(null, false);
            ValidateBossCompletion(runtime);
            foreach (var name in new[] { "DefeatScreen", "VictoryScreen" }) ValidatePage(runtime, name, rowType);
            Debug.Log("[Settlement] PASS: accumulated damage, invalid input, terminal lock, both prefab bindings, defeat/victory controls, zero-damage display, percentages and ESC guard. No saves changed.");
        }
        finally { reset.Invoke(null, null); }
    }

    static void ValidateBossCompletion(Assembly runtime)
    {
        var managerType = runtime.GetType("MoreMountains.WaveManager", true);
        var levelType = runtime.GetType("MoreMountains.WaveLevelConfig", true);
        var configType = runtime.GetType("MoreMountains.WaveConfig", true);
        var brickType = runtime.GetType("MoreMountains.Brick", true);
        var owner = new GameObject("SettlementIsolatedTest");
        owner.SetActive(false); // 不运行实体 Awake/Start，不接触实际关卡。
        var level = ScriptableObject.CreateInstance(levelType);
        try
        {
            var manager = owner.AddComponent(managerType);
            var enemyObject = new GameObject("Boss");
            enemyObject.transform.SetParent(owner.transform);
            enemyObject.SetActive(false);
            var boss = enemyObject.AddComponent(brickType);
            var waves = (System.Collections.IList)levelType.GetField("waves").GetValue(level);
            waves.Add(Activator.CreateInstance(configType)); waves.Add(Activator.CreateInstance(configType));
            managerType.GetProperty("CurLevel").SetValue(manager, level);
            managerType.GetProperty("CurWave").SetValue(manager, waves[1]);
            var monsters = (System.Collections.IList)managerType.GetProperty("ActiveMonsters").GetValue(manager);
            var bosses = (System.Collections.IList)managerType.GetProperty("ActiveBosses").GetValue(manager);
            var unregister = managerType.GetMethod("UnregisterMonster");
            managerType.GetProperty("WaveNumber").SetValue(manager, 1);
            monsters.Add(boss); bosses.Add(boss);
            unregister.Invoke(manager, new[] { boss, null });
            Require(managerType.GetProperty("FinalResult").GetValue(manager).ToString() == "None", "Intermediate boss must not win the run.");
            managerType.GetProperty("WaveNumber").SetValue(manager, 2);
            monsters.Add(boss); bosses.Add(boss);
            unregister.Invoke(manager, new[] { boss, null });
            Require(managerType.GetProperty("FinalResult").GetValue(manager).ToString() == "Victory", "Final boss must immediately win.");
            Require((int)managerType.GetProperty("WaveKillCount").GetValue(manager) == 2, "Final boss must count as a kill.");
            unregister.Invoke(manager, new[] { boss, null });
            managerType.GetMethod("ForceWaveFailed").Invoke(manager, null);
            Require(managerType.GetProperty("FinalResult").GetValue(manager).ToString() == "Victory" &&
                (int)managerType.GetProperty("WaveKillCount").GetValue(manager) == 2, "Duplicate death/late defeat must not overwrite victory.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(owner);
            UnityEngine.Object.DestroyImmediate(level);
        }
    }

    static void ValidatePage(Assembly runtime, string name, Type rowType)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameResources/UI/UIPrefab/" + name + ".prefab");
        var instance = UnityEngine.Object.Instantiate(prefab);
        try
        {
            var type = runtime.GetType("MoreMountains." + name, true);
            var page = (LayoutScript)Activator.CreateInstance(type);
            typeof(LayoutScript).GetField("mTransform", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(page, (RectTransform)instance.transform);
            page.assignWindow();
            page.init();
            var summaryType = runtime.GetType("MoreMountains.RunSettlement+Summary", true);
            var summary = Activator.CreateInstance(summaryType);
            Set(summary, "Result", Enum.Parse(runtime.GetType("MoreMountains.GameResult"), name == "VictoryScreen" ? "Victory" : "Defeat"));
            Set(summary, "Character", "测试角色"); Set(summary, "Level", "测试关卡");
            Set(summary, "Kills", 128); Set(summary, "Wave", 20); Set(summary, "MaxWave", 20); Set(summary, "Ascension", 3); Set(summary, "Seconds", 1265f);
            Set(summary, "Balls", new[] { "普通球 Lv.4", "火球 Lv.3", "冰球 Lv.2" });
            Set(summary, "Relics", new[] { "白模遗物 A", "白模遗物 B" });
            var rows = Array.CreateInstance(rowType, 2);
            for (int i = 0; i < 2; i++)
            {
                var row = Activator.CreateInstance(rowType);
                Set(row, "Name", i == 0 ? "普通球" : "火球");
                Set(row, "Damage", i == 0 ? 7500d : 2500d);
                rows.SetValue(row, i);
            }
            Set(summary, "Damage", rows);
            type.GetMethod("Open").Invoke(page, new[] { summary });
            Require(instance.transform.Find("Panel/Restart").gameObject.activeSelf == (name == "DefeatScreen"), "Restart visibility mismatch.");
            float originalScale = Time.timeScale;
            try
            {
                Time.timeScale = 0;
                page.update(0);
                Require(instance.transform.Find("Panel").localScale.x > 0, "Panel must remain visible while paused.");
            }
            finally { Time.timeScale = originalScale; }
            Require(page.onESCDown(), "Settlement must consume Escape.");
            var body = instance.transform.Find("Panel/Damage/Viewport/Body").GetComponent<TMP_Text>();
            Require(body.text.Contains("75") && body.text.Contains("25"), "Damage shares not displayed.");
            Set(summary, "Damage", Array.CreateInstance(rowType, 0));
            type.GetMethod("Open").Invoke(page, new[] { summary });
            Require(!body.text.Contains("NaN"), "Zero damage must not produce NaN.");
            Set(summary, "Damage", rows);
            type.GetMethod("Open").Invoke(page, new[] { summary });
            instance.transform.Find("Panel/Celebration").GetComponent<CanvasGroup>().alpha = 1;
            MainMenuPrefabBuilder.PreviewResult(instance, name);
        }
        finally { if (instance != null) UnityEngine.Object.DestroyImmediate(instance); }
    }

    static void Set(object target, string name, object value) => target.GetType().GetField(name).SetValue(target, value);
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
