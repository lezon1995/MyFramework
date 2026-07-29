using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 一站式接入指南。
    ///
    /// 背景：四个核心系统（BallManagementSystem / InventorySystem / ShopSystem / PlayerWallet）
    /// 都是 PlayerAbility（即 MonoBehaviour）。它们会在 APlayer.Initialization 中通过
    ///     getOrAddUnityComponent<T>()
    /// 自动加在玩家 GameObject 上（参考 APlayer.Balls.cs 现有代码）。
    ///
    /// 不需要额外的"RegisterAll"调用。但是为了让设计师/策划数据被注册进来，
    /// 仍然需要一个"启动时一次性跑一次"的静态 helper。下面是接入模板：
    ///
    /// ------------------------------------------------------------------------
    /// 在 GameHotFix.initFrameSystem 或 Game.Awake 中：
    ///
    ///     protected override void onGameStateInit() {
    ///         base.onGameStateInit();
    ///
    ///         // 1. 注册 BallDefLibrary —— 你项目里所有策划填的 BallDef SO：
    ///         var allDef = Resources.LoadAll<BallDef>("");
    ///         BallDefLibrary.Instance.RegisterAll(allDef);
    ///
    ///         // 2. 同理 RelicDef（如果用了）
    ///         // RelicDefLibrary.Instance.RegisterAll(Resources.LoadAll<RelicDef>(""));
    ///     }
    ///
    /// ------------------------------------------------------------------------
    /// 系统组件则不需要静态注册 —— APlayer 在它 Initialization() 里把它们挂上。
    ///
    /// ------------------------------------------------------------------------
    /// OperationPanel + Binder 的接入（在你的 UI 系统启动时）：
    ///
    ///     var panel = new OperationPanel();
    ///     panel.assignWindow();
    ///     panel.init();
    ///
    ///     var slotBinder = new BallSlotGroupBinder(panel.PlayerInfo.SlotGroup);
    ///     var ballBinder = new BallInventoryBinder(panel.BallInventory);
    ///     var relicBinder = new RelicInventoryBinder(panel.RelicInventory);
    ///     var shopBinder = new ShopBinder(panel.Shop);
    ///     var infoBinder = new PlayerInfoBinder(panel.PlayerInfo, slotBinder);
    ///
    ///     var opBinder = new OperationPanelBinder(
    ///         panel, ballBinder, relicBinder, slotBinder, shopBinder, infoBinder);
    ///
    ///     OperationPanelService.Instance.Register(panel, opBinder);
    ///
    ///     // 玩家准备好后(比如主菜单选完角色):
    ///     OperationPanelService.Instance.Bind(player);
    ///
    /// 之后 ShoppingPhase.onBegin/onEnd 会自动调 Open/Close。
    /// ------------------------------------------------------------------------
    /// </summary>
    public static class ShopBallInventoryBootstrap
    {
        public static void RegisterAll() { /* 见注释接入指南 */ }
    }
}
