using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameBaseUtility;
using static MathUtility;

// 可变换的物体,2D和3D物体都是可变换,也就是都会包含一个Transform
public class Transformable : ComponentOwner
{
    protected Transform t; // 变换组件
    protected GameObject go; // 物体节点
    protected bool needUpdate; // 是否启用更新,与Active共同控制是否执行更新
    protected Action onPosition;
    protected Action onRotation;
    protected Action onScale;
    protected bool positionDirty = true;
    protected Vector3 position; // 单独存储位置,可以在大多数时候避免访问Transform

    public Transformable()
    {
        needUpdate = true;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        t = null;
        go = null;
        needUpdate = true;
        onPosition = null;
        onRotation = null;
        onScale = null;
        positionDirty = true;
        position = Vector3.zero;
    }

    public virtual void setObject(GameObject obj)
    {
        go = obj;
        if (go)
        {
            t = go.transform;
            if (go.name != name)
            {
                go.name = name;
            }
        }
        else
        {
            t = null;
        }
    }

    public override void setActive(bool active)
    {
        if (go)
        {
            if (active == go.activeSelf)
                return;

            using var a = new ProfilerScope("active object");
            go.SetActive(active);
        }

        base.setActive(active);
    }

    public void resetActive()
    {
        // 重新禁用再启用,可以重置状态
        go.SetActive(false);
        go.SetActive(true);
    }

    public void enableAllColliders(bool enable)
    {
        foreach (var collider in go.GetComponents<Collider>())
        {
            collider.enabled = enable;
        }
    }

    // 返回第一个碰撞体,当前节点找不到,则会在子节点中寻找
    public Collider getColliderInChild()
    {
        return getUnityComponentInChild<Collider>(true);
    }

    // 返回第一个碰撞体,仅在当前节点中寻找,允许子类重写此函数,碰撞体可能不在当前节点,或者也不在此节点的子节点中
    public virtual Collider getCollider(bool addIfNotExist = false)
    {
        var collider = tryGetUnityComponent<Collider>();
        // 由于Collider无法直接添加到GameObject上,所以只能默认添加BoxCollider
        if (addIfNotExist && collider == null)
        {
            collider = getOrAddUnityComponent<BoxCollider>();
        }

        return collider;
    }

    public override void setName(string name)
    {
        base.setName(name);
        if (go && go.name != name)
        {
            go.name = name;
        }
    }

    public virtual bool raycastSelf(ref Ray ray, out RaycastHit hit, float maxDistance)
    {
        Collider collider = getCollider();
        if (collider == null)
        {
            hit = new();
            return false;
        }

        return collider.Raycast(ray, out hit, maxDistance);
    }

    public GameObject getObject()
    {
        return go;
    }

    public bool isUnityComponentEnabled<T>() where T : Behaviour
    {
        T com = tryGetUnityComponent<T>();
        return com && com.enabled;
    }

    public void enableUnityComponent<T>(bool enable) where T : Behaviour
    {
        T com = tryGetUnityComponent<T>();
        if (com)
        {
            com.enabled = enable;
        }
    }

    public T tryGetUnityComponent<T>() where T : Component
    {
        if (go == null)
            return null;

        go.TryGetComponent(out T com);
        return com;
    }

    public bool tryGetUnityComponent<T>(out T com) where T : Component
    {
        if (go == null)
        {
            com = null;
            return false;
        }

        return go.TryGetComponent(out com);
    }

    public T getOrAddUnityComponent<T>() where T : Component
    {
        FrameBaseUtility.getOrAddComponent(go, out T com);
        return com;
    }

    // 从当前节点以及所有子节点中查找指定组件
    public void getUnityComponentsInChild<T>(bool includeInactive, List<T> list) where T : Component
    {
        if (go == null)
            return;

        go.GetComponentsInChildren(includeInactive, list);
    }

    // 从当前节点以及所有子节点中查找指定组件
    public T[] getUnityComponentsInChild<T>(bool includeInactive) where T : Component
    {
        if (go == null)
            return null;

        return go.GetComponentsInChildren<T>(includeInactive);
    }

    // 从当前节点以及所有子节点中查找指定组件,includeInactive默认为false
    public void getUnityComponentsInChild<T>(List<T> list) where T : Component
    {
        if (go == null)
            return;

        go.GetComponentsInChildren(list);
    }

    // 从当前节点以及所有子节点中查找指定组件,includeInactive默认为false
    public T[] getUnityComponentsInChild<T>() where T : Component
    {
        if (go == null)
            return null;

        return go.GetComponentsInChildren<T>();
    }

    // 从指定的子节点中查找指定组件
    public T getUnityComponentInChild<T>(string childName) where T : Component
    {
        GameObject go = getGameObject(childName, this.go);
        if (go == null)
            return null;

        go.TryGetComponent(out T com);
        return com;
    }

