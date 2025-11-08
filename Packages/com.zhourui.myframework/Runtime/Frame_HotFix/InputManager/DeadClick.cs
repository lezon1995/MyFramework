using UnityEngine;
using System;

// 已经完成过的单击行为,用于双击检测
// 记录单击发生的时间和位置,InputSystem通过对比两次单击的间隔和位置差判断是否为双击
public struct DeadClick
{
    public DateTime clickTime;
    public Vector3 clickPos;

    public DeadClick(Vector3 p)
    {
        clickTime = DateTime.Now;
        clickPos = p;
    }
}