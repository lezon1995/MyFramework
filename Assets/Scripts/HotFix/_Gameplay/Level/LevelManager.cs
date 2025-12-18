using PrimeTween;
using UnityEngine;

namespace MarbleHero;

public class LevelManager : FrameSystem
{
    public BorderLeft borderLeft;
    public BorderRight borderRight;
    public BorderTop borderTop;
    public BorderBot borderBot;

    protected float defaultBorderLeftX;
    protected float defaultBorderRightX;

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
        defaultBorderLeftX = borderLeft.getWorldPosition().x;
        defaultBorderRightX = borderRight.getWorldPosition().x;
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

    public Vector2 getBorderSize()
    {
        return new(getBorderWidth(), getBorderHeight());
    }

    public float getBorderWidth()
    {
        return abs(borderRight.getWorldPosition().x - borderLeft.getWorldPosition().x);
    }

    public float getBorderHeight()
    {
        return abs(borderTop.getWorldPosition().y - borderBot.getWorldPosition().y);
    }

    public void moveBorderLeftBy(float offset)
    {
        Tween
            .PositionX(borderLeft.getTransform(), endValue: defaultBorderLeftX + offset, duration: 1F, ease: Ease.OutCubic)
            .OnComplete(borderLeft, border =>
            {
                border.setWorldPosition(new(defaultBorderLeftX + offset, 0, 0));
            });
    }

    public void moveBorderRightBy(float offset)
    {
        Tween
            .PositionX(borderRight.getTransform(), endValue: defaultBorderRightX + offset, duration: 1F, ease: Ease.OutCubic)
            .OnComplete(borderRight, border =>
            {
                border.setWorldPosition(new(defaultBorderRightX + offset, 0, 0));
            });
    }

    public float getDefaultBorderLeftX()
    {
        return defaultBorderLeftX;
    }

    public float getDefaultBorderRightX()
    {
        return defaultBorderRightX;
    }
}