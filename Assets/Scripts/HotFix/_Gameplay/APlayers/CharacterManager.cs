using System.Collections.Generic;

namespace MarbleHero
{
    public class CharacterManager
    {
        static List<APlayer> masterCharacterList = new();

        public CharacterManager()
        {
            if (masterCharacterList.Count == 0)
            {
                masterCharacterList.Add(new Ironclad(Game.playerName));
                // masterCharacterList.Add(new TheSilent(Game.playerName));
                // masterCharacterList.Add(new Defect(Game.playerName));
                // masterCharacterList.Add(new Watcher(Game.playerName));
            }
            else
            {
                foreach (APlayer c in masterCharacterList)
                    c.loadPrefs();
            }
        }

        public APlayer setChosenCharacter(APlayer.PlayerClass c)
        {
            foreach (APlayer character in masterCharacterList)
            {
                if (character.chosenClass == c)
                {
                    player = character;
                    return character;
                }
            }

            logError("The character " + c + " does not exist in the CharacterManager's master character list");
            return null;
        }

        public bool anySaveFileExists()
        {
            foreach (APlayer character in masterCharacterList)
            {
                if (character.saveFileExists())
                    return true;
            }

            return false;
        }

        public APlayer loadChosenCharacter()
        {
            foreach (APlayer character in masterCharacterList)
            {
                if (character.saveFileExists())
                {
                    player = character;
                    return character;
                }
            }

            log("No character save file was found!");
            return null;
        }

        public List<CharStat> getAllCharacterStats()
        {
            List<CharStat> allCharStats = new();
            foreach (APlayer c in masterCharacterList)
                allCharStats.Add(c.getCharStat());
            return allCharStats;
        }

        public void refreshAllCharStats()
        {
            foreach (APlayer c in masterCharacterList)
                c.refreshCharStat();
        }

        public List<Prefs> getAllPrefs()
        {
            List<Prefs> allPrefs = new();
            foreach (APlayer c in masterCharacterList)
                allPrefs.Add(c.getPrefs());
            return allPrefs;
        }

        public APlayer getRandomCharacter(Rand rng)
        {
            int index = rng.random(masterCharacterList.Count - 1);
            return masterCharacterList[index];
        }

        public APlayer recreateCharacter(APlayer.PlayerClass p)
        {
            foreach (APlayer old in masterCharacterList)
            {
                if (old.chosenClass == p)
                {
                    APlayer newChar = old.newInstance();
                    masterCharacterList[masterCharacterList.IndexOf(old)] = newChar;
                    old.dispose();
                    log("Successfully recreated " + newChar.chosenClass);
                    return newChar;
                }
            }

            return null;
        }

        public APlayer getCharacter(APlayer.PlayerClass c)
        {
            foreach (APlayer character in masterCharacterList)
            {
                if (character.chosenClass == c)
                    return character;
            }

            logError("The character " + c + " does not exist in the CharacterManager's master character list");
            return null;
        }

        public List<APlayer> getAllCharacters()
        {
            return masterCharacterList;
        }
    }
}