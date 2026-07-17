using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace MoreMountains;

public class LocalizedStrings : FrameSystem
{
    // static ILogger logger = Log.GetLogger<LocalizedStrings>();
    const string LOCALIZATION_DIR = "Localizations";
    const string SEP = "/";

    public static string PERIOD;

    // static Dictionary<string, MonsterStrings> monsters;
    // static Dictionary<string, PowerStrings> powers;
    static Dictionary<string, CardStrings> cards;

    static Dictionary<string, RelicStrings> relics;

    // static Dictionary<string, EventStrings> events;
    // static Dictionary<string, PotionStrings> potions;
    // static Dictionary<string, CreditStrings> credits;
    // static Dictionary<string, TutorialStrings> tutorials;
    static Dictionary<string, KeywordStrings> keywords;

    // static Dictionary<string, ScoreBonusStrings> scoreBonuses;
    // static Dictionary<string, CharacterStrings> characters;
    static Dictionary<string, UIStrings> ui;
    // static Dictionary<string, OrbStrings> orb;
    // static Dictionary<string, StanceStrings> stance;
    // static Dictionary<string, RunModStrings> mod;
    // static Dictionary<string, BlightStrings> blights;
    // static Dictionary<string, AchievementStrings> achievements;
    // public static string break_chars = null;

    public void initLocalizedStrings()
    {
        using var _ = new ProfilerScope("initLocalizedStrings");
        string langCode = Settings.language switch
        {
            GameLanguage.ENG => "eng",
            GameLanguage.DUT => "dut",
            GameLanguage.EPO => "epo",
            GameLanguage.PTB => "ptb",
            GameLanguage.ZHS => "zhs",
            GameLanguage.ZHT => "zht",
            GameLanguage.FIN => "fin",
            GameLanguage.FRA => "fra",
            GameLanguage.DEU => "deu",
            GameLanguage.GRE => "gre",
            GameLanguage.IND => "ind",
            GameLanguage.ITA => "ita",
            GameLanguage.JPN => "jpn",
            GameLanguage.KOR => "kor",
            GameLanguage.NOR => "nor",
            GameLanguage.POL => "pol",
            GameLanguage.RUS => "rus",
            GameLanguage.SPA => "spa",
            GameLanguage.SRP => "srp",
            GameLanguage.SRB => "srb",
            GameLanguage.THA => "tha",
            GameLanguage.TUR => "tur",
            GameLanguage.UKR => "ukr",
            GameLanguage.VIE => "vie",
            GameLanguage.WWW => "www",
            _ => "www"
        };

        var langPackDir = GAMEPLAY_PATH + SEP + LOCALIZATION_DIR + SEP + langCode;
        var monsterPath = langPackDir + SEP + "monsters.json";
        var powerPath = langPackDir + SEP + "powers.json";
        var cardPath = langPackDir + SEP + "cards";
        // var cardRes = res.loadGameResource<TextAsset>(cardPath);
        // cards = JsonConvert.DeserializeObject<Dictionary<string, CardStrings>>(cardRes.getResource().text);

        var relicPath = langPackDir + SEP + "RelicStrings.json";
        var relicRes = resource.loadGameResource<TextAsset>(relicPath);
        relics = JsonConvert.DeserializeObject<Dictionary<string, RelicStrings>>(relicRes.getResource().text);
        var eventPath = langPackDir + SEP + "events.json";
        var potionPath = langPackDir + SEP + "potions.json";
        var creditPath = langPackDir + SEP + "credits.json";
        var tutorialsPath = langPackDir + SEP + "tutorials.json";
        var keywordsPath = langPackDir + SEP + "KeywordStrings.json";
        var keywordRes = resource.loadGameResource<TextAsset>(keywordsPath);
        keywords = JsonConvert.DeserializeObject<Dictionary<string, KeywordStrings>>(keywordRes.getResource().text);

        var scoreBonusesPath = langPackDir + SEP + "score_bonuses.json";
        var characterPath = langPackDir + SEP + "characters.json";
        var uiPath = langPackDir + SEP + "UIStrings.json";
        var uiRes = resource.loadGameResource<TextAsset>(uiPath);
        ui = JsonConvert.DeserializeObject<Dictionary<string, UIStrings>>(uiRes.getResource().text);

        // PERIOD = (getUIString("Period")).TEXT[0];

        var orbPath = langPackDir + SEP + "orbs.json";
        var stancePath = langPackDir + SEP + "stances.json";
        var modPath = langPackDir + SEP + "run_mods.json";
        var blightPath = langPackDir + SEP + "blights.json";
        var achievePath = langPackDir + SEP + "achievements.json";
        var lineBreakPath = langPackDir + SEP + "line_break.json";
        // if (Gdx.files.internal(lineBreakPath).exists())
        // break_chars = Gdx.files.internal(lineBreakPath).readString(string.valueOf(StandardCharsets.UTF_8));

        // logger.Info("Loc Strings load time: " + (TimeHelper.currentTimeMillis() - startTime) + "ms");
    }

