using MoreMountains.Tools;

namespace MoreMountains
{
    public class MonsterRoomBoss : MonsterRoom
    {
        public override RoomType Type => RoomType.BOSS;

        public MonsterRoomBoss()
        {
        }

        public override void onPlayerEntry()
        {
            monsters = _dungeon.getBoss();
            log("BOSSES: " + ADungeon.bossList.Count);
            metricData.path_taken.Add("BOSS");
            // music.silenceBGM();
            ADungeon.bossList.RemoveAt(0);
            monsters?.init();
            waitTimer = COMBAT_WAIT_TIME;
            new OnPlayerEnterBattleRoom().trigger();
        }

        public override CardRarity getCardRarity(int roll)
        {
            return CardRarity.Rare;
        }

        protected override void onPlayerCompletedRewardGold()
        {
            if (ADungeon.loading_post_combat) 
                return;

            if (!Game.loadingSave)
            {
                int gold;
                if (Settings.isDailyRun)
                {
                    gold = 100;
                }
                else
                {
                    int tmp = 100 + ADungeon.miscRng.random(-5, 5);
                    if (ADungeon.ascensionLevel >= 13)
                        gold = MathUtils.round(tmp * 0.75F);
                    else
                        gold = tmp;
                }

                addGoldToRewards(gold);
            }

            // if (ModHelper.isModEnabled("Cursed Run"))
            // ADungeon.effectList.Add(new ShowCardAndObtainEffect(ADungeon.returnRandomCurse(), Settings.WIDTH / 2.0F, Settings.HEIGHT / 2.0F));
        }
    }
}