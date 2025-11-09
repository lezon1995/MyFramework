using System.Collections.Generic;

// 用于存储跨域数据,在非热更时存储,热更时读取
public class FrameCrossParam
{
    public static string localizationName; // 当前选择的语言类型
    public static string downloadURL; // 下载地址
    public static string streamingAssetsVersion; // StreamingAssets中的版本号
    public static string persistentDataVersion; // PersistentData中的版本号
    public static string remoteVersion; // 远端的版本号
    public static Dictionary<string, GameFileInfo> streamingAssetsFileList = new();
    public static Dictionary<string, GameFileInfo> persistentAssetsFileList = new();
    public static Dictionary<string, GameFileInfo> remoteAssetsFileList = new();
    public static List<string> totalDownloadedFiles = new(); // 已经下载的文件列表,用于统计下载文件记录
    public static long totalDownloadByteCount; // 已经消耗的总下载量,单位字节,用于统计下载字节数
    public static ASSET_READ_PATH assetReadPath; // 资源路径类型
}