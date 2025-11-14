using System;
using System.Collections.Generic;
using UnityEngine;
using static MathUtility;
using static FrameUtility;
using static StringUtility;
using static UnityUtility;

// 自定义的循环滚动列表,暂时只支持从上往下纵向排列的滚动列表
public class UGUIDragViewLoop<T, DataType> : WindowObjectUGUI, IDragViewLoop, ICommonUI where T : DragViewItem<DataType> where DataType : ClassObject
{
    protected WindowStructPool<T> itemPool; // 只用作显示的节点列表
    protected List<DataType> dataList = new(); // 所有节点的数据列表
    protected List<Vector3> itemPositions = new(); // 所有节点的位置
    protected myUGUIObject viewport; // 视口节点,也就是根节点
    protected myUGUIDragView content; // 所有节点的父节点,也是主要用于实现滑动的节点
    protected Vector3 lastRefreshedContentPos; // 上一次刷新时Content的位置,只有当Content位置改变时才会自动刷新
    protected Vector2 itemSize; // 节点的大小
    protected Vector2 interval; // 适配后的排列间隔
    protected int column; // 节点的排列列数
    protected int lastStartItemIndex; // 用于判断是否刷新
    protected bool needUpdateItems; // 是否需要调用所有节点的update
    protected static List<DataType> tempDataList; // 用于外部临时构造一个数据列表的

    public UGUIDragViewLoop(IWindowObjectOwner parent) : base(parent)
    {
        itemPool = new(this);
    }

    protected override void assignWindowInternal()
    {
        base.assignWindowInternal();
        viewport = root;
        newObject(out content, viewport, "Content");
    }

    public void assignTemplate(string templateName)
    {
        itemPool.assignTemplate(content, templateName);
    }

    public void initDragView(DRAG_DIRECTION direction = DRAG_DIRECTION.VERTICAL)
    {
        initDragView(direction, Vector2.zero, true);
    }

    // interval是适配前的间隔,默认根据Content的适配方式进行计算
    // refreshDepthIgnoreInactive刷新节点深度时,是否忽略没有激活的节点,默认是忽略的
    public void initDragView(DRAG_DIRECTION direction, Vector2 _interval, bool refreshDepthIgnoreInactive)
    {
        if (content == null || viewport == null)
        {
            logError("是否忘了调用UGUIDragViewLoop的assignWindow?");
            return;
        }

        if (itemPool.getTemplate() == null)
        {
            logError("是否忘了调用UGUIDragViewLoop的initTemplate?");
            return;
        }

        content.initDragView(direction);
        itemPool.init();
        interval = content.tryGetUnityComponent<ScaleAnchor>().getRealScale() * _interval;
        itemSize = itemPool.getTemplate().getWindowSize();
        if (itemSize.x < 1 || itemSize.y < 1)
        {
            logError("无法获取节点的显示大小");
            return;
        }

        Vector2 viewportSize = viewport.getWindowSize();
        column = floor((viewportSize.x + interval.x) / (itemSize.x + interval.x));
        // 这里+2比较重要,需要算出在任意情况下一定能够显示完整个viewport所需要的数量,最极限的就是中间已经铺了若干个完整的节点,上下留了一些空间,所以需要+2
        int maxDisplayRowCount = floor((viewportSize.y + interval.y) / (itemSize.y + interval.y)) + 2;
        int maxDisplayItemCount = column * maxDisplayRowCount;
        for (int i = 0; i < maxDisplayItemCount; ++i)
        {
            itemPool.newItem();
        }

        script.getLayout().refreshUIDepth(content, refreshDepthIgnoreInactive);
    }

