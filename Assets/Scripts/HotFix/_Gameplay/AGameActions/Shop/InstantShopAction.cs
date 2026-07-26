using System;

namespace MoreMountains
{
    /// <summary>
    /// 公用基类 —— 一次性执行一个动作。
    /// 不会用 update 做帧定时，直接在 nextFrame 里 execute，保留 AGameAction 的扩展能力。
    /// </summary>
    public abstract class InstantShopAction : AGameAction
    {
        public override void update(float dt)
        {
            try { Execute(); }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ShopAction] {GetType().Name}: {ex}");
            }
            isDone = true;
        }

        protected abstract void Execute();
    }
}
