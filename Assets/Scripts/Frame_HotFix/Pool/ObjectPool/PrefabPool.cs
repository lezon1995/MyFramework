using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBaseHotFix;
using static FrameDefine;
using static FrameUtility;

// 单个prefab的实例化池
public class PrefabPool : ClassObject
{
    protected HashSet<ObjectInfo> inuse = new(); // 正在使用的实例化列表,第一个key是文件名,第二个列表中的key是实例化出的物体,value是物品信息,为了提高运行时效率,仅在编辑器下使用
    protected List<ObjectInfo> unuse = new(); // 未使用的实例化列表,第一个key是文件名,第二个列表中的key是实例化出的物体,value是物品信息
    protected GameObject prefab; // 从资源管理器中加载的预设
    protected string fileName; // 此实例物体的预设文件名,相对于GameResources的路径,带后缀
    protected int asyncLoading; // 是否有正在进行的异步操作

    public PrefabPool()
    {
    }

    public override void resetProperty()
    {
        base.resetProperty();
        inuse.Clear();
        unuse.Clear();
        prefab = null;
        fileName = null;
        asyncLoading = 0;
    }

    public override void destroy()
    {
        base.destroy();
        destroyAllInstance();
        mResourceManager?.unload(ref prefab);
    }

    public void destroyAllInstance()
    {
        foreach (var item in inuse)
            item.destroyObject();

        inuse.Clear();
        foreach (var item in unuse)
            item.destroyObject();

        unuse.Clear();
    }

    public void setFileName(string n)
    {
        fileName = n;
    }

    public void setPrefab(GameObject p)
    {
        prefab = p;
    }

    public GameObject getPrefab()
    {
        return prefab;
    }

    public string getFileName()
    {
        return fileName;
    }

    public List<ObjectInfo> getUnuseList()
    {
        return unuse;
    }

    public HashSet<ObjectInfo> getInuseList()
    {
        return inuse;
    }

    public int getInuseCount()
    {
        return inuse.Count;
    }

    public int getUnuseCount()
    {
        return unuse.Count;
    }

    public bool isEmpty()
    {
        return inuse.Count == 0 && unuse.Count == 0 && asyncLoading == 0;
    }

    public int getAsyncLoadingCount()
    {
        return asyncLoading;
    }

    // 向池中异步初始化一定数量的对象
    public CustomAsyncOperation initToPoolAsync(int tag, int count, bool moveToHide, Action callback)
    {
        if (prefab != null)
        {
            doInitToPool(tag, count, moveToHide);
            callback?.Invoke();
            return new CustomAsyncOperation().setFinish();
        }

        // 预设未加载,异步加载预设
        ++asyncLoading;
        long assignID = id;
        return mResourceManager.loadGameResourceAsync(fileName, (GameObject asset) =>
        {
            --asyncLoading;
            if (asset == null)
            {
                callback?.Invoke();
                return;
            }

            if (assignID != id)
            {
                mResourceManager.unload(ref asset);
                callback?.Invoke();
                return;
            }

            setPrefab(asset);
            doInitToPool(tag, count, moveToHide);
            callback?.Invoke();
        });
    }

    // 向池中同步初始化一定数量的对象
    public void initToPool(int tag, int count, bool moveToHide)
    {
        if (prefab == null)
        {
            // 预设未加载,同步加载预设
            var go = mResourceManager.loadGameResource<GameObject>(fileName);
            if (go == null)
            {
                return;
            }

            setPrefab(go);
        }

        doInitToPool(tag, count, moveToHide);
    }

    // 从池中异步获取一个对象
    public CustomAsyncOperation getOneUnusedAsync(int tag, Action<ObjectInfo, bool> callback)
    {
        if (prefab != null)
        {
            getOneUnusedAsyncInternal(tag, (ObjectInfo info) =>
            {
                callback?.Invoke(info, false);
            });
            return new CustomAsyncOperation().setFinish();
        }

        // 预设未加载,异步加载预设
        ++asyncLoading;
        long assignID = id;
        return mResourceManager.loadGameResourceAsync(fileName, (GameObject asset) =>
        {
            --asyncLoading;
            if (asset == null)
            {
                callback?.Invoke(null, false);
                return;
            }

            if (assignID != id)
            {
                callback?.Invoke(null, true);
                mResourceManager.unload(ref asset);
                return;
            }

            setPrefab(asset);
            getOneUnusedAsyncInternal(tag, (ObjectInfo info) =>
            {
                callback?.Invoke(info, false);
            });
        });
    }

