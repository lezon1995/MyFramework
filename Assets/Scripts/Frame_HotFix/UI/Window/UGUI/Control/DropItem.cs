// 自定义的下拉列表的项
public class DropItem : WindowRecyclableUGUI
{
    protected UGUIDropList dropList; // 自定义的下拉列表
    protected myUGUIObject hover; // 悬停窗口
#if USE_TMP
	protected myUGUITextTMP label;				// 名字窗口
#else
    protected myUGUIText label; // 名字窗口
#endif
    protected int mCustomValue; // 附带的自定义数据,一般都是枚举之类的

    public DropItem(IWindowObjectOwner parent) : base(parent)
    {
    }

    protected override void assignWindowInternal()
    {
        base.assignWindowInternal();
        newObject(out hover, "Hover", false);
        newObject(out label, "Label");
    }

    public override void init()
    {
        base.init();
        root.registerCollider(onClick);
        if (hover != null)
        {
            root.setHoverCallback(onHover);
        }
    }

    public override void reset()
    {
        base.reset();
        hover?.setActive(false);
    }

    public string getText()
    {
        return label.getText();
    }

    public int getCustomValue()
    {
        return mCustomValue;
    }

    public void setText(string text)
    {
        label.setText(text);
    }

    public void setCustomValue(int value)
    {
        mCustomValue = value;
    }

    public void setParent(UGUIDropList parent)
    {
        this.parent = parent;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void onClick()
    {
        dropList.dropItemClick(this);
    }

    protected void onHover(bool hover)
    {
        this.hover?.setActive(hover);
    }
}