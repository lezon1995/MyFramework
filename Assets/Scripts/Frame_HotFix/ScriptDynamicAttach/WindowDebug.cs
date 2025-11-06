using UnityEngine;

// UI窗口调试信息
public class WindowDebug : MonoBehaviour
{
    protected myUGUIObject window; // 当前的窗口
    public bool ForceRefresh; // 是否强制刷新,无论是否启用了EnableScriptDebug
    public string Depth; // 窗口深度
    public bool DepthOverAllChild; // 计算深度时是否将深度设置为所有子节点之上,实际调整的是mExtraDepth
    public bool PassRay; // 当存在且注册了碰撞体时是否允许射线穿透
    public bool Enable; // 是否启用更新
    public int OrderInParent; // 在父节点中的顺序
    public int ID; // 每个窗口的唯一ID

    public void setWindow(myUGUIObject o)
    {
        window = o;
    }

    public void Update()
    {
        if (GameEntry.getInstance() == null || (!GameEntry.getInstance().frameworkParam.enableScriptDebug && !ForceRefresh) || window == null)
            return;

        Depth = window.getDepth().toDepthString();
        DepthOverAllChild = window.isDepthOverAllChild();
        PassRay = window.isPassRay();
        Enable = window.isNeedUpdate();
        OrderInParent = window.getDepth().getOrderInParent();
        ID = window.getID();
    }
}