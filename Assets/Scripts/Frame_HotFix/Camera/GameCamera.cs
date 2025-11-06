#if USE_URP
using UnityEngine.Rendering.Universal;
#endif
using System;
using UnityEngine;
using static MathUtility;
using static FrameBaseUtility;

// 封装Unity的Camera
public class GameCamera : MovableObject
{
    protected CameraLinker linker; // 只是记录当前连接器方便外部获取
    protected Camera camera; // 摄像机组件
    protected int overlayDepth; // 作为Overlay摄像机时的排序深度
    protected int lastVisibleLayer; // 上一次的渲染层

    // 如果要实现摄像机震动,则需要将摄像机挂接到一个节点上,一般操作的是父节点的Transform,震动时是操作摄像机自身节点的Transform
    public override void setObject(GameObject obj)
    {
        base.setObject(obj);
        mObject.TryGetComponent(out camera);
        if (isEditor())
        {
            getOrAddUnityComponent<CameraDebug>().setCamera(this);
        }
    }

    public override void destroy()
    {
        base.destroy();
        destroyComponent<CameraDebug>(mObject);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        camera = null;
        linker = null;
        overlayDepth = 0;
        lastVisibleLayer = 0;
    }

    public void unlinkTarget()
    {
        linker?.onUnlink();
        linker?.setLinkObject(null);
        linker?.setActive(false);
        linker = null;
    }

    public CameraLinker linkTarget(Type linkerType, MovableObject target)
    {
        if (getOrAddComponent(linkerType) is not CameraLinker cameraLinker)
        {
            return null;
        }

        // 先断开旧的连接器
        unlinkTarget();
        linker = cameraLinker;
        linker.setLinkObject(target);
        linker.setActive(true);
        linker.onLinked();
        return linker;
    }

    public Camera getCamera()
    {
        return camera;
    }

    public CameraLinker getCurLinker()
    {
        return linker;
    }

    public float getNearClip()
    {
        return camera.nearClipPlane;
    }

    public float getFOVX(bool radian = false)
    {
        float radianFovX = atan(getAspect() * tan(getFOVY(true) * 0.5f)) * 2.0f;
        return radian ? radianFovX : toDegree(radianFovX);
    }

    // 计算透视投影下显示的宽高,与屏幕大小无关,只是指定距离下视锥体的截面宽高
    public Vector2 getViewSize(float distance)
    {
        float viewHeight = tan(getFOVY(true) * 0.5f) * abs(distance) * 2.0f;
        return new(viewHeight * getAspect(), viewHeight);
    }

    // radian为true表示输入的fovy是弧度制的值,false表示角度制的值
    public void setFOVY(float fovy, bool radian = false)
    {
        camera.fieldOfView = radian ? toDegree(fovy) : fovy;
    }

    public float getFOVY(bool radian = false)
    {
        return radian ? toRadian(camera.fieldOfView) : camera.fieldOfView;
    }

    public float getAspect()
    {
        return camera.aspect;
    }

    public float getOrthoSize()
    {
        return camera.orthographicSize;
    }

    public void setOrthoSize(float size)
    {
        camera.orthographicSize = size;
    }

    public float getCameraDepth()
    {
        return camera.depth;
    }

    public void setCameraDepth(float depth)
    {
        camera.depth = depth;
    }

    public int getOverlayDepth()
    {
        return overlayDepth;
    }

    public void setOverlayDepth(int depth)
    {
        overlayDepth = depth;
    }

    public void copyCamera(GameObject obj)
    {
        copyObjectTransform(obj);
        obj.TryGetComponent(out Camera c);
        camera.fieldOfView = c.fieldOfView;
        camera.cullingMask = c.cullingMask;
    }

    public void setVisibleLayer(int layer)
    {
        if (layer == 0)
        {
            return;
        }

        lastVisibleLayer = camera.cullingMask;
        camera.cullingMask = layer;
    }

    public int getLastVisibleLayer()
    {
        return lastVisibleLayer;
    }

    public void setPostProcessing(bool post)
    {
#if USE_URP
		getOrAddUnityComponent<UniversalAdditionalCameraData>().renderPostProcessing = post;
#endif
    }

    public void setRenderTarget(RenderTexture renderTarget)
    {
#if USE_URP
		if (getOrAddUnityComponent<UniversalAdditionalCameraData>().cameraStack.Count > 0)
		{
			logError("设置RenderTexture的摄像机不能再添加cameraStack,请移除此摄像机上所有的cameraStack");
		}
		if (getOrAddUnityComponent<UniversalAdditionalCameraData>().renderType != CameraRenderType.Base)
		{
			logError("只能给Base摄像机添加RenderTarget,否则会添加失败");
		}
#endif
        camera.targetTexture = renderTarget;
    }

    public RenderTexture createRenderTarget(Vector2 size)
    {
        if (camera.targetTexture != null)
            return camera.targetTexture;

        RenderTexture rt = RenderTexture.GetTemporary((int)size.x, (int)size.y, 4);
        setRenderTarget(rt);
        return rt;
    }

    public void destroyRenderTexture()
    {
        if (camera.targetTexture == null)
            return;

        RenderTexture.ReleaseTemporary(camera.targetTexture);
        camera.targetTexture = null;
    }

    public RenderTexture getRenderTarget()
    {
        return camera.targetTexture;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected override void initComponents()
    {
        base.initComponents();
        addInitComponent<CameraLinkerAcceleration>(false);
        addInitComponent<CameraLinkerThirdPerson>(false);
        addInitComponent<CameraLinkerFree>(false);
        addInitComponent<CameraLinkerSmoothFollow>(false);
        addInitComponent<CameraLinkerSmoothRotate>(false);
        addInitComponent<CameraLinkerFirstPerson>(false);
    }
}