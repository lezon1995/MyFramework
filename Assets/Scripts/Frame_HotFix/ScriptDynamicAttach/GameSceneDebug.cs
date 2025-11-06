using UnityEngine;

// 逻辑场景调试信息
public class GameSceneDebug : MonoBehaviour
{
    protected GameScene gameScene; // 所属场景
    public string curProcedure; // 当前流程

    public void setGameScene(GameScene scene)
    {
        gameScene = scene;
    }

    public void Update()
    {
        if (GameEntry.getInstance() == null || !GameEntry.getInstance().frameworkParam.enableScriptDebug)
            return;

        SceneProcedure sceneProcedure = gameScene.getCurProcedure();
        if (sceneProcedure != null)
        {
            curProcedure = sceneProcedure.GetType().ToString();
        }
    }
}