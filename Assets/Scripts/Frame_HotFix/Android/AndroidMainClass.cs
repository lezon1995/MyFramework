using UnityEngine;
using static UnityUtility;
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
                logError("initJava failed! classPath not valid");
                return;
            }

            mainClass = new(classPath);
        }
    }

    public static AndroidJavaClass getMainClass()
    {
        return mainClass;
    }

    public override void destroy()
    {
        mainClass?.Dispose();
        mainClass = null;
        base.destroy();
    }

    // 获取当前的电流大小,单位微安
    public static int getBatteryEnergy()
    {
        return mainClass?.CallStatic<int>("getBatteryEnergy") ?? 0;
    }
}