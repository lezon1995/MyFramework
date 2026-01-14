namespace MarbleHero
{
    public class RollMoveAction : AGameAction, IArgs<AMonster>
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
            actionManager.addToBot<DisplayMovesAction>().with(monster);
            isDone = true;
        }
    }
}