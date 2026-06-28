using System;
using UnityEngine;

namespace MarbleHero
{
    public partial class ARelic
    {
        // static TutorialStrings tutorialStrings = Game.languagePack.getTutorialString("Relic Tip");
        // public static string[] MSG = tutorialStrings.TEXT;
        // public static string[] LABEL = tutorialStrings.LABEL;
        // public static string USED_UP_MSG = MSG[2];
        RelicStrings relicStrings;
        public string[] DESCRIPTIONS;
        // public List<PowerTip> tips = new();
        // FloatyEffect f_effect = new FloatyEffect(10.0F, 0.2F);
        // public Hitbox hb = new Hitbox(PAD_X, PAD_X);

        float animationTimer;
        float glowTimer;
        public float flashTimer;

        public static string IMG_DIR = "images/relics/";
        public static string OUTLINE_DIR = "images/relics/outline/";
        static string L_IMG_DIR = "images/largeRelics/";
        public string imgUrl;
        public static int RAW_W = 128;
        public static int relicPage = 0;
        static float offsetX = 0.0F;
        public static int MAX_RELICS_PER_PAGE = (int)(Settings.WIDTH / 75.0F * Settings.scale);
        public float currentX;
        public float currentY;
        public float targetX;
        public float targetY;
        static float START_X = 64.0F * Settings.scale;
        static float START_Y = Settings.isMobile ? (Settings.HEIGHT - 132.0F * Settings.scale) : (Settings.HEIGHT - 102.0F * Settings.scale);
        public static float PAD_X = 72.0F * Settings.scale;
        public static Color PASSIVE_OUTLINE_COLOR = new Color(0.0F, 0.0F, 0.0F, 0.33F);
        Color flashColor = new Color(1.0F, 1.0F, 1.0F, 0.0F);
        Color goldOutlineColor = new Color(1.0F, 0.9F, 0.4F, 0.0F);

        /*
        protected void initializeTips()
        {
            Scanner desc = new Scanner(description);
            while (desc.hasNext())
            {
                string s = desc.next();
                if (s.charAt(0) == '#')
                    s = s.substring(2);
                s = s.replace(',', ' ');
                s = s.replace('.', ' ');
                s = s.trim();
                s = s.toLowerCase();
                bool alreadyExists = false;
                if (GameDictionary.keywords.containsKey(s))
                {
                    s = GameDictionary.parentWord.get(s);
                    foreach (PowerTip t in tips)
                    {
                        if (t.header.toLowerCase().equals(s))
                        {
                            alreadyExists = true;
                            break;
                        }
                    }

                    if (!alreadyExists)
                        tips.Add(new PowerTip(TipHelper.capitalize(s), GameDictionary.keywords.get(s)));
                }
            }

            desc.close();
        }
        */

