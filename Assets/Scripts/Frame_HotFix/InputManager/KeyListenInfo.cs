using System;
using UnityEngine;

// 按键监听信息
public class KeyListenInfo : ClassObject
{
    public Action callback; // 按键回调
    public IEventListener listener; // 监听者
    public COMBINATION_KEY combinationKey; // 指定可组合的键是否按下
    public KeyCode key; // 按键值

    public override void resetProperty()
    {
        base.resetProperty();
        callback = null;
        listener = null;
        combinationKey = COMBINATION_KEY.NONE;
        key = KeyCode.None;
    }
}