    public override void init()
    {
        base.init();
        initLocalizedStrings();
    }

    /*public PowerStrings getPowerStrings(string powerName)
    {
        if (powers.ContainsKey(powerName))
            return powers[powerName];
        logger.Info("[ERROR] PowerString: " + powerName + " not found");
        return PowerStrings.getMockPowerString();
    }

    public MonsterStrings getMonsterStrings(string monsterName)
    {
        if (monsters.ContainsKey(monsterName))
            return monsters[monsterName];
        logger.Info("[ERROR] MonsterString: " + monsterName + " not found");
        return MonsterStrings.getMockMonsterString();
    }

    public EventStrings getEventString(string eventName)
    {
        if (events.ContainsKey(eventName))
            return events[eventName];
        logger.Info("[ERROR] EventString: " + eventName + " not found");
        return EventStrings.getMockEventString();
    }

    public PotionStrings getPotionString(string potionName)
    {
        if (potions.ContainsKey(potionName))
            return potions[potionName];
        logger.Info("[ERROR] PotionString: " + potionName + " not found");
        return PotionStrings.getMockPotionString();
    }

    public CreditStrings getCreditString(string creditName)
    {
        if (credits.ContainsKey(creditName))
            return credits[creditName];
        logger.Info("[ERROR] CreditString: " + creditName + " not found");
        return CreditStrings.getMockCreditString();
    }

    public TutorialStrings getTutorialString(string tutorialName)
    {
        if (tutorials.ContainsKey(tutorialName))
            return tutorials[tutorialName];
        logger.Info("[ERROR] TutorialString: " + tutorialName + " not found");
        return TutorialStrings.getMockTutorialString();
    }


    public CharacterStrings getCharacterString(string characterName)
    {
        return characters[characterName];
    }

    public OrbStrings getOrbString(string orbName)
    {
        if (orb.ContainsKey(orbName))
            return orb[orbName];
        logger.Info("[ERROR] OrbStrings: " + orbName + " not found");
        return OrbStrings.getMockOrbString();
    }

    public StanceStrings getStanceString(string stanceName)
    {
        return stance[stanceName];
    }

    public RunModStrings getRunModString(string modName)
    {
        if (mod.ContainsKey(modName))
            return mod[modName];
        logger.Info("[ERROR] RunModStrings: " + modName + " not found");
        return RunModStrings.getMockModString();
    }

    public BlightStrings getBlightString(string blightName)
    {
        if (blights.ContainsKey(blightName))
            return blights[blightName];
        logger.Info("[ERROR] BlightStrings: " + blightName + " not found");
        return BlightStrings.getBlightOrbString();
    }

    public ScoreBonusStrings getScoreString(string scoreName)
    {
        if (scoreBonuses.ContainsKey(scoreName))
            return scoreBonuses[scoreName];
        logger.Info("[ERROR] ScoreBonusStrings: " + scoreName + " not found");
        return ScoreBonusStrings.getScoreBonusString();
    }

    public AchievementStrings getAchievementString(string achievementName)
    {
        return achievements[achievementName];
    }*/


    public KeywordStrings getKeywordString(string keywordName)
    {
        if (keywords.TryGetValue(keywordName, out var strings))
            return strings;

        return null;
    }

    public UIStrings getUIString(string uiName)
    {
        if (ui.TryGetValue(uiName, out var strings))
            return strings;

        return UIStrings.getMockUIString();
    }

    public CardStrings getCardStrings(string cardName)
    {
        if (cards.TryGetValue(cardName, out var strings))
            return strings;

        // logger.Info("[ERROR] CardString: " + cardName + " not found");
        return CardStrings.getMockCardString();
    }

    public static string[] createMockStringArray(int size)
    {
        string[] retVal = new string[size];
        for (int i = 0; i < retVal.Length; i++)
            retVal[i] = "[MISSING_" + i + "]";
        return retVal;
    }

    public RelicStrings getRelicStrings(string relicName)
    {
        if (relics.TryGetValue(relicName, out var strings))
            return strings;

        return RelicStrings.getMockRelicString();
    }

    static string loadJson(string jsonPath)
    {
        return null;
        // return Gdx.files.internal(jsonPath).readString(string.valueOf(StandardCharsets.UTF_8));
    }
}