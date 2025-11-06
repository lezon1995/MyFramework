using UnityEditor;

[CustomEditor(typeof(GameEntry), true)]
public class EditorGameFramework : GameEditorBase
{
	protected GameEntry entry;
	public override void OnInspectorGUI()
	{
		entry = target as GameEntry;

		bool modified = false;
		modified |= displayEnum("WindowMode", "窗口类型", ref entry.frameworkParam.windowMode);
		if (entry.frameworkParam.windowMode != WINDOW_MODE.FULL_SCREEN)
		{
			modified |= displayInt("ScreenWidth", "窗口宽度,当WindowMode为FULL_SCREEN时无效", ref entry.frameworkParam.screenWidth);
			modified |= displayInt("ScreenHeight", "窗口高度,当WindowMode为FULL_SCREEN时无效", ref entry.frameworkParam.screenHeight);
		}
		modified |= displayInt("TargetFrameRate", "窗口高度,当WindowMode为FULL_SCREEN时无效", ref entry.frameworkParam.defaultFrameRate);
		modified |= toggle("PoolStackTrace", "是否启用对象池中的堆栈追踪,由于堆栈追踪非常耗时,所以默认关闭,可使用F4动态开启", ref entry.frameworkParam.enablePoolStackTrace);
		modified |= toggle("ScriptDebug", "是否启用调试脚本,也就是挂接在GameObject上用于显示调试信息的脚本,可使用F3动态开启", ref entry.frameworkParam.enableScriptDebug);
		modified |= displayEnum("LoadSource", "加载源", ref entry.frameworkParam.loadSource);

		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}