using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

// 音效管理器的调试信息
public class AudioManagerDebug : MonoBehaviour
{
    public List<string> audios = new(); // 音频列表
    public List<string> loadedAudios = new(); // 已加载音频列表

    public void Update()
    {
        if (GameEntry.getInstance() == null || !GameEntry.getInstance().frameworkParam.enableScriptDebug)
            return;

        audios.Clear();
        loadedAudios.Clear();
        foreach (var item in mAudioManager.getAudioList().Values)
        {
            if (item.mIsLocal)
            {
                audios.Add(item.mState + "\t" + item.mAudioName + ",  InResources");
            }
            else
            {
                audios.Add(item.mState + "\t" + item.mAudioName);
            }

            if (item.mState == LOAD_STATE.LOADED)
            {
                loadedAudios.Add(item.mAudioName);
            }
        }
    }
}