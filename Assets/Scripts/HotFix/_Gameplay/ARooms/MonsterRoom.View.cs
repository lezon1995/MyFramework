using System.Collections.Generic;

namespace MoreMountains
{
    public partial class MonsterRoom
    {
        public ARoomPhase curPhase;
        protected Dictionary<RoomPhaseType, ARoomPhase> _phases = new();

        protected void nextPhase(RoomPhaseType type)
        {
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