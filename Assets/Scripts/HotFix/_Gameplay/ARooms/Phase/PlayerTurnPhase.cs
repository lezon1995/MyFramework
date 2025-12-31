namespace MarbleHero
{
    public class PlayerTurnPhase : APhase
    {
        public PlayerTurnPhase(MonsterRoom room) : base(room)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
        }


        protected override void onBindListeners()
        {
        }

        protected override void onUnbindListeners()
        {
        }

        public override void onBegin(APhase last)
        {
            base.onBegin(last);
        }


        public override void fixedUpdate(float dt)
        {
        }

        public override void onEnd()
        {
            base.onEnd();
        }

        public override void update(float dt)
        {
        }
    }
}