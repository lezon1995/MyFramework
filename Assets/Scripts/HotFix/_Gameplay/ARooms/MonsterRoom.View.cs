namespace MarbleHero
{
    public partial class MonsterRoom
    {
        protected enum PhaseType
        {
            PlayerTurn,
            EnemyTurn,
            Fighting,
            Settlement,
        }

        public APhase curPhase;

        protected APhase[] _phases;

        protected int myCount;
        protected int opCount;

        void nextPhase(PhaseType type)
        {
            if (type == PhaseType.PlayerTurn)
            {
                myCount = 0;
                opCount = 0;
            }

            curPhase?.onEnd();
            var last = curPhase;
            curPhase = _phases[(int)type];
            curPhase.onBegin(last);
        }

        protected override void onCombatUpdate(float dt)
        {
            curPhase?.update(dt);
        }

        protected override void onCombatFixedUpdate(float dt)
        {
            curPhase?.fixedUpdate(dt);
        }
    }
}