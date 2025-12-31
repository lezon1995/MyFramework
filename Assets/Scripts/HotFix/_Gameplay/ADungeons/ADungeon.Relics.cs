using System;
using System.Collections.Generic;
using MoreMountains.AutoBattleEngine.Gameplay.Helpers;
using MoreMountains.AutoBattleEngine.Gameplay.Relics;
using MoreMountains.Tools;

namespace MarbleHero
{
    public partial class ADungeon
    {
        public static List<string> commonRelicPool = new();
        public static List<string> uncommonRelicPool = new();
        public static List<string> rareRelicPool = new();
        public static List<string> shopRelicPool = new();
        public static List<string> bossRelicPool = new();

        public static List<string> relicsToRemoveOnStart = new();

        protected void initializeRelicList()
        {
            commonRelicPool.Clear();
            uncommonRelicPool.Clear();
            rareRelicPool.Clear();
            shopRelicPool.Clear();
            bossRelicPool.Clear();
            
            RelicLibrary.populateRelicPool(ref commonRelicPool, RelicTier.COMMON, player.chosenClass);
            RelicLibrary.populateRelicPool(ref uncommonRelicPool, RelicTier.UNCOMMON, player.chosenClass);
            RelicLibrary.populateRelicPool(ref rareRelicPool, RelicTier.RARE, player.chosenClass);
            RelicLibrary.populateRelicPool(ref shopRelicPool, RelicTier.SHOP, player.chosenClass);
            RelicLibrary.populateRelicPool(ref bossRelicPool, RelicTier.BOSS, player.chosenClass);

            if (floorNum >= 1)
            {
                foreach (var r in player.relics)
                    relicsToRemoveOnStart.Add(r.relicId);
            }

            commonRelicPool.Shuffle(new Random(relicRng.randomInt()));
            uncommonRelicPool.Shuffle(new Random(relicRng.randomInt()));
            rareRelicPool.Shuffle(new Random(relicRng.randomInt()));
            shopRelicPool.Shuffle(new Random(relicRng.randomInt()));
            bossRelicPool.Shuffle(new Random(relicRng.randomInt()));

            if (ModHelper.isModEnabled("Flight") || ModHelper.isModEnabled("Uncertain Future"))
                relicsToRemoveOnStart.Add("WingedGreaves");

            if (ModHelper.isModEnabled("Diverse"))
                relicsToRemoveOnStart.Add("PrismaticShard");

            if (ModHelper.isModEnabled("DeadlyEvents"))
                relicsToRemoveOnStart.Add("Juzu Bracelet");

            if (ModHelper.isModEnabled("Hoarder"))
                relicsToRemoveOnStart.Add("Smiling Mask");

            if (ModHelper.isModEnabled("Draft") || ModHelper.isModEnabled("SealedDeck") || ModHelper.isModEnabled("Shiny") || ModHelper.isModEnabled("Insanity"))
                relicsToRemoveOnStart.Add("Pandora's Box");

            foreach (string remove in relicsToRemoveOnStart)
            {
                for (var i = commonRelicPool.Count - 1; i >= 0; i--)
                {
                    var relicId = commonRelicPool[i];
                    if (relicId == remove)
                    {
                        commonRelicPool.RemoveAt(i);
                        log(relicId + " removed.");
                    }
                }

                for (var i = uncommonRelicPool.Count - 1; i >= 0; i--)
                {
                    var relicId = uncommonRelicPool[i];
                    if (relicId == remove)
                    {
                        uncommonRelicPool.RemoveAt(i);
                        log(relicId + " removed.");
                    }
                }

                for (var i = rareRelicPool.Count - 1; i >= 0; i--)
                {
                    var relicId = rareRelicPool[i];
                    if (relicId == remove)
                    {
                        rareRelicPool.RemoveAt(i);
                        log(relicId + " removed.");
                    }
                }

                for (var i = bossRelicPool.Count - 1; i >= 0; i--)
                {
                    var relicId = bossRelicPool[i];
                    if (relicId == remove)
                    {
                        bossRelicPool.RemoveAt(i);
                        log(relicId + " removed.");
                    }
                }

                for (var i = shopRelicPool.Count - 1; i >= 0; i--)
                {
                    var relicId = shopRelicPool[i];
                    if (relicId == remove)
                    {
                        shopRelicPool.RemoveAt(i);
                        log(relicId + " removed.");
                    }
                }
            }

            if (Settings.isDebug)
            {
                log("Relic (Common):");
                foreach (string s in commonRelicPool)
                    log(" " + s);

                log("Relic (Uncommon):");
                foreach (string s in uncommonRelicPool)
                    log(" " + s);

                log("Relic (Rare):");
                foreach (string s in rareRelicPool)
                    log(" " + s);

                log("Relic (Shop):");
                foreach (string s in shopRelicPool)
                    log(" " + s);

                log("Relic (Boss):");
                foreach (string s in bossRelicPool)
                    log(" " + s);
            }
        }
    }
}