using System;

// 布局信息
public struct LayoutInfo
{
    public GameLayoutCallback callback; // 加载完成的回调
    public Type type; // 布局脚本类型
    public int renderOrder; // 显示顺序
}