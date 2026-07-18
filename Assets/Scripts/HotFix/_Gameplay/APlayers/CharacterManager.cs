using System.Collections.Generic;
using System.Linq;

namespace MoreMountains
{
    public class PlayerInfo
    {
        public Prefs prefs;
        public CharStat stat;
    }

    public class CharacterManager
    {
        static Dictionary<APlayer.PlayerClass, PlayerInfo> masterCharacterList = new();

        public CharacterManager()
        {
            if (masterCharacterList.Count == 0)
            {
                masterCharacterList.Add(APlayer.PlayerClass.IRONCLAD, new PlayerInfo());
                // masterCharacterList.Add(new TheSilent(Game.playerName));
                // masterCharacterList.Add(new Defect(Game.playerName));
                // masterCharacterList.Add(new Watcher(Game.playerName));
            }
            else
            {
                foreach (var (k, v) in masterCharacterList)
                {
                    v.prefs = SaveHelper.getPrefs("DataVagabond");
                }
            }
        }


        public bool anySaveFileExists()
        {
            foreach (var (k, v) in masterCharacterList)
            {
                if (SaveAndContinue.saveExistsAndNotCorrupted(k.ToString()))
                    return true;
            }

            return false;
        }

        public APlayer.PlayerClass loadChosenCharacter()
        {
            foreach (var (k, v) in masterCharacterList)
            {
                if (SaveAndContinue.saveExistsAndNotCorrupted(k.ToString()))
                {
                    return k;
                }
            }

            log("No character save file was found!");
            return APlayer.PlayerClass.IRONCLAD;
        }

        public List<CharStat> getAllCharacterStats()
        {
            List<CharStat> allCharStats = new();
            foreach (var  (k, v)  in masterCharacterList)
                allCharStats.Add(v.stat);
            return allCharStats;
        }

        public void refreshAllCharStats()
        {
            foreach (var (k, v) in masterCharacterList)
                v.stat = new CharStat((APlayer)null);
        }

        public List<Prefs> getAllPrefs()
        {
            List<Prefs> allPrefs = new();
            foreach (var  (k, v)  in masterCharacterList)
                allPrefs.Add(v.prefs);
            return allPrefs;
        }

        public APlayer.PlayerClass getRandomCharacter(Rand rng)
        {
            var playerClasses = masterCharacterList.Keys.ToArray();
            int index = rng.random(playerClasses.Length - 1);
            return playerClasses[index];
        }

        public APlayer recreateCharacter(APlayer.PlayerClass p)
        {
            foreach (var (k,v) in masterCharacterList)
            {
                if (k == p)
                {
                    var path = $"{GAMEPLAY_PATH}/Characters/MyCharacter.prefab";
                    var o = prefabPool.createObject(path);
                    o.TryGetComponent(out APlayer newPlayer);
                    newPlayer.setName($"PlayerCharacter");
                    log("Successfully recreated " + newPlayer.chosenClass);
                    newPlayer.onAcquire();
                    return newPlayer;
                }
            }

            return null;
        }
    }
}