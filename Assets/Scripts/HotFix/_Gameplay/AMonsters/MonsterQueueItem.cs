namespace MarbleHero
{
    public struct MonsterQueueItem
    {
        public AMonster monster;
        public EnemyMoveInfo moveInfo;

        public MonsterQueueItem(AMonster m, EnemyMoveInfo info)
        {
            monster = m;
            moveInfo = info;
        }
    }
}