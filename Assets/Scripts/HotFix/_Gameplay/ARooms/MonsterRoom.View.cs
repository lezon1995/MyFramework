using System.Collections.Generic;

namespace MoreMountains
{
    public partial class MonsterRoom
    {
        public APhase curPhase;
        protected Dictionary<RoomPhaseType, APhase> _phases = new();

        void nextPhase(RoomPhaseType type)
        {
            RoomPhaseType = type;
            if (type == RoomPhaseType.PLAYER_TURN)
            {
            }

            curPhase?.onEnd();
            var last = curPhase;
            curPhase = _phases[type];
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