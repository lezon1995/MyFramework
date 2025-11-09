using System;

// 用于记录布局的注册信息
public struct LayoutRegisteInfo
{
	public Type type;				// 布局脚本类型
	public bool inResource;				// 布局是否在Resources中
	public LAYOUT_LIFE_CYCLE lifeCycle;    // 布局的生命周期
	public LayoutScriptCallback callback;	// 用于加载或者卸载后对脚本变量进行赋值
}