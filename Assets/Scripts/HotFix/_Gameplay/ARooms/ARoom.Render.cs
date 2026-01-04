using UnityEngine;

namespace MarbleHero
{
    public partial class ARoom
    {
        // static UIStrings uiStrings = Game.languagePack.getUIString("AbstractRoom");
        // public static string[] TEXT = uiStrings.TEXT;
        // protected Texture mapImg;
        // protected Texture mapImgOutline;

        // public Texture getMapImg() => mapImg;
        // public Texture getMapImgOutline() => mapImgOutline;

        public void eventControllerInput()
        {
            if (!Settings.isControllerMode)
                return;

            if (room.evt == null)
                return;

            if (room.inCombat())
                return;

            /*if (ADungeon.topPanel.selectPotionMode)
                return;

            if (!ADungeon.topPanel.potionUi.isHidden)
                return;

            if (ADungeon.topPanel.potionUi.targetMode)
                return;

            if (player.viewingRelics)
                return;

            if (!RoomEventDialog.optionList.isEmpty())
            {
                bool anyHovered = false;
                int index = 0;
                foreach (LargeDialogOptionButton o in RoomEventDialog.optionList)
                {
                    if (o.hb.hovered)
                    {
                        anyHovered = true;
                        break;
                    }

                    index++;
                }

                if (!anyHovered)
                {
                    Gdx.input.setCursorPosition((int)RoomEventDialog.optionList.get(0).hb.cX, Settings.HEIGHT - (int)RoomEventDialog.optionList.get(0).hb.cY);
                }
                else if (CInputActionSet.down.isJustPressed() || CInputActionSet.altDown.isJustPressed())
                {
                    index++;
                    if (index > RoomEventDialog.optionList.size() - 1)
                        index = 0;
                    Gdx.input.setCursorPosition((int)RoomEventDialog.optionList.get(index).hb.cX, Settings.HEIGHT - (int)RoomEventDialog.optionList.get(index).hb.cY);
                }
                else if (CInputActionSet.up.isJustPressed() || CInputActionSet.altUp.isJustPressed())
                {
                    index--;
                    if (index < 0)
                        index = RoomEventDialog.optionList.size() - 1;
                    Gdx.input.setCursorPosition((int)RoomEventDialog.optionList.get(index).hb.cX, Settings.HEIGHT - (int)RoomEventDialog.optionList.get(index).hb.cY);
                }
            }
            else if (!evt.imageEventText.optionList.isEmpty())
            {
                bool anyHovered = false;
                int index = 0;
                foreach (LargeDialogOptionButton o in evt.imageEventText.optionList)
                {
                    if (o.hb.hovered)
                    {
                        anyHovered = true;
                        break;
                    }

                    index++;
                }

                if (!anyHovered)
                {
                    Gdx.input.setCursorPosition((int)evt.imageEventText.optionList.get(0).hb.cX, Settings.HEIGHT - (int)evt.imageEventText.optionList.get(0).hb.cY);
                }
                else if (CInputActionSet.down.isJustPressed() || CInputActionSet.altDown.isJustPressed())
                {
                    index++;
                    if (index > evt.imageEventText.optionList.size() - 1)
                        index = 0;
                    Gdx.input.setCursorPosition((int)evt.imageEventText.optionList.get(index).hb.cX, Settings.HEIGHT - (int)evt.imageEventText.optionList.get(index).hb.cY);
                }
                else if (CInputActionSet.up.isJustPressed() || CInputActionSet.altUp.isJustPressed())
                {
                    index--;
                    if (index < 0)
                        index = evt.imageEventText.optionList.size() - 1;
                    Gdx.input.setCursorPosition((int)evt.imageEventText.optionList.get(index).hb.cX, Settings.HEIGHT - (int)evt.imageEventText.optionList.get(index).hb.cY);
                }
            }*/
        }

        // public void setMapImg(Texture img, Texture imgOutline)
        // {
        //     mapImg = img;
        //     mapImgOutline = imgOutline;
        // }

        public void playBGM(string key)
        {
            //music.playTempBGM(key);
        }

        public void playBgmInstantly(string key)
        {
            //music.playTempBgmInstantly(key);
        }

        /*
        public void render(SpriteBatch sb)
        {
            if (this is EventRoom || this is VictoryRoom)
            {
                if (evt != null && (evt is not AbstractImageEvent || evt.combatTime))
                {
                    evt.renderRoomEventPanel(sb);
                    if (ADungeon.screen != CurrentScreen.VICTORY)
                        player.render(sb);
                }
            }
            else if (ADungeon.screen != CurrentScreen.BOSS_REWARD)
            {
                player.render(sb);
            }

            if (room is not RestRoom)
            {
                if (monsters != null && ADungeon.screen != CurrentScreen.DEATH)
                    monsters.render(sb);
                if (inCombat())
                    player.renderPlayerBattleUi(sb);
                foreach (var i in potions)
                {
                    if (!i.isObtained)
                        i.render(sb);
                }
            }

            foreach (var r in relics)
                r.render(sb);
            renderTips(sb);
        }
        */

        /*
        public void renderAboveTopPanel(SpriteBatch sb)
        {
            foreach (var i in potions)
            {
                if (i.isObtained)
                    i.render(sb);
            }

            souls.render(sb);
            if (Settings.isInfo)
            {
                string msg = "[GAME MODE DATA]\n isDaily: " + Settings.isDailyRun + "\n isSpecialSeed: " + Settings.isTrial + "\n isAscension: " + ADungeon.isAscensionMode + "\n\n[CARDGROUPS]\n Deck: " + player.masterDeck.size() + "\n Draw Pile: " + player.drawPile.size() + "\n Discard Pile: " + player.discardPile.size() + "\n Exhaust Pile: " + player.exhaustPile.size() + "\n\n[ACTION MANAGER]\n Phase: " + actionManager.phase.name() + "\n turnEnded: " + actionManager.turnHasEnded + "\n numTurns: " + GameActionManager.turn + "\n\n[Misc]\n Publisher Connection: " + Game.publisherIntegration.isInitialized() + "\n CUR_SCREEN: " + ADungeon.screen.name() + "\n Controller Mode: " + Settings.isControllerMode + "\n isFadingOut: " + ADungeon.isFadingOut + "\n isScreenUp: " + ADungeon.isScreenUp + "\n Particle Count: " + ADungeon.effectList.size();
                FontHelper.renderFontCenteredHeight(sb, FontHelper.tipBodyFont, msg, 30.0F, Settings.HEIGHT * 0.5F, Color.WHITE);
            }
        }
        */

        /*public void renderTips(SpriteBatch sb)
        {
        }*/

        /*
        public void renderEventTexts(SpriteBatch sb)
        {
            if (evt != null)
                evt.renderText(sb);
        }
        */
    }
}