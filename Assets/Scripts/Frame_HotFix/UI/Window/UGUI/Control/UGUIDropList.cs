using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static StringUtility;
using static WidgetUtility;
using static FrameBaseHotFix;
using static UnityUtility;

// 自定义的下拉列表
public class UGUIDropList : WindowObjectUGUI, ICommonUI
{
    protected WindowStructPool<DropItem> itemPool; // 显示项的对象池
    protected Action onSelected; // 选项切换时的回调
    protected myUGUIObject mask; // 点击遮罩,用于点击空白处关闭下拉列表
#if USE_TMP
	protected myUGUITextTMP label;                 // 显示当前选项的文本
#else
    protected myUGUIText label; // 显示当前选项的文本
#endif
    protected myUGUIObject options; // 所有选项的下拉框
    protected myUGUIObject viewport; // 所有选项的父节点
    protected myUGUIDragView content; // 所有选项的父节点
    protected myUGUIObject template; // 选项的模板
    protected DropItem selectedItem; // 当前选中的选项
    protected int selectedIndex; // 当前选中的下标

    public UGUIDropList(IWindowObjectOwner parent) : base(parent)
    {
        itemPool = new(this);
    }

    protected override void assignWindowInternal()
    {
        base.assignWindowInternal();
        newObject(out label, "Label");
        newObject(out options, "Options");
        newObject(out mask, options, "Mask");
        newObject(out viewport, options, "Viewport");
        newObject(out content, viewport, "Content");
        itemPool.assignTemplate(content, "Template");
    }

    public override void init()
    {
        base.init();
        itemPool.init();
        label.registerCollider(onClick);
        mask.registerCollider(onMaskClick);
        content.initDragView(DRAG_DIRECTION.VERTICAL);
        options.setActive(false);
        mask.setActive(false);
        // 确认选项的父节点拥有Canvas组件,可以渲染在所有节点之上
        if (options.tryGetUnityComponent(out Canvas optionsCanvas))
        {
            optionsCanvas.overrideSorting = true;
        }
        else
        {
            logError("下拉列表框的Options节点需要拥有Canvas组件");
        }

        if (!options.tryGetUnityComponent<GraphicRaycaster>(out _))
        {
            logError("下拉列表框的Options节点需要拥有GraphicRaycaster组件");
        }
    }

    public void clearOptions()
    {
        selectedIndex = 0;
        itemPool.unuseAll();
    }

    public void setSelectCallback(Action callback)
    {
        onSelected = callback;
    }

    public void setOptions(List<string> options, List<int> customValue = null, bool triggerEvent = true)
    {
        if (!initialized)
        {
            logError("还未执行初始化,不能设置选项");
            return;
        }

        if (customValue != null && options.Count != customValue.Count)
        {
            logError("附加数据的数量与选项的数量不一致");
            return;
        }

        itemPool.unuseAll();
        int count = options.Count;
        for (int i = 0; i < count; ++i)
        {
            DropItem item = itemPool.newItem();
            item.setText(options[i]);
            if (customValue != null)
            {
                item.setCustomValue(customValue[i]);
            }

            item.setParent(this);
        }

        autoGridVertical(content);
        content.alignParentTopCenter();
        setSelect(0, triggerEvent);
    }

    public void setSelect(int value, bool triggerEvent = true)
    {
        var usedList = itemPool.getUsedList();
        if (value >= 0 && value < usedList.Count)
        {
            selectedIndex = value;
            selectedItem = usedList[selectedIndex];
            label.setText(selectedItem.getText());
            if (triggerEvent)
            {
                onSelected?.Invoke();
            }
        }
    }

    public int getSelect()
    {
        return selectedIndex;
    }

    public string getSelectedText()
    {
        return selectedItem?.getText() ?? EMPTY;
    }

    public int getSelectedCustomValue()
    {
        return selectedItem?.getCustomValue() ?? 0;
    }

    public void dropItemClick(DropItem item)
    {
        setSelect(itemPool.getUsedList().IndexOf(item));
        showOptions(false);
    }

    public void showOptions(bool show)
    {
        options.setActive(show);
        mask.setActive(show);
        // 每次显示下拉列表时,都需要重新计算一下显示深度
        if (show)
        {
            var order = mLayoutManager.generateRenderOrder(null, 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO);
            options.getOrAddUnityComponent<Canvas>().sortingOrder = order;
            options.getLayout().refreshUIDepth(root, true);
            content.alignParentTop();
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void onClick()
    {
        Vector3 labelPivot = label.getPositionNoPivot();
        Vector3 labelSize = label.getWindowSize();
        float labelLeftInParent = labelPivot.x - labelSize.x * 0.5f;
        float labelBottomInParent = labelPivot.y - labelSize.y * 0.5f;
        Vector2 size = options.getWindowSize();
        options.setPosition(new(labelLeftInParent + size.x * 0.5f, labelBottomInParent - size.y * 0.5f));
        showOptions(true);
    }

    protected void onMaskClick()
    {
        showOptions(false);
    }
}