    // 从对象池中同步获取或者创建一个物体
    public ObjectInfo getOneUnused(int tag)
    {
        if (prefab == null)
        {
            prefab = mResourceManager.loadGameResource<GameObject>(fileName);
        }

        ObjectInfo objInfo;
        // 未使用列表中有就从未使用列表中获取
        if (unuse.Count > 0)
        {
            objInfo = unuse.popBack();
            if (objInfo.getTag() != tag)
            {
                logError("不能为同一个物体设置不同的tag, file:" + objInfo.getFileWithPath());
            }
        }
        // 没有就创建一个新的
        else
        {
            // 实例化
            CLASS(out objInfo).createObject(prefab, fileName);
            objInfo.setTag(tag);
        }

        objInfo.setUsing(true);
        return inuse.add(objInfo);
    }

    // 销毁物体,destroyReally为true表示将对象直接从内存中销毁,false表示只是放到未使用列表中
    // moveToHide为true则表示回收时不会改变GameObject的显示,只是将位置设置到很远的地方
    public void destroyObject(ObjectInfo obj, bool destroyReally)
    {
        if (obj.getPool() != this)
        {
            logError("要销毁的物体不属于当前对象池");
            return;
        }

        GameObject go = obj.getObject();
        if (!inuse.Remove(obj))
        {
            logError("从使用列表中移除失败:" + go.name);
        }

        if (destroyReally)
        {
            unuse.Remove(obj);
            UN_CLASS(ref obj);
            return;
        }

        bool moveToHide = obj.isMoveToHide();
        if (go.transform.parent == null || go.transform.parent.gameObject != mPrefabPoolManager.getObject())
        {
            // 只有在PrefabPoolManager节点下的物体才可以在回收时只改变位置
            moveToHide = false;
        }

        // 隐藏物体,并且将物体重新挂接到预设管理器下,重置物体变换
        if (moveToHide)
        {
            go.transform.localPosition = FAR_POSITION;
        }
        else
        {
            if (go.activeSelf)
            {
                go.SetActive(false);
            }

            setNormalProperty(go, mPrefabPoolManager.getObject());
        }

        obj.setUsing(false);
        unuse.add(obj);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void doInitToPool(int tag, int count, bool moveToHide)
    {
        if (prefab == null)
        {
            return;
        }

        int needCreate = clampMin(count - inuse.Count - unuse.Count);
        int needCapacity = unuse.count() + needCreate;
        if (unuse.Capacity < needCapacity)
        {
            unuse.Capacity = needCapacity;
        }

        for (int i = 0; i < needCreate; ++i)
        {
            ObjectInfo objInfo = unuse.addClass();
            objInfo.setTag(tag);
            // 实例化,同步进行
            objInfo.createObject(prefab, fileName);
            GameObject go = objInfo.getObject();
            if (go != null)
            {
                // 隐藏物体,并且将物体重新挂接到预设管理器下,重置物体变换
                setNormalProperty(go, mPrefabPoolManager.getObject());
                if (moveToHide)
                {
                    go.transform.localPosition = FAR_POSITION;
                }
                else
                {
                    if (go.activeSelf)
                    {
                        go.SetActive(false);
                    }
                }
            }

            objInfo.setUsing(false);
        }
    }

    // 从池中异步获取一个可用的对象
    protected void getOneUnusedAsyncInternal(int tag, Action<ObjectInfo> callback)
    {
        // 未使用列表中有就从未使用列表中获取
        if (unuse.Count > 0)
        {
            ObjectInfo objInfo = unuse.popBack();
            if (objInfo.getTag() != tag)
            {
                logError("不能为同一个物体设置不同的tag, file:" + objInfo.getFileWithPath());
            }

            objInfo.setUsing(true);
            callback?.Invoke(inuse.add(objInfo));
        }
        // 没有就创建一个新的
        else
        {
            // 实例化
            CLASS<ObjectInfo>().createObjectAsync(prefab, fileName, (ObjectInfo info) =>
            {
                info.setTag(tag);
                info.setUsing(true);
                callback?.Invoke(inuse.add(info));
            });
        }
    }
}