    // 从当前以及所有子节点中查找指定组件
    public T getUnityComponentInChild<T>(bool includeInactive = true) where T : Component
    {
        if (go == null)
            return null;

        return go.GetComponentInChildren<T>(includeInactive);
    }

    public GameObject getUnityObject()
    {
        return go;
    }

    public Transform getTransform()
    {
        return t;
    }

    public Vector3 getLeft(bool ignoreY = false)
    {
        return ignoreY ? normalize(resetY(-t.right)) : -t.right;
    }

    public Vector3 getRight(bool ignoreY = false)
    {
        return ignoreY ? normalize(resetY(t.right)) : t.right;
    }

    public Vector3 getBack(bool ignoreY = false)
    {
        return ignoreY ? normalize(resetY(-t.forward)) : -t.forward;
    }

    public Vector3 getForward(bool ignoreY = false)
    {
        return ignoreY ? normalize(resetY(t.forward)) : t.forward;
    }

    public virtual bool isActive()
    {
        return go && go.activeSelf;
    }

    public virtual bool isActiveInHierarchy()
    {
        return go && go.activeInHierarchy;
    }

    public string getLayerName()
    {
        return LayerMask.LayerToName(go.layer);
    }

    public int getLayer()
    {
        return go.layer;
    }

    public virtual bool isNeedUpdate()
    {
        return needUpdate;
    }

    public virtual void setNeedUpdate(bool enable)
    {
        needUpdate = enable;
    }

    public void addPositionModifyCallback(Action callback)
    {
        onPosition += callback;
    }

    public void removePositionModifyCallback(Action callback)
    {
        onPosition -= callback;
    }

    public void addRotationModifyCallback(Action callback)
    {
        onRotation += callback;
    }

    public void removeRotationModifyCallback(Action callback)
    {
        onRotation -= callback;
    }

    public void addScaleModifyCallback(Action callback)
    {
        onScale += callback;
    }

    public void removeScaleModifyCallback(Action callback)
    {
        onScale -= callback;
    }

    public Vector3 getPosition()
    {
        if (positionDirty)
        {
            positionDirty = false;
            position = t.localPosition;
        }

        return position;
    }

    public Vector3 getRotation()
    {
        var angles = t.localEulerAngles;
        adjustAngle180(ref angles.z);
        return angles;
    }

    public int getSiblingIndex()
    {
        return t ? t.GetSiblingIndex() : 0;
    }

    public int getChildCount()
    {
        return t ? t.childCount : 0;
    }

    public GameObject getChild(int index)
    {
        return t ? t.GetChild(index).gameObject : null;
    }

    public Vector3 getScale()
    {
        return t ? t.localScale : Vector3.zero;
    }

    public Vector3 getWorldPosition()
    {
        return t ? t.position : Vector3.zero;
    }

    public Vector3 getWorldScale()
    {
        return t ? t.lossyScale : Vector3.zero;
    }

    public Vector3 getWorldRotation()
    {
        return t ? t.rotation.eulerAngles : Vector3.zero;
    }

    public Vector3 getRotationRadian()
    {
        if (t == null)
            return Vector3.zero;

        Vector3 vector3 = toRadian(t.localEulerAngles);
        adjustRadian180(ref vector3.z);
        return vector3;
    }

    public Quaternion getRotationQuaternion()
    {
        return t ? t.localRotation : Quaternion.identity;
    }

    public Quaternion getWorldQuaternionRotation()
    {
        return t ? t.rotation : Quaternion.identity;
    }

    public void setPosition(Vector3 pos)
    {
        if (t == null)
            return;
        positionDirty = true;
        if (isVectorEqual(t.localPosition, pos))
            return;
        t.localPosition = pos;
        onPosition?.Invoke();
    }

    public void setScale(float scale)
    {
        setScale(new Vector3(scale, scale, scale));
    }

    public void setScale(Vector3 scale)
    {
        if (t == null || isVectorEqual(t.localScale, scale))
            return;

        t.localScale = scale;
        onScale?.Invoke();
    }

    // 角度制的欧拉角,分别是绕xyz轴的旋转角度
    public void setRotation(Vector3 rot)
    {
        if (t == null || isVectorEqual(t.localEulerAngles, rot))
            return;

        t.localEulerAngles = rot;
        onRotation?.Invoke();
    }

    // 角度制的欧拉角,分别是绕xyz轴的旋转角度
    public void setRotation(Quaternion rot)
    {
        if (t == null)
            return;

        t.localRotation = rot;
        onRotation?.Invoke();
    }

    public void setWorldPosition(Vector3 pos)
    {
        if (t == null)
            return;

        positionDirty = true;
        t.position = pos;
        onPosition?.Invoke();
    }

    public void setWorldRotation(Vector3 rot)
    {
        if (t == null)
            return;

        t.eulerAngles = rot;
        onRotation?.Invoke();
    }

    public void setWorldRotation(Quaternion rot)
    {
        if (t == null)
            return;

        t.rotation = rot;
        onRotation?.Invoke();
    }

