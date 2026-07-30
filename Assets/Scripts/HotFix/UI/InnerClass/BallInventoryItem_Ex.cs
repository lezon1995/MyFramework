using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains;

public partial class BallInventoryItem : IDraggableItem
{
    BallInventoryBinder ballBinder;
    BallItem ballItem;
    public myUGUIButton Btn => btn;
    public GameObject ItemGO => mRoot.getGameObject();
    public void SetSelected(bool on) => focus.setActive(on);

    public void SetEnabledState(bool on)
    {
        /* 视觉留白:normal/disable 切换,可视迭代阶段再补 */
    }

    public void SetIconVisible(bool on) => icon.setActive(on);
    public void SetBallIcon(Sprite s) => icon.setSpriteOnly(s);
    public void SetBallItem(BallItem item) => ballItem = item;
    public void SetBallInventoryBinder(BallInventoryBinder binder) => ballBinder = binder;

    /// <summary>把 0..stars.Length 颗星点亮, 简单的 0/1 表达:可见=已到达该等级。</summary>
    public void SetStarCount(int count)
    {
        for (int i = 0; i < stars.Length; ++i)
            stars[i].setActive(i < count);
    }

    /// <summary>替换此 item 的点击回调(先清空已注册,再设)。
    /// 传 null 时只清空监听器,不再 AddListener(null),避免 UnityEvent 抛 NRE。
    /// 注意:正常路径下 BallInventoryItem 在 init() 时已经一次性订阅 UnityEvent,
    /// 不需要在 Rebuild 时再调用本方法(那样会产生新的 lambda 分配)。</summary>
    public void SetOnClick(UnityAction callback)
    {
        if (btn == null) return;
        btn.setUGUIButtonClick(callback);
    }

    /// <summary>替换此 item 的拖拽松开回调(先清空已注册,再设)。
    /// 传 null 时清空回调,释放时不会触发任何逻辑。
    /// 注意:正常路径下 BallInventoryItem 在 init() 时已经一次性订阅,
    /// 不需要在 Rebuild 时再调用本方法(那样会产生新的 lambda 分配)。</summary>
    public void SetOnDragReleased(Action<BallInventoryItem, UIDragReleaseEventData> callback)
    {
        _onDragReleased = callback;
    }

    // IDraggableItem 实现:拖拽事件已通过 UIEventListener 自动桥接到下面这五个方法
    public void onPotentialDragInitialized(PointerEventData data)
    {
    }

    public void onDragStarted(PointerEventData data)
    {
    }

    public void onDragging(PointerEventData data)
    {
    }

    public void onDragEnded(PointerEventData data)
    {
    }

    public void onDragReleasedOverUI(UIDragReleaseEventData data)
    {
        _onDragReleased?.Invoke(this, data);
    }

    Action<BallInventoryItem, UIDragReleaseEventData> _onDragReleased;
}