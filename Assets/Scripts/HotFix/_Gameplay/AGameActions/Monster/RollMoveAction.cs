namespace MarbleHero
{
    public class RollMoveAction : AGameAction, IGameActionArgs<AMonster>
    {
        AMonster monster;

        public void onCreate(AMonster m)
        {
            monster = m;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            monster = null;
        }

        public override void update(float dt)
        {
            monster.rollMove();
            isDone = true;
        }
    }
}