        public void update(float dt)
        {
            updateFlash(dt);
            if (!isDone)
            {
                if (isAnimating)
                {
                    glowTimer -= dt;
                    if (glowTimer < 0.0F)
                    {
                        glowTimer = 0.5F;
                        // ADungeon.effectList.Add(new GlowRelicParticle(img, currentX + f_effect.x, currentY + f_effect.y, rotation));
                    }

                    // f_effect.update();
                    // if (hb.hovered)
                        // scale = Settings.scale * 1.5F;
                    // else
                        // scale = MathHelper.scaleLerpSnap(scale, Settings.scale * 1.1F);
                }
                // else if (hb.hovered)
                    // scale = Settings.scale * 1.25F;
                // else
                    // scale = MathHelper.scaleLerpSnap(scale, Settings.scale);

                if (isObtained)
                {
                    if (rotation != 0.0F)
                        rotation = MathUtils.lerp(rotation, 0.0F, dt * 6.0F * 2.0F);
                    
                    if (currentX != targetX)
                    {
                        currentX = MathUtils.lerp(currentX, targetX, dt * 6.0F);
                        if (Math.Abs(currentX - targetX) < 0.5F)
                            currentX = targetX;
                    }

                    if (currentY != targetY)
                    {
                        currentY = MathUtils.lerp(currentY, targetY, dt * 6.0F);
                        if (Math.Abs(currentY - targetY) < 0.5F)
                            currentY = targetY;
                    }

                    if (currentY == targetY && currentX == targetX)
                    {
                        isDone = true;
                        // if (ADungeon.topPanel != null)
                            // ADungeon.topPanel.adjustRelicHbs();
                        
                        // hb.move(currentX, currentY);
                        // if (tier == RelicTier.BOSS && room is TreasureRoomBoss)
                            // ADungeon.overlayMenu.proceedButton.show();
                            
                        onEquip(player);
                    }

                    scale = Settings.scale;
                }

                // if (hb != null)
                // {
                //     hb.update();
                //     if (hb.hovered && (!ADungeon.isScreenUp || ADungeon.screen == ADungeon.CurrentScreen.BOSS_REWARD) && ADungeon.screen != ADungeon.CurrentScreen.NEOW_UNLOCK)
                //     {
                //         if (InputHelper.justClickedLeft && !isObtained)
                //         {
                //             InputHelper.justClickedLeft = false;
                //             hb.clickStarted = true;
                //         }
                //
                //         if ((hb.clicked || CInputActionSet.select.isJustPressed()) && !isObtained)
                //         {
                //             CInputActionSet.select.unpress();
                //             hb.clicked = false;
                //             if (!Settings.isTouchScreen)
                //             {
                //                 bossObtainLogic();
                //             }
                //             else
                //             {
                //                 ADungeon.bossRelicScreen.confirmButton.show();
                //                 ADungeon.bossRelicScreen.confirmButton.isDisabled = false;
                //                 ADungeon.bossRelicScreen.touchRelic = this;
                //             }
                //         }
                //     }
                // }

                // if (ADungeon.screen == ADungeon.CurrentScreen.BOSS_REWARD)
                    // updateAnimation(dt);
            }
            else
            {
                // if (player != null && player.relics.IndexOf(this) / MAX_RELICS_PER_PAGE == relicPage)
                //     hb.update();
                // else
                //     hb.hovered = false;
                //
                // if (hb.hovered && ADungeon.topPanel.potionUi.isHidden)
                // {
                //     scale = Settings.scale * 1.25F;
                //     Game.cursor.changeType(GameCursor.CursorType.INSPECT);
                // }
                // else
                // {
                //     scale = MathHelper.scaleLerpSnap(scale, Settings.scale);
                // }

                updateRelicPopupClick();
            }
        }

        public void playLandingSFX()
        {
            // switch (landingSFX)
            // {
            //     case LandingSound.CLINK:
            //         Game.sound.play("RELIC_DROP_CLINK");
            //         return;
            //     case LandingSound.FLAT:
            //         Game.sound.play("RELIC_DROP_FLAT");
            //         return;
            //     case LandingSound.SOLID:
            //         Game.sound.play("RELIC_DROP_ROCKY");
            //         return;
            //     case LandingSound.HEAVY:
            //         Game.sound.play("RELIC_DROP_HEAVY");
            //         return;
            //     case LandingSound.MAGICAL:
            //         Game.sound.play("RELIC_DROP_MAGICAL");
            //         return;
            // }
            //
            // Game.sound.play("RELIC_DROP_CLINK");
        }

        void updateRelicPopupClick()
        {
            // if (hb.hovered && InputHelper.justClickedLeft)
            //     hb.clickStarted = true;
            // if (hb.clicked || (hb.hovered && CInputActionSet.select.isJustPressed()))
            // {
            //     Game.relicPopup.open(this, player.relics);
            //     CInputActionSet.select.unpress();
            //     hb.clicked = false;
            //     hb.clickStarted = false;
            // }
        }

        public void updateDescription(APlayer.PlayerClass c)
        {
        }

        public string getUpdatedDescription()
        {
            return "";
        }

        protected void updateAnimation(float dt)
        {
            if (animationTimer != 0.0F)
            {
                animationTimer -= dt;
                if (animationTimer < 0.0F)
                    animationTimer = 0.0F;
            }
        }

        void updateFlash(float dt)
        {
            if (flashTimer != 0.0F)
            {
                flashTimer -= dt;
                flashTimer = flashTimer switch
                {
                    < 0.0F when pulse => 1.0F,
                    < 0.0F => 0.0F,
                    _ => flashTimer
                };
            }
        }

        public void loadLargeImg()
        {
            // if (largeImg == null)
            // largeImg = ImageMaster.loadImage("images/largeRelics/" + imgUrl);
        }

