namespace MoreMountains
{
    public class ShopRoom : ARoom
    {
        public override RoomType Type => RoomType.SHOP;
        
        // private static UIStrings uiStrings = Game.languagePack.getUIString("ShopRoom");
        // public static String[] TEXT = uiStrings.TEXT;
        public int shopRarityBonus = 6;
        // public Merchant merchant;

        public ShopRoom()
        {
            phase = RoomPhase.COMPLETE;
            // merchant = null;
            mapSymbol = "$";
            // mapImg = ImageMaster.MAP_NODE_MERCHANT;
            // mapImgOutline = ImageMaster.MAP_NODE_MERCHANT_OUTLINE;
            baseRareCardChance = 9;
            baseUncommonCardChance = 37;
        }

        // public void setMerchant(Merchant merc)
        // {
        // merchant = merc;
        // }

        public override void onPlayerEntry(APlayer p)
        {
            if (ADungeon.id != ("TheEnding"))
                playBGM("SHOP");
            // ADungeon.overlayMenu.proceedButton.setLabel(TEXT[0]);
            // setMerchant(new Merchant());
        }

        public override CardRarity getCardRarity(int roll)
        {
            return getCardRarity(roll, false);
        }

        public override void update(float dt)
        {
            base.update(dt);
            // merchant?.update();
            updatePurge();
        }

        private void updatePurge()
        {
            // if (!ADungeon.gridSelectScreen.selectedCards.isEmpty())
            // {
            //     ShopScreen.purgeCard();
            //     foreach (ACard card in ADungeon.gridSelectScreen.selectedCards)
            //     {
            //         metricData.addPurgedItem(card.getMetricID());
            //         ADungeon.topLevelEffects.add(new PurgeCardEffect(card, Settings.WIDTH / 2.0F, Settings.HEIGHT / 2.0F));
            //         player.masterDeck.removeCard(card);
            //     }
            //     ADungeon.gridSelectScreen.selectedCards.clear();
            //     ADungeon.shopScreen.purgeAvailable = false;
            // }
        }

        // public override void render(SpriteBatch sb)
        // {
        //     merchant?.render(sb);
        //     base.render(sb);
        //     renderTips(sb);
        // }

        public override void Dispose()
        {
            base.Dispose();
            // merchant?.dispose();
            // merchant = null;
        }
    }
}