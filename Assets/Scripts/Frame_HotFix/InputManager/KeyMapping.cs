using UnityEngine;

// 按键映射信息
public class KeyMapping : ClassObject
{
    public int mappingId; // 映射对象ID,也就是KeyCode映射到的值,一般是一个枚举
    public KeyCode defaultKey; // 默认的键盘按键
    public KeyCode key; // 键盘按键
    public string mappingName; // 映射名字,一般用于显示的,比如移动,攻击

    public override void resetProperty()
    {
        base.resetProperty();
        mappingId = 0;
        defaultKey = KeyCode.None;
        key = KeyCode.None;
        mappingName = null;
    }
}