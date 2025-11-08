using System;

// 用于记录布局的注册信息
public struct LayoutRegisterInfo
{
    public Type type; // 布局脚本类型
    public GameLayoutCallback callback; // 用于加载或者卸载后对脚本变量进行赋值
    public string fileNameNoSuffix; // 相对于UIPrefab的路径,且不带后缀名
}