using System.Collections.Generic;
using System.Text;
using static UnityUtility;
using static StringUtility;
using static FrameBaseDefine;
using static FrameBaseUtility;

// 用于检测资源的版本号
public class AssetVersionSystem : FrameSystem
{
    protected Dictionary<string, GameFileInfo> streamingAssetFiles = new();
    protected Dictionary<string, GameFileInfo> persistentAssetFiles = new();
    protected Dictionary<string, GameFileInfo> remoteAssetFiles = new();
    protected List<string> totalDownloadedFiles = new(); // 已经下载的文件列表,用于统计下载文件记录
    protected StringCallback mDownloadRemoteFileListCallback;
    protected string streamingAssetsVersion;
    protected string persistentAssetsVersion;
    protected string remoteAssetsVersion;
    protected long totalDownloadBytes; // 已经消耗的总下载量,单位字节,用于统计下载字节数
    protected static ASSET_READ_PATH readPathType = ASSET_READ_PATH.SAME_TO_REMOTE;

    public long getTotalDownloadedByteCount()
    {
        return totalDownloadBytes;
    }

    public List<string> getTotalDownloadedFiles()
    {
        return totalDownloadedFiles;
    }

    public void setTotalDownloadedFiles(List<string> files)
    {
        totalDownloadedFiles.setRange(files);
    }

    public void setTotalDownloadedByteCount(long count)
    {
        totalDownloadBytes = count;
    }

    public void clearDownloadedInfo()
    {
        totalDownloadBytes = 0;
        totalDownloadedFiles.Clear();
    }

    // 未启用热更或者本地版本号大于远端版本号时,都应该设置为强制从StreamingAssets中加载
    public void setAssetReadPath(ASSET_READ_PATH pathType)
    {
        readPathType = pathType;
    }

    // 获取文件的加载路径,filePath是StreamingAssets下的相对路径
    public string getFileReadPath(string filePath)
    {
        if (readPathType == ASSET_READ_PATH.SAME_TO_REMOTE)
        {
            // 远端没有此文件
            if (!remoteAssetFiles.TryGetValue(filePath, out var remoteInfo))
            {
                // 完全没有此文件信息,无法加载
                logError("远端没有此文件,filePath:" + filePath);
                return null;
            }

            // persistent中的文件信息与远端一致,则读取persistent中的文件
            if (persistentAssetFiles.TryGetValue(filePath, out var persistentInfo) &&
                persistentInfo.size == remoteInfo.size &&
                persistentInfo.md5 == remoteInfo.md5)
            {
                return F_PERSISTENT_ASSETS_PATH + filePath;
            }

            // streamingAssets中的文件信息与远端一致,则读取streamingAssets中的文件
            if (streamingAssetFiles.TryGetValue(filePath, out var streamingInfo) &&
                streamingInfo.size == remoteInfo.size &&
                streamingInfo.md5 == remoteInfo.md5)
            {
                return F_ASSET_BUNDLE_PATH + filePath;
            }

            // 本地没有此文件,则从远端下载
            return null;
        }

        if (readPathType == ASSET_READ_PATH.STREAMING_ASSETS_ONLY)
        {
            return F_ASSET_BUNDLE_PATH + filePath;
        }

        if (readPathType == ASSET_READ_PATH.REMOTE_ASSETS_ONLY)
        {
            // 返回null,会自动开始下载
            return null;
        }

        logError("无法获取文件路径,filePath:" + filePath);
        return null;
    }

    public void setStreamingAssetsVersion(string streamingVersion)
    {
        streamingAssetsVersion = streamingVersion;
    }

    public void setPersistentAssetsVersion(string persistentVersion)
    {
        persistentAssetsVersion = persistentVersion;
    }

    public void setRemoteVersion(string version)
    {
        remoteAssetsVersion = version;
    }

    public string getStreamingAssetsVersion()
    {
        return streamingAssetsVersion;
    }

    public string getPersistentAssetsVersion()
    {
        return persistentAssetsVersion;
    }

    public string getRemoteAssetsVersion()
    {
        return remoteAssetsVersion;
    }

    public string getLocalVersion()
    {
        if (streamingAssetsVersion == null && persistentAssetsVersion == null)
        {
            return null;
        }

        // 选择更高版本号的
        if (persistentAssetsVersion == null ||
            compareVersion3(streamingAssetsVersion, persistentAssetsVersion, out _, out _) == VERSION_COMPARE.LOCAL_LOWER)
        {
            return streamingAssetsVersion;
        }

        return persistentAssetsVersion;
    }

    public void addPersistentFile(GameFileInfo info)
    {
        persistentAssetFiles.TryAdd(info.name, info);
    }

    public string generatePersistentAssetFileList()
    {
        StringBuilder fileString = new();
        fileString.Append(IToS(persistentAssetFiles.Count));
        fileString.Append("\n");
        foreach (GameFileInfo item in persistentAssetFiles.Values)
        {
            item.toString(fileString);
            fileString.Append("\n");
        }

        return fileString.ToString();
    }

    public void setStreamingAssetsFile(Dictionary<string, GameFileInfo> infoList)
    {
        streamingAssetFiles.setRange(infoList);
    }

    public void setPersistentAssetsFile(Dictionary<string, GameFileInfo> infoList)
    {
        persistentAssetFiles.setRange(infoList);
    }

    public void setRemoteAssetsFile(Dictionary<string, GameFileInfo> infoList)
    {
        remoteAssetFiles.setRange(infoList);
    }
}