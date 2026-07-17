using System.Collections.Generic;

namespace MoreMountains;

public static class GameDictionary
{
    static KeywordStrings keywordStrings = languagePack.getKeywordString("Game Dictionary");
    public static string[] TEXT = keywordStrings.TEXT;
    public static Keyword ARTIFACT = keywordStrings.ARTIFACT;
    public static Keyword BLOCK = keywordStrings.BLOCK;
    public static Keyword EVOKE = keywordStrings.EVOKE;
    public static Keyword CONFUSED = keywordStrings.CONFUSED;
    public static Keyword CHANNEL = keywordStrings.CHANNEL;
    public static Keyword CURSE = keywordStrings.CURSE;
    public static Keyword DARK = keywordStrings.DARK;
    public static Keyword DEXTERITY = keywordStrings.DEXTERITY;
    public static Keyword ETHEREAL = keywordStrings.ETHEREAL;
    public static Keyword EXHAUST = keywordStrings.EXHAUST;
    public static Keyword FRAIL = keywordStrings.FRAIL;
    public static Keyword FROST = keywordStrings.FROST;
    public static Keyword INNATE = keywordStrings.INNATE;
    public static Keyword INTANGIBLE = keywordStrings.INTANGIBLE;
    public static Keyword FOCUS = keywordStrings.FOCUS;
    public static Keyword LIGHTNING = keywordStrings.LIGHTNING;
    public static Keyword LOCKED = keywordStrings.LOCKED;
    public static Keyword LOCK_ON = keywordStrings.LOCK_ON;
    public static Keyword OPENER = keywordStrings.OPENER;
    public static Keyword PLASMA = keywordStrings.PLASMA;
    public static Keyword POISON = keywordStrings.POISON;
    public static Keyword RETAIN = keywordStrings.RETAIN;
    public static Keyword SHIV = keywordStrings.SHIV;
    public static Keyword STATUS = keywordStrings.STATUS;
    public static Keyword STRENGTH = keywordStrings.STRENGTH;
    public static Keyword STRIKE = keywordStrings.STRIKE;
    public static Keyword TRANSFORM = keywordStrings.TRANSFORM;
    public static Keyword UNKNOWN = keywordStrings.UNKNOWN;
    public static Keyword UNPLAYABLE = keywordStrings.UNPLAYABLE;
    public static Keyword UPGRADE = keywordStrings.UPGRADE;
    public static Keyword VIGOR = keywordStrings.VIGOR;
    public static Keyword VOID = keywordStrings.VOID;
    public static Keyword VULNERABLE = keywordStrings.VULNERABLE;
    public static Keyword WEAK = keywordStrings.WEAK;
    public static Keyword WOUND = keywordStrings.WOUND;
    public static Keyword DAZED = keywordStrings.DAZED;
    public static Keyword BURN = keywordStrings.BURN;
    public static Keyword THORNS = keywordStrings.THORNS;
    public static Keyword STANCE = keywordStrings.STANCE;
    public static Keyword WRATH = keywordStrings.WRATH;
    public static Keyword CALM = keywordStrings.CALM;
    public static Keyword ENLIGHTENMENT = keywordStrings.DIVINITY;
    public static Keyword SCRY = keywordStrings.SCRY;
    public static Keyword PRAYER = keywordStrings.PRAYER;
    public static Keyword REGEN = keywordStrings.REGEN;
    public static Keyword RITUAL = keywordStrings.RITUAL;
    public static Keyword FATAL = keywordStrings.FATAL;
    public static Dictionary<string, string> keywords = new();
    public static Dictionary<string, string> parentWord = new();

