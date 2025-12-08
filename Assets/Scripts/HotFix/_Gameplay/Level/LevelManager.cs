using UnityEngine;

namespace MarbleHero;

public class LevelManager : FrameSystem
{
    public BorderLeft borderLeft;
    public BorderRight borderRight;
    public BorderTop borderTop;
    public BorderBot borderBot;

    public override void init()
    {
        base.init();
        var go = getRootGameObject("Level");
        var left = getGameObject("BorderLeft", go);
        var right = getGameObject("BorderRight", go);
        var top = getGameObject("BorderTop", go);
        var bot = getGameObject("BorderBot", go);
        borderLeft = createBorder<BorderLeft>(left);
        borderRight = createBorder<BorderRight>(right);
        borderTop = createBorder<BorderTop>(top);
        borderBot = createBorder<BorderBot>(bot);
    }

    protected override void initComponents()
    {
        base.initComponents();

        // addInitComponent(out mAvatar, true);
    }

    protected T createBorder<T>(GameObject obj) where T : Border
    {
        var border = CLASS<Border>(typeof(T));
        border.setName(obj.name);
        border.setObject(obj);
        border.init();
        return (T)border;
    }
}