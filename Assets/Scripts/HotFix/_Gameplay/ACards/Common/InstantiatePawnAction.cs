using System.Collections.Generic;

namespace MarbleHero
{
    public struct OnSpawnPawn
    {
        public APawn pawn;
        public OnSpawnPawn(APawn p) => pawn = p;
    }

    public class InstantiatePawnAction : AGameAction
    {
        static float DURATION = 0.2F;
        ACreature creature;
        PawnInfo info;

        InstantiatePawnAction(ACreature c, PawnInfo p)
        {
            duration = DURATION;
            creature = c;
            info = p;
        }

        public InstantiatePawnAction(AMonster c, PawnInfo p) : this((ACreature)c, p)
        {
        }

        public InstantiatePawnAction(APlayer c, PawnInfo p) : this((ACreature)c, p)
        {
        }

        public override void update(float dt)
        {
            tickDuration(dt);
            if (isDone)
            {
                var pawn = APawn.New(creature, info.data, info.slot, info.level);
                creature.pawns.Add(pawn);
                new OnSpawnPawn(pawn).Trigger();
            }
        }
    }

    public class InstantiatePawnsAction : AGameAction
    {
        static float GAP = 0.2F;
        ACreature creature;
        Queue<PawnInfo> pawnInfos = new();

        InstantiatePawnsAction(ACreature c, PawnInfo[] infos)
        {
            duration = GAP;
            creature = c;
            foreach (var info in infos)
            {
                pawnInfos.Enqueue(info);
            }
        }

        public InstantiatePawnsAction(AMonster m, PawnInfo[] infos) : this((ACreature)m, infos)
        {
        }

        public InstantiatePawnsAction(APlayer p, PawnInfo[] infos) : this((ACreature)p, infos)
        {
        }

        public override void update(float dt)
        {
            tickDuration(dt);
            if (isDone && pawnInfos.TryDequeue(out var info))
            {
                duration = GAP;
                isDone = false;
                var pawn = APawn.New(creature, info.data, info.slot, info.level);
                creature.pawns.Add(pawn);
                new OnSpawnPawn(pawn).Trigger();
            }
        }
    }
}