namespace MoreMountains
{
    public class FontHelper
    {
        const string TINY_NUMBERS_FONT = "font/04b03.ttf";
        const string ENG_DEFAULT_FONT = "font/Kreon-Regular.ttf";
        const string ENG_BOLD_FONT = "font/Kreon-Bold.ttf";
        const string ENG_ITALIC_FONT = "font/ZillaSlab-RegularItalic.otf";
        const string ENG_DRAMATIC_FONT = "font/FeDPrm27C.otf";
        const string ZHS_DEFAULT_FONT = "font/zhs/NotoSansMonoCJKsc-Regular.otf";
        const string ZHS_BOLD_FONT = "font/zhs/SourceHanSerifSC-Bold.otf";
        const string ZHS_ITALIC_FONT = "font/zhs/SourceHanSerifSC-Medium.otf";
        const string ZHT_DEFAULT_FONT = "font/zht/NotoSansCJKtc-Regular.otf";
        const string ZHT_BOLD_FONT = "font/zht/NotoSansCJKtc-Bold.otf";
        const string ZHT_ITALIC_FONT = "font/zht/NotoSansCJKtc-Medium.otf";
        const string EPO_DEFAULT_FONT = "font/epo/Andada-Regular.otf";
        const string EPO_BOLD_FONT = "font/epo/Andada-Bold.otf";
        const string EPO_ITALIC_FONT = "font/epo/Andada-Italic.otf";
        const string GRE_DEFAULT_FONT = "font/gre/Roboto-Regular.ttf";
        const string GRE_BOLD_FONT = "font/gre/Roboto-Bold.ttf";
        const string GRE_ITALIC_FONT = "font/gre/Roboto-Italic.ttf";
        const string JPN_DEFAULT_FONT = "font/jpn/NotoSansCJKjp-Regular.otf";
        const string JPN_BOLD_FONT = "font/jpn/NotoSansCJKjp-Bold.otf";
        const string JPN_ITALIC_FONT = "font/jpn/NotoSansCJKjp-Medium.otf";
        const string KOR_DEFAULT_FONT = "font/kor/GyeonggiCheonnyeonBatangBold.ttf";
        const string KOR_BOLD_FONT = "font/kor/GyeonggiCheonnyeonBatangBold.ttf";
        const string KOR_ITALIC_FONT = "font/kor/GyeonggiCheonnyeonBatangBold.ttf";
        const string RUS_DEFAULT_FONT = "font/rus/FiraSansExtraCondensed-Regular.ttf";
        const string RUS_BOLD_FONT = "font/rus/FiraSansExtraCondensed-Bold.ttf";
        const string RUS_ITALIC_FONT = "font/rus/FiraSansExtraCondensed-Italic.ttf";
        const string SRB_DEFAULT_FONT = "font/srb/InfluBG.otf";
        const string SRB_BOLD_FONT = "font/srb/InfluBG-Bold.otf";
        const string SRB_ITALIC_FONT = "font/srb/InfluBG-Italic.otf";
        const string THA_DEFAULT_FONT = "font/tha/CSChatThaiUI.ttf";
        const string THA_BOLD_FONT = "font/tha/CSChatThaiUI.ttf";
        const string THA_ITALIC_FONT = "font/tha/CSChatThaiUI.ttf";
        const string VIE_DEFAULT_FONT = "font/vie/Grenze-Regular.ttf";
        const string VIE_BOLD_FONT = "font/vie/Grenze-SemiBold.ttf";
        const string VIE_DRAMATIC_FONT = "font/vie/Grenze-Black.ttf";
        const string VIE_ITALIC_FONT = "font/vie/Grenze-RegularItalic.ttf";

