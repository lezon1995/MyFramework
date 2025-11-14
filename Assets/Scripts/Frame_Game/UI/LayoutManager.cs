using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBase;
using static FrameDefine;
using static FrameBaseDefine;
using static FileUtility;
using static FrameBaseUtility;

// 布局管理器
public class LayoutManager : FrameSystem
{
    protected Dictionary<Type, LayoutRegisterInfo> layoutInfos = new(); // 布局注册信息列表
    protected Dictionary<Type, GameLayout> layouts = new(); // 所有布局的列表
    protected Dictionary<string, LayoutInfo> asyncLayouts = new(); // 正在异步加载的布局列表
    protected Canvas root; // 所有UI的根节点

    public LayoutManager()
    {
        // 在构造中获取UI根节点,确保其他组件能在任意时刻正常访问
        root = getRootGameObject(UGUI_ROOT, true).GetComponent<Canvas>();
    }

    public Vector2 getRootSize()
    {
        return ((RectTransform)root.transform).rect.size;
    }

    public Canvas getUIRoot()
    {
        return root;
    }

    public override void update(float dt)
    {
        base.update(dt);
        foreach (var layout in layouts.Values)
        {
            try
            {
                layout.updateLayout(dt);
            }
            catch (Exception e)
            {
                logExceptionBase(e, "界面:" + layout.getName());
            }
        }
    }

    public override void willDestroy()
    {
        foreach (var layout in layouts.Values)
            layout.destroy();

        layouts.Clear();
        asyncLayouts.Clear();
        root = null;
        Resources.UnloadUnusedAssets();
        base.willDestroy();
    }

    public string getLayoutPathByType(Type type)
    {
        return layoutInfos.get(type).fileNameNoSuffix;
    }

    public GameLayout getLayout(Type type)
    {
        return layouts.get(type);
    }

    public GameLayout createLayout(LayoutInfo info)
    {
        if (layouts.TryGetValue(info.type, out GameLayout existLayout))
            return existLayout;

        if (isWebGL())
        {
            logErrorBase("webgl无法同步加载界面");
            return null;
        }

        var path = R_UI_PREFAB_PATH + getLayoutPathByType(info.type) + ".prefab";
        var prefab = res.loadInResource<GameObject>(path);
        return newLayout(info, prefab);
    }

    public void createLayoutAsync(LayoutInfo info, GameLayoutCallback callback)
    {
        if (layouts.TryGetValue(info.type, out GameLayout existLayout))
        {
            callback?.Invoke(existLayout);
            return;
        }

        asyncLayouts.Add(info.type.ToString(), info);
        var path = R_UI_PREFAB_PATH + getLayoutPathByType(info.type) + ".prefab";
        res.loadInResourceAsync(path, (GameObject prefab) =>
        {
            if (asyncLayouts.Remove(prefab.name, out var layoutInfo))
            {
                var layout = newLayout(layoutInfo, prefab);
                callback?.Invoke(layout);
            }
        });
    }

    public void registerLayout(Type type, string fileName, GameLayoutCallback callback)
    {
        // 编辑器下检查文件是否存在
        if (isEditor() && !isFileExist(P_RESOURCES_UI_PREFAB_PATH + fileName + ".prefab"))
        {
            logErrorBase("界面文件不存在:" + P_RESOURCES_UI_PREFAB_PATH + fileName + ".prefab");
            return;
        }

        var info = new LayoutRegisterInfo();
        info.type = type;
        info.callback = callback;
        info.fileNameNoSuffix = fileName;
        layoutInfos.Add(type, info);
    }

    // 卸载所有非常驻的布局
    public void unloadAllLayout()
    {
        foreach (var layout in layouts.Values)
        {
            layout.setVisible(false);
            layout.destroy();
        }

        layouts.Clear();
    }

    public void notifyLayoutChanged(GameLayout layout, bool createOrDestroy)
    {
        var registerInfo = layoutInfos.get(layout.getType());
        registerInfo.callback?.Invoke(createOrDestroy ? layout : null);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected GameLayout newLayout(LayoutInfo info, GameObject prefab)
    {
        var go = instantiatePrefab(root.gameObject, prefab, info.type.ToString(), true);
        var layout = Activator.CreateInstance(info.type) as GameLayout;
        layout.setPrefab(prefab);
        layout.setType(info.type);
        layout.setRenderOrder(info.renderOrder);
        layout.initLayout();
        if (layout.getRoot().gameObject != go)
        {
            logErrorBase("布局的根节点不是实例化出来的节点,请确保运行前UI根节点下没有与布局同名的节点");
        }

        return layouts.add(info.type, layout);
    }
}