using System;

// 布局信息
public struct LayoutInfo
{
	public LAYOUT_ORDER layoutOrder;		// 显示顺序类型
	public Type type;					// 布局脚本类型
	public string name;				// 布局名字
	public bool isScene;				// 是否为场景布局,场景布局不会挂在UGUIRoot下面
	public int renderOrder;			// 显示顺序
}