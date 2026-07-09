using MoreMountains.Tools;

namespace MarbleHero
{
    public class MonsterRoomElite : MonsterRoom
    {
        public override RoomType Type => RoomType.ELITE;

        public override void applyEmeraldEliteBuff()
        {
            if (Settings.isFinalActAvailable && mapNode.hasEmeraldKey)
            {
                /*switch (ADungeon.mapRng.random(0, 3))
                {
                    case 0:
                        foreach (var m in monsters.monsters)
                            actionManager.addToBot(new ApplyPowerAction(m, m, new StrengthPower(m, ADungeon.actNum + 1), ADungeon.actNum + 1));
                        break;
                    case 1:
                        foreach (var m in monsters.monsters)
                            actionManager.addToBot(new IncreaseMaxHpAction(m, 0.25F, true));
                        break;
                    case 2:
                        foreach (var m in monsters.monsters)
                            actionManager.addToBot(new ApplyPowerAction(m, m, new MetallicizePower(m, ADungeon.actNum * 2 + 2), ADungeon.actNum * 2 + 2));
                        break;
                    case 3:
                        foreach (var m in monsters.monsters)
                            actionManager.addToBot(new ApplyPowerAction(m, m, new RegenerateMonsterPower(m, 1 + ADungeon.actNum * 2), 1 + ADungeon.actNum * 2));
                        break;
                }*/
            }
        }

        public override void onPlayerEntry()
        {
            playBGM(null);
            if (monsters == null)
            {
                monsters = _dungeon.getEliteMonsterForRoomCreation();
                monsters.init();
            }

            waitTimer = COMBAT_WAIT_TIME;
            new OnPlayerEnterBattleRoom().trigger();
        }

        public override void dropReward()
        {
            RelicTier tier = returnRandomRelicTier();
            // if (Settings.isEndless && player.hasBlight("MimicInfestation"))
            // {
            //     player.getBlight("MimicInfestation").flash();
            // }
            // else
            {
                addRelicToRewards(tier);
                if (player.hasRelic("Black Star"))
                    addNoncampRelicToRewards(returnRandomRelicTier());

                addEmeraldKey();
            }
        }

        private void addEmeraldKey()
        {
            // if (Settings.isFinalActAvailable && !Settings.hasEmeraldKey && rewards.Count > 0 && mapNode.hasEmeraldKey)
                // rewards.Add(new RewardItem(rewards[^1], RewardType.EMERALD_KEY));
        }

        private RelicTier returnRandomRelicTier()
        {
            int roll = ADungeon.relicRng.random(0, 99);
            if (ModHelper.isModEnabled("Elite Swarm"))
                roll += 10;

            return roll switch
            {
                < 50 => RelicTier.COMMON,
                > 82 => RelicTier.RARE,
                _ => RelicTier.UNCOMMON
            };
        }

        public override CardRarity getCardRarity(int roll)
        {
            if (ModHelper.isModEnabled("Elite Swarm"))
                return CardRarity.Rare;

            return base.getCardRarity(roll);
        }

        protected override void onPlayerCompletedRewardGold()
        {
            if (ADungeon.loading_post_combat)
                return;

            int slain = _dungeon switch
            {
                Exordium => ++Game.elites1Slain,
                TheCity => ++Game.elites2Slain,
                TheBeyond => ++Game.elites3Slain,
                _ => ++Game.elitesModdedSlain
            };

            log("Elites Slain " + slain);

            if (!Game.loadingSave)
            {
                int gold;
                if (Settings.isDailyRun)
                    gold = 30;
                else
                    gold = ADungeon.treasureRng.random(25, 35);

                addGoldToRewards(gold);
            }
        }

        protected override int onPlayerCompletedGetPotionChance()
        {
            return 40 + blizzardPotionMod;
        }
    }
}