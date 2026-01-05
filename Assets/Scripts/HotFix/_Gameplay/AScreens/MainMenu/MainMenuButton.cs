using UnityEngine;

namespace MarbleHero;

public abstract class MainMenuButton
{
    public abstract string name { get; }
    public string content => name;
    protected myUGUIButton button;
    protected myUGUIText text;

    protected MainMenuButton()
    {
        var path = $"{GAMEPLAY_PATH}/Prefabs/UI/MainMenuButton.prefab";
        var o = mPrefabPoolManager.createObject(path, 0, false, true);
        button = LayoutScript.newUIObject<myUGUIButton>(null, null, o);
        button.setName(name);
        button.setUGUIButtonClick(onClick);
        var child = button.getChild(0);
        text = LayoutScript.newUIObject<myUGUIText>(button, null, child);
        text.setText(content);
    }

    public void setParent(Transform parent)
    {
        button.setParent(parent.gameObject);
    }

    protected abstract void onClick();
}

public class PLAY : MainMenuButton
{
    public override string name => "PLAY";

    protected override void onClick()
    {
    }
}

public class RESUME_GAME : MainMenuButton
{
    public override string name => "RESUME_GAME";

    protected override void onClick()
    {
        Game.mainMenuScreen.screen = MainMenuScreen.CurScreen.NONE;
        Game.mainMenuScreen.hideMenuButtons();
        Game.mainMenuScreen.darken();
        Game.loadingSave = true;
        Game.chosenCharacter = (Game.characterManager.loadChosenCharacter()).chosenClass;
        Game.mainMenuScreen.fadingOut();
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
    public override string name => "ABANDON_RUN";

    protected override void onClick()
    {
    }
}

public class INFO : MainMenuButton
{
    public override string name => "INFO";

    protected override void onClick()
    {
    }
}

public class STAT : MainMenuButton
{
    public override string name => "STAT";

    protected override void onClick()
    {
    }
}

public class SETTINGS : MainMenuButton
{
    public override string name => "SETTINGS";

    protected override void onClick()
    {
    }
}

public class PATCH_NOTES : MainMenuButton
{
    public override string name => "PATCH_NOTES";

    protected override void onClick()
    {
    }
}

public class QUIT : MainMenuButton
{
    public override string name => "QUIT";

    protected override void onClick()
    {
        log("Quit Game button clicked!");
        Application.Quit();
    }
}