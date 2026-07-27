using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public class MainManagerBehaviour : MMMonoBehaviour
        , IResetable
    {
        protected string mName;
        protected Transform mTransform; // 变换组件
        protected GameObject mObject; // 物体节点
        [SerializeField]
        protected bool mNeedUpdate = true; // 是否启用Update,与Active共同控制是否执行更新
        [SerializeField]
        protected bool mNeedFixedUpdate = true; // 是否启用FixedUpdate,与Active共同控制是否执行更新
        protected bool mActive; // 缓存的GameObject.activeSelf状态,用于避免频繁访问activeSelf属性

        public virtual void reset()
        {
            mName = null;
            mNeedUpdate = true;
            mNeedFixedUpdate = true;
        }

        public string getName() => mName;

        public virtual void setName(string n)
        {
            mName = n;
            if (mObject && mObject.name != n)
            {
                mObject.name = n;
            }
        }

        void Awake()
        {
            mObject = base.gameObject;
            mTransform = base.transform;
            mName = mObject.name;
            mActive = mObject.activeSelf;
            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }

        void Start()
        {
            OnStart();
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void Update()
        {
            if (mNeedUpdate)
            {
                OnUpdate(Time.deltaTime);
            }
        }

        protected virtual void OnUpdate(float dt)
        {
        }

        protected virtual void FixedUpdate()
        {
            if (mNeedFixedUpdate)
            {
                OnFixedUpdate(Time.fixedDeltaTime);
            }
        }

        protected virtual void OnFixedUpdate(float dt)
        {
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnDestroy()
        {
        }

        public virtual void setActive(bool active)
        {
            if (active == mActive)
                return;

            if (mObject)
            {
                using var a = new ProfilerScope("active object");
                mObject.SetActive(active);
            }

            mActive = active;
        }

        public void resetActive()
        {
            // 重新禁用再启用,可以重置状态
            mObject.SetActive(false);
            mObject.SetActive(true);
            mActive = true;
        }

        public void enableAllColliders(bool enable)
        {
            foreach (var c in mObject.GetComponents<Collider>())
            {
                c.enabled = enable;
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
            var c = tryGetUnityComponent<Collider>();
            // 由于Collider无法直接添加到GameObject上,所以只能默认添加BoxCollider
            if (addIfNotExist && c == null)
            {
                c = getOrAddUnityComponent<BoxCollider>();
            }

            return c;
        }


        public virtual bool raycastSelf(ref Ray ray, out RaycastHit hit, float maxDistance)
        {
            var c = getCollider();
            if (c == null)
            {
                hit = new();
                return false;
            }

            return c.Raycast(ray, out hit, maxDistance);
        }

        public GameObject getGameObject()
        {
            return mObject;
        }

        public string getGameObjectPath()
        {
            return UnityUtility.getGameObjectPath(mObject);
        }

        public int getGameObjectInstanceID()
        {
            return getGameObjectID(mObject);
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
            if (mObject == null)
            {
                return null;
            }

            mObject.TryGetComponent(out T com);
            return com;
        }

        public bool tryGetUnityComponent<T>(out T com) where T : Component
        {
            if (mObject == null)
            {
                com = null;
                return false;
            }

            return mObject.TryGetComponent(out com);
        }

        public T getOrAddUnityComponent<T>() where T : Component
        {
            FrameBaseUtility.getOrAddComponent(mObject, out T com);
            return com;
        }

        public T getOrAddUnityComponent<T>(out T com) where T : Component
        {
            FrameBaseUtility.getOrAddComponent(mObject, out com);
            return com;
        }

        // 从当前节点以及所有子节点中查找指定组件
        public void getUnityComponentsInChild<T>(bool includeInactive, List<T> list) where T : Component
        {
            if (mObject == null)
                return;

            mObject.GetComponentsInChildren(includeInactive, list);
        }

        // 从当前节点以及所有子节点中查找指定组件
        public T[] getUnityComponentsInChild<T>(bool includeInactive) where T : Component
        {
            if (mObject == null)
            {
                return null;
            }

            return mObject.GetComponentsInChildren<T>(includeInactive);
        }

        // 从当前节点以及所有子节点中查找指定组件,includeInactive默认为false
        public void getUnityComponentsInChild<T>(List<T> list) where T : Component
        {
            if (mObject == null)
                return;

            mObject.GetComponentsInChildren(list);
        }

        // 从当前节点以及所有子节点中查找指定组件,includeInactive默认为false
        public T[] getUnityComponentsInChild<T>() where T : Component
        {
            if (mObject == null)
            {
                return null;
            }

            return mObject.GetComponentsInChildren<T>();
        }

        // 从指定的子节点中查找指定组件
        public T getUnityComponentInChild<T>(string childName) where T : Component
        {
            GameObject go = FrameBaseUtility.getGameObject(childName, mObject);
            if (go == null)
            {
                return null;
            }

            go.TryGetComponent(out T com);
            return com;
        }

        // 从当前以及所有子节点中查找指定组件
        public T getUnityComponentInChild<T>(bool includeInactive = true) where T : Component
        {
            if (mObject == null)
            {
                return null;
            }

            return mObject.GetComponentInChildren<T>(includeInactive);
        }

        public GameObject getUnityObject()
        {
            return mObject;
        }

        public new GameObject gameObject => mObject;
        public new Transform transform => mTransform;

        public Transform getTransform()
        {
            return mTransform;
        }

        public Vector3 getLeft(bool ignoreY = false)
        {
            return ignoreY ? normalize(resetY(-mTransform.right)) : -mTransform.right;
        }

        public Vector3 getRight(bool ignoreY = false)
        {
            return ignoreY ? normalize(resetY(mTransform.right)) : mTransform.right;
        }

        public Vector3 getBack(bool ignoreY = false)
        {
            return ignoreY ? normalize(resetY(-mTransform.forward)) : -mTransform.forward;
        }

        public Vector3 getForward(bool ignoreY = false)
        {
            return ignoreY ? normalize(resetY(mTransform.forward)) : mTransform.forward;
        }

        public virtual bool isActive()
        {
            return mActive;
        }

        public virtual bool isActiveInHierarchy()
        {
            return mObject && mObject.activeInHierarchy;
        }

        public string getLayerName()
        {
            return LayerMask.LayerToName(mObject.layer);
        }

        public int getLayer()
        {
            return mObject.layer;
        }

        public virtual bool isNeedUpdate()
        {
            return mNeedUpdate;
        }

        public virtual void setNeedUpdate(bool enable)
        {
            mNeedUpdate = enable;
        }

        public virtual void setNeedFixedUpdate(bool enable)
        {
            mNeedFixedUpdate = enable;
        }

        public Vector3 getPosition()
        {
            return mTransform.localPosition;
        }

        public Vector3 getRotation()
        {
            Vector3 vector3 = mTransform.localEulerAngles;
            adjustAngle180(ref vector3.z);
            return vector3;
        }

        public int getSiblingIndex()
        {
            return mTransform ? mTransform.GetSiblingIndex() : 0;
        }

        public int getChildCount()
        {
            return mTransform ? mTransform.childCount : 0;
        }

        public GameObject getChild(int index)
        {
            return mTransform ? mTransform.GetChild(index).gameObject : null;
        }

        public Vector3 getScale()
        {
            return mTransform ? mTransform.localScale : Vector3.zero;
        }

        public Vector3 getWorldPosition()
        {
            return mTransform ? mTransform.position : Vector3.zero;
        }

        public Vector3 getWorldScale()
        {
            return mTransform ? mTransform.lossyScale : Vector3.zero;
        }

        public Vector3 getWorldRotation()
        {
            return mTransform ? mTransform.rotation.eulerAngles : Vector3.zero;
        }

        public Vector3 getRotationRadian()
        {
            if (mTransform == null)
            {
                return Vector3.zero;
            }

            Vector3 vector3 = toRadian(mTransform.localEulerAngles);
            adjustRadian180(ref vector3.z);
            return vector3;
        }

        public Quaternion getRotationQuaternion()
        {
            return mTransform ? mTransform.localRotation : Quaternion.identity;
        }

        public Quaternion getWorldQuaternionRotation()
        {
            return mTransform ? mTransform.rotation : Quaternion.identity;
        }

        public void setPosition(Vector3 pos)
        {
            if (mTransform == null || isVectorEqual(mTransform.localPosition, pos))
                return;

            mTransform.localPosition = pos;
        }

        public void setScale(float scale)
        {
            setScale(new Vector3(scale, scale, scale));
        }

        public void setScale(Vector3 scale)
        {
            if (mTransform == null || isVectorEqual(mTransform.localScale, scale))
                return;

            mTransform.localScale = scale;
        }

        // 角度制的欧拉角,分别是绕xyz轴的旋转角度
        public void setRotation(Vector3 rot)
        {
            if (mTransform == null || isVectorEqual(mTransform.localEulerAngles, rot))
                return;

            mTransform.localEulerAngles = rot;
        }

        // 角度制的欧拉角,分别是绕xyz轴的旋转角度
        public void setRotation(Quaternion rot)
        {
            if (mTransform == null)
                return;

            mTransform.localRotation = rot;
        }

        public void setWorldPosition(Vector3 pos)
        {
            if (mTransform == null)
                return;

            mTransform.position = pos;
        }

        public void setWorldRotation(Vector3 rot)
        {
            if (mTransform == null)
                return;

            mTransform.eulerAngles = rot;
        }

        public void setWorldRotation(Quaternion rot)
        {
            if (mTransform == null)
                return;

            mTransform.rotation = rot;
        }

        public void setWorldScale(Vector3 scale)
        {
            if (mTransform == null)
                return;

            if (mTransform.parent)
            {
                mTransform.localScale = divideVector3(scale, mTransform.parent.lossyScale);
            }
            else
            {
                mTransform.localScale = scale;
            }
        }

        public Vector3 localToWorld(Vector3 point)
        {
            return UnityUtility.localToWorld(mTransform, point);
        }

        public Vector3 worldToLocal(Vector3 point)
        {
            return UnityUtility.worldToLocal(mTransform, point);
        }

        public Vector3 localToWorldDirection(Vector3 direction)
        {
            return UnityUtility.localToWorldDirection(mTransform, direction);
        }

        public Vector3 worldToLocalDirection(Vector3 direction)
        {
            return UnityUtility.worldToLocalDirection(mTransform, direction);
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

        public void setWorldPositionX(float x)
        {
            setWorldPosition(replaceX(getWorldPosition(), x));
        }

        public void setWorldPositionY(float y)
        {
            setWorldPosition(replaceY(getWorldPosition(), y));
        }

        public void setWorldPositionZ(float z)
        {
            setWorldPosition(replaceZ(getWorldPosition(), z));
        }

        public void setRotationX(float rotX)
        {
            setRotation(replaceX(mTransform.localEulerAngles, rotX));
        }

        public void setRotationY(float rotY)
        {
            setRotation(replaceY(mTransform.localEulerAngles, rotY));
        }

        public void setRotationZ(float rotZ)
        {
            setRotation(replaceZ(mTransform.localEulerAngles, rotZ));
        }

        public void setScaleX(float x)
        {
            setScale(replaceX(mTransform.localScale, x));
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
            if (mTransform == null)
                return;

            mTransform.Rotate(rotation, Space.Self);
        }

        public void rotateWorld(Vector3 rotation)
        {
            if (mTransform == null)
                return;

            mTransform.Rotate(rotation, Space.World);
        }

        // 绕本地坐标系下某个轴原地旋转,angle为角度制
        public void rotateAround(Vector3 axis, float angle)
        {
            if (mTransform == null)
                return;

            mTransform.Rotate(axis, angle, Space.Self);
        }

        // 绕世界某条直线旋转
        public void rotateAround(Vector3 point, Vector3 axis, float angle)
        {
            if (mTransform == null)
                return;

            mTransform.RotateAround(point, axis, angle);
        }

        public void rotateAroundWorld(Vector3 axis, float angle)
        {
            if (mTransform == null)
                return;

            mTransform.Rotate(axis, angle, Space.World);
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
            if (mTransform == null)
                return;

            mTransform.localPosition = Vector3.zero;
            mTransform.localEulerAngles = Vector3.zero;
            mTransform.localScale = Vector3.one;
        }

        public void setLayer(int layer)
        {
            if (mObject == null)
                return;

            mObject.layer = layer;
        }

        public void setParent(GameObject parent, bool resetTrans = true)
        {
            if (mTransform == null)
                return;

            Transform parentTrans = parent ? parent.transform : null;
            if (parentTrans != mTransform.parent)
            {
                mTransform.SetParent(parentTrans);
                if (resetTrans)
                {
                    resetTransform();
                }
            }
        }
    }
}