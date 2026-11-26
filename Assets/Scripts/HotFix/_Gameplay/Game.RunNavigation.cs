using System;
using System.Linq;
using UnityEngine;

namespace MoreMountains;

public partial class Game
{
    public static bool RunExitPending { get; private set; }
    static bool restartRequested;

    public static void RequestRunExit(bool restart)
    {
        if (RunExitPending || mode != GameMode.GAMEPLAY) return;
        restartRequested = restart;
        RunExitPending = true;
    }

    void CompleteRunExit()
    {
        bool restart = restartRequested;
        var oldPlayer = player;
        try
        {
            // 先解绑 UI 和房间监听，再回收实体；清场不触发死亡奖励或下一波。
            EscPanelService.Instance.ReleasePlayer();
            OperationPanelService.Instance.Close();
            OperationPanelService.Instance.ReleasePlayer();
            OverlayMenuService.Instance.Close();
            OverlayMenuService.Instance.ReleasePlayer();
            room?.onPlayerExit();
            room?.onRoomUninitialize();
            if (room is MonsterRoom battle)
            {
                battle.curPhase?.Dispose();
                battle.curPhase = null;
                if (battle.waveGameMode != null)
                {
                    battle.waveGameMode.gameObject.SetActive(false);
                    UnityEngine.Object.Destroy(battle.waveGameMode.gameObject);
                }
            }
            if (oldPlayer != null)
            {
                foreach (var weapon in oldPlayer.mainHandleWeapons) weapon.ForceStop();
                oldPlayer.BallManagement?.Instance.destroyAllBall();
                oldPlayer.Inventory.BallBag.Clear();
            }
            if (brickManager != null)
                foreach (var brick in brickManager.getActiveBrickList().ToArray())
                    if (brick != null) brickManager.releaseBrick(brick);
            if (waveManager != null)
            {
                waveManager.ActiveMonsters.Clear();
                waveManager.ActiveBosses.Clear();
                waveManager.enabled = false;
            }
            if (coinManager != null)
            {
                coinManager.ClearAllCoins();
                coinManager.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(coinManager.gameObject);
            }
            if (expManager != null)
            {
                expManager.ClearAllExps();
                expManager.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(expManager.gameObject);
            }
            waveManager = null;
            coinManager = null;
            expManager = null;
            if (GameManager.Instance != null) GameManager.Instance.Reset();
            stopClock = false;
            loadingSave = false;
            saveFile = null;
            // 复用现有 startOver 的种子、地图、指标、解锁及主菜单重置，
            // 在本次安全的帧边界完成切换，不让旧 dungeon 再执行 update。
            startOver();
            updateFade(2.01f);
            isStartingOver = false;
            dungeon = null;
            _dungeon = null;
            if (oldPlayer != null)
            {
                oldPlayer.onRelease();
                prefabPool.destroyObject(oldPlayer.gameObject, true);
            }
            player = null;
            if (cinemachineCamera != null) cinemachineCamera.Target = default;
            music?.changeBGM("MENU");
            RunExitPending = false;
            if (restart) mainMenuScreen.BeginNewRun();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            RunExitPending = false;
            // 不再驱动已经部分释放的旧房间。
            mode = GameMode.MAIN_MENU;
            dungeon = null;
            isStartingOver = false;
            LT.SHOW(out mainMenuScreen);
        }
        finally
        {
            if (mode == GameMode.MAIN_MENU)
            {
                stopClock = false;
                if (GameManager.Instance != null && GameManager.Instance.Paused) GameManager.Instance.Reset();
            }
            restartRequested = false;
            InputHelper.pressedEscape = false;
            InputHelper.justClickedLeft = false;
            InputHelper.justReleasedClickLeft = false;
        }
    }
}
