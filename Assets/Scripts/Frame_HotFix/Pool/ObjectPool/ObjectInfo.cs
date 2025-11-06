using System;
using UnityEngine;
using static StringUtility;
using static UnityUtility;
using static FrameBaseUtility;

// 已经从资源加载的物体的信息
[Serializable]
public class ObjectInfo : ClassObject
{
    protected PrefabPool pool; // 所属的对象池
    protected GameObject obj; // 物体实例
    protected string fileWithPath; // 带GameResources下相对路径的文件名,不带后缀
    protected int tag; // 物体的标签,外部给物体添加标签后,方便统一对指定标签的物体进行销毁,从而不用指定具体的实例或名字
    protected bool inuse; // 是否正在使用
    protected bool moveToHide; // 是否通过移动到远处来隐藏

    public override void destroy()
    {
        base.destroy();
        destroyObject();
    }

    public PrefabPool getPool()
    {
        return pool;
    }

    public GameObject getObject()
    {
        return obj;
    }

    public string getFileWithPath()
    {
        return fileWithPath;
    }

    public int getTag()
    {
        return tag;
    }

    public bool isUsing()
    {
        return inuse;
    }

    public bool isMoveToHide()
    {
        return moveToHide;
    }

    public void setPool(PrefabPool pool)
    {
        this.pool = pool;
    }

    public void setObject(GameObject obj)
    {
        this.obj = obj;
    }

    public void setTag(int tag)
    {
        this.tag = tag;
    }

    public void setUsing(bool value)
    {
        inuse = value;
    }

    public void setMoveToHide(bool moveToHide)
    {
        this.moveToHide = moveToHide;
    }

    // 同步创建物体
    public void createObject(GameObject prefab, string fileWithPath)
    {
        if (prefab == null)
        {
            return;
        }

        obj = instantiatePrefab(null, prefab, getFileNameWithSuffix(prefab.name), true);
        this.fileWithPath = fileWithPath;
    }

    // 异步创建物体
    public void createObjectAsync(GameObject prefab, string fileWithPath, Action<ObjectInfo> callback)
    {
        if (prefab == null)
        {
            callback?.Invoke(this);
            return;
        }
#if UNITY_6000_0_OR_NEWER
		long curAssignID = mAssignID;
		instantiatePrefabAsync(prefab, getFileNameWithSuffix(prefab.name), true, (GameObject go)=> 
		{
			mObject = go;
			callback?.Invoke(curAssignID == mAssignID ? this : null);
		});
#else
        obj = instantiatePrefab(null, prefab, getFileNameWithSuffix(prefab.name), true);
        callback?.Invoke(this);
#endif
        this.fileWithPath = fileWithPath;
    }

    // 销毁物体
    public void destroyObject()
    {
        destroyUnityObject(ref obj);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        pool = null;
        obj = null;
        fileWithPath = null;
        tag = 0;
        inuse = false;
        moveToHide = false;
    }
}