using UnityEngine;

namespace MarbleHero
{
    /// <summary>
    /// 结算阶段
    /// 当自动战斗结束后，会根据战斗结果对自身玩家和AI玩家扣除生命值
    /// 如果自身玩家的生命值扣除到0，则当前遭遇战通关失败，也就是爬塔失败
    /// 如果AI玩家的生命值扣除到0，则当前遭遇战通关成功，可以领取奖励并选择下一遭遇战
    /// </summary>
    public class SettlementPhase : APhase
    {
        public SettlementPhase(MonsterRoom room) : base(room)
        {
        }

        public override void onBegin(APhase last)
        {
            SettleWin();
        }

        public override void update(float dt)
        {
        }

        public override void fixedUpdate(float dt)
        {
        }

        public override void onEnd()
        {
        }

        protected override void onBindListeners()
        {
        }

        protected override void onUnbindListeners()
        {
        }

        void SettleWin()
        {
            Debug.Log($"Settle Fight Win Start");
            var monster = enemy;
            // actionManager.addToBot(new WaitAction(1F));
            // actionManager.addToBot(new DamageAction(monster, new DamageInfo(player, 10), AttackEffect.BLUNT_HEAVY));
            // actionManager.addToBot(new WaitAction(3F));
        }
    }
}