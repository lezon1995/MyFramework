
// 锁定物体旋转的组件
public class COMTransformableRotateFixed : ComponentRotateFixed
{
	public override void update(float dt)
	{
		(owner as Transformable).setWorldRotation(mFixedEuler);
		base.update(dt);
	}
}