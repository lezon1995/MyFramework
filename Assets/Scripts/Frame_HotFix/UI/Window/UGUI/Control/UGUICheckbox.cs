using static UnityUtility;

// 自定义的勾选框
public class UGUICheckbox : WindowObjectUGUI, ICommonUI
{
    protected myUGUIObject mark; // 勾选图片节点
#if USE_TMP
	protected myUGUITextTMP label;     // 文字节点
#else
    protected myUGUIText label; // 文字节点
#endif
    protected OnCheck mCheckCallback; // 勾选状态改变的回调

    public UGUICheckbox(IWindowObjectOwner parent) : base(parent)
    {
    }

    protected override void assignWindowInternal()
    {
        base.assignWindowInternal();
        newObject(out mark, "Mark");
        newObject(out label, "Label", false);
    }

    public override void init()
    {
        if (mark == null)
        {
            logError("UGUICheckbox需要有一个名为Mark的节点");
        }

        root.registerCollider(onCheckClick);
    }

#if USE_TMP
    public myUGUITextTMP getLabelObject()
    {
        return label;
    }
#else
    public myUGUIText getLabelObject()
    {
        return label;
    }
#endif
    public void setLabel(string s)
    {
        label?.setText(s);
    }

    public void setOnCheck(OnCheck callback)
    {
        mCheckCallback = callback;
    }

    public void setChecked(bool check)
    {
        mark.setActive(check);
    }

    public bool isChecked()
    {
        return mark.isActiveInHierarchy();
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void onCheckClick()
    {
        mark.setActive(!mark.isActiveInHierarchy());
        mCheckCallback?.Invoke(this, mark.isActiveInHierarchy());
    }
}