using System.Collections.Generic;

namespace MarbleHero
{
    public class MonsterGroup
    {
        public List<AMonster> monsters = new();
        public AMonster hoveredMonster;
        public AMonster main => monsters[0];

        public MonsterGroup(AMonster[] input)
        {
            monsters.AddRange(input);
        }

        public MonsterGroup(AMonster m) : this(new[] { m })
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

        public void showIntent()
        {
            foreach (var m in monsters)
                m.createIntent();
        }

        public void init()
        {
            foreach (var m in monsters)
                m.init();
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
                    if (m.isDead || m.escaped)
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
                    if (m.isDying || m.isEscaping)
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
                if (m.isDying || m.isEscaping)
                    continue;

                if (!m.hasPower("Barricade"))
                    m.loseBlock();

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

        public void queueMonsters()
        {
            foreach (var m in monsters)
            {
                if (m.isDeadOrEscaped() && !m.halfDead)
                    continue;

                actionManager.addMonsterQueueItem(new MonsterQueueItem(m));
            }
        }

        public bool haveMonstersEscaped()
        {
            foreach (var m in monsters)
            {
                if (m.escaped)
                    continue;

                return false;
            }

            return true;
        }

        public bool isMonsterEscaping()
        {
            foreach (var m in monsters)
                if (m.nextMove == 99)
                    return true;

            return false;
        }

        public bool hasMonsterEscaped()
        {
            foreach (var m in monsters)
                if (m.isEscaping)
                    return true;

            return _dungeon is TheCity;
        }

        public AMonster getRandomMonster() => getRandomMonster(null, false);

        public AMonster getRandomMonster(bool aliveOnly) => getRandomMonster(null, aliveOnly);

        public AMonster getRandomMonster(AMonster exception, bool aliveOnly, Rand rng)
        {
            if (areMonstersBasicallyDead)
                return null;

            if (exception == null)
            {
                if (aliveOnly)
                {
                    List<AMonster> arrayList = new();
                    foreach (var m in monsters)
                    {
                        if (!m.halfDead && !m.isDying && !m.isEscaping)
                            arrayList.Add(m);
                    }

                    if (arrayList.Count <= 0)
                        return null;

                    return arrayList[rng.random(0, arrayList.Count - 1)];
                }

                return monsters[rng.random(0, monsters.Count - 1)];
            }

            if (monsters.Count == 1)
                return monsters[0];

            if (aliveOnly)
            {
                List<AMonster> arrayList = new();
                foreach (var m in monsters)
                {
                    if (m.halfDead || m.isDying || m.isEscaping || exception == m)
                        continue;

                    arrayList.Add(m);
                }

                if (arrayList.Count == 0)
                    return null;

                return arrayList[rng.random(0, arrayList.Count - 1)];
            }

            List<AMonster> tmp = new();
            foreach (var m in monsters)
            {
                if (exception != m)
                    tmp.Add(m);
            }

            return tmp[rng.random(0, tmp.Count - 1)];
        }

        public AMonster getRandomMonster(AMonster exception, bool aliveOnly)
        {
            if (areMonstersBasicallyDead)
                return null;

            if (exception == null)
            {
                if (aliveOnly)
                {
                    List<AMonster> arrayList = new();
                    foreach (var m in monsters)
                    {
                        if (m.halfDead || m.isDying || m.isEscaping)
                            continue;

                        arrayList.Add(m);
                    }

                    if (arrayList.Count <= 0)
                        return null;

                    return arrayList[MathUtils.random(0, arrayList.Count - 1)];
                }

                return monsters[MathUtils.random(0, monsters.Count - 1)];
            }

            if (monsters.Count == 1)
                return monsters[0];

            if (aliveOnly)
            {
                List<AMonster> arrayList = new();
                foreach (var m in monsters)
                {
                    if (m.halfDead || m.isDying || m.isEscaping || exception == m)
                        continue;

                    arrayList.Add(m);
                }

                if (arrayList.Count == 0)
                    return null;

                return arrayList[MathUtils.random(0, arrayList.Count - 1)];
            }

            List<AMonster> tmp = new();
            foreach (var m in monsters)
            {
                if (exception != m)
                    tmp.Add(m);
            }

            return tmp[MathUtils.random(0, tmp.Count - 1)];
        }

        public void update(float dt)
        {
            foreach (var m in monsters)
                m.update(dt);

            // if (ADungeon.screen != ADungeon.CurrentScreen.DEATH)
            // {
            //     hoveredMonster = null;
            //     foreach (var m in monsters)
            //     {
            //         if (!m.isDying && !m.isEscaping)
            //         {
            //             m.hb.update();
            //             m.intentHb.update();
            //             m.healthHb.update();
            //             if ((m.hb.hovered || m.intentHb.hovered || m.healthHb.hovered) && !player.isDraggingCard)
            //             {
            //                 hoveredMonster = m;
            //                 break;
            //             }
            //         }
            //     }
            //
            //     if (hoveredMonster == null)
            //         player.hoverEnemyWaitTimer = -1.0F;
            // }
            // else
            // {
            //     hoveredMonster = null;
            // }
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

        public void escape()
        {
            foreach (var m in monsters)
                m.escape();
        }

        public void applyEndOfTurnPowers()
        {
            foreach (var m in monsters)
            {
                if (m.isDying || m.isEscaping)
                    continue;

                m.applyEndOfTurnTriggers();
            }

            foreach (var p in player.powers)
                p.atEndOfRound();

            foreach (var m in monsters)
            {
                if (m.isDying || m.isEscaping)
                    continue;

                foreach (var p in m.powers)
                    p.atEndOfRound();
            }
        }

        // public void render(SpriteBatch sb)
        // {
        //     if (hoveredMonster is { isDead: false, escaped: false } && player.hoverEnemyWaitTimer < 0.0F)
        //     {
        //         if (!ADungeon.isScreenUp || PeekButton.isPeeking)
        //             hoveredMonster.renderTip(sb);
        //     }
        //
        //     foreach (var m in monsters)
        //         m.render(sb);
        // }

        // public void renderReticle(SpriteBatch sb)
        // {
        //     foreach (var m in monsters)
        //     {
        //         if (!m.isDying && !m.isEscaping)
        //             m.renderReticle(sb);
        //     }
        // }

        public List<string> getMonsterNames()
        {
            List<string> arr = new();
            foreach (var m in monsters)
                arr.Add(m.id);
            return arr;
        }
    }
}