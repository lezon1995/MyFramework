using System;
using System.Collections.Generic;
using static StringUtility;
using static FileUtility;
using static FrameBaseDefine;
using static FrameBaseUtility;
using static FrameBase;

// 用于更新资源
public class GameDownload
{
    protected List<string> needDownloadFiles = new();
    protected List<string> dynamicDownloadFiles = new(); // 只动态下载的资源,此目录中的资源不会在更新时下载,只是在需要加载时才会下载
    protected GameDownloadCallback onProgress; // 下载进度的回调
    protected GameDownloadTipCallback onTip; // 下载提示的回调
    protected DateTime downloadingTimer; // 用于计时更新下载速度
    protected string downloadPath = F_PERSISTENT_ASSETS_PATH; // 默认下载到PersistentPath中
    protected int downloadedCount; // 已经下载的文件数量
    protected int downloadSpeed; // 下载速度
    protected bool allFinish = true; // 是否已经全部完成
    protected bool needWritePersistentFileList; // 是否在完成时写入Persist的文件列表

    public void willDestroy()
    {
        // 如果在未更新完成就关闭了程序,则确保在关闭之前更新文件列表
        if (!allFinish)
        {
            // 确保在缓存目录有当前的版本号文件
            writeTxtFile(downloadPath + VERSION, mAssetVersionSystem.getLocalVersion());
            updateLocalFileList();
            allFinish = true;
        }
    }

    public void setDynamicDownloadList(List<string> list)
    {
        dynamicDownloadFiles.setRange(list);
    }

    public void setShowTipCallback(GameDownloadTipCallback callback)
    {
        onTip = callback;
    }

    public void skipDownload(GameDownloadCallback callback)
    {
        onProgress = callback;
        allFinished();
    }

