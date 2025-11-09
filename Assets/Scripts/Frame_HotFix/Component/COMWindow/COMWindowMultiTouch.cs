
// 变化UI颜色的组件
public class COMWindowMultiTouch : ComponentMultiTouch
{
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		var window = (myUGUIObject)owner;
		window.setOnMouseDown(onTouchStart);
		window.setOnMouseUp(onTouchEnd);
	}
}