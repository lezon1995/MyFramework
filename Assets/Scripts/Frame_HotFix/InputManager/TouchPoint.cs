using UnityEngine;
using System.Collections.Generic;
using System;
using static MathUtility;
using static FrameDefine;

// 触摸点信息
public class TouchPoint : ClassObject
{
    protected DateTime downTime; // 触点按下的时间
    protected DateTime upTime; // 触点抬起的时间
    protected Vector3 downPos; // 触点按下的坐标
    protected Vector3 lastPos; // 上一帧的触点位置
    protected Vector3 curPos; // 当前的触点位置
    protected Vector2 moveDelta; // 触点在这一帧的移动量
    protected int touchId; // 触点ID
    protected bool currentDown; // 触点是否在这一帧才按下的
    protected bool doubleClick; // 是否为双击操作
    protected bool currentUp; // 触点是否在这一帧抬起
    protected bool mouse; // 是否为鼠标的触点
    protected bool clicked; // 是否已点击
    protected bool down; // 当前是否按下

    public override void resetProperty()
    {
        base.resetProperty();
        downTime = DateTime.Now;
        upTime = DateTime.Now;
        downPos = Vector3.zero;
        lastPos = Vector3.zero;
        curPos = Vector3.zero;
        moveDelta = Vector2.zero;
        touchId = 0;
        currentDown = false;
        doubleClick = false;
        currentUp = false;
        mouse = false;
        clicked = false;
        down = false;
    }

    public void pointDown(Vector3 pos)
    {
        downPos = pos;
        downTime = DateTime.Now;
        curPos = pos;
        lastPos = pos;
        currentDown = true;
        down = true;
    }

    public void pointUp(Vector3 pos, List<DeadClick> deadTouchList)
    {
        currentUp = true;
        down = false;
        upTime = DateTime.Now;
        curPos = pos;
        clicked = lengthLess(downPos - curPos, CLICK_LENGTH);
        // 遍历一段时间内已经完成点击的触点列表,查看有没有点坐标相近,时间相近的触点
        if (clicked)
        {
            foreach (DeadClick deadClick in deadTouchList)
            {
                if ((upTime - deadClick.clickTime).TotalSeconds < DOUBLE_CLICK_TIME &&
                    lengthLess(curPos - deadClick.clickPos, CLICK_LENGTH))
                {
                    doubleClick = true;
                    break;
                }
            }
        }
    }

    public void update(Vector3 newPosition)
    {
        lastPos = curPos;
        curPos = newPosition;
        moveDelta = curPos - lastPos;
    }

    public void lateUpdate()
    {
        currentDown = false;
        currentUp = false;
        clicked = false;
        doubleClick = false;
    }

    public void setMouse(bool mouse)
    {
        this.mouse = mouse;
    }

    public void setTouchID(int touchID)
    {
        touchId = touchID;
    }

    public int getTouchID()
    {
        return touchId;
    }

    public bool isMouse()
    {
        return mouse;
    }

    public bool isCurrentUp()
    {
        return currentUp;
    }

    public bool isCurrentDown()
    {
        return currentDown;
    }

    public bool isDoubleClick()
    {
        return doubleClick;
    }

    public bool isClick()
    {
        return clicked;
    }

    public bool isDown()
    {
        return down;
    }

    public Vector3 getDownPosition()
    {
        return downPos;
    }

    public Vector3 getMoveDelta()
    {
        return moveDelta;
    }

    public Vector3 getLastPosition()
    {
        return lastPos;
    }

    public Vector3 getCurPosition()
    {
        return curPos;
    }

    public DateTime getDownTime()
    {
        return downTime;
    }

    public DateTime getUpTime()
    {
        return upTime;
    }

    public void resetState()
    {
        currentUp = false;
        down = false;
        upTime = DateTime.Now;
        clicked = false;
        doubleClick = false;
    }
}