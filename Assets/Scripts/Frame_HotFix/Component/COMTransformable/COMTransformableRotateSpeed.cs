using UnityEngine;

// 使物体按速度旋转的组件
public class COMTransformableRotateSpeed : ComponentRotateSpeed
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyRotation(ref Vector3 rotation)
	{
		(owner as Transformable).setRotation(rotation);
	}
	protected override Vector3 getCurRotation() { return (owner as Transformable).getRotation(); }
}