namespace MarbleHero
{
    public class RollMoveAction : AGameAction
    {
        AMonster monster;

        public RollMoveAction(AMonster m)
        {
            monster = m;
        }

        public override void update(float dt)
        {
            monster.rollMove();
            isDone = true;
        }
    }
}