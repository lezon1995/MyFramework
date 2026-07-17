using System.Collections.Generic;

namespace MoreMountains
{
    public class MonsterGroup
    {
        public List<AMonster> monsters = new();

        public MonsterGroup(AMonster[] input)
        {
            monsters.AddRange(input);
        }

        public MonsterGroup()
        {
        }

        public void addMonster(int newIndex, AMonster m)
        {
            if (newIndex < 0)
                newIndex = 0;

            monsters.Insert(newIndex, m);
        }

        public void add(AMonster m) => monsters.Add(m);
        public void addMonster(AMonster m) => monsters.Add(m);
        public void addSpawnedMonster(AMonster m) => monsters.Insert(0, m);

        public void init()
        {
            foreach (var m in monsters)
                m.initMoves();
        }

        public void usePreBattleAction()
        {
            if (ADungeon.loading_post_combat)
                return;

            foreach (var m in monsters)
            {
                m.usePreBattleAction();
                m.useUniversalPreBattleAction();
            }
        }

        public bool anyAlive => !areMonstersBasicallyDead;

        public bool areMonstersDead
        {
            get
            {
                foreach (var m in monsters)
                {
                    if (m.isDead)
                        continue;

                    return false;
                }

                return true;
            }
        }

        public bool areMonstersBasicallyDead
        {
            get
            {
                foreach (var m in monsters)
                {
                    if (m.isDying)
                        continue;

                    return false;
                }

                return true;
            }
        }

        public void applyPreTurnLogic()
        {
            foreach (var m in monsters)
            {
                if (m.isDying)
                    continue;

                if (!m.hasPower("Barricade"))
                    m.block.loseBlock();

                m.applyStartOfTurnPowers();
            }
        }

        public AMonster getMonster(string id)
        {
            foreach (var m in monsters)
            {
                if (m.id == id)
                    return m;
            }

            log("MONSTER GROUP getMonster(): Could not find monster: " + id);
            return null;
        }

        public void update(float dt)
        {
            foreach (var m in monsters)
                m.OnUpdate(dt);
        }

        public void updateAnimations(float dt)
        {
            foreach (var m in monsters)
                m.updatePowers(dt);
        }

        public bool shouldFlipVfx()
        {
            return ADungeon.lastCombatMetricKey == "Shield and Spear" && monsters[1].isDying;
        }

        public void applyEndOfTurnPowers()
        {
            foreach (var m in monsters)
            {
                if (m.isDying)
                    continue;

                m.applyEndOfTurnTriggers();
            }

            foreach (var p in player.powers)
                p.atEndOfRound();

            foreach (var m in monsters)
            {
                if (m.isDying)
                    continue;

                foreach (var p in m.powers)
                    p.atEndOfRound();
            }
        }
    }
}