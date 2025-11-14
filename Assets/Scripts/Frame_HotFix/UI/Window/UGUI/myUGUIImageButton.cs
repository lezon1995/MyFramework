using UnityEngine;

// 监听鼠标事件,然后自动修改显示图片的button
public class myUGUIImageButton : myUGUIImage
{
    protected string normalSprite; // 正常时的图片
    protected string pressSprite; // 按下时的图片
    protected string hoverSprite; // 悬停时的图片
    protected string selectedSprite; // 选中时的图片
    protected bool useStateSprite; // 状态改变时是否切换图片
    protected bool selected; // 是否选中

    public override void init()
    {
        base.init();
        normalSprite = getSpriteName();
        pressSprite = normalSprite;
        hoverSprite = normalSprite;
        selectedSprite = normalSprite;
    }

    public void setNormalSprite(string sprite, bool apply = true, bool resetStateSprites = true)
    {
        useStateSprite = true;
        normalSprite = sprite;
        if (apply)
        {
            setSpriteName(normalSprite);
        }

        if (resetStateSprites)
        {
            pressSprite = normalSprite;
            hoverSprite = normalSprite;
            selectedSprite = normalSprite;
        }
    }

    public void setPressSprite(string sprite)
    {
        useStateSprite = true;
        pressSprite = sprite;
    }

    public void setHoverSprite(string sprite)
    {
        useStateSprite = true;
        hoverSprite = sprite;
    }

    public void setSelectedSprite(string sprite)
    {
        useStateSprite = true;
        selectedSprite = sprite;
    }

    public void setSpriteNames(string press, string hover)
    {
        useStateSprite = true;
        pressSprite = press;
        hoverSprite = hover;
    }

    public void setSpriteNames(string press, string hover, string selected)
    {
        useStateSprite = true;
        pressSprite = press;
        hoverSprite = hover;
        selectedSprite = selected;
    }

    public override void onMouseEnter(Vector3 mousePos, int touchID)
    {
        base.onMouseEnter(mousePos, touchID);
        if (useStateSprite)
        {
            setSpriteName(selected ? selectedSprite : hoverSprite);
        }
    }

    public override void onMouseLeave(Vector3 mousePos, int touchID)
    {
        base.onMouseLeave(mousePos, touchID);
        if (useStateSprite)
        {
            setSpriteName(selected ? selectedSprite : normalSprite);
        }
    }

    public override void onMouseDown(Vector3 mousePos, int touchID)
    {
        base.onMouseDown(mousePos, touchID);
        if (useStateSprite)
        {
            setSpriteName(selected ? selectedSprite : pressSprite);
        }
    }

    public override void onMouseUp(Vector3 mousePos, int touchID)
    {
        base.onMouseUp(mousePos, touchID);
        // 一般都会再mouseUp时触发点击消息,跳转界面,所以基类中可能会将当前窗口销毁,需要注意
        if (useStateSprite)
        {
            setSpriteName(selected ? selectedSprite : normalSprite);
        }
    }

    public void setSelected(bool select)
    {
        if (selected == select)
            return;

        selected = select;
        if (useStateSprite)
        {
            setSpriteName(selected ? selectedSprite : normalSprite);
        }
    }
}