    public static void initialize()
    {
        keywords["[R]"] = TEXT[0];
        keywords["[G]"] = TEXT[0];
        keywords["[B]"] = TEXT[0];
        keywords["[W]"] = TEXT[0];
        keywords["[E]"] = TEXT[0];

        createEntry(ARTIFACT.NAMES, ARTIFACT.DESCRIPTION);
        createEntry(BLOCK.NAMES, BLOCK.DESCRIPTION);
        createEntry(BURN.NAMES, BURN.DESCRIPTION);
        createEntry(CALM.NAMES, CALM.DESCRIPTION);
        createEntry(CHANNEL.NAMES, CHANNEL.DESCRIPTION);
        createEntry(CONFUSED.NAMES, CONFUSED.DESCRIPTION);
        createEntry(CURSE.NAMES, CURSE.DESCRIPTION);
        createEntry(DARK.NAMES, DARK.DESCRIPTION);
        createEntry(DAZED.NAMES, DAZED.DESCRIPTION);
        createEntry(DEXTERITY.NAMES, DEXTERITY.DESCRIPTION);
        createEntry(ENLIGHTENMENT.NAMES, ENLIGHTENMENT.DESCRIPTION);
        createEntry(ETHEREAL.NAMES, ETHEREAL.DESCRIPTION);
        createEntry(EVOKE.NAMES, EVOKE.DESCRIPTION);
        createEntry(EXHAUST.NAMES, EXHAUST.DESCRIPTION);
        createEntry(FOCUS.NAMES, FOCUS.DESCRIPTION);
        createEntry(FRAIL.NAMES, FRAIL.DESCRIPTION);
        createEntry(FROST.NAMES, FROST.DESCRIPTION);
        createEntry(INNATE.NAMES, INNATE.DESCRIPTION);
        createEntry(INTANGIBLE.NAMES, INTANGIBLE.DESCRIPTION);
        createEntry(LIGHTNING.NAMES, LIGHTNING.DESCRIPTION);
        createEntry(LOCK_ON.NAMES, LOCK_ON.DESCRIPTION);
        createEntry(LOCKED.NAMES, LOCKED.DESCRIPTION);
        createEntry(OPENER.NAMES, OPENER.DESCRIPTION);
        createEntry(PLASMA.NAMES, PLASMA.DESCRIPTION);
        createEntry(POISON.NAMES, POISON.DESCRIPTION);
        createEntry(PRAYER.NAMES, PRAYER.DESCRIPTION);
        createEntry(RETAIN.NAMES, RETAIN.DESCRIPTION);
        createEntry(SCRY.NAMES, SCRY.DESCRIPTION);
        createEntry(SHIV.NAMES, SHIV.DESCRIPTION);
        createEntry(STANCE.NAMES, STANCE.DESCRIPTION);
        createEntry(STATUS.NAMES, STATUS.DESCRIPTION);
        createEntry(STRENGTH.NAMES, STRENGTH.DESCRIPTION);
        createEntry(STRIKE.NAMES, STRIKE.DESCRIPTION);
        createEntry(THORNS.NAMES, THORNS.DESCRIPTION);
        createEntry(TRANSFORM.NAMES, TRANSFORM.DESCRIPTION);
        createEntry(UNKNOWN.NAMES, UNKNOWN.DESCRIPTION);
        createEntry(UNPLAYABLE.NAMES, UNPLAYABLE.DESCRIPTION);
        createEntry(UPGRADE.NAMES, UPGRADE.DESCRIPTION);
        createEntry(VIGOR.NAMES, VIGOR.DESCRIPTION);
        createEntry(VOID.NAMES, VOID.DESCRIPTION);
        createEntry(VULNERABLE.NAMES, VULNERABLE.DESCRIPTION);
        createEntry(WEAK.NAMES, WEAK.DESCRIPTION);
        createEntry(WOUND.NAMES, WOUND.DESCRIPTION);
        createEntry(WRATH.NAMES, WRATH.DESCRIPTION);
        createEntry(REGEN.NAMES, REGEN.DESCRIPTION);
        createEntry(RITUAL.NAMES, RITUAL.DESCRIPTION);
        createEntry(FATAL.NAMES, FATAL.DESCRIPTION);
    }

    private static void createEntry(string[] names, string desc)
    {
        foreach (string n in names)
        {
            keywords[n] = desc;
            parentWord[n] = names[0];
        }
    }
}