    public void setWorldScale(Vector3 scale)
    {
        if (t == null)
            return;

        if (t.parent)
        {
            t.localScale = divideVector3(scale, t.parent.lossyScale);
        }
        else
        {
            t.localScale = scale;
        }

        onScale?.Invoke();
    }

    public Vector3 localToWorld(Vector3 point)
    {
        return UnityUtility.localToWorld(t, point);
    }

    public Vector3 worldToLocal(Vector3 point)
    {
        return UnityUtility.worldToLocal(t, point);
    }

    public Vector3 localToWorldDirection(Vector3 direction)
    {
        return UnityUtility.localToWorldDirection(t, direction);
    }

    public Vector3 worldToLocalDirection(Vector3 direction)
    {
        return UnityUtility.worldToLocalDirection(t, direction);
    }

    public void setPositionX(float x)
    {
        setPosition(replaceX(getPosition(), x));
    }

    public void setPositionY(float y)
    {
        setPosition(replaceY(getPosition(), y));
    }

    public void setPositionZ(float z)
    {
        setPosition(replaceZ(getPosition(), z));
    }

    public void setRotationX(float rotX)
    {
        setRotation(replaceX(t.localEulerAngles, rotX));
    }

    public void setRotationY(float rotY)
    {
        setRotation(replaceY(t.localEulerAngles, rotY));
    }

    public void setRotationZ(float rotZ)
    {
        setRotation(replaceZ(t.localEulerAngles, rotZ));
    }

    public void setScaleX(float x)
    {
        setScale(replaceX(t.localScale, x));
    }

    public virtual void move(Vector3 moveDelta, Space space = Space.Self)
    {
        if (space == Space.Self)
        {
            moveDelta = rotateVector3(moveDelta, getRotationQuaternion());
        }

        setPosition(getPosition() + moveDelta);
    }

    public void rotate(Vector3 rotation)
    {
        if (t == null)
            return;

        t.Rotate(rotation, Space.Self);
    }

    public void rotateWorld(Vector3 rotation)
    {
        if (t == null)
            return;

        t.Rotate(rotation, Space.World);
    }

    // 绕本地坐标系下某个轴原地旋转,angle为角度制
    public void rotateAround(Vector3 axis, float angle)
    {
        if (t == null)
            return;

        t.Rotate(axis, angle, Space.Self);
    }

    // 绕世界某条直线旋转
    public void rotateAround(Vector3 point, Vector3 axis, float angle)
    {
        if (t == null)
            return;

        t.RotateAround(point, axis, angle);
    }

    public void rotateAroundWorld(Vector3 axis, float angle)
    {
        if (t == null)
            return;

        t.Rotate(axis, angle, Space.World);
    }

    public void lookAt(Vector3 direction)
    {
        if (isVectorZero(direction))
            return;

        setRotation(getLookAtQuaternion(direction));
    }

    public void lookAtPoint(Vector3 point)
    {
        if (!isVectorEqual(point, getPosition()))
        {
            setRotation(getLookAtQuaternion(point - getPosition()));
        }
    }

    public void yawPitch(float yaw, float pitch)
    {
        Vector3 curRot = getRotation();
        curRot.x += pitch;
        curRot.y += yaw;
        setRotation(curRot);
    }

    public void resetTransform()
    {
        if (t == null)
            return;

        positionDirty = true;
        t.localPosition = Vector3.zero;
        t.localEulerAngles = Vector3.zero;
        t.localScale = Vector3.one;
    }

    public void setLayer(int layer)
    {
        if (go == null)
            return;

        go.layer = layer;
    }

    public void setParent(GameObject parent, bool resetTrans = true)
    {
        if (t == null)
            return;

        Transform parentTrans = parent ? parent.transform : null;
        if (parentTrans != t.parent)
        {
            t.SetParent(parentTrans);
            positionDirty = true;
            if (resetTrans)
            {
                resetTransform();
            }
        }
    }

    public void copyObjectTransform(GameObject obj)
    {
        Transform objTrans = obj.transform;
        FT.MOVE(this, objTrans.localPosition);
        FT.ROTATE(this, objTrans.localEulerAngles);
        FT.SCALE(this, objTrans.localScale);
        positionDirty = true;
    }

    public virtual bool isChildOf(IMouseEventCollect parent)
    {
        if (t == null)
            return false;

        return parent is Transformable obj && t.IsChildOf(obj.getTransform());
    }

    public virtual void setAlpha(float alpha)
    {
        using var a = new ListScope<Renderer>(out var renderers);
        getUnityComponentsInChild(true, renderers);
        foreach (var renderer in renderers)
        {
            var material = renderer.material;
            var color = material.color;
            color.a = alpha;
            material.color = color;
        }
    }

    public virtual float getAlpha()
    {
        var renderer = tryGetUnityComponent<Renderer>();
        if (renderer == null)
            return 1.0f;

        return renderer.material.color.a;
    }

    public virtual bool canUpdate()
    {
        return needUpdate && go.activeInHierarchy;
    }
}