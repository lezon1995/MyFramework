using System;
using PrimeTween;
using UnityEngine;

namespace MarbleHero;

public enum MainMenuType
{
    PLAY, //开始游戏
    RESUME_GAME, //继续
    ABANDON_RUN, //放弃当前游戏
    INFO, //百科大全
    STAT, //统计内容
    SETTINGS, //设定
    PATCH_NOTES, //补丁内容清单
    QUIT, //退出
}

public abstract class MainMenuButton : IDisposable
{
    public abstract MainMenuType type { get; }
    public abstract string name { get; }
    public string content => name;
    protected GameObject o;
    protected myUGUIButton button;
    protected myUGUIText text;

    protected MainMenuButton()
    {
        var path = $"{GAMEPLAY_PATH}/Prefabs/UI/MainMenuButton.prefab";
        o = mPrefabPoolManager.createObject(path);
        button = LayoutScript.newUIObject<myUGUIButton>(null, null, o);
        button.setName(name);
        button.setUGUIButtonClick(onClick);
        // button.setOnTouchEnter((pos, v) =>
        // {
        //     log($" onTouchEnter pos={pos} v={v}");
        // });
        button.setUGUIMouseEnter((pointer, go) =>
        {
            Tween.UIAnchoredPositionX(text.getRectTransform(), endValue: 30, duration: 0.1F, ease: Ease.OutCubic);
        });
        // button.setOnTouchLeave((pos, v) =>
        // {
        //     log($" onTouchExit pos={pos} v={v}");
        // });
        button.setUGUIMouseExit((pointer, go) =>
        {
            Tween.UIAnchoredPositionX(text.getRectTransform(), endValue: 0, duration: 0.1F, ease: Ease.OutCubic);
        });
        var child = button.getChild(0);
        text = LayoutScript.newUIObject<myUGUIText>(button, null, child);
        text.setText(content);
    }

    public void Dispose()
    {
        mPrefabPoolManager.destroyObject(ref o, true);
        button.dispose();
        text.dispose();
    }

    public void setParent(Transform parent)
    {
        button.setParent(parent.gameObject);
    }

    public void setActive(bool active)
    {
        o.SetActive(active);
    }

    protected abstract void onClick();
}

public class PLAY : MainMenuButton
{
    public override MainMenuType type => MainMenuType.PLAY;
    public override string name => "PLAY";

    protected override void onClick()
    {
        if (Settings.seed == 0)
        {
            setRandomSeed();
        }
        else
        {
            Settings.seedSet = true;
        }

        Game.mainMenuScreen.screen = MainMenuScreen.CurScreen.NONE;
        Game.mainMenuScreen.hideMenuButtons();
        Game.mainMenuScreen.darken();
        Game.loadingSave = false;
        Game.chosenCharacter = APlayer.PlayerClass.IRONCLAD;
        Game.mainMenuScreen.fadeOut();
        Game.mainMenuScreen.fadeOutMusic();
        Settings.isDailyRun = false;
        Settings.isTrial = false;
        ModHelper.setModsFalse();
        ADungeon.generateSeeds();

        // if (Game.steelSeries.isEnabled)
        // Game.steelSeries.event_character_chosen(Game.chosenCharacter);

        // if (Settings.isDemo || Settings.isPublisherBuild)
        // {
        //     BotDataUploader poster = new BotDataUploader();
        //     poster.setValues(BotDataUploader.GameDataType.DEMO_EMBARK, null, null);
        //     Thread t = new Thread(poster);
        //     t.setName("LeaderboardPoster");
        //     t.start();
        // }
        return;

        void setRandomSeed()
        {
            long sourceTime = TimeUtility.getNowTimeStampMS();
            Rand rng = new Rand(sourceTime);
            Settings.seedSourceTimestamp = sourceTime;
            Settings.seed = SeedHelper.generateUnoffensiveSeed(rng);
            Settings.seedSet = false;
        }
    }
}

public class RESUME_GAME : MainMenuButton
{
    public override MainMenuType type => MainMenuType.RESUME_GAME;
    public override string name => "RESUME_GAME";

    protected override void onClick()
    {
        Game.mainMenuScreen.screen = MainMenuScreen.CurScreen.NONE;
        Game.mainMenuScreen.hideMenuButtons();
        Game.mainMenuScreen.darken();
        Game.loadingSave = true;
        Game.chosenCharacter = Game.characterManager.loadChosenCharacter().chosenClass;
        Game.mainMenuScreen.fadeOut();
        Game.mainMenuScreen.fadeOutMusic();
        Settings.isDailyRun = false;
        Settings.isTrial = false;
        ModHelper.setModsFalse();
        // if (Game.steelSeries.isEnabled)
        // Game.steelSeries.event_character_chosen(Game.chosenCharacter);
    }
}

public class ABANDON_RUN : MainMenuButton
{
    public override MainMenuType type => MainMenuType.ABANDON_RUN;
    public override string name => "ABANDON_RUN";

    protected override void onClick()
    {
    }
}

public class INFO : MainMenuButton
{
    public override MainMenuType type => MainMenuType.INFO;
    public override string name => "INFO";

    protected override void onClick()
    {
    }
}

public class STAT : MainMenuButton
{
    public override MainMenuType type => MainMenuType.STAT;
    public override string name => "STAT";

    protected override void onClick()
    {
    }
}

public class SETTINGS : MainMenuButton
{
    public override MainMenuType type => MainMenuType.SETTINGS;
    public override string name => "SETTINGS";

    protected override void onClick()
    {
    }
}

public class PATCH_NOTES : MainMenuButton
{
    public override MainMenuType type => MainMenuType.PATCH_NOTES;
    public override string name => "PATCH_NOTES";

    protected override void onClick()
    {
    }
}

public class QUIT : MainMenuButton
{
    public override MainMenuType type => MainMenuType.QUIT;
    public override string name => "QUIT";

    protected override void onClick()
    {
        log("Quit Game button clicked!");
        Application.Quit();
    }
}