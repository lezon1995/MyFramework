using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MarbleHero;

public class FTextManager : FrameSystem
    , IEvent<DmgTextEvent>
// , IEvent<HealTextEvent>
{
    GameObject textParent;
    SafeList<FText> usings = new();
    List<FText> unused = new();
    Dictionary<Transform, FText> reusedTexts = new();
    Dictionary<string, FTextSetting> settings = new();

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
        var damage = mResourceManager.loadGameResource<FTextSetting>($"{GAMEPLAY_PATH}/FTextSetting_Damage.asset");
        var damage_Crit = mResourceManager.loadGameResource<FTextSetting>($"{GAMEPLAY_PATH}/FTextSetting_Damage_Crit.asset");
        settings.add("Damage", damage);
        settings.add("Damage_Crit", damage_Crit);
    }

    public override void destroy()
    {
        base.destroy();
        this.removeListener<DmgTextEvent>();
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);

        using var a = new SafeListReader<FText>(usings);
        foreach (var text in a.mReadList)
        {
            if (text && text.isActiveInHierarchy())
            {
                var dt = !text.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime;
                text.update(dt);
            }
        }
    }

    public FTextSetting getSetting(string settingName)
    {
        settings.TryGetValue(settingName, out var setting);
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
        FText text;
        if (data.reuseTimes is > 0 or -2 && data.target)
        {
            if (reusedTexts.TryGetValue(data.target, out text))
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

        if (unused.any())
        {
            text = unused.popBack();
        }
        else
        {
            text = CLASS<FText>();
            text.setName("FText");
            var path = $"{GAMEPLAY_PATH}/FTextTMP.prefab";
            var o = mPrefabPoolManager.createObject(path, 0, true, true, textParent);
            text.setObject(o);
        }

        usings.add(text);
        return text;
    }

    public void release(FText text)
    {
        text.Clear();
        usings.remove(text);
        unused.add(text);
    }

    public void addToReused(FText text, FText.Data data)
    {
        reusedTexts.TryAdd(data.target, text);
    }

    public void removeFromReused(FText.Data data)
    {
        reusedTexts.Remove(data.target);
    }

    public static void showDamage(Transform target, Dmg dmg)
    {
        if (dmg.isSelf)
            return;

        var setting = dmg.isCrit ? "Damage_Crit" : "Damage";

        var mix = dmg.mix;
        if (mix.off)
        {
            new FText.Data($"{dmg.damageDealt:F0}")
                .setSetting(setting)
                .setValue(dmg.damageDealt)
                .setDirection(dmg.direction)
                .setTarget(target)
                .setOffset(Random.insideUnitCircle * 0.25F)
                .setExtraContentSize(Mathf.InverseLerp(50, 1000, dmg.damageDealt) * 1F) //this should be based on the amount of damage
                .setType((int)dmg.actualType)
                .show();

            return;
        }

        var damage = mix.physicDamageDealt;
        if ((int)damage > 0)
        {
            new FText.Data($"{damage:F0}")
                .setSetting(setting)
                .setValue(damage)
                .setDirection(dmg.direction)
                .setTarget(target)
                .setOffset(Random.insideUnitCircle * 0.25F)
                .setExtraContentSize(Mathf.InverseLerp(50, 1000, dmg.damageDealt) * 1F) //this should be based on the amount of damage
                .setType((int)Dmg.Types.PHYSIC)
                .show();
        }

        damage = mix.magicDamageDealt;
        if ((int)damage > 0)
        {
            new FText.Data($"{damage:F0}")
                .setSetting(setting)
                .setValue(damage)
                .setDirection(dmg.direction)
                .setTarget(target)
                .setOffset(Random.insideUnitCircle * 0.25F)
                .setExtraContentSize(Mathf.InverseLerp(50, 1000, dmg.damageDealt) * 1F) //this should be based on the amount of damage
                .setType((int)Dmg.Types.MAGIC)
                .show();
        }

        damage = mix.trueDamageDealt;
        if ((int)damage > 0)
        {
            new FText.Data($"{damage:F0}")
                .setSetting(setting)
                .setValue(damage)
                .setDirection(dmg.direction)
                .setTarget(target)
                .setOffset(Random.insideUnitCircle * 0.25F)
                .setExtraContentSize(Mathf.InverseLerp(50, 1000, dmg.damageDealt) * 1F) //this should be based on the amount of damage
                .setType((int)Dmg.Types.TRUE)
                .show();
        }
    }

    public void onEvent(DmgTextEvent e)
    {
        showDamage(e.Target, e.Dmg);
    }

    /*public void onEvent(HealTextEvent e)
    {
        ShowHeal(e.Target, e.Heal);
    }

    public static void ShowHeal(Transform target, Heal heal)
    {
        new FText.Data($"+{heal.Healing:F0}")
            .Setting("Heal")
            .Value(heal.Healing)
            .Target(target)
            .ExtraContentSize(Mathf.InverseLerp(50, 1000, heal.Healing) * 1F) //this should be based on the amount of damage
            .Type(0)
            .Show();
    }*/
}