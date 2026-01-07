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

    public class StartPlayerTurnAction : AGameAction
    {
        ARoom room;
        public StartPlayerTurnAction(ARoom r) => room = r;

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
}