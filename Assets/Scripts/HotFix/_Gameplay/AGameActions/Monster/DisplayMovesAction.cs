/*using System.Collections.Generic;

namespace MoreMountains
{
    public class DisplayMovesAction : AGameAction, IArgs<AMonster>
    {
        const float GAP = 0.25F;
        Queue<EnemyMoveInfo> queue = new();
        AMonster monster;
        bool lastOne;

        public void onCreate(AMonster m)
        {
            monster = m;
            duration = GAP;
            queue.Clear();
            foreach (var info in m.moveInfoGroup.moveInfos)
                queue.Enqueue(info);
            
            ADungeon.overlayMenu.intents.clearIntentItems();
        }

        public override void resetProperty()
        {
            base.resetProperty();
            monster = null;
            lastOne = false;
            queue.Clear();
        }

        public override void update(float dt)
        {
            if (duration.unstarted)
            {
                if (queue.TryDequeue(out var info))
                {
                    var remain = queue.Count;
                    ADungeon.overlayMenu.intents.addIntent(info);
                    if (remain == 0)
                    {
                        lastOne = true;
                        isDone = true;
                    }
                }
                else
                {
                    lastOne = true;
                    isDone = true;
                }
            }

            tickDuration(dt);
            if (isDone && !lastOne)
            {
                duration.reset();
                isDone = false;
            }
        }
    }
}*/