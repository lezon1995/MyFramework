using static MathUtility;

// 渐变摄像机FOV的组件
public class COMCameraFOV : ComponentKeyFrame
{
    protected float targetFOV; // 目标FOV
    protected float startFOV; // 起始FOV

    public override void resetProperty()
    {
        base.resetProperty();
        targetFOV = 0.0f;
        startFOV = 0.0f;
    }

    public void setStartFOV(float fov)
    {
        startFOV = fov;
    }

    public void setTargetFOV(float fov)
    {
        targetFOV = fov;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected override void applyTrembling(float value)
    {
        var obj = owner as GameCamera;
        obj.setFOVY(lerpSimple(startFOV, targetFOV, value));
    }
}