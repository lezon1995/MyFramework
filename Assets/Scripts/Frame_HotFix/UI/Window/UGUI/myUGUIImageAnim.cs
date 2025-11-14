using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static MathUtility;

// Image的序列帧
public class myUGUIImageAnim : myUGUIImage, IUIAnimation
{
    protected List<TextureAnimCallback> onPlayEndList; // 一个序列播放完时的回调函数,只在非循环播放状态下有效
    protected List<TextureAnimCallback> onPlayingList; // 一个序列正在播放时的回调函数
    protected List<Vector2> texturePosList; // 每一帧的位置偏移列表
    protected List<Sprite> sprites = new(); // 序列帧图片列表
    protected OnPlayEndCallback _onPlayEnd; // 播放完成时的回调
    protected OnPlayingCallback _onPlaying; // 正在播放的回调
    protected AnimControl control = new(); // 序列帧控制器
    protected string textureSetName; // 序列帧名字
    protected bool useTextureSize; // 是否使用图片的大小改变当前窗口大小
    protected EFFECT_ALIGN effectAlign; // 图片的位置对齐方式

    public myUGUIImageAnim()
    {
        needUpdate = true;
        _onPlayEnd = onPlayEnd;
        _onPlaying = onPlaying;
    }

    public override void init()
    {
        base.init();
        string spriteName = getSpriteName();
        if (!spriteName.isEmpty() && spriteName.Contains('_'))
        {
            setTextureSet(spriteName.rangeToLast('_'));
        }

        control.setObject(this);
        control.setPlayEndCallback(_onPlayEnd);
        control.setPlayingCallback(_onPlaying);
    }

    public override void update(float dt)
    {
        base.update(dt);
        if (isCulled())
            return;

        if (sprites.Count == 0)
        {
            setSpriteName(null);
        }

        control.update(dt);
    }

    public override void setAtlas(UGUIAtlasPtr atlas, bool clearSprite = false, bool force = false)
    {
        if (!force && atlas?.getAtlas() == getAtlas()?.getAtlas())
            return;

        // 改变图集时先停止播放
        stop();
        base.setAtlas(atlas, clearSprite, force);
        // 图集改变后自动设置默认的序列帧
        setTextureSet(null);
    }

    // 设置图集,并且设置将第一张图片设置为要播放的序列帧
    public void setAtlasWithFirstSprite(UGUIAtlasPtr atlas, bool clearSprite = false, bool force = false)
    {
        if (!force && atlas?.getAtlas() == getAtlas()?.getAtlas())
            return;

        // 改变图集时先停止播放
        stop();
        base.setAtlas(atlas, clearSprite, force);
        // 图集改变后自动设置默认的序列帧
        setTextureSet(atlas.getFirstSpriteName().rangeToLast('_'));
    }

    public string getTextureSet()
    {
        return textureSetName;
    }

    public int getTextureFrameCount()
    {
        return sprites.Count;
    }

    public void setUseTextureSize(bool useSize)
    {
        useTextureSize = useSize;
    }

    public void setTexturePosList(List<Vector2> posList)
    {
        texturePosList = posList;
        if (texturePosList != null)
        {
            setEffectAlign(EFFECT_ALIGN.POSITION_LIST);
        }
    }

    public List<Vector2> getTexturePosList()
    {
        return texturePosList;
    }

    public void setEffectAlign(EFFECT_ALIGN align)
    {
        effectAlign = align;
    }

    public void setTextureSet(string name)
    {
        if (textureSetName == name)
            return;

        sprites.Clear();
        textureSetName = name;
        if (!textureSetName.isEmpty())
        {
            int index = 0;
            while (sprites.addNotNull(getSpriteInAtlas(textureSetName + "_" + IToS(index++))))
            {
            }

            if (getTextureFrameCount() == 0)
            {
                logError("invalid sprite anim! atlas : " + (getAtlas()?.getFilePath() ?? EMPTY) + ", anim set : " + name);
            }
        }

        control.setFrameCount(getTextureFrameCount());
    }

    public LOOP_MODE getLoop()
    {
        return control.getLoop();
    }

    public float getInterval()
    {
        return control.getInterval();
    }

