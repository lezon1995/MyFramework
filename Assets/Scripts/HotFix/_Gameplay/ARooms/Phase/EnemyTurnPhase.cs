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