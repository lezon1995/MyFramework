namespace MarbleHero
{
    public class EndTurnAction : AGameAction
    {
        public override void update(float dt)
        {
            actionManager.endTurn();
            if (!room.skipMonsterTurn)
                ADungeon.topLevelEffects.Add(CLASS<EnemyTurnEffect>());
            isDone = true;
        }
    }

    public class PlayerStartTurnAction : AGameAction
    {
        public override void update(float dt)
        {
            ADungeon.topLevelEffects.Add(CLASS<PlayerTurnEffect>());
            isDone = true;
        }
    }
}