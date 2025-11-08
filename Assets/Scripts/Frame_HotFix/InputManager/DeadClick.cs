using UnityEngine;
using System;

// 已经完成过的单击行为,用于双击检测
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