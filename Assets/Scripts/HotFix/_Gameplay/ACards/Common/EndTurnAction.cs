namespace MarbleHero
{
    public class EndTurnAction : AGameAction
    {
        public override void update(float dt)
        {
            actionManager.endTurn();
            if (!room.skipMonsterTurn)
                ADungeon.topLevelEffects.Add(new EnemyTurnEffect());
            isDone = true;
        }
    }

    public class PlayerStartTurnAction : AGameAction
    {
        public override void update(float dt)
        {
            ADungeon.topLevelEffects.Add(new PlayerTurnEffect());
            isDone = true;
        }
    }
}