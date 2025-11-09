using System.Collections.Generic;
using UnityEngine;
using static StringUtility;
using static UnityUtility;
using static FrameBaseDefine;
using static FrameBaseUtility;

// 用于加载Android平台下的资源
public class AndroidAssetLoader : FrameSystem
{
    protected static AndroidJavaObject assetLoader; // Java中加载类的实例

    public static void initJava(string classPath)
    {
        if (!isEditor() && isAndroid())
        {
            if (classPath.isEmpty())
            {
                logError("initJava failed! classPath not valid");
                return;
            }

            var assetManager = AndroidPluginManager.getMainActivity().Call<AndroidJavaObject>("getAssets");
            if (assetManager == null)
            {
                logError("assetManager is null");
            }

            assetLoader = new AndroidJavaClass(classPath).CallStatic<AndroidJavaObject>("getAssetLoader", assetManager);
            if (assetLoader == null)
            {
                logError("assetLoader is null");
            }
        }
    }

    public override void destroy()
    {
        assetLoader?.Dispose();
        assetLoader = null;
        base.destroy();
    }

    // 相对于StreamingAssets的路径
    public static byte[] loadAsset(string path, bool errorIfNull)
    {
        return assetLoader?.Call<byte[]>("loadAsset", path, errorIfNull);
    }

    public static string loadTxtAsset(string path, bool errorIfNull)
    {
        return assetLoader?.Call<string>("loadTxtAsset", path, errorIfNull);
    }

    public static bool isAssetExist(string path)
    {
        return assetLoader != null && assetLoader.Call<bool>("isAssetExist", path);
    }

    public static void findAssets(string path, List<string> fileList, List<string> patterns, bool recursive)
    {
        if (assetLoader == null)
            return;

        string pattern = stringsToString(patterns, ' ');
        var fileListObject = assetLoader.Call<AndroidJavaObject>("startFindAssets", path, pattern, recursive);
        javaListToList(fileListObject, fileList);
    }

    public static void findAssetsFolder(string path, List<string> fileList, bool recursive)
    {
        if (assetLoader == null)
            return;

        var fileListObject = assetLoader.Call<AndroidJavaObject>("startFindAssetsFolder", path, recursive);
        javaListToList(fileListObject, fileList);
    }

    // 将安卓下的StreamingAsset目录中的文件拷贝到PersistentDataPath中
    public static void copyAssetToPersistentPath(string sourcePath, string destPath, bool errorIfNull)
    {
        if (assetLoader == null)
            return;

        checkPersistentDataPath(destPath);
        assetLoader.Call("copyAssetToPersistentPath", sourcePath, destPath, errorIfNull);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    // 以下函数只能用于Android平台的persistentDataPath目录操作,path为绝对路径
    public static byte[] loadFile(string path, bool errorIfNull)
    {
        if (assetLoader == null)
            return null;

        checkPersistentDataPath(path);
        return assetLoader.CallStatic<byte[]>("loadFile", path, errorIfNull);
    }

    public static string loadTxtFile(string path, bool errorIfNull)
    {
        if (assetLoader == null)
            return null;

        checkPersistentDataPath(path);
        return assetLoader.CallStatic<string>("loadTxtFile", path, errorIfNull);
    }

    public static void writeFile(string path, byte[] buffer, int writeCount, bool appendData)
    {
        if (assetLoader == null)
            return;

        checkPersistentDataPath(path);
        assetLoader.CallStatic("writeFile", path, buffer, writeCount, appendData);
    }

    public static void writeTxtFile(string path, string str, bool appendData)
    {
        if (assetLoader == null)
            return;

        checkPersistentDataPath(path);
        assetLoader.CallStatic("writeTxtFile", path, str, appendData);
    }

    public static bool deleteFile(string path)
    {
        if (assetLoader == null)
            return false;

        checkPersistentDataPath(path);
        return assetLoader.CallStatic<bool>("deleteFile", path);
    }

    public static bool isDirExist(string path)
    {
        if (assetLoader == null)
            return false;

        checkPersistentDataPath(path);
        return assetLoader.CallStatic<bool>("isDirExist", path);
    }

    public static bool isFileExist(string path)
    {
        if (assetLoader == null)
            return false;

        checkPersistentDataPath(path);
        return assetLoader.CallStatic<bool>("isFileExist", path);
    }

    public static int getFileSize(string path)
    {
        if (assetLoader == null)
        {
            return 0;
        }

        checkPersistentDataPath(path);
        return assetLoader.CallStatic<int>("getFileSize", path);
    }

    public static void findFiles(string path, List<string> fileList, IList<string> patterns, bool recursive)
    {
        if (assetLoader == null)
            return;

        checkPersistentDataPath(path);
        string pattern = stringsToString(patterns, ' ');
        var fileListObject = assetLoader.CallStatic<AndroidJavaObject>("startFindFiles", path, pattern, recursive);
        javaListToList(fileListObject, fileList);
    }

    public static void findFolders(string path, List<string> fileList, bool recursive)
    {
        if (assetLoader == null)
            return;

        checkPersistentDataPath(path);
        var fileListObject = assetLoader.CallStatic<AndroidJavaObject>("startFindFolders", path, recursive);
        javaListToList(fileListObject, fileList);
    }

    public static void createDirectoryRecursive(string path)
    {
        if (assetLoader == null)
            return;

        checkPersistentDataPath(path);
        assetLoader.CallStatic("createDirectoryRecursive", path);
    }

    public static bool deleteDirectory(string path)
    {
        if (assetLoader == null)
            return false;

        checkPersistentDataPath(path);
        return assetLoader.CallStatic<bool>("deleteDirectory", path);
    }

    public static string generateMD5(string path)
    {
        if (assetLoader == null)
            return null;

        checkPersistentDataPath(path);
        return assetLoader.CallStatic<string>("generateMD5", path);
    }

    public static void generateMD5List(List<string> pathList, List<string> md5List)
    {
        if (assetLoader == null)
            return;

        md5List.Capacity = pathList.Count;
        foreach (string path in pathList)
        {
            md5List.Add(generateMD5(path));
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected static void checkPersistentDataPath(string path)
    {
        path = path.addEndSlash();
        if (!path.startWith(F_PERSISTENT_DATA_PATH))
        {
            logError("path must start with " + F_PERSISTENT_DATA_PATH + ", path : " + path);
        }
    }

    protected static int getListSize(AndroidJavaObject javaListObject)
    {
        return assetLoader.CallStatic<int>("getListSize", javaListObject);
    }

    protected static void javaListToList(AndroidJavaObject javaListObject, List<string> list)
    {
        int count = getListSize(javaListObject);
        for (int i = 0; i < count; ++i)
        {
            if (!list.addNotEmpty(assetLoader.CallStatic<string>("getListElement", javaListObject, i)))
            {
                break;
            }
        }
    }
}