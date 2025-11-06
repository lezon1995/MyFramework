using System;

// 场景注册信息
public class SceneRegisterInfo
{
    public string name; // 场景名
    public string path; // 场景路径
    public Type type; // 场景逻辑类的类型
    public SceneScriptCallback callback; // 用于给场景脚本对象赋值
}