    // 设置显示的数据,dataList的元素需要从对象池中创建,会在内部进行回收
    // keepContentTopPos表示是否保持Content的顶部的位置不变,否则将会自动是Content顶部对齐Viewport顶部
    public void setDataList(List<DataType> list, bool keepContentTopPos = false)
    {
        UN_CLASS_LIST(dataList);
        itemPositions.Clear();
        if (list.count() == 0)
        {
            content.setWindowHeight(0);
            updateDisplayItem();
            return;
        }

        dataList.setRange(list);
        float contentWidth = content.getWindowSize().x;
        Vector2 curLeftTop = Vector2.zero;
        // 为了避免浮点计算的误差,使用计数的方式来获得行数,如果通过(最大高度 + 高度间隔) / (节点高度 + 高度间隔)的方式计算,可能会由于误差而算错
        int rowCount = list.Count > 0 ? 1 : 0;
        for (int i = 0; i < list.Count; ++i)
        {
            // 这一排已经放不下了, 放到下一排
            if (curLeftTop.x + itemSize.x > contentWidth)
            {
                curLeftTop = new(0, curLeftTop.y - interval.y - itemSize.y);
                ++rowCount;
            }

            itemPositions.add(new(curLeftTop.x + itemSize.x * 0.5f, curLeftTop.y - itemSize.y * 0.5f));
            curLeftTop.x += itemSize.x + interval.x;
        }

        float maxHeight = itemSize.y * rowCount + interval.y * (rowCount - 1);
        // 计算完高度, 重新计算所有节点在Content下的位置
        for (int i = 0; i < itemPositions.Count; ++i)
        {
            itemPositions[i] = new(itemPositions[i].x - content.getPivot().x * contentWidth, itemPositions[i].y + (1 - content.getPivot().y) * maxHeight);
        }

        // Content的顶部在Viewport中的位置
        float contentTop = content.getPosition().y + content.getPivot().y * content.getWindowSize().y;
        // 计算出Content节点的总高度
        content.setWindowHeight(maxHeight);
        // 只有当top不在Viewport内时才会调整,否则即使调整了,也会自动被拉回
        if (keepContentTopPos && contentTop >= viewport.getWindowSize().y * 0.5f)
        {
            content.setPosition(new(0.0f, contentTop - content.getPivot().y * content.getWindowSize().y));
        }
        else
        {
            content.alignParentTopCenter();
        }

        updateDisplayItem();
    }

    public List<DataType> getDataList()
    {
        return dataList;
    }

    // 每次设置DateList时,可以先使用getTempDataList获取一个临时列表,然后填充列表,再调用setDataList将填充好的临时列表传进来
    public List<DataType> startSetDataList()
    {
        tempDataList ??= new();
        tempDataList.Clear();
        return tempDataList;
    }

    public void setData(int index, DataType data, bool forceRefresh = true)
    {
        UN_CLASS(dataList[index]);
        dataList[index] = data;
        if (forceRefresh)
        {
            foreach (T item in itemPool.getUsedList())
            {
                if (item.getIndex() == index)
                {
                    item.setData(data);
                    break;
                }
            }
        }
    }

    public void setDragEnable(bool enable)
    {
        content.getDragViewComponent()?.setActive(enable);
    }

    public void setNeedUpdateItems(bool needUpdate)
    {
        needUpdateItems = needUpdate;
    }

    public void updateDisplayItem(Predicate<DataType> match, DataType newData, bool forceRefresh = true)
    {
        if (dataList.findIndex(match, out int index))
        {
            setData(index, newData, forceRefresh);
        }
    }

    public void updateDisplayItem(bool forceRefresh = true)
    {
        // 根据当前Content的位置,计算出当前显示的节点
        float viewportTopToContentTop = content.getWindowSize().y * (1 - content.getPivot().y) + content.getPosition().y - viewport.getWindowSize().y * (1 - viewport.getPivot().y);
        int topRowIndex = floor(viewportTopToContentTop / (itemSize.y + interval.y));
        int startItemIndex = clampMin(topRowIndex * column);
        if (lastStartItemIndex != startItemIndex || forceRefresh)
        {
            var displayItemList = itemPool.getUsedList();
            for (int i = 0; i < displayItemList.Count; ++i)
            {
                T item = displayItemList[i];
                int dataIndex = i + startItemIndex;
                item.setActive(dataIndex < dataList.Count);
                item.setIndex(dataIndex);
                if (item.isActive())
                {
                    item.getRoot().setName("Item" + IToS(dataIndex));
                    item.setData(dataList[dataIndex]);
                    item.setPosition(itemPositions[dataIndex]);
                }
            }
        }

        lastRefreshedContentPos = content.getPosition();
        lastStartItemIndex = startItemIndex;
    }

    public void updateDragView()
    {
        // 这一帧Content移动过位置,就需要刷新显示
        if (!isVectorEqual(content.getPosition(), lastRefreshedContentPos))
        {
            updateDisplayItem(false);
        }

        // 更新
        if (needUpdateItems)
        {
            foreach (T item in itemPool.getUsedList())
            {
                item.update();
            }
        }
    }
}