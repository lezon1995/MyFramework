using UnityEngine;
using UnityEngine.EventSystems;
using System;

// 用于监听UGUI鼠标事件的脚本
public class EventTriggerListener : EventTrigger
{
    public Action<PointerEventData, GameObject> onClick; // 点击回调
    public Action<PointerEventData, GameObject> onDown; // 鼠标按下回调
    public Action<PointerEventData, GameObject> onEnter; // 鼠标进入回调
    public Action<PointerEventData, GameObject> onExit; // 鼠标离开回调
    public Action<PointerEventData, GameObject> onUp; // 鼠标抬起回调
    public Action<AxisEventData, GameObject> onMove; // 鼠标移动回调
    public Action<BaseEventData, GameObject> onSelect; // 选择回调
    public Action<BaseEventData, GameObject> onUpdateSelect; // 更新选择回调
    public override void OnPointerClick(PointerEventData eventData) => onClick?.Invoke(eventData, gameObject);
    public override void OnPointerDown(PointerEventData eventData) => onDown?.Invoke(eventData, gameObject);
    public override void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke(eventData, gameObject);
    public override void OnPointerExit(PointerEventData eventData) => onExit?.Invoke(eventData, gameObject);
    public override void OnPointerUp(PointerEventData eventData) => onUp?.Invoke(eventData, gameObject);
    public override void OnSelect(BaseEventData eventData) => onSelect?.Invoke(eventData, gameObject);
    public override void OnUpdateSelected(BaseEventData eventData) => onUpdateSelect?.Invoke(eventData, gameObject);
    public override void OnMove(AxisEventData eventData) => onMove?.Invoke(eventData, gameObject);
}