using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 跨场景的 OperationPanel 持有者。
    ///
    /// 为什么需要这个？
    ///   • UI 实例由 UI 系统(GameHotFix / Game 等)创建,通常不在 APlayer 派生树上。
    ///   • ShoppingPhase 这种"在战斗外的系统级阶段"需要主动打开面板，
    ///     但它们拿不到 UI 实例的引用；用这个静态 service 当单入口。
    ///
    /// 使用：
    ///   • 在 GameHotFix / Game 启动时,UI 系统实例化一份 OperationPanel + Binder,
    ///     然后调用 EscPanelService.Register(panel, binder)。
    ///   • 任何想"按 APlayer 上下文打开商店面板"的地方都调
    ///     EscPanelService.Instance.Open(player)。
    ///   • 关闭时调 Close(player)。
    /// </summary>
    public sealed class EscPanelService
    {
        static EscPanelService sInstance;

        public static EscPanelService Instance => sInstance ??= new();

        EscPanelBinder _binder;
        APlayer _boundPlayer;
        ARoom _room;
        bool _opened;
        bool ownsPause;
        bool previousStopClock;
        int lastToggleFrame = -1;

        public EscPanelBinder Binder => _binder;
        public APlayer CurrentPlayer => _boundPlayer;
        public bool Opened => _opened;
        public bool HasChildPage => _binder?.Panel.HasChildPage == true;

        public void Register(EscPanelBinder binder)
        {
            _binder = binder;
        }

        public void Unregister()
        {
            Close();
            ReleasePlayer();
            _binder = null;
        }

        /// <summary>绑定 APlayer 并打开面板。</summary>
        public void Open(ARoom room, APlayer player)
        {
            if (_opened || Game.RunExitPending || player == null) return;
            if (_binder == null)
            {
                logError("EscPanelService.Open: panel / binder not registered.");
                return;
            }

            // 重绑（如果切换了玩家）
            if (!ReferenceEquals(player, _boundPlayer))
            {
                _binder.Unbind();
                _boundPlayer = player;
                _binder.Bind(player);
            }

            _room =  room;
            _opened = true;
            lastToggleFrame = Time.frameCount;
            previousStopClock = Game.stopClock;
            Game.stopClock = true;
            ownsPause = GameManager.Instance != null && !GameManager.Instance.Paused;
            if (ownsPause) GameManager.Instance.Pause(unpauseIfPaused: false);
            _binder.Open();
        }

        /// <summary>打开面板，使用最近一次绑定过的玩家；不存在则报错。</summary>
        public void Open()
        {
            if (_boundPlayer)
            {
                Open(_room, _boundPlayer);
                return;
            }
            logError("EscPanelService.Open: no player bound; please call Open(APlayer) or Register then Bind.");
        }

        /// <summary>关闭面板并保留绑定（同一个玩家下一阶段直接 Open）。</summary>
        public void Close()
        {
            if (!_opened) return;
            lastToggleFrame = Time.frameCount;
            _binder?.Close();
            _opened = false;
            Game.stopClock = previousStopClock;
            if (ownsPause && GameManager.Instance != null) GameManager.Instance.UnPause();
            ownsPause = false;
        }

        public void HandleRoomInput(ARoom room, APlayer player)
        {
            if (Game.RunExitPending || HasChildPage || MainMenuPage.LastBackFrame == Time.frameCount ||
                lastToggleFrame == Time.frameCount) return;
            bool back = InputHelper.pressedEscape || Input.GetKeyDown(KeyCode.JoystickButton1);
            if (!back) return;
            if (_opened) Close();
            else if (!ADungeon.isScreenUp) Open(room, player);
        }

        /// <summary>完全解除绑定（玩家销毁时）。</summary>
        public void ReleasePlayer()
        {
            Close();
            _binder?.Unbind();
            _boundPlayer = null;
            _room = null;
        }
    }
}
