using UnityEngine;
using static FrameBaseUtility;

// 用于加载Android平台下的资源
public class AndroidMainClass : FrameSystem
{
    protected static AndroidJavaClass mainClass; // Java中加载类的实例

    public static void initJava(string classPath)
    {
        if (!isEditor() && isAndroid())
        {
            if (classPath.isEmpty())
            {
                logErrorBase("initJava failed! classPath not valid");
                return;
            }

            mainClass = new(classPath);
        }
    }

    public override void destroy()
    {
        mainClass?.Dispose();
        mainClass = null;
        base.destroy();
    }

    public static AndroidJavaClass getMainClass()
    {
        return mainClass;
    }

    public static void gameStart()
    {
        if (isEditor() || !isAndroid())
            return;

        if (mainClass == null)
        {
            logErrorBase("MainClass is null");
            return;
        }

        mainClass.CallStatic("gameStart", AndroidPluginManager.getMainActivity(), AndroidPluginManager.getApplication());
    }
}