    public void startCheckVersion(GameDownloadCallback callback)
    {
        onProgress = callback;
        logBase("下载目录:" + downloadPath);
        logBase("资源下载地址:" + res.getDownloadURL());
        allFinish = false;

        onTip?.Invoke(DOWNLOAD_TIP.CHECKING_UPDATE);
        onProgress?.Invoke(0.0f, PROGRESS_TYPE.CHECKING_UPDATE, "", 0, 0);

        // 检查是否需要更新安装包,移动端会判断是否需要重新下载整个安装包
        string remoteVersion = mAssetVersionSystem.getRemoteVersion();
        string localVersion = mAssetVersionSystem.getLocalVersion();
        VERSION_COMPARE fullCompare = compareVersion3(remoteVersion, localVersion, out _, out VERSION_COMPARE bigCompare);
        // 如果本地版本号大于远端的,则不下载,此时远端资源还未上传,本地可以直接正常运行
        // 仅限安装的是全量资源包,才能从StreamingAssets中读取,如果不是全量资源包,则无法运行,但是此处无法判断是否为全量,只能默认为全量
        if (fullCompare == VERSION_COMPARE.REMOTE_LOWER)
        {
            // 根据StreamingAssets的文件数来判断是否为全量包,为了保险起见,文件数量小于等于5个时为非全量包
            if (mAssetVersionSystem.getStreamingAssetsFile().Count <= 5)
            {
                logErrorBase("当前不是全量安装包,且本地版本号大于远端版本号,无法运行游戏");
            }

            mAssetVersionSystem.setAssetReadPath(ASSET_READ_PATH.STREAMING_ASSETS_ONLY);
            onTip?.Invoke(DOWNLOAD_TIP.NONE);
            allFinished();
            return;
        }

        // 大版本号低于远端,本地无法直接升级大版本号,所以直接忽略,如果网络消息版本号匹配就继续运行,不匹配就会提示退出
        if (bigCompare == VERSION_COMPARE.LOCAL_LOWER)
        {
            mAssetVersionSystem.setAssetReadPath(ASSET_READ_PATH.PERSISTENT_FIRST);
            onTip?.Invoke(DOWNLOAD_TIP.NONE);
            allFinished();
            return;
        }

        var streamingFiles = mAssetVersionSystem.getStreamingAssetsFile();
        var persistentFiles = mAssetVersionSystem.getPersistentAssetsFile();
        var remoteFiles = mAssetVersionSystem.getRemoteAssetsFile();
        logBase("本地StreamingAssets文件数量：" + streamingFiles.Count);
        logBase("本地PersistentAssets文件数量：" + persistentFiles.Count);
        logBase("远端文件数量：" + remoteFiles.Count);

        onProgress?.Invoke(0.0f, PROGRESS_TYPE.DELETE_FILE, "", 0, 0);

        // 删除文件,只能删除Persistent中的文件,但是列表中的元素还是需要都删除掉
        // Persistent中需要删除列表记录,删除文件
        DateTime start = DateTime.Now;
        List<string> deleteFileList = checkDeleteFile(persistentFiles, remoteFiles);
        logBase("需要删除" + deleteFileList.Count + "个文件");
        foreach (string fileToDelete in deleteFileList)
        {
            persistentFiles.Remove(fileToDelete);
            string fullPath = F_PERSISTENT_ASSETS_PATH + fileToDelete;
            logBase("删除文件:" + fullPath);
            if (!deleteFile(fullPath))
            {
                logErrorBase("删除文件失败:" + fullPath);
            }

            needWritePersistentFileList = true;
        }

        // StreamingAssets中无法删除文件,只能删除列表记录
        foreach (string fileToDelete in checkDeleteFile(streamingFiles, remoteFiles))
        {
            streamingFiles.Remove(fileToDelete);
        }

        logBase("删除文件耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒");

        // 要下载的文件,其中不包含版本文件,下载新文件,每次只下载一个文件
        DateTime start1 = DateTime.Now;
        checkNeedDownloadFile(needDownloadFiles, streamingFiles, persistentFiles, remoteFiles, dynamicDownloadFiles);
        logBase("对比需要下载的文件列表耗时:" + (int)(DateTime.Now - start1).TotalMilliseconds + "毫秒");
        needDownloadFiles.Remove(VERSION);
        logBase("需要下载" + needDownloadFiles.Count + "个文件");
        onTip?.Invoke(DOWNLOAD_TIP.NONE);
        if (needDownloadFiles.Count == 0)
        {
            allFinished();
        }
        else
        {
            downloadingTimer = DateTime.Now;
            downloadFile(downloadedCount);
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------
    // 下载普通资源文件
    protected void downloadFile(int index)
    {
        string fileName = needDownloadFiles[index];
        downloadProgress(fileName, index, 0.0f);
        ResourceManager.loadAssetsFromUrl(res.getDownloadURL() + fileName, bytes =>
        {
            // 单个资源文件下载完毕
            if (bytes == null)
            {
                logBase("下载失败! " + fileName);
                onTip?.Invoke(DOWNLOAD_TIP.DOWNLOAD_FAILED);
                return;
            }

            mAssetVersionSystem.addDownloadedInfo(bytes.Length, getFileNameWithSuffix(fileName));
            // 将文件保存到本地
            writeFile(downloadPath + fileName, bytes, bytes.Length);
            // 只要有下载完成的,就需要在最后重新写入Persistent的FileList
            needWritePersistentFileList = true;

            // 检查下载的文件是否正确
            if (!mAssetVersionSystem.getRemoteAssetsFile().TryGetValue(fileName, out GameFileInfo remoteInfo))
            {
                logErrorBase("已下载的文件不存在与远端文件列表, 下载的文件:" + fileName);
                onTip?.Invoke(DOWNLOAD_TIP.NOT_IN_REMOTE_FILE_LIST);
                return;
            }

            GameFileInfo localInfo = new();
            localInfo.name = fileName;
            localInfo.size = bytes.Length;
            localInfo.md5 = generateFileMD5(bytes);
            mAssetVersionSystem.getPersistentAssetsFile().set(fileName, localInfo);
            if (remoteInfo.name != localInfo.name || remoteInfo.size != localInfo.size || remoteInfo.md5 != localInfo.md5)
            {
                logErrorBase("下载的文件信息与远端的信息不一致:下载的信息:" + localInfo.name + ", " + localInfo.size + ", " + localInfo.md5 + ", 远端的信息:" + remoteInfo.name + ", " + remoteInfo.size + ", " + remoteInfo.md5);
                onTip?.Invoke(DOWNLOAD_TIP.VERIFY_FAILED);
            }

			// 所有文件已经下载完毕
			if (++downloadedCount >= needDownloadFiles.Count)
			{
				allFinished();
			}
			// 还没下载完,下载下一个文件,这里延迟执行,避免可能的递归太深,导致栈溢出
			else
			{
				downloadFile(downloadedCount);
			}
		}, (downloaded, downloadDelta, deltaTimeMillis, progress)=>
		{
			downloadSpeed = (int)(downloadDelta * 1000 / (float)deltaTimeMillis);
			if ((DateTime.Now - downloadingTimer).TotalSeconds > 1.0f)
			{
				downloadProgress(fileName, index, progress);
			}
		});
	}
	// 计算剩余下载时间
	protected void downloadProgress(string fileName, int index, float progress)
	{
		var remoteFiles = mAssetVersionSystem.getRemoteAssetsFile();
		// 计算剩余的下载字节数,计算剩余时间
		int allCount = needDownloadFiles.Count;
		ulong remainBytes = (ulong)(remoteFiles.get(needDownloadFiles[index]).size * (1.0f - progress));
		for (int i = index + 1; i < allCount; ++i)
		{
			remainBytes += (ulong)remoteFiles.get(needDownloadFiles[i]).size;
		}
		int remainTime = downloadSpeed != 0 ? (int)(remainBytes / (ulong)downloadSpeed) : 0;
		onProgress?.Invoke(allCount != 0 ? index / (float)allCount : 0.0f, PROGRESS_TYPE.DOWNLOAD_RESOURCE, fileName, downloadSpeed, remainTime);
	}
	// 所有资源更新完毕
	protected void allFinished()
	{
		// 更新FileList文件,VERSION文件
		string remoteVersion = mAssetVersionSystem.getRemoteVersion();
		if (!remoteVersion.isEmpty())
		{
			writeTxtFile(downloadPath + VERSION, remoteVersion);
			mAssetVersionSystem.setPersistentDataVersion(remoteVersion);
		}
		updateLocalFileList();

        // 游戏更新完毕
        allFinish = true;
        onProgress?.Invoke(1.0f, PROGRESS_TYPE.FINISH, "", 0, 0);
    }

    protected void updateLocalFileList()
    {
        if (needWritePersistentFileList)
        {
            writeFileList(F_PERSISTENT_ASSETS_PATH, mAssetVersionSystem.generatePersistentAssetFileList());
            logBase("本地文件信息列表更新完毕");
        }
    }
}