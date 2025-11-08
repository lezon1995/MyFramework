using UnityEngine;

// 按指定缩放列表缩放物体的组件,缩放速度恒定
public class COMTransformableScaleCurve : ComponentCurve, IComponentModifyScale
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void setValue(Vector3 value)
	{
		(owner as Transformable).setScale(value);
	}
}