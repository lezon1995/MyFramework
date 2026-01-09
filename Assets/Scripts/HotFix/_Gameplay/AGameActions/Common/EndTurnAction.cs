namespace MarbleHero
{
    public class EndTurnAction : AGameAction
    {
        public override void update(float dt)
        {
            actionManager.endTurn();
            // if (!room.skipMonsterTurn)
            // ADungeon.topLevelEffects.Add(new EnemyTurnEffect());
            isDone = true;
        }
    }

    public class PlayerStartTurnAction : AGameAction
    {
        public override void update(float dt)
        {
            effectManager.addToTop<PlayerTurnEffect>();
            // player.energy.recharge();
            isDone = true;
        }
    }

    public class StartPlayerTurnAction : AGameAction, IGameActionArgs<ARoom>
    {
        ARoom room;

        public void onCreate(ARoom r)
        {
            room = r;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            room = null;
        }

        public override void update(float dt)
        {
            room.startPlayerTurn();
            isDone = true;
        }
    }

    public class EnemyStartTurnAction : AGameAction
    {
        public override void update(float dt)
        {
            if (!room.skipMonsterTurn)
                effectManager.addToTop<EnemyTurnEffect>();
            isDone = true;
        }
    }
    
    
    public class EndPlayerTurnAction : AGameAction, IGameActionArgs<ARoom>
    {
        ARoom room;

        public void onCreate(ARoom r)
        {
            room = r;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            room = null;
        }

        public override void update(float dt)
        {
            actionManager.addToBot<EndTurnAction>();
            // addToBot(new WaitAction(END_TURN_WAIT_DURATION));
            // if (!room.skipMonsterTurn)
            //     addToBot(new MonsterStartTurnAction());
            // actionManager.monsterAttacksQueued = false;
            isDone = true;
        }
    }

    public class StartEnemyTurnAction : AGameAction, IGameActionArgs<ARoom>
    {
        const float END_TURN_WAIT_DURATION = 1.2F;
        ARoom room;

        public void onCreate(ARoom r)
        {
            room = r;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            room = null;
        }

        public override void update(float dt)
        {
            actionManager.addToBot<EnemyStartTurnAction>();
            actionManager.addToBot<WaitAction>().with(END_TURN_WAIT_DURATION);
            if (!room.skipMonsterTurn)
                actionManager.addToBot<MonsterStartTurnAction>();
            actionManager.monsterAttacksQueued = false;
            isDone = true;
        }
    }
}