        // public void renderInTopPanel(SpriteBatch sb)
        // {
        //     if (Settings.hideRelics)
        //         return;
        //     renderOutline(sb, true);
        //     if (grayscale)
        //         ShaderHelper.setShader(sb, ShaderHelper.Shader.GRAYSCALE);
        //     sb.setColor(Color.WHITE);
        //     sb.draw(img, currentX - 64.0F + offsetX, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //     if (grayscale)
        //         ShaderHelper.setShader(sb, ShaderHelper.Shader.DEFAULT);
        //     renderCounter(sb, true);
        //     renderFlash(sb, true);
        //     hb.render(sb);
        // }
        //
        // public void render(SpriteBatch sb)
        // {
        //     if (Settings.hideRelics)
        //         return;
        //     renderOutline(sb, false);
        //     if (!isObtained && (!ADungeon.isScreenUp || ADungeon.screen == ADungeon.CurrentScreen.BOSS_REWARD || ADungeon.screen == ADungeon.CurrentScreen.SHOP))
        //     {
        //         if (hb.hovered)
        //             renderBossTip(sb);
        //         if (ADungeon.screen == ADungeon.CurrentScreen.BOSS_REWARD)
        //             if (hb.hovered)
        //             {
        //                 sb.setColor(PASSIVE_OUTLINE_COLOR);
        //                 sb.draw(outlineImg, currentX - 64.0F + f_effect.x, currentY - 64.0F + f_effect.y, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //             }
        //             else
        //             {
        //                 sb.setColor(PASSIVE_OUTLINE_COLOR);
        //                 sb.draw(outlineImg, currentX - 64.0F + f_effect.x, currentY - 64.0F + f_effect.y, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //             }
        //     }
        //
        //     if (ADungeon.screen == ADungeon.CurrentScreen.BOSS_REWARD)
        //     {
        //         if (!isObtained)
        //         {
        //             sb.setColor(Color.WHITE);
        //             sb.draw(img, currentX - 64.0F + f_effect.x, currentY - 64.0F + f_effect.y, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //         }
        //         else
        //         {
        //             sb.setColor(Color.WHITE);
        //             sb.draw(img, currentX - 64.0F, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //             renderCounter(sb, false);
        //         }
        //     }
        //     else
        //     {
        //         sb.setColor(Color.WHITE);
        //         sb.draw(img, currentX - 64.0F, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //         renderCounter(sb, false);
        //     }
        //
        //     if (isDone)
        //         renderFlash(sb, false);
        //     hb.render(sb);
        // }
        //
        // public void renderLock(SpriteBatch sb, Color outlineColor)
        // {
        //     sb.setColor(outlineColor);
        //     sb.draw(ImageMaster.RELIC_LOCK_OUTLINE, currentX - 64.0F, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //     sb.setColor(Color.WHITE);
        //     sb.draw(ImageMaster.RELIC_LOCK, currentX - 64.0F, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //     if (hb.hovered)
        //     {
        //         string unlockReq = UnlockTracker.unlockReqs.get(relicId);
        //         if (unlockReq == null)
        //             unlockReq = "Missing unlock req.";
        //         unlockReq = LABEL[2];
        //         if (InputHelper.mX < 1400.0F * Settings.scale)
        //         {
        //             if (Game.mainMenuScreen.screen == MainMenuScreen.CurScreen.RELIC_VIEW && InputHelper.mY < Settings.HEIGHT / 5.0F)
        //             {
        //                 TipHelper.renderGenericTip(InputHelper.mX + 60.0F * Settings.scale, InputHelper.mY + 100.0F * Settings.scale, LABEL[3], unlockReq);
        //             }
        //             else
        //             {
        //                 TipHelper.renderGenericTip(InputHelper.mX + 60.0F * Settings.scale, InputHelper.mY - 50.0F * Settings.scale, LABEL[3], unlockReq);
        //             }
        //         }
        //         else
        //         {
        //             TipHelper.renderGenericTip(InputHelper.mX - 350.0F * Settings.scale, InputHelper.mY - 50.0F * Settings.scale, LABEL[3], unlockReq);
        //         }
        //
        //         float tmpX = currentX;
        //         float tmpY = currentY;
        //         if (ADungeon.screen == ADungeon.CurrentScreen.BOSS_REWARD)
        //         {
        //             tmpX += f_effect.x;
        //             tmpY += f_effect.y;
        //         }
        //
        //         sb.setColor(Color.WHITE);
        //         sb.draw(ImageMaster.RELIC_LOCK, tmpX - 64.0F, tmpY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //     }
        //
        //     hb.render(sb);
        // }
        //
        // public void render(SpriteBatch sb, bool renderAmount, Color outlineColor)
        // {
        //     if (isSeen)
        //     {
        //         renderOutline(outlineColor, sb, false);
        //     }
        //     else
        //     {
        //         renderOutline(Color.LIGHT_GRAY, sb, false);
        //     }
        //
        //     if (isSeen)
        //     {
        //         sb.setColor(Color.WHITE);
        //     }
        //     else if (hb.hovered)
        //     {
        //         sb.setColor(Settings.HALF_TRANSPARENT_BLACK_COLOR);
        //     }
        //     else
        //     {
        //         sb.setColor(Color.BLACK);
        //     }
        //
        //     if (ADungeon.screen != null && ADungeon.screen == ADungeon.CurrentScreen.NEOW_UNLOCK)
        //     {
        //         if (largeImg == null)
        //         {
        //             sb.draw(img, currentX - 64.0F, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, Settings.scale * 2.0F +
        //                                                                                            MathUtils.cosDeg((float)(System.currentTimeMillis() / 5L % 360L)) / 15.0F, Settings.scale * 2.0F +
        //                                                                                                                                                                       MathUtils.cosDeg((float)(System.currentTimeMillis() / 5L % 360L)) / 15.0F, rotation, 0, 0, 128, 128, false, false);
        //         }
        //         else
        //         {
        //             sb.draw(largeImg, currentX - 128.0F, currentY - 128.0F, 128.0F, 128.0F, 256.0F, 256.0F, Settings.scale +
        //                                                                                                     MathUtils.cosDeg((float)(System.currentTimeMillis() / 5L % 360L)) / 30.0F, Settings.scale +
        //                                                                                                                                                                                MathUtils.cosDeg((float)(System.currentTimeMillis() / 5L % 360L)) / 30.0F, rotation, 0, 0, 256, 256, false, false);
        //         }
        //     }
        //     else
        //     {
        //         sb.draw(img, currentX - 64.0F, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //         if (relicId.equals("Circlet"))
        //             renderCounter(sb, false);
        //     }
        //
        //     if (hb.hovered && !Game.relicPopup.isOpen)
        //     {
        //         if (!isSeen)
        //         {
        //             if (InputHelper.mX < 1400.0F * Settings.scale)
        //             {
        //                 TipHelper.renderGenericTip(InputHelper.mX + 60.0F * Settings.scale, InputHelper.mY - 50.0F * Settings.scale, LABEL[1], MSG[1]);
        //             }
        //             else
        //             {
        //                 TipHelper.renderGenericTip(InputHelper.mX - 350.0F * Settings.scale, InputHelper.mY - 50.0F * Settings.scale, LABEL[1], MSG[1]);
        //             }
        //
        //             return;
        //         }
        //
        //         renderTip(sb);
        //     }
        //
        //     hb.render(sb);
        // }
        //
        // public void renderWithoutAmount(SpriteBatch sb, Color c)
        // {
        //     renderOutline(c, sb, false);
        //     sb.setColor(Color.WHITE);
        //     sb.draw(img, currentX - 64.0F, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //     if (hb.hovered)
        //     {
        //         renderTip(sb);
        //         float tmpX = currentX;
        //         float tmpY = currentY;
        //         if (ADungeon.screen == ADungeon.CurrentScreen.BOSS_REWARD)
        //         {
        //             tmpX += f_effect.x;
        //             tmpY += f_effect.y;
        //         }
        //
        //         sb.setColor(Color.WHITE);
        //         sb.draw(img, tmpX - 64.0F, tmpY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //     }
        //
        //     hb.render(sb);
        // }
        //
        // public void renderCounter(SpriteBatch sb, bool inTopPanel)
        // {
        //     if (counter > -1)
        //         if (inTopPanel)
        //         {
        //             FontHelper.renderFontRightTopAligned(sb, FontHelper.topPanelInfoFont,
        //                 Integer.toString(counter), offsetX + currentX + 30.0F * Settings.scale, currentY - 7.0F * Settings.scale, Color.WHITE);
        //         }
        //         else
        //         {
        //             FontHelper.renderFontRightTopAligned(sb, FontHelper.topPanelInfoFont,
        //                 Integer.toString(counter), currentX + 30.0F * Settings.scale, currentY - 7.0F * Settings.scale, Color.WHITE);
        //         }
        // }
        //
        // public void renderOutline(Color c, SpriteBatch sb, bool inTopPanel)
        // {
        //     sb.setColor(c);
        //     if (ADungeon.screen != null && ADungeon.screen == ADungeon.CurrentScreen.NEOW_UNLOCK)
        //     {
        //         sb.draw(outlineImg, currentX - 64.0F, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, Settings.scale * 2.0F +
        //                                                                                               MathUtils.cosDeg((float)(System.currentTimeMillis() / 5L % 360L)) / 15.0F, Settings.scale * 2.0F +
        //                                                                                                                                                                          MathUtils.cosDeg((float)(System.currentTimeMillis() / 5L % 360L)) / 15.0F, rotation, 0, 0, 128, 128, false, false);
        //     }
        //     else if (hb.hovered && Settings.isControllerMode)
        //     {
        //         sb.setBlendFunction(770, 1);
        //         goldOutlineColor.a = 0.6F + MathUtils.cosDeg((float)(System.currentTimeMillis() / 2L % 360L)) / 5.0F;
        //         sb.setColor(goldOutlineColor);
        //         sb.draw(outlineImg, currentX - 64.0F, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //         sb.setBlendFunction(770, 771);
        //     }
        //     else
        //     {
        //         sb.draw(outlineImg, currentX - 64.0F, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //     }
        // }
        //
        // public void renderOutline(SpriteBatch sb, bool inTopPanel)
        // {
        //     float tmpX = currentX - 64.0F;
        //     if (inTopPanel)
        //         tmpX += offsetX;
        //     if (hb.hovered && Settings.isControllerMode)
        //     {
        //         sb.setBlendFunction(770, 1);
        //         goldOutlineColor.a = 0.6F + MathUtils.cosDeg((float)(System.currentTimeMillis() / 2L % 360L)) / 5.0F;
        //         sb.setColor(goldOutlineColor);
        //         sb.draw(outlineImg, tmpX, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //         sb.setBlendFunction(770, 771);
        //     }
        //     else
        //     {
        //         sb.setColor(PASSIVE_OUTLINE_COLOR);
        //         sb.draw(outlineImg, tmpX, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale, scale, rotation, 0, 0, 128, 128, false, false);
        //     }
        // }
        //
        // public void renderFlash(SpriteBatch sb, bool inTopPanel)
        // {
        //     float tmp = Interpolation.exp10In.apply(0.0F, 4.0F, flashTimer / 2.0F);
        //     sb.setBlendFunction(770, 1);
        //     flashColor.a = flashTimer * 0.2F;
        //     sb.setColor(flashColor);
        //     float tmpX = currentX - 64.0F;
        //     if (inTopPanel)
        //         tmpX += offsetX;
        //     sb.draw(img, tmpX, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale + tmp, scale + tmp, rotation, 0, 0, 128, 128, false, false);
        //     sb.draw(img, tmpX, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale + tmp * 0.66F, scale + tmp * 0.66F, rotation, 0, 0, 128, 128, false, false);
        //     sb.draw(img, tmpX, currentY - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, scale + tmp / 3.0F, scale + tmp / 3.0F, rotation, 0, 0, 128, 128, false, false);
        //     sb.setBlendFunction(770, 771);
        // }
        //
        public void beginPulse()
        {
            flashTimer = 1.0F;
        }

