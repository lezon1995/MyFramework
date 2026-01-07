using Drawing;
using UnityEngine;

namespace MarbleHero
{
    public class EnemyTurnPhase : APhase
    {
        public EnemyTurnPhase(MonsterRoom room) : base(room)
        {
        }

        public override void onBegin(APhase last)
        {
            base.onBegin(last);
        }

        public override void onEnd()
        {
            base.onEnd();
        }

        public override void update(float dt)
        {
            Draw.xy.Label2D(new Vector2(Screen.width / 2F, Screen.height / 2F), "EnemyTurnPhase", 20, LabelAlignment.Center, Color.white);
        }

        public override void fixedUpdate(float dt)
        {
        }

        protected override void onBindListeners()
        {
        }

        protected override void onUnbindListeners()
        {
        }
    }
}