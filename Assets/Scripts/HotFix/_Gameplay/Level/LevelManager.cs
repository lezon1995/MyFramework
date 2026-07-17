using PrimeTween;
using UnityEngine;

namespace MoreMountains;

public class LevelManager : FrameSystem
{
    public BorderLeft borderLeft;
    public BorderRight borderRight;
    public BorderTop borderTop;
    public BorderBot borderBot;

    protected float defaultBorderLeftX;
    protected float defaultBorderRightX;
    protected float curBorderWidth;
    protected float curBorderHeight;

    public int rows = 14;
    public int cols = 9;

    public override void init()
    {
        base.init();
        var go = getRootGameObject("Level");
        go.find(out var left, "BorderLeft");
        go.find(out var right, "BorderRight");
        go.find(out var top, "BorderTop");
        go.find(out var bot, "BorderBot");
        borderLeft = createBorder<BorderLeft>(left);
        borderRight = createBorder<BorderRight>(right);
        borderTop = createBorder<BorderTop>(top);
        borderBot = createBorder<BorderBot>(bot);
        defaultBorderLeftX = borderLeft.getWorldPosition().x;
        defaultBorderRightX = borderRight.getWorldPosition().x;

        var width = abs(defaultBorderLeftX - defaultBorderRightX);
        setBorderWidth(width);
        var height = getBorderHeight();
        setBorderHeight(height);
    }

    protected override void initComponents()
    {
        base.initComponents();

        // addInitComponent(out mAvatar, true);
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);

        var width = getBorderWidth();
        if (!isFloatEqual(curBorderWidth, width))
        {
            setBorderWidth(width);
        }

        var height = getBorderHeight();
        if (!isFloatEqual(curBorderHeight, height))
        {
            setBorderHeight(height);
        }
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

    public void setBorderWidth(float width)
    {
        borderTop.setWidth(width);
        borderBot.setWidth(width);
        curBorderWidth = width;
    }

    public void setBorderHeight(float height)
    {
        borderLeft.setHeight(height);
        borderRight.setHeight(height);
        curBorderHeight = height;
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