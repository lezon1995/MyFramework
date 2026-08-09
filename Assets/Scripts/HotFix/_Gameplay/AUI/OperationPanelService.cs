using System;

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
    ///     然后调用 OperationPanelService.Register(panel, binder)。
    ///   • 任何想"按 APlayer 上下文打开商店面板"的地方都调
    ///     OperationPanelService.Instance.Open(player)。
    ///   • 关闭时调 Close(player)。
    /// </summary>
    public sealed class OperationPanelService
    {
        static OperationPanelService sInstance;

        public static OperationPanelService Instance => sInstance ??= new();

        OperationPanelBinder _binder;
        APlayer _boundPlayer;
        ARoom _room;

        public OperationPanelBinder Binder => _binder;
        public APlayer CurrentPlayer => _boundPlayer;

        public void Register(OperationPanelBinder binder)
        {
            _binder = binder;

            WaveBridge.OnShopPhaseFinished = () =>
            {
                _room.ToPhase = RoomPhaseType.BATTLE;
            };
        }

        public void Unregister()
        {
            _binder = null;

            WaveBridge.OnShopPhaseFinished = null;
        }

        /// <summary>绑定 APlayer 并打开面板。</summary>
        public void Open(ARoom room, APlayer player)
        {
            if (_binder == null)
            {
                logError("OperationPanelService.Open: panel / binder not registered.");
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
            logError("OperationPanelService.Open: no player bound; please call Open(APlayer) or Register then Bind.");
        }

        /// <summary>关闭面板并保留绑定（同一个玩家下一阶段直接 Open）。</summary>
        public void Close()
        {
            _binder?.Close();
        }

        /// <summary>完全解除绑定（玩家销毁时）。</summary>
        public void ReleasePlayer()
        {
            _binder?.Unbind();
            _boundPlayer = null;
        }
    }
}