        public void beginLongPulse()
        {
            flashTimer = 1.0F;
            pulse = true;
        }

        public void stopPulse()
        {
            pulse = false;
        }

        public void flash()
        {
            flashTimer = 2.0F;
        }
        //
        // public void renderBossTip(SpriteBatch sb)
        // {
        //     TipHelper.queuePowerTips(Settings.WIDTH * 0.63F, Settings.HEIGHT * 0.63F, tips);
        // }
        //
        // public void renderTip(SpriteBatch sb)
        // {
        //     if (InputHelper.mX < 1400.0F * Settings.scale)
        //     {
        //         if (Game.mainMenuScreen.screen == MainMenuScreen.CurScreen.RELIC_VIEW)
        //         {
        //             TipHelper.queuePowerTips(180.0F * Settings.scale, Settings.HEIGHT * 0.7F, tips);
        //         }
        //         else if (ADungeon.screen == ADungeon.CurrentScreen.SHOP && tips.Count > 2 &&
        //                  !player.hasRelic(relicId))
        //         {
        //             TipHelper.queuePowerTips(InputHelper.mX + 60.0F * Settings.scale, InputHelper.mY + 180.0F * Settings.scale, tips);
        //         }
        //         else if (player != null && player.hasRelic(relicId))
        //         {
        //             TipHelper.queuePowerTips(InputHelper.mX + 60.0F * Settings.scale, InputHelper.mY - 30.0F * Settings.scale, tips);
        //         }
        //         else if (ADungeon.screen == ADungeon.CurrentScreen.COMBAT_REWARD)
        //         {
        //             TipHelper.queuePowerTips(360.0F * Settings.scale, InputHelper.mY + 50.0F * Settings.scale, tips);
        //         }
        //         else
        //         {
        //             TipHelper.queuePowerTips(InputHelper.mX + 50.0F * Settings.scale, InputHelper.mY + 50.0F * Settings.scale, tips);
        //         }
        //     }
        //     else
        //     {
        //         TipHelper.queuePowerTips(InputHelper.mX - 350.0F * Settings.scale, InputHelper.mY - 50.0F * Settings.scale, tips);
        //     }
        // }
    }
}