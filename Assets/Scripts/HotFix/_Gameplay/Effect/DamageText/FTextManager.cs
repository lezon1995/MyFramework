using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MoreMountains;

public class FTextManager : FrameSystem
    , IEvent<DmgTextEvent>
    , IEvent<HealTextEvent>
    , IEvent<GainCoinTextEvent>
{
    GameObject textParent;

    Dictionary<TextType, SafeList<FText>> usings = new()
    {
        { TextType.Damage, new() },
        { TextType.DamageCrit, new() },
        { TextType.Healing, new() },
        { TextType.GainCoin, new() },
    };

    Dictionary<TextType, List<FText>> unused = new()
    {
        { TextType.Damage, new() },
        { TextType.DamageCrit, new() },
        { TextType.Healing, new() },
        { TextType.GainCoin, new() },
    };

    Dictionary<TextType, Dictionary<Transform, FText>> reusedTexts = new()
    {
        { TextType.Damage, new() },
        { TextType.DamageCrit, new() },
        { TextType.Healing, new() },
        { TextType.GainCoin, new() },
    };

    Dictionary<TextType, FTextSetting> settings = new();

    public FTextManager()
    {
        mCreateObject = true;
    }

    public override void init()
    {
        base.init();

        initCanvas();
        initSettings();

        this.addListener<DmgTextEvent>();
        this.addListener<HealTextEvent>();
        this.addListener<GainCoinTextEvent>();
    }

    void initCanvas()
    {
        textParent = createGameObject("FTextCanvas", mObject);
        var canvas = textParent.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var canvasScaler = textParent.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new(1920, 1080);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.referencePixelsPerUnit = 100;
    }

    void initSettings()
    {
        var damage = resource.loadGameResource<FTextSetting>($"{GAMEPLAY_PATH}/FTextSetting_Damage.asset");
        var damage_Crit = resource.loadGameResource<FTextSetting>($"{GAMEPLAY_PATH}/FTextSetting_Damage_Crit.asset");
        var healing = resource.loadGameResource<FTextSetting>($"{GAMEPLAY_PATH}/FTextSetting_Healing.asset");
        var gainCoin = resource.loadGameResource<FTextSetting>($"{GAMEPLAY_PATH}/FTextSetting_GainCoin.asset");
        settings.add(TextType.Damage, damage.getResource());
        settings.add(TextType.DamageCrit, damage_Crit.getResource());
        settings.add(TextType.Healing, healing.getResource());
        settings.add(TextType.GainCoin, gainCoin.getResource());
    }

    public override void destroy()
    {
        base.destroy();
        this.removeListener<DmgTextEvent>();
        this.removeListener<HealTextEvent>();
        this.removeListener<GainCoinTextEvent>();
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);

        foreach (var (type, _usings) in usings)
        {
            using var a = new SafeListReader<FText>(_usings);
            foreach (var text in a.mReadList)
            {
                if (text && text.isActiveInHierarchy())
                {
                    var dt = !text.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime;
                    text.update(dt);
                }
            }
        }
    }

    public FTextSetting getSetting(TextType type)
    {
        settings.TryGetValue(type, out var setting);
        return setting;
    }

    public void show(FText.Data data)
    {
        var text = getText(data);
        text.Set(data);
        text.getTransform().SetAsLastSibling();
    }

    FText getText(FText.Data data)
    {
        var type = data.textType;
        FText text;
        if (data.reuseTimes is > 0 or -2 && data.target)
        {
            if (reusedTexts[type].TryGetValue(data.target, out text))
            {
                switch (text.useTimes)
                {
                    case > 0:
                        text.useTimes--;
                        return text;
                    case -2:
                        return text;
                }
            }
        }

        if (unused[type].any())
        {
            text = unused[type].popBack();
        }
        else
        {
            text = CLASS<FText>();
            string path = type switch
            {
                TextType.Damage => $"{GAMEPLAY_PATH}/FText_Damage.prefab",
                TextType.DamageCrit => $"{GAMEPLAY_PATH}/FText_Damage.prefab",
                TextType.Healing => $"{GAMEPLAY_PATH}/FText_Healing.prefab",
                TextType.GainCoin => $"{GAMEPLAY_PATH}/FText_GainCoin.prefab",
                _ => throw new ArgumentOutOfRangeException()
            };
            text.setName($"FText_{type}");
            var o = prefabPool.createObject(path, true, textParent);
            text.setObject(o);
        }

        usings[type].add(text);
        return text;
    }

    public void release(TextType type, FText text)
    {
        text.Clear();
        usings[type].remove(text);
        unused[type].add(text);
    }

    public void addToReused(FText text, FText.Data data)
    {
        reusedTexts[data.textType].TryAdd(data.target, text);
    }

    public void removeFromReused(FText.Data data)
    {
        reusedTexts[data.textType].Remove(data.target);
    }

    public static void showDamage(Transform target, Dmg dmg)
    {
        if (dmg.Self)
            return;

        var type = dmg.IsCrit ? TextType.DamageCrit : TextType.Damage;
        var mix = dmg.Mix;
        if (mix.Off)
        {
            new FText.Data($"{dmg.DamageDealt:F0}", type)
                .setSetting(type)
                .setValue(dmg.DamageDealt)
                .setDirection(dmg.Direction)
                .setTarget(target)
                .setOffset(Random.insideUnitCircle * 0.15F)
                .setExtraContentSize(Mathf.InverseLerp(50, 1000, dmg.DamageDealt) * 0.25F) //this should be based on the amount of damage
                .setType((int)dmg.ActualType)
                .show();

            return;
        }

        var damage = mix.DamageDealtAD;
        if ((int)damage > 0)
        {
            new FText.Data($"{damage:F0}", type)
                .setSetting(type)
                .setValue(damage)
                .setDirection(dmg.Direction)
                .setTarget(target)
                .setOffset(Random.insideUnitCircle * 0.15F)
                .setExtraContentSize(Mathf.InverseLerp(50, 1000, dmg.DamageDealt) * 0.25F) //this should be based on the amount of damage
                .setType((int)Dmg.Types.AD)
                .show();
        }

        damage = mix.DamageDealtAP;
        if ((int)damage > 0)
        {
            new FText.Data($"{damage:F0}", type)
                .setSetting(type)
                .setValue(damage)
                .setDirection(dmg.Direction)
                .setTarget(target)
                .setOffset(Random.insideUnitCircle * 0.15F)
                .setExtraContentSize(Mathf.InverseLerp(50, 1000, dmg.DamageDealt) * 0.25F) //this should be based on the amount of damage
                .setType((int)Dmg.Types.AP)
                .show();
        }

        damage = mix.DamageDealtTrue;
        if ((int)damage > 0)
        {
            new FText.Data($"{damage:F0}", type)
                .setSetting(type)
                .setValue(damage)
                .setDirection(dmg.Direction)
                .setTarget(target)
                .setOffset(Random.insideUnitCircle * 0.15F)
                .setExtraContentSize(Mathf.InverseLerp(50, 1000, dmg.DamageDealt) * 0.25F) //this should be based on the amount of damage
                .setType((int)Dmg.Types.True)
                .show();
        }
    }

    public static void showGainCoin(Transform target, int coin)
    {
        const TextType TYPE = TextType.GainCoin;
        new FText.Data($"+ {coin}", TYPE)
            .setSetting(TYPE)
            .setValue(coin)
            .setDirection(Vector3.up)
            .setTarget(target)
            .setOffset(new(0, 0.65F, 0))
            .setExtraContentSize(0F) //this should be based on the amount of damage
            .setType(0)
            .show();
    }


    public void onEvent(DmgTextEvent e)
    {
        showDamage(e.Target, e.Dmg);
    }

    public void onEvent(HealTextEvent e)
    {
        showHealing(e.Target, e.Heal);
    }

    public void onEvent(GainCoinTextEvent e)
    {
        showGainCoin(e.Target, e.Value);
    }

    public static void showHealing(Transform target, Heal heal)
    {
        const TextType TYPE = TextType.Healing;
        new FText.Data($"+{heal.Healing:F0}", TYPE)
            .setSetting(TYPE)
            .setValue(heal.Healing)
            .setDirection(Vector3.up)
            .setTarget(target)
            .setExtraContentSize(Mathf.InverseLerp(50, 1000, heal.Healing) * 0.25F) //this should be based on the amount of damage
            .setType(0)
            .show();
    }
}