    public float getSpeed()
    {
        return control.getSpeed();
    }

    public int getStartIndex()
    {
        return control.getStartIndex();
    }

    public float getPlayedTime()
    {
        return control.getPlayedTime();
    }

    public float getLength()
    {
        return control.getLength();
    }

    public PLAY_STATE getPlayState()
    {
        return control.getPlayState();
    }

    public bool getPlayDirection()
    {
        return control.getPlayDirection();
    }

    public int getEndIndex()
    {
        return control.getEndIndex();
    }

    public bool isAutoHide()
    {
        return control.isAutoResetIndex();
    }

    // 获得实际的终止下标,如果是自动获得,则返回最后一张的下标
    public int getRealEndIndex()
    {
        return control.getRealEndIndex();
    }

    public void setLoop(LOOP_MODE loop)
    {
        control.setLoop(loop);
    }

    public void setInterval(float interval)
    {
        control.setInterval(interval);
    }

    public void setSpeed(float speed)
    {
        control.setSpeed(speed);
    }

    public void setPlayDirection(bool direction)
    {
        control.setPlayDirection(direction);
    }

    public void setAutoHide(bool autoHide)
    {
        control.setAutoHide(autoHide);
    }

    public void setStartIndex(int startIndex)
    {
        control.setStartIndex(startIndex);
    }

    public void setEndIndex(int endIndex)
    {
        control.setEndIndex(endIndex);
    }

    public void stop(bool resetStartIndex = true, bool callback = true, bool isBreak = true)
    {
        control.stop(resetStartIndex, callback, isBreak);
    }

    public void play()
    {
        control.play();
    }

    public void pause()
    {
        control.pause();
    }

    public int getCurFrameIndex()
    {
        return control.getCurFrameIndex();
    }

    public void setCurFrameIndex(int index)
    {
        control.setCurFrameIndex(index);
    }

    public void addPlayEndCallback(TextureAnimCallback callback, bool clear = true)
    {
        if (clear && !onPlayEndList.isEmpty())
        {
            using var a = new ListScope<TextureAnimCallback>(out var tempList);
            // 如果回调函数当前不为空,则是中断了更新
            foreach (var onPlayEnd in tempList.move(onPlayEndList))
                onPlayEnd(this, true);
        }

        if (callback != null)
        {
            onPlayEndList ??= new();
            onPlayEndList.Add(callback);
        }
    }

    public void addPlayingCallback(TextureAnimCallback callback, bool clear = true)
    {
        if (clear)
            onPlayingList?.Clear();

        if (callback != null)
        {
            onPlayingList ??= new();
            onPlayingList.Add(callback);
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void onPlaying(AnimControl c, int frame, bool isPlaying)
    {
        if (control.getCurFrameIndex() >= sprites.Count)
        {
            return;
        }

        setSprite(sprites[control.getCurFrameIndex()], useTextureSize);
        // 使用位置列表进行校正
        if (effectAlign == EFFECT_ALIGN.POSITION_LIST)
        {
            if (!texturePosList.isEmpty())
            {
                setPosition(texturePosList[round(divide(frame, sprites.Count * texturePosList.Count))]);
            }
        }
        // 对齐父节点的底部
        else if (effectAlign == EFFECT_ALIGN.PARENT_BOTTOM)
        {
            myUGUIObject parent = getParent();
            if (parent != null)
            {
                setPositionY((getWindowSize().y - parent.getWindowSize().y) * 0.5f);
            }
        }

        foreach (TextureAnimCallback item in onPlayingList.safe())
        {
            item(this, false);
        }
    }

    protected void onPlayEnd(AnimControl control, bool callback, bool isBreak)
    {
        // 正常播放完毕后根据是否重置下标来判断是否自动隐藏
        if (!isBreak && this.control.isAutoResetIndex())
        {
            setActive(false);
        }

        if (onPlayEndList.isEmpty())
            return;

        if (callback)
        {
            using var a = new ListScope<TextureAnimCallback>(out var tempList);
            foreach (var onPlayEnd in tempList.move(onPlayEndList))
            {
                onPlayEnd(this, isBreak);
            }
        }
        else
        {
            onPlayEndList.Clear();
        }
    }
}