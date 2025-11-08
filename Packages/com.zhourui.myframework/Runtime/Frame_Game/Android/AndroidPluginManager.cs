using UnityEngine;
using static FrameBaseUtility;

// 用于管理所有跟Java交互的对象
public class AndroidPluginManager : FrameSystem
{
    protected static AndroidJavaClass unityPlayer; // 固定的UnityPlayer的Java实例
    protected static AndroidJavaObject mainActivity; // 固定的MainActivity的Java实例
    protected static AndroidJavaObject application; // 固定的Application的Java实例
    protected static string androidPackageName;

    public static void initAndroidPlugin(string packageName)
    {
        androidPackageName = packageName;
        if (!isEditor() && isAndroid())
        {
            unityPlayer = new("com.unity3d.player.UnityPlayer");
            mainActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            application = mainActivity.Call<AndroidJavaObject>("getApplication");
            if (mainActivity == null)
            {
                logErrorBase("mainActivity is null");
            }
        }
    }

    public override void destroy()
    {
        unityPlayer?.Dispose();
        mainActivity?.Dispose();
        application?.Dispose();
        unityPlayer = null;
        mainActivity = null;
        application = null;
        base.destroy();
    }

    public static AndroidJavaObject getMainActivity()
    {
        return mainActivity;
    }

    public static AndroidJavaObject getApplication()
    {
        return application;
    }

    public static string getPackageName()
    {
        return androidPackageName;
    }
}