        /*static Logger logger = LogManager.getLogger(FontHelper.class.getName());
        static FreeTypeFontGenerator.FreeTypeFontParameter param = new FreeTypeFontGenerator.FreeTypeFontParameter();
        static FreeTypeFontGenerator.FreeTypeBitmapFontData data = new FreeTypeFontGenerator.FreeTypeBitmapFontData();
        static HashMap<string, FreeTypeFontGenerator> generators = new HashMap<>();
        static string fontFile = null;
        static float fontScale = 1.0F;
        static Vector2 rotatedTextTmp = new Vector2(0.0F, 0.0F);
        static Matrix4 rotatedTextMatrix = new Matrix4();

        public static BitmapFont charDescFont;
        public static BitmapFont charTitleFont;
        public static BitmapFont cardTitleFont;
        public static BitmapFont cardTypeFont;
        public static BitmapFont cardEnergyFont_L;
        public static BitmapFont cardDescFont_N;
        public static BitmapFont cardDescFont_L;
        public static BitmapFont SCP_cardDescFont;
        public static BitmapFont SCP_cardEnergyFont;
        public static BitmapFont SCP_cardTitleFont_small;
        public static BitmapFont SRV_quoteFont;
        public static BitmapFont losePowerFont;
        public static BitmapFont energyNumFontRed;
        public static BitmapFont energyNumFontGreen;
        public static BitmapFont energyNumFontBlue;
        public static BitmapFont energyNumFontPurple;
        public static BitmapFont turnNumFont;
        public static BitmapFont damageNumberFont;
        public static BitmapFont buttonLabelFont;
        public static BitmapFont dungeonTitleFont;
        public static BitmapFont bannerNameFont;
        static float CARD_ENERGY_IMG_WIDTH = 26.0F * Settings.scale;
        public static BitmapFont topPanelAmountFont;
        public static BitmapFont powerAmountFont;
        public static BitmapFont panelNameFont;
        public static BitmapFont healthInfoFont;
        public static BitmapFont blockInfoFont;
        public static BitmapFont topPanelInfoFont;
        public static BitmapFont tipHeaderFont;
        public static BitmapFont tipBodyFont;
        public static BitmapFont panelEndTurnFont;
        public static BitmapFont largeDialogOptionFont;
        public static BitmapFont smallDialogOptionFont;
        public static BitmapFont largeCardFont;
        public static BitmapFont menuBannerFont;
        public static BitmapFont leaderboardFont;
        public static GlyphLayout layout = new GlyphLayout();
        static TextureAtlas.AtlasRegion orb;
        static Color color;
        static float curWidth;
        static float curHeight;
        static float spaceWidth;
        static int currentLine;
        static Matrix4 mx4 = new Matrix4();
        static StringBuilder newMsg = new StringBuilder();
        static int TIP_OFFSET_X = 50;
        static float TIP_PADDING = 8.0F;

        public static void initialize()
        {
            long startTime = System.currentTimeMillis();
            generators.clear();
            switch (Settings.language)
            {
                case GameLanguage.ZHS:
                    fontFile = ZHS_DEFAULT_FONT;
                    break;
                case GameLanguage.ZHT:
                    fontFile = ZHT_DEFAULT_FONT;
                    break;
                case GameLanguage.EPO:
                    fontFile = EPO_DEFAULT_FONT;
                    break;
                case GameLanguage.GRE:
                    fontFile = GRE_DEFAULT_FONT;
                    break;
                case GameLanguage.JPN:
                    fontFile = JPN_DEFAULT_FONT;
                    break;
                case GameLanguage.KOR:
                    fontFile = KOR_DEFAULT_FONT;
                    break;
                case GameLanguage.POL:
                case GameLanguage.RUS:
                case GameLanguage.UKR:
                    fontFile = RUS_DEFAULT_FONT;
                    break;
                case GameLanguage.SRP:
                case GameLanguage.SRB:
                    fontFile = SRB_DEFAULT_FONT;
                    break;
                case GameLanguage.THA:
                    fontFile = THA_DEFAULT_FONT;
                    fontScale = 0.95F;
                    break;
                case GameLanguage.VIE:
                    fontFile = VIE_DEFAULT_FONT;
                    break;
                case GameLanguage.ENG:
                case GameLanguage.DUT:
                case GameLanguage.PTB:
                case GameLanguage.FIN:
                case GameLanguage.FRA:
                case GameLanguage.DEU:
                case GameLanguage.IND:
                case GameLanguage.ITA:
                case GameLanguage.NOR:
                case GameLanguage.SPA:
                case GameLanguage.TUR:
                case GameLanguage.WWW:
                default:
                    fontFile = ENG_DEFAULT_FONT;
                    break;
            }

            data.xChars = new char[] { '动' };
            data.capChars = new char[] { '动' };
            param.hinting = FreeTypeFontGenerator.Hinting.Slight;
            param.spaceX = 0;
            param.kerning = true;
            param.borderWidth = 0.0F;
            param.gamma = 0.9F;
            param.borderGamma = 0.9F;
            param.shadowColor = Settings.QUARTER_TRANSPARENT_BLACK_COLOR;
            param.shadowOffsetX = (int)(4.0F * Settings.scale);
            charDescFont = Settings.isMobile ? prepFont(31.0F, false) : prepFont(30.0F, false);
            param.gamma = 1.8F;
            param.borderGamma = 1.8F;
            param.shadowOffsetX = (int)(6.0F * Settings.scale);
            charTitleFont = prepFont(44.0F, false);
            param.gamma = 0.9F;
            param.borderGamma = 0.9F;
            param.shadowOffsetX = Math.round(3.0F * Settings.scale);
            param.shadowOffsetY = Math.round(3.0F * Settings.scale);
            param.borderStraight = false;
            param.shadowColor = new Color(0.0F, 0.0F, 0.0F, 0.25F);
            param.borderColor = new Color(0.35F, 0.35F, 0.35F, 1.0F);
            param.borderWidth = 2.0F * Settings.scale;
            cardTitleFont = prepFont(27.0F, true);
            param.borderWidth = 2.25F * Settings.scale;
            param.borderWidth = 0.0F;
            param.shadowOffsetX = 1;
            param.shadowOffsetY = 1;
            param.spaceX = 0;
            cardDescFont_N = prepFont(24.0F, false);
            cardDescFont_L = prepFont(24.0F, true);
            param.shadowColor = Settings.QUARTER_TRANSPARENT_BLACK_COLOR;
            param.shadowOffsetX = Math.round(4.0F * Settings.scale);
            param.shadowOffsetY = Math.round(3.0F * Settings.scale);
            SCP_cardDescFont = prepFont(48.0F, true);
            param.shadowOffsetX = Math.round(6.0F * Settings.scale);
            param.shadowOffsetY = Math.round(6.0F * Settings.scale);
            param.shadowColor = Settings.QUARTER_TRANSPARENT_BLACK_COLOR;
            param.borderColor = new Color(0.35F, 0.35F, 0.35F, 1.0F);
            param.borderWidth = 4.0F * Settings.scale;
            SCP_cardTitleFont_small = prepFont(46.0F, true);
            param.borderWidth = 0.0F;
            param.shadowColor = Settings.QUARTER_TRANSPARENT_BLACK_COLOR;
            param.shadowOffsetX = Math.round(3.0F * Settings.scale);
            param.shadowOffsetY = Math.round(3.0F * Settings.scale);
            panelNameFont = prepFont(34.0F, true);
            param.shadowOffsetX = (int)(3.0F * Settings.scale);
            param.shadowOffsetY = (int)(3.0F * Settings.scale);
            param.borderColor = new Color(0.67F, 0.06F, 0.22F, 1.0F);
            param.gamma = 0.9F;
            param.borderGamma = 0.9F;
            param.borderColor = new Color(0.4F, 0.1F, 0.1F, 1.0F);
            param.borderWidth = 0.0F;
            tipBodyFont = prepFont(22.0F, false);
            param.borderColor = new Color(0.1F, 0.1F, 0.1F, 0.5F);
            param.borderWidth = 2.0F * Settings.scale;
            topPanelAmountFont = prepFont(24.0F, false);
            param.borderColor = Color.valueOf("42514dff");
            param.shadowOffsetX = (int)(4.0F * Settings.scale);
            param.shadowOffsetY = (int)(4.0F * Settings.scale);
            param.borderWidth = 3.0F * Settings.scale;
            panelEndTurnFont = prepFont(26.0F, false);
            param.spaceX = 0;
            param.borderWidth = 0.0F;
            param.shadowOffsetX = (int)(3.0F * Settings.scale);
            param.shadowOffsetY = (int)(3.0F * Settings.scale);
            largeDialogOptionFont = prepFont(30.0F, false);
            (largeDialogOptionFont.getData()).markupEnabled = false;
            smallDialogOptionFont = prepFont(26.0F, false);
            (smallDialogOptionFont.getData()).markupEnabled = false;
            param.shadowOffsetX = 0;
            param.shadowOffsetY = 0;
            turnNumFont = prepFont(32.0F, true);
            param.borderWidth = 4.0F * Settings.scale;
            param.borderColor = new Color(0.3F, 0.3F, 0.3F, 1.0F);
            param.shadowColor = Settings.QUARTER_TRANSPARENT_BLACK_COLOR;
            param.shadowOffsetX = Math.round(3.0F * Settings.scale);
            param.shadowOffsetY = Math.round(3.0F * Settings.scale);
            losePowerFont = prepFont(36.0F, true);
            param.borderWidth = 3.0F * Settings.scale;
            param.borderColor = Color.DARK_GRAY;
            damageNumberFont = prepFont(48.0F, true);
            damageNumberFont.getData().setLineHeight((damageNumberFont.getData()).lineHeight * 0.85F);
            param.borderWidth = 2.7F * Settings.scale;
            param.shadowOffsetX = (int)(3.0F * Settings.scale);
            param.shadowOffsetY = (int)(3.0F * Settings.scale);
            param.borderColor = new Color(0.45F, 0.1F, 0.12F, 1.0F);
            param.shadowColor = Settings.QUARTER_TRANSPARENT_BLACK_COLOR;
            healthInfoFont = Settings.isMobile ? prepFont(29.0F, false) : prepFont(22.0F, false);
            param.borderWidth = 4.0F * Settings.scale;
            param.spaceX = (int)(-2.5F * Settings.scale);
            param.borderColor = Settings.QUARTER_TRANSPARENT_BLACK_COLOR;
            buttonLabelFont = Settings.isMobile ? prepFont(37.0F, true) : prepFont(32.0F, true);
            param.spaceX = 0;
            fontScale = 1.0F;
            fontFile = ENG_BOLD_FONT;
            param.borderStraight = true;
            param.borderWidth = 4.0F * Settings.scale;
            param.borderColor = new Color(0.4F, 0.15F, 0.15F, 1.0F);
            energyNumFontRed = prepFont(36.0F, true);
            param.borderColor = new Color(0.15F, 0.4F, 0.15F, 1.0F);
            energyNumFontGreen = prepFont(36.0F, true);
            param.borderColor = new Color(0.1F, 0.2F, 0.4F, 1.0F);
            energyNumFontBlue = prepFont(36.0F, true);
            param.borderColor = new Color(1595767551);
            energyNumFontPurple = prepFont(36.0F, true);
            param.borderWidth = 4.0F * Settings.scale;
            param.borderColor = new Color(0.3F, 0.3F, 0.3F, 1.0F);
            cardEnergyFont_L = prepFont(38.0F, true);
            param.borderWidth = 8.0F * Settings.scale;
            SCP_cardEnergyFont = prepFont(76.0F, true);
            param.shadowOffsetX = (int)(2.0F * Settings.scale);
            param.shadowOffsetY = (int)(2.0F * Settings.scale);
            param.borderColor = new Color(0.0F, 0.33F, 0.2F, 0.8F);
            param.borderWidth = 2.7F * Settings.scale;
            param.spaceX = (int)(-0.9F * Settings.scale);
            blockInfoFont = Settings.isMobile ? prepFont(30.0F, false) : prepFont(24.0F, false);
            switch (Settings.language)
            {
                case GameLanguage.ZHS:
                    fontFile = ZHS_BOLD_FONT;
                    break;
                case GameLanguage.ZHT:
                    fontFile = ZHT_BOLD_FONT;
                    break;
                case GameLanguage.EPO:
                    fontFile = EPO_BOLD_FONT;
                    break;
                case GameLanguage.GRE:
                    fontFile = GRE_BOLD_FONT;
                    break;
                case GameLanguage.JPN:
                    fontFile = JPN_BOLD_FONT;
                    break;
                case GameLanguage.KOR:
                    fontFile = KOR_BOLD_FONT;
                    break;
                case GameLanguage.POL:
                case GameLanguage.RUS:
                case GameLanguage.UKR:
                    fontFile = RUS_BOLD_FONT;
                    break;
                case GameLanguage.SRP:
                case GameLanguage.SRB:
                    fontFile = SRB_BOLD_FONT;
                    break;
                case GameLanguage.THA:
                    fontScale = 0.95F;
                    fontFile = THA_BOLD_FONT;
                    break;
                case GameLanguage.VIE:
                    fontFile = VIE_BOLD_FONT;
                    break;
                default:
                    fontFile = ENG_BOLD_FONT;
                    break;
            }

            param.gamma = 1.2F;
            param.borderWidth = 0.0F;
            param.shadowOffsetX = 0;
            param.shadowOffsetY = 0;
            if (Settings.WIDTH >= 1600)
                param.spaceX = -1;
            cardTypeFont = prepFont(17.0F, true);
            param.gamma = 1.2F;
            param.borderGamma = 1.2F;
            param.borderWidth = 0.0F;
            param.shadowColor = new Color(0.0F, 0.0F, 0.0F, 0.12F);
            param.shadowOffsetX = (int)(5.0F * Settings.scale);
            param.shadowOffsetY = (int)(4.0F * Settings.scale);
            menuBannerFont = prepFont(38.0F, true);
            param.characters = "?";
            param.shadowOffsetX = (int)(15.0F * Settings.scale);
            param.shadowOffsetY = (int)(12.0F * Settings.scale);
            largeCardFont = prepFont(120.0F, true);
            param.shadowOffsetX = 2;
            param.shadowOffsetY = 2;
            param.shadowColor = new Color(0.0F, 0.0F, 0.0F, 0.33F);
            param.gamma = 2.0F;
            param.borderGamma = 2.0F;
            param.borderStraight = true;
            param.borderColor = Color.DARK_GRAY;
            param.borderWidth = 2.0F * Settings.scale;
            param.shadowOffsetX = 1;
            param.shadowOffsetY = 1;
            tipHeaderFont = prepFont(23.0F, false);
            param.shadowOffsetX = 2;
            param.shadowOffsetY = 2;
            topPanelInfoFont = prepFont(26.0F, false);
            param.spaceX = 0;
            param.gamma = 0.9F;
            param.borderGamma = 0.9F;
            param.borderWidth = 0.0F;
            fontScale = 1.0F;
            fontFile = TINY_NUMBERS_FONT;
            param.borderWidth = 2.0F * Settings.scale;
            powerAmountFont = Settings.isMobile ? prepFont(20.0F, false) : prepFont(16.0F, false);
            switch (Settings.language)
            {
                case GameLanguage.ZHS:
                    fontFile = ZHS_BOLD_FONT;
                    break;
                case GameLanguage.ZHT:
                    fontFile = ZHT_BOLD_FONT;
                    break;
                case GameLanguage.EPO:
                    fontFile = EPO_BOLD_FONT;
                    break;
                case GameLanguage.GRE:
                    fontFile = GRE_BOLD_FONT;
                    break;
                case GameLanguage.JPN:
                    fontFile = JPN_BOLD_FONT;
                    break;
                case GameLanguage.KOR:
                    fontFile = KOR_BOLD_FONT;
                    break;
                case GameLanguage.POL:
                case GameLanguage.RUS:
                case GameLanguage.UKR:
                    fontFile = RUS_BOLD_FONT;
                    break;
                case GameLanguage.SRP:
                case GameLanguage.SRB:
                    fontFile = SRB_BOLD_FONT;
                    break;
                case GameLanguage.THA:
                    fontScale = 0.95F;
                    fontFile = THA_BOLD_FONT;
                    break;
                case GameLanguage.VIE:
                    fontFile = VIE_DRAMATIC_FONT;
                    break;
                default:
                    fontFile = ENG_DRAMATIC_FONT;
                    break;
            }

            param.gamma = 0.5F;
            param.borderGamma = 0.5F;
            param.shadowOffsetX = 0;
            param.shadowOffsetY = 0;
            param.borderWidth = 6.0F * Settings.scale;
            param.borderColor = new Color(0.0F, 0.0F, 0.0F, 0.5F);
            param.spaceX = (int)(-5.0F * Settings.scale);
            dungeonTitleFont = prepFont(115.0F, true);
            dungeonTitleFont.getData().setScale(1.25F);
            param.borderWidth = 4.0F * Settings.scale;
            param.borderColor = new Color(0.0F, 0.0F, 0.0F, 0.33F);
            param.spaceX = (int)(-2.0F * Settings.scale);
            bannerNameFont = prepFont(72.0F, true);
            fontScale = 1.0F;
            switch (Settings.language)
            {
                case GameLanguage.ZHS:
                    fontFile = ZHS_ITALIC_FONT;
                    break;
                case GameLanguage.ZHT:
                    fontFile = ZHT_ITALIC_FONT;
                    break;
                case GameLanguage.EPO:
                    fontFile = EPO_ITALIC_FONT;
                    break;
                case GameLanguage.GRE:
                    fontFile = GRE_ITALIC_FONT;
                    break;
                case GameLanguage.JPN:
                    fontFile = JPN_ITALIC_FONT;
                    break;
                case GameLanguage.KOR:
                    fontFile = KOR_ITALIC_FONT;
                    break;
                case GameLanguage.POL:
                case GameLanguage.RUS:
                case GameLanguage.UKR:
                    fontFile = RUS_ITALIC_FONT;
                    break;
                case GameLanguage.SRP:
                case GameLanguage.SRB:
                    fontFile = SRB_ITALIC_FONT;
                    break;
                case GameLanguage.THA:
                    fontScale = 0.95F;
                    fontFile = THA_ITALIC_FONT;
                    break;
                case GameLanguage.VIE:
                    fontFile = VIE_ITALIC_FONT;
                    break;
                default:
                    fontFile = ENG_ITALIC_FONT;
                    break;
            }

            param.shadowOffsetX = 0;
            param.shadowOffsetY = 0;
            param.borderWidth = 0.0F;
            param.shadowOffsetX = Math.round(2.0F * Settings.scale);
            param.shadowOffsetY = Math.round(2.0F * Settings.scale);
            param.spaceX = 0;
            SRV_quoteFont = prepFont(28.0F, false);
            fontScale = 1.0F;
            fontFile = ZHS_DEFAULT_FONT;
            leaderboardFont = prepFont(30.0F, false);
            logger.info("Font load time: " + (System.currentTimeMillis() - startTime) + "ms");
        }

        public static void ClearSCPFontTextures()
        {
            System.out.println("Clearing SCP font textures...");
            SCP_cardDescFont.dispose();
            SCP_cardEnergyFont.dispose();
            SCP_cardTitleFont_small.dispose();
            fontScale = (Settings.language == GameLanguage.THA) ? 0.95F : 1.0F;
            fontFile = SCP_cardDescFont.getData().getFontFile();
            param.borderWidth = 0.0F;
            param.shadowColor = Settings.QUARTER_TRANSPARENT_BLACK_COLOR;
            param.shadowOffsetX = Math.round(4.0F * Settings.scale);
            param.shadowOffsetY = Math.round(3.0F * Settings.scale);
            SCP_cardDescFont = prepFont(48.0F, true);
            fontScale = 1.0F;
            param.shadowOffsetX = Math.round(6.0F * Settings.scale);
            param.shadowOffsetY = Math.round(6.0F * Settings.scale);
            param.borderColor = new Color(0.35F, 0.35F, 0.35F, 1.0F);
            param.borderWidth = 4.0F * Settings.scale;
            SCP_cardTitleFont_small = prepFont(46.0F, true);
            param.borderStraight = true;
            param.borderColor = new Color(0.3F, 0.3F, 0.3F, 1.0F);
            param.borderWidth = 8.0F * Settings.scale;
            SCP_cardEnergyFont = prepFont(76.0F, true);
        }

        public static void ClearSRVFontTextures()
        {
            System.out.println("Clearing SRV font textures...");
            SRV_quoteFont.dispose();
            SCP_cardDescFont.dispose();
            fontScale = (Settings.language == GameLanguage.THA) ? 0.95F : 1.0F;
            fontFile = SCP_cardDescFont.getData().getFontFile();
            param.borderWidth = 0.0F;
            param.shadowColor = Settings.QUARTER_TRANSPARENT_BLACK_COLOR;
            param.shadowOffsetX = Math.round(4.0F * Settings.scale);
            param.shadowOffsetY = Math.round(3.0F * Settings.scale);
            SCP_cardDescFont = prepFont(48.0F, true);
            fontScale = 1.0F;
            fontFile = SRV_quoteFont.getData().getFontFile();
            param.shadowColor = new Color(0.0F, 0.0F, 0.0F, 0.33F);
            param.shadowOffsetX = Math.round(2.0F * Settings.scale);
            param.shadowOffsetY = Math.round(2.0F * Settings.scale);
            param.spaceX = 0;
            SRV_quoteFont = prepFont(28.0F, false);
        }

        public static void ClearLeaderboardFontTextures()
        {
            System.out.println("Clearing leaderboard font textures...");
            leaderboardFont.dispose();
            fontScale = 1.0F;
            param.shadowOffsetX = 0;
            param.shadowOffsetY = 0;
            param.borderWidth = 0.0F;
            param.spaceX = 0;
            fontFile = leaderboardFont.getData().getFontFile();
            leaderboardFont = prepFont(30.0F, false);
        }

        public static BitmapFont prepFont(float size, boolean isLinearFiltering)
        {
            FreeTypeFontGenerator g;
            if (generators.containsKey(fontFile.path()))
            {
                g = generators.get(fontFile.path());
            }
            else
            {
                g = new FreeTypeFontGenerator(fontFile);
                generators.put(fontFile.path(), g);
            }

            if (Settings.BIG_TEXT_MODE)
                size *= 1.2F;
            return prepFont(g, size, isLinearFiltering);
        }

        static BitmapFont prepFont(FreeTypeFontGenerator g, float size, boolean isLinearFiltering)
        {
            FreeTypeFontGenerator.FreeTypeFontParameter p = new FreeTypeFontGenerator.FreeTypeFontParameter();
            p.characters = "";
            p.incremental = true;
            p.size = Math.round(size * fontScale * Settings.scale);
            p.gamma = param.gamma;
            p.spaceX = param.spaceX;
            p.spaceY = param.spaceY;
            p.borderColor = param.borderColor;
            p.borderStraight = param.borderStraight;
            p.borderWidth = param.borderWidth;
            p.borderGamma = param.borderGamma;
            p.shadowColor = param.shadowColor;
            p.shadowOffsetX = param.shadowOffsetX;
            p.shadowOffsetY = param.shadowOffsetY;
            if (isLinearFiltering)
            {
                p.minFilter = Texture.TextureFilter.Linear;
                p.magFilter = Texture.TextureFilter.Linear;
            }
            else
            {
                p.minFilter = Texture.TextureFilter.Nearest;
                p.magFilter = Texture.TextureFilter.MipMapLinearNearest;
            }

            g.scaleForPixelHeight(p.size);
            BitmapFont font = g.generateFont(p);
            font.setUseIntegerPositions(!isLinearFiltering);
            (font.getData()).markupEnabled = true;
            if (LocalizedStrings.break_chars != null)
                (font.getData()).breakChars = LocalizedStrings.break_chars.toCharArray();
            (font.getData()).fontFile = fontFile;
            return font;
        }

        public static void renderTipLeft(SpriteBatch sb, string msg)
        {
            layout.setText(cardDescFont_N, msg);
            sb.setColor(Color.BLACK);
            sb.draw(ImageMaster.WHITE_SQUARE_IMG, InputHelper.mX - layout.width - 16.0F - 12.5F, InputHelper.mY - layout.height, layout.width + 16.0F, layout.height + 16.0F);
            renderFont(sb, cardDescFont_N, msg, InputHelper.mX - layout.width - 8.0F - 12.0F, InputHelper.mY + 8.0F, Color.WHITE);
        }

        public static void renderFont(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color c)
        {
            font.setColor(c);
            font.draw(sb, msg, x, y);
        }

        public static void renderRotatedText(SpriteBatch sb, BitmapFont font, string msg, float x, float y, float offsetX, float offsetY, float angle, boolean roundY, Color c)
        {
            if (roundY)
                y = Math.round(y) + 0.25F;
            if ((font.getData()).scaleX == 1.0F)
            {
                x = MathUtils.round(x);
                y = MathUtils.round(y);
                offsetX = MathUtils.round(offsetX);
                offsetY = MathUtils.round(offsetY);
            }

            mx4.setToRotation(0.0F, 0.0F, 1.0F, angle);
            rotatedTextTmp.x = offsetX;
            rotatedTextTmp.y = offsetY;
            rotatedTextTmp.rotate(angle);
            mx4.trn(x + rotatedTextTmp.x, y + rotatedTextTmp.y, 0.0F);
            sb.end();
            sb.setTransformMatrix(mx4);
            sb.begin();
            font.setColor(c);
            layout.setText(font, msg);
            font.draw(sb, msg, -layout.width / 2.0F, layout.height / 2.0F);
            sb.end();
            sb.setTransformMatrix(rotatedTextMatrix);
            sb.begin();
        }

        public static void renderWrappedText(SpriteBatch sb, BitmapFont font, string msg, float x, float y, float width)
        {
            renderWrappedText(sb, font, msg, x, y, width, Color.WHITE, 1.0F);
        }

        public static void renderWrappedText(SpriteBatch sb, BitmapFont font, string msg, float x, float y, float width, float scale)
        {
            renderWrappedText(sb, font, msg, x, y, width, Color.WHITE, scale);
        }

        public static void renderWrappedText(SpriteBatch sb, BitmapFont font, string msg, float x, float y, float width, Color c, float scale)
        {
            font.getData().setScale(scale);
            font.setColor(c);
            layout.setText(font, msg, Color.WHITE, width, 1, true);
            font.draw(sb, msg, x - width / 2.0F, y + layout.height / 2.0F * scale, width, 1, true);
            font.getData().setScale(1.0F);
        }

        public static void renderFontLeftDownAligned(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color c)
        {
            layout.setText(font, msg);
            renderFont(sb, font, msg, x, y + layout.height, c);
        }

        public static void renderFontRightToLeft(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color c)
        {
            layout.setText(font, msg, c, 1.0F, 18, false);
            font.setColor(c);
            font.draw(sb, msg, x - layout.width, y);
        }

        public static void renderFontRightTopAligned(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color c)
        {
            layout.setText(font, msg);
            renderFont(sb, font, msg, x - layout.width, y, c);
        }

        public static void renderFontRightAligned(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color c)
        {
            layout.setText(font, msg);
            renderFont(sb, font, msg, x - layout.width, y + layout.height / 2.0F, c);
        }

        public static void renderFontRightTopAligned(SpriteBatch sb, BitmapFont font, string msg, float x, float y, float scale, Color c)
        {
            font.getData().setScale(1.0F);
            layout.setText(font, msg);
            float offsetX = layout.width / 2.0F;
            float offsetY = layout.height;
            font.getData().setScale(scale);
            layout.setText(font, msg);
            renderFont(sb, font, msg, x - layout.width / 2.0F - offsetX, y + layout.height / 2.0F + offsetY, c);
        }

        public static void renderSmartText(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color baseColor)
        {
            renderSmartText(sb, font, msg, x, y, Float.MAX_VALUE, font.getLineHeight(), baseColor);
        }

        public static void renderSmartText(SpriteBatch sb, BitmapFont font, string msg, float x, float y, float lineWidth, float lineSpacing, Color baseColor)
        {
            if (msg == null)
                return;
            if (Settings.lineBreakViaCharacter && (font.getData()).markupEnabled)
            {
                exampleNonWordWrappedText(sb, font, msg, x, y, baseColor, lineWidth, lineSpacing);
                return;
            }

            curWidth = 0.0F;
            curHeight = 0.0F;
            layout.setText(font, " ");
            spaceWidth = layout.width;
            for (string word :
            msg.split(" "))
            {
                if (word.equals("NL"))
                {
                    curWidth = 0.0F;
                    curHeight -= lineSpacing;
                }
                else if (word.equals("TAB"))
                {
                    curWidth += spaceWidth * 5.0F;
                }
                else
                {
                    orb = identifyOrb(word);
                    if (orb == null)
                    {
                        color = identifyColor(word).cpy();
                        if (!color.equals(Color.WHITE))
                        {
                            word = word.substring(2);
                            color.a = baseColor.a;
                            font.setColor(color);
                        }
                        else
                        {
                            font.setColor(baseColor);
                        }

                        layout.setText(font, word);
                        if (curWidth + layout.width > lineWidth)
                        {
                            curHeight -= lineSpacing;
                            font.draw(sb, word, x, y + curHeight);
                            curWidth = layout.width + spaceWidth;
                        }
                        else
                        {
                            font.draw(sb, word, x + curWidth, y + curHeight);
                            curWidth += layout.width + spaceWidth;
                        }
                    }
                    else
                    {
                        sb.setColor(new Color(1.0F, 1.0F, 1.0F, baseColor.a));
                        if (curWidth + CARD_ENERGY_IMG_WIDTH > lineWidth)
                        {
                            curHeight -= lineSpacing;
                            sb.draw(orb, x - orb.packedWidth / 2.0F + 13.0F * Settings.scale, y + curHeight - orb.packedHeight / 2.0F - 8.0F * Settings.scale, orb.packedWidth / 2.0F, orb.packedHeight / 2.0F, orb.packedWidth, orb.packedHeight, Settings.scale, Settings.scale, 0.0F);
                            curWidth = CARD_ENERGY_IMG_WIDTH + spaceWidth;
                        }
                        else
                        {
                            sb.draw(orb, x + curWidth - orb.packedWidth / 2.0F + 13.0F * Settings.scale, y + curHeight - orb.packedHeight / 2.0F - 8.0F * Settings.scale, orb.packedWidth / 2.0F, orb.packedHeight / 2.0F, orb.packedWidth, orb.packedHeight, Settings.scale, Settings.scale, 0.0F);
                            curWidth += CARD_ENERGY_IMG_WIDTH + spaceWidth;
                        }
                    }
                }
            }
            layout.setText(font, msg);
        }

        public static void renderSmartText(SpriteBatch sb, BitmapFont font, string msg, float x, float y, float lineWidth, float lineSpacing, Color baseColor, float scale)
        {
            BitmapFont.BitmapFontData data = font.getData();
            float prevScale = data.scaleX;
            data.setScale(scale);
            renderSmartText(sb, font, msg, x, y, lineWidth, lineSpacing, baseColor);
            data.setScale(prevScale);
        }

        public static float getSmartHeight(BitmapFont font, string msg, float lineWidth, float lineSpacing)
        {
            if (msg == null)
                return 0.0F;
            if (Settings.lineBreakViaCharacter)
                return -getHeightForCharLineBreak(font, msg, lineWidth, lineSpacing);
            curWidth = 0.0F;
            curHeight = 0.0F;
            layout.setText(font, " ");
            spaceWidth = layout.width;
            for (string word :
            msg.split(" "))
            {
                if (word.equals("NL"))
                {
                    curWidth = 0.0F;
                    curHeight -= lineSpacing;
                }
                else if (word.equals("TAB"))
                {
                    curWidth += spaceWidth * 5.0F;
                }
                else
                {
                    orb = identifyOrb(word);
                    if (orb == null)
                    {
                        if (!identifyColor(word).equals(Color.WHITE))
                            word = word.substring(2);
                        layout.setText(font, word);
                        if (curWidth + layout.width > lineWidth)
                        {
                            curHeight -= lineSpacing;
                            curWidth = layout.width + spaceWidth;
                        }
                        else
                        {
                            curWidth += layout.width + spaceWidth;
                        }
                    }
                    else if (curWidth + CARD_ENERGY_IMG_WIDTH > lineWidth)
                    {
                        curHeight -= lineSpacing;
                        curWidth = CARD_ENERGY_IMG_WIDTH + spaceWidth;
                    }
                    else
                    {
                        curWidth += CARD_ENERGY_IMG_WIDTH + spaceWidth;
                    }
                }
            }
            return curHeight;
        }

        static float getHeightForCharLineBreak(BitmapFont font, string msg, float lineWidth, float lineSpacing)
        {
            newMsg.setLength(0);
            for (string word :
            msg.split(" "))
            {
                if (word.equals("NL"))
                {
                    newMsg.append("\n");
                }
                else if (word.length() > 0 && word.charAt(0) == '#')
                {
                    newMsg.append(word.substring(2));
                }
                else
                {
                    newMsg.append(word);
                }
            }
            msg = newMsg.toString();
            if (msg != null && msg.length() > 0)
                layout.setText(font, msg, Color.WHITE, lineWidth, -1, true);
            return layout.height - 16.0F * Settings.scale;
        }

        public static float getHeight(BitmapFont font)
        {
            layout.setText(font, "gl0!");
            return layout.height;
        }

        public static float getSmartWidth(BitmapFont font, string msg, float lineWidth, float lineSpacing)
        {
            curWidth = 0.0F;
            layout.setText(font, " ");
            spaceWidth = layout.width;
            for (string word :
            msg.split(" "))
            {
                if (word.equals("NL"))
                {
                    curWidth = 0.0F;
                }
                else if (word.equals("TAB"))
                {
                    curWidth += spaceWidth * 5.0F;
                }
                else
                {
                    orb = identifyOrb(word);
                    if (orb == null)
                    {
                        if (!identifyColor(word).equals(Color.WHITE))
                            word = word.substring(2);
                        layout.setText(font, word);
                        if (curWidth + layout.width > lineWidth)
                        {
                            curWidth = layout.width + spaceWidth;
                        }
                        else
                        {
                            curWidth += layout.width + spaceWidth;
                        }
                    }
                    else if (curWidth + CARD_ENERGY_IMG_WIDTH > lineWidth)
                    {
                        curWidth = CARD_ENERGY_IMG_WIDTH + spaceWidth;
                    }
                    else
                    {
                        curWidth += CARD_ENERGY_IMG_WIDTH + spaceWidth;
                    }
                }
            }
            return curWidth;
        }

        public static float getSmartWidth(BitmapFont font, string msg, float lineWidth, float lineSpacing, float scale)
        {
            BitmapFont.BitmapFontData data = font.getData();
            float prevScale = data.scaleX;
            data.setScale(scale);
            float retVal = getSmartWidth(font, msg, lineWidth, lineSpacing);
            data.setScale(prevScale);
            return retVal;
        }

        static TextureAtlas.AtlasRegion identifyOrb(string word)
        {
            switch (word)
            {
                case GameLanguage. "[E]":
                    return (AbstractDungeon.player != null) ? AbstractDungeon.player.getOrb() : AbstractCard.orb_red;
                case GameLanguage. "[R]":
                    return AbstractCard.orb_red;
                case GameLanguage. "[G]":
                    return AbstractCard.orb_green;
                case GameLanguage. "[B]":
                    return AbstractCard.orb_blue;
                case GameLanguage. "[W]":
                    return AbstractCard.orb_purple;
                case GameLanguage. "[C]":
                    return AbstractCard.orb_card;
                case GameLanguage. "[P]":
                    return AbstractCard.orb_potion;
                case GameLanguage. "[T]":
                    return AbstractCard.orb_relic;
                case GameLanguage. "[S]":
                    return AbstractCard.orb_special;
            }

            return null;
        }

        static Color identifyColor(string word)
        {
            if (word.length() > 0 && word.charAt(0) == '#')
            {
                switch (word.charAt(1))
                {
                    case GameLanguage. 'r':
                        return Settings.RED_TEXT_COLOR;
                    case GameLanguage. 'g':
                        return Settings.GREEN_TEXT_COLOR;
                    case GameLanguage. 'b':
                        return Settings.BLUE_TEXT_COLOR;
                    case GameLanguage. 'y':
                        return Settings.GOLD_COLOR;
                    case GameLanguage. 'p':
                        return Settings.PURPLE_COLOR;
                }

                return Color.WHITE;
            }

            return Color.WHITE;
        }

        public static void renderDeckViewTip(SpriteBatch sb, string msg, float y, Color color)
        {
            layout.setText(cardDescFont_N, msg);
            sb.setColor(Settings.TWO_THIRDS_TRANSPARENT_BLACK_COLOR);
            sb.draw(ImageMaster.WHITE_SQUARE_IMG, Settings.WIDTH / 2.0F - layout.width / 2.0F - 12.0F * Settings.scale, y - 24.0F * Settings.scale, layout.width + 24.0F * Settings.scale, 48.0F * Settings.scale);
            renderFontCentered(sb, cardDescFont_N, msg, Settings.WIDTH / 2.0F, y, color);
        }

        public static void renderFontLeftTopAligned(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color c)
        {
            layout.setText(font, msg);
            renderFont(sb, font, msg, x, y, c);
        }

        public static void renderFontCentered(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color c)
        {
            layout.setText(font, msg);
            renderFont(sb, font, msg, x - layout.width / 2.0F, y + layout.height / 2.0F, c);
        }

        public static void renderFontLeft(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color c)
        {
            layout.setText(font, msg);
            renderFont(sb, font, msg, x, y + layout.height / 2.0F, c);
        }

        public static void exampleNonWordWrappedText(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color c, float widthMax, float lineSpacing)
        {
            layout.setText(font, msg, Color.WHITE, 0.0F, -1, false);
            currentLine = 0;
            curWidth = 0.0F;
            for (string word :
            msg.split(" "))
            {
                if (word.length() != 0)
                    if (word.equals("NL"))
                    {
                        curWidth = 0.0F;
                        currentLine++;
                    }
                    else if (word.equals("TAB"))
                    {
                        layout.setText(font, word);
                        curWidth += layout.width;
                    }
                    else if (word.charAt(0) == '[')
                    {
                        orb = identifyOrb(word);
                        if (orb != null)
                        {
                            sb.setColor(new Color(1.0F, 1.0F, 1.0F, c.a));
                            if (CARD_ENERGY_IMG_WIDTH <= widthMax * 2.0F)
                                if (curWidth + CARD_ENERGY_IMG_WIDTH > widthMax)
                                {
                                    sb.draw(orb, x - orb.packedWidth / 2.0F + 14.0F * Settings.scale, y - currentLine * lineSpacing - orb.packedHeight / 2.0F - 38.0F * Settings.scale, orb.packedWidth / 2.0F, orb.packedHeight / 2.0F, orb.packedWidth, orb.packedHeight, Settings.scale, Settings.scale, 0.0F);
                                }
                                else
                                {
                                    sb.draw(orb, x + curWidth - orb.packedWidth / 2.0F + 14.0F * Settings.scale, y - currentLine * lineSpacing - orb.packedHeight / 2.0F - 8.0F * Settings.scale, orb.packedWidth / 2.0F, orb.packedHeight / 2.0F, orb.packedWidth, orb.packedHeight, Settings.scale, Settings.scale, 0.0F);
                                }

                            curWidth += CARD_ENERGY_IMG_WIDTH;
                            if (curWidth > widthMax)
                            {
                                curWidth = CARD_ENERGY_IMG_WIDTH;
                                currentLine++;
                            }
                        }
                    }
                    else if (word.charAt(0) == '#')
                    {
                        layout.setText(font, word.substring(2));
                        switch (word.charAt(1))
                        {
                            case GameLanguage. 'r':
                                word = "[#ff6563]" + word.substring(2) + "[]";
                                break;
                            case GameLanguage. 'g':
                                word = "[#7fff00]" + word.substring(2) + "[]";
                                break;
                            case GameLanguage. 'b':
                                word = "[#87ceeb]" + word.substring(2) + "[]";
                                break;
                            case GameLanguage. 'y':
                                word = "[#efc851]" + word.substring(2) + "[]";
                                break;
                            case GameLanguage. 'p':
                                word = "[#0e82ee]" + word.substring(2) + "[]";
                                break;
                        }

                        curWidth += layout.width;
                        if (curWidth > widthMax)
                        {
                            curWidth = 0.0F;
                            currentLine++;
                            font.draw(sb, word, x + curWidth, y - lineSpacing * currentLine);
                            curWidth = layout.width;
                        }
                        else
                        {
                            font.draw(sb, word, x + curWidth - layout.width, y - lineSpacing * currentLine);
                        }
                    }
                    else
                    {
                        font.setColor(c);
                        for (int i = 0; i < word.length(); i++)
                        {
                            string j = Character.toString(word.charAt(i));
                            layout.setText(font, j);
                            curWidth += layout.width;
                            if (curWidth > widthMax && !j.equals(LocalizedStrings.PERIOD))
                            {
                                curWidth = layout.width;
                                currentLine++;
                            }

                            font.draw(sb, j, x + curWidth - layout.width, y - lineSpacing * currentLine);
                        }
                    }
            }
        }

        public static void renderFontCenteredTopAligned(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color c)
        {
            layout.setText(font, "lL");
            font.setColor(c);
            font.draw(sb, msg, x, y + layout.height / 2.0F, 0.0F, 1, false);
        }

        public static void renderFontCentered(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color c, float scale)
        {
            font.getData().setScale(scale);
            layout.setText(font, msg);
            renderFont(sb, font, msg, x - layout.width / 2.0F, y + layout.height / 2.0F, c);
            font.getData().setScale(1.0F);
        }

        public static void renderFontCentered(SpriteBatch sb, BitmapFont font, string msg, float x, float y)
        {
            layout.setText(font, msg);
            renderFont(sb, font, msg, x - layout.width / 2.0F, y + layout.height / 2.0F, Color.WHITE);
        }

        public static void renderFontCenteredWidth(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color c)
        {
            layout.setText(font, msg);
            renderFont(sb, font, msg, x - layout.width / 2.0F, y, c);
        }

        public static void renderFontCenteredWidth(SpriteBatch sb, BitmapFont font, string msg, float x, float y)
        {
            layout.setText(font, msg);
            renderFont(sb, font, msg, x - layout.width / 2.0F, y, Color.WHITE);
        }

        public static void renderFontCenteredHeight(SpriteBatch sb, BitmapFont font, string msg, float x, float y, float lineWidth, Color c)
        {
            layout.setText(font, msg, c, lineWidth, 1, true);
            font.setColor(c);
            font.draw(sb, msg, x, y + layout.height / 2.0F, lineWidth, 1, true);
        }

        public static void renderFontCenteredHeight(SpriteBatch sb, BitmapFont font, string msg, float x, float y, float lineWidth, Color c, float scale)
        {
            font.getData().setScale(scale);
            layout.setText(font, msg, c, lineWidth, 1, true);
            font.setColor(c);
            font.draw(sb, msg, x, y + layout.height / 2.0F, lineWidth, 1, true);
            font.getData().setScale(1.0F);
        }

        public static void renderFontCenteredHeight(SpriteBatch sb, BitmapFont font, string msg, float x, float y, Color c)
        {
            layout.setText(font, msg);
            renderFont(sb, font, msg, x, y + layout.height / 2.0F, c);
        }

        public static void renderFontCenteredHeight(SpriteBatch sb, BitmapFont font, string msg, float x, float y)
        {
            layout.setText(font, msg);
            renderFont(sb, font, msg, x, y + layout.height / 2.0F, Color.WHITE);
        }

        public static string colorString(string input, string colorValue)
        {
            newMsg.setLength(0);
            for (string word :
            input.split(" "))
            newMsg.append("#").append(colorValue).append(word).append(' ');
            return newMsg.toString().trim();
        }

        public static float getWidth(BitmapFont font, string text, float scale)
        {
            layout.setText(font, text);
            return layout.width * scale;
        }

        public static float getHeight(BitmapFont font, string text, float scale)
        {
            layout.setText(font, text);
            return layout.height * scale;
        }*/
    }
}