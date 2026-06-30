using UnityEngine;
using System;
using static StringUtility;
using static UnityUtility;
using static FrameBaseUtility;

// 已经从Template加载的物体的信息
[Serializable]
public class TemplateObjectInfo : ClassObject
{
	protected TemplatePool mPool;			// 所属的对象池
	protected GameObject mObject;		// 物体实例
	protected GameObject mTemplate;
	protected int mTag;					// 物体的标签,外部给物体添加标签后,方便统一对指定标签的物体进行销毁,从而不用指定具体的实例或名字
	protected bool mUsing;				// 是否正在使用
	protected bool mMoveToHide;         // 是否通过移动到远处来隐藏
	public override void destroy()
	{
		base.destroy();
		destroyObject();
	}
	public TemplatePool getPool()					{ return mPool; }
	public GameObject getObject()				{ return mObject; }
	public GameObject getTemplate()				{ return mTemplate; }
	public int getTag()							{ return mTag; }
	public bool isUsing()						{ return mUsing; }
	public bool isMoveToHide()					{ return mMoveToHide; }
	public void setPool(TemplatePool pool)		{ mPool = pool; }
	public void setObject(GameObject obj)		{ mObject = obj; }
	public void setTemplate(GameObject template)		{ mTemplate = template; }
	public void setTag(int tag)					{ mTag = tag; }
	public void setUsing(bool value)			{ mUsing = value; }
	public void setMoveToHide(bool moveToHide)	{ mMoveToHide = moveToHide; }
	// 同步创建物体
	public void createObject()
	{
		if (mTemplate == null)
			return;
		mObject = instantiatePrefab(null, mTemplate, getFileNameWithSuffix(mTemplate.name), true);
	}
	// 异步创建物体
	public void createObjectAsync(Action<TemplateObjectInfo> callback)
	{
		if (mTemplate == null)
		{
			callback?.Invoke(this);
			return;
		}
#if UNITY_6000_0_OR_NEWER
		long curAssignID = mAssignID;
		instantiatePrefabAsync(mTemplate, getFileNameWithSuffix(mTemplate.name), true, (GameObject go)=> 
		{
			mObject = go;
			callback?.Invoke(curAssignID == mAssignID ? this : null);
		});
#else
		mObject = instantiatePrefab(null, mTemplate, getFileNameWithSuffix(mTemplate.name), true);
		callback?.Invoke(this);
#endif
	}
	// 销毁物体
	public void destroyObject()
	{
		destroyUnityObject(ref mObject);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mPool = null;
		mObject = null;
		mTemplate = null;
		mTag = 0;
		mUsing = false;
		mMoveToHide = false;
	}
}