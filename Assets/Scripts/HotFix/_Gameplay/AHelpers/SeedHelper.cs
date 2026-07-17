using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace MoreMountains
{
    public class SeedHelper
    {
        static Regex regex = new("([A-Z]*[0-9]*)*");

        const string CHARACTERS = "0123456789ABCDEFGHIJKLMNPQRSTUVWXYZ";
        public static string cachedSeed;
        public static int SEED_DEFAULT_LENGTH = getString(long.MinValue).Length;

        public static void setSeed(string seedStr)
        {
            if (string.IsNullOrEmpty(seedStr))
            {
                Settings.seedSet = false;
                Settings.seed = 0;
                Settings.specialSeed = 0;
            }
            else
            {
                long seed = getLong(seedStr);
                Settings.seedSet = true;
                Settings.seed = seed;
                Settings.specialSeed = 0;
                Settings.isDailyRun = false;
                cachedSeed = null;
            }
        }

        public static string getUserFacingSeedString()
        {
            if (Settings.seed != 0)
                return cachedSeed ??= getString(Settings.seed);

            return "";
        }

        public static string getValidCharacter(string character, string textSoFar)
        {
            character = character.ToUpper();
            if (character == "O")
                character = "0";
            if (CHARACTERS.Contains(character))
                return character;
            return null;
        }

        public static string sterilizeString(string raw)
        {
            raw = raw.Trim().ToUpper();
            if (regex.IsMatch(raw))
                return raw.Replace("O", "0");
            return "";
        }

        public static string getString(long seed)
        {
            StringBuilder sb = new StringBuilder();
            BigInteger leftover = new BigInteger((ulong)seed);
            BigInteger charCount = CHARACTERS.Length;
            while (leftover != BigInteger.Zero)
            {
                BigInteger remainder = leftover % charCount;
                leftover /= charCount;
                int charIndex = (int)remainder;
                char c = CHARACTERS[charIndex];
                sb.Insert(0, c);
            }

            return sb.ToString();
        }

        public static long getLong(string seedStr)
        {
            long total = 0L;
            seedStr = seedStr.ToUpper().Replace("O", "0");
            for (int i = 0; i < seedStr.Length; i++)
            {
                char toFind = seedStr[i];
                int remainder = CHARACTERS.IndexOf(toFind);
                if (remainder == -1)
                    logError("Character in seed is invalid: " + toFind);
                total *= CHARACTERS.Length;
                total += remainder;
            }

            return total;
        }

        public static long generateUnoffensiveSeed(Rand rng)
        {
            string safeString = "fuck";
            while (BadWordChecker.containsBadWord(safeString)/* || TrialHelper.isTrialSeed(safeString)*/)
            {
                long possible = rng.randomLong();
                safeString = getString(possible);
            }

            return getLong(safeString);
        }
    }
}