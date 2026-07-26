namespace MoreMountains
{
    /// <summary>
    /// 球的购买 / 售出入口（不画 UI、不动金币数字，只调对应系统）。
    /// 调用方约定：
    ///   购买：先确保 InventorySystem.CanAddBall() == true，并且 PlayerWallet.CanPay(price) == true
    ///   售出：调 SellToShop 即可，内部会负责"找 holder + 移除 + 加金币"
    /// </summary>
    public sealed class BallShopService
    {
        BallManagementSystem _owner;

        public BallShopService(BallManagementSystem owner)
        {
            _owner = owner;
        }

        /// <summary>商店流程（BuyBallAction）专用：在玩家已付金币、并通过满格校验后被调用。</summary>
        public BallInstance PurchaseAndStore(BallDef def)
        {
            // 双层校验：UI Action 已校验过，但调用方也可能是直调，所以兜底。
            if (!_owner.Player.Inventory.CanAddBall())
                return null;

            if (def == null) 
                return null;

            var ball = BallInstance.CreateNew(def, level: 1);
            if (!_owner.Player.Inventory.AddBall(ball))
                return null;

            BallEvents.RaiseCreated(ball);
            BallEvents.RaisePurchased(ball);
            return ball;
        }

        /// <summary>售出：自动找 holder、移除、并用半价加金币。</summary>
        public int SellToShop(BallInstance ball)
        {
            if (ball == null) 
                return 0;
            
            if (!InventoryLocate.FindHolderOf(ball, out var holder)) 
                return 0;

            holder.TryRemoveByInstance(ball);

            int refund = ball.SellPrice > 0 ? ball.SellPrice : 1;
            _owner.Player.gainGold(refund, EarnType.SELL_BALL);

            BallEvents.RaiseDestroyed(ball);
            BallEvents.RaiseSold(ball, refund);
            return refund;
        }
    }
}