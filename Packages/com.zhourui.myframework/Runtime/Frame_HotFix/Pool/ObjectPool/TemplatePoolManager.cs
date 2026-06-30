using System;
using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static FrameBaseUtility;

// 从Template实例化的物体对象池
public class TemplatePoolManager : FrameSystem
{
	protected Dictionary<GameObject, TemplateObjectInfo> mInstanceList = new(); // 根据实例化的物体查找的列表
	protected SafeDictionary<GameObject, TemplatePool> mPrefabPoolList = new();	// 已实例化对象的实例池列表
	protected float mTimerInterval = 3.0f;									// 扫描间隔,默认3秒
	protected float mDestroyTimer;                                          // 扫描是否有需要卸载的资源的计时器
	public TemplatePoolManager()
	{
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
		if (isEditor())
		{
			mObject.AddComponent<ObjectPoolDebug>();
		}
	}
	public override void destroy()
	{
		mInstanceList.Clear();
		UN_CLASS_LIST(mPrefabPoolList.getMainList());
		mPrefabPoolList.clear();
		base.destroy();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 每隔一定时间销毁不再使用的对象池
		if (tickTimerLoop(ref mDestroyTimer, elapsedTime, mTimerInterval))
		{
			using var a = new SafeDictionaryReader<GameObject, TemplatePool>(mPrefabPoolList);
			foreach (var item in a.mReadList)
			{
				TemplatePool pool = item.Value;
				if (!pool.isEmptyInUse())
				{
					continue;
				}
				foreach (TemplateObjectInfo obj in pool.getUnuseList())
				{
					if (obj?.getObject() == null)
					{
						logWarning("object is null:" + obj?.getTemplate());
						continue;
					}
					mInstanceList.Remove(obj.getObject());
				}
				destroyPool(pool);
			}
		}
		if (isEditor())
		{
			foreach (var item in mInstanceList)
			{
				if (item.Value.getObject() == null)
				{
					logError("Object can not be destroy outside of TemplatePoolManager! filePath:" + item.Value.getTemplate());
				}
			}
		}
	}
	public float getTimerInterval() { return mTimerInterval; }
	public void setTimerInterval(float interval) { mTimerInterval = interval; }
	// 同步预加载prefab,加载prefab文件,并实例化对象,fileWithPath是GameResource下的相对路径
	public void initObjectToPool(GameObject fileWithPath, int objectTag, int count, bool moveToHide)
	{
		getTemplatePool(fileWithPath).initToPool(objectTag, count, moveToHide);
	}

	public GameObject createObject(GameObject template) => createObject(template, 0, false, true, null);
	public GameObject createObject(GameObject template, bool moveToHide, GameObject parent = null) => createObject(template, 0, moveToHide, true, parent);

	public Transform createObject(Transform template)
	{
		var o = createObject(template.gameObject, 0, false, true, null);
		if (o)
			return o.transform;
		return null;
	}

	public Transform createObject(Transform template, bool moveToHide, GameObject parent = null)
	{
		var o = createObject(template.gameObject, 0, moveToHide, true, parent);
		if (o)
			return o.transform;
		return null;
	}

	public Transform createObject(Transform template, int objectTag, bool moveToHide, bool active, GameObject parent = null)
	{
		var o = createObject(template.gameObject, objectTag, moveToHide, active, parent);
		if (o)
			return o.transform;
		return null;
	}

	// 同步创建物体,fileWithPath是GameResource下的相对路径
	public GameObject createObject(GameObject template, int objectTag, bool moveToHide, bool active, GameObject parent = null)
	{
		using var a = new ProfilerScope(0);
		var pool = getTemplatePool(template);
		var objInfo = pool.getOneUnused(objectTag);
		if (objInfo == null)
		{
			string info = "template加载失败:" + template + ",请确认GameObject存在";
			logError(info);
			return null;
		}
		postCreateObject(pool, objInfo, moveToHide, parent, active);
		return objInfo.getObject();
	}
	// 销毁指定tag的所有物体,会从内存中真正销毁,不会放回到池中
	public void destroyAllWithTag(int objectTag)
	{
		using var a = new ListScope<TemplateObjectInfo>(out var tempList);
		mInstanceList.For(item => tempList.addIf(item.Value, item.Value.getTag() == objectTag));
		tempList.For(item => destroyObject(item.getObject(), true));
	}
	public void destroyObject(GameObject obj, bool destroyReally)
	{
		destroyObject(ref obj, destroyReally);
	}
	// 销毁一个物体,destroyReally为true表示真正从内存中销毁,false表示仅仅只是放回到池中
	public void destroyObject(ref GameObject obj, bool destroyReally)
	{
		if (mHasDestroy || obj == null)
		{
			return;
		}
		if (!mInstanceList.Remove(obj, out TemplateObjectInfo info))
		{
			logError("can not find gameObject in ObjectPool! obj:" + obj.name + ", HashCode:" + obj.GetHashCode());
			return;
		}

		if (!mPrefabPoolList.tryGetValue(info.getTemplate(), out TemplatePool prefabPool))
		{
			logError("找不到此预设的对象池:" + info.getTemplate());
			return;
		}
		prefabPool.destroyObject(info, destroyReally);
		// 如果已经没有任何实例化对象,则销毁此对象池
		if (prefabPool.isEmpty())
		{
			destroyPool(prefabPool);
		}
		obj = null;
	}
	public bool isExistInPool(GameObject go) { return go != null && mInstanceList.ContainsKey(go); }
	public Dictionary<GameObject, TemplateObjectInfo> getInstanceList() { return mInstanceList; }
	public SafeDictionary<GameObject, TemplatePool> getPrefabPoolList() { return mPrefabPoolList; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void destroyPool(TemplatePool pool)
	{
		mPrefabPoolList.remove(pool.getTemplate());
		UN_CLASS(ref pool);
	}
	protected void postCreateObject(TemplatePool pool, TemplateObjectInfo objInfo, bool moveToHide, GameObject parent, bool active)
	{
		objInfo.setPool(pool);
		objInfo.setMoveToHide(moveToHide);
		GameObject go = objInfo.getObject();
		if (go == null)
		{
			return;
		}
		mInstanceList.TryAdd(go, objInfo);
		// 返回前先确保物体是挂接到预设管理器下的
		if (parent == null)
		{
			parent = mObject;
		}
		setNormalProperty(go, parent);
		if (go.activeSelf != active)
		{
			go.SetActive(active);
		}
	}
	// 根据名字获取一个对象池
	protected TemplatePool getTemplatePool(GameObject template)
	{
		if (template == null)
		{
			logError("template is null");
		}
		if (!mPrefabPoolList.tryGetValue(template, out TemplatePool pool))
		{
			pool = mPrefabPoolList.addClass(template);
			pool.setTemplate(template);
		}
		return pool;
	}
}