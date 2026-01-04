using UnityEngine;

namespace MarbleHero
{
    public class DungeonMap
    {
        static Texture top;
        static Texture mid;
        static Texture bot;
        static Texture blend;
        public static Texture boss;
        public static Texture bossOutline;
        public float targetAlpha;
        static Color NOT_TAKEN_COLOR = new Color(0.34F, 0.34F, 0.34F, 1.0F);
        Color bossNodeColor = NOT_TAKEN_COLOR;
        Color baseMapColor = new Color(1.0F, 1.0F, 1.0F, 0.0F);
        float mapMidDist;
        static float mapOffsetY;
        static float BOSS_W = Settings.isMobile ? (560.0F * Settings.scale) : (512.0F * Settings.scale);
        static float BOSS_OFFSET_Y = 1416.0F * Settings.scale;
        static float H = 1020.0F * Settings.scale;

        static float BLEND_H = 512.0F * Settings.scale;

        // public Hitbox bossHb;
        public bool atBoss = false;
        Color reticleColor = new Color(1.0F, 1.0F, 1.0F, 0.0F);
        public Legend legend = new Legend();

        public DungeonMap()
        {
            if (top == null)
            {
                // top = ImageMaster.loadImage("images/ui/map/mapTop.png");
                // mid = ImageMaster.loadImage("images/ui/map/mapMid.png");
                // bot = ImageMaster.loadImage("images/ui/map/mapBot.png");
                // blend = ImageMaster.loadImage("images/ui/map/mapBlend.png");
            }

            // bossHb = new Hitbox(400.0F * Settings.scale, 360.0F * Settings.scale);
        }

        public void update(float dt)
        {
            legend.update(baseMapColor.a, (ADungeon.screen == CurrentScreen.MAP), dt);
            baseMapColor.a = MathHelper.fadeLerpSnap(baseMapColor.a, targetAlpha, dt);
            // bossHb.move(Settings.WIDTH / 2.0F, DungeonMapScreen.offsetY + mapOffsetY + BOSS_OFFSET_Y + BOSS_W / 2.0F);
            // bossHb.update();
            updateReticle();
            if (room.isCompleted() && ADungeon.screen == CurrentScreen.MAP && (Settings.isDebug || mapNode.y == 14 || (ADungeon.id == "TheEnding" && mapNode.y == 2)))
            {
                if ( /*bossHb.hovered && (InputHelper.justClickedLeft || CInputActionSet.select.isJustPressed())*/ true)
                {
                    mapNode.markAsTaken();
                    var node2 = mapNode;
                    foreach (var e in node2.edges)
                        e?.markAsTaken();

                    // InputHelper.justClickedLeft = false;
                    // music.fadeOutTempBGM();
                    MapRoomNode node = new MapRoomNode(-1, 15, new MonsterRoomBoss());
                    ADungeon.nextRoom = node;
                    if (ADungeon.path.Count > 1)
                    {
                        var (x, y) = ADungeon.path[^1];
                        ADungeon.path.Add((x: x, y + 1));
                    }
                    else
                    {
                        ADungeon.path.Add((1, 15));
                    }

                    ADungeon.nextRoomTransitionStart();
                    // bossHb.hovered = false;
                }
            }

            if ( /*bossHb.hovered || */atBoss)
            {
                bossNodeColor = MapRoomNode.AVAILABLE_COLOR;
            }
            else
            {
                bossNodeColor = Color.Lerp(bossNodeColor, NOT_TAKEN_COLOR, Time.deltaTime * 8.0F);
            }

            bossNodeColor.a = baseMapColor.a;
        }

        void updateReticle()
        {
            if (!Settings.isControllerMode)
                return;

            // if (bossHb.hovered)
            //     reticleColor.a = 1.0F;
            // else
            //     reticleColor.a = 0.0F;
        }

        float calculateMapSize()
        {
            if (ADungeon.id == "TheEnding")
                return Settings.MAP_DST_Y * 4.0F - 1380.0F * Settings.scale;

            return Settings.MAP_DST_Y * 16.0F - 1380.0F * Settings.scale;
        }

        public void show()
        {
            targetAlpha = 1.0F;
            mapMidDist = calculateMapSize();
            mapOffsetY = mapMidDist - 120.0F * Settings.scale;
        }

        public void hide()
        {
            targetAlpha = 0.0F;
        }

        public void hideInstantly()
        {
            targetAlpha = 0.0F;
            baseMapColor.a = 0.0F;
            legend.c.a = 0.0F;
        }

        // public void render(SpriteBatch sb)
        // {
        //     if (ADungeon.id != "TheEnding")
        //     {
        //         renderNormalMap(sb);
        //     }
        //     else
        //     {
        //         renderFinalActMap(sb);
        //     }
        // }

        // void renderNormalMap(SpriteBatch sb)
        // {
        //     sb.setColor(baseMapColor);
        //     if (!Settings.isMobile)
        //     {
        //         sb.draw(top, 0.0F, H + DungeonMapScreen.offsetY + mapOffsetY, Settings.WIDTH, 1080.0F * Settings.scale);
        //     }
        //     else
        //     {
        //         sb.draw(top, -Settings.WIDTH * 0.05F, H + DungeonMapScreen.offsetY + mapOffsetY, Settings.WIDTH * 1.1F, 1080.0F * Settings.scale);
        //     }
        //
        //     renderMapCenters(sb);
        //     if (!Settings.isMobile)
        //     {
        //         sb.draw(bot, 0.0F, -mapMidDist + DungeonMapScreen.offsetY + mapOffsetY + 1.0F, Settings.WIDTH, 1080.0F * Settings.scale);
        //     }
        //     else
        //     {
        //         sb.draw(bot, -Settings.WIDTH * 0.05F, -mapMidDist + DungeonMapScreen.offsetY + mapOffsetY + 1.0F, Settings.WIDTH * 1.1F, 1080.0F * Settings.scale);
        //     }
        //
        //     renderMapBlender(sb);
        //     legend.render(sb);
        // }

        // void renderFinalActMap(SpriteBatch sb)
        // {
        //     sb.setColor(baseMapColor);
        //     if (!Settings.isMobile)
        //     {
        //         sb.draw(top, 0.0F, H + DungeonMapScreen.offsetY + mapOffsetY, Settings.WIDTH, 1080.0F * Settings.scale);
        //         sb.draw(bot, 0.0F, -mapMidDist + DungeonMapScreen.offsetY + mapOffsetY + 1.0F, Settings.WIDTH, 1080.0F * Settings.scale);
        //     }
        //     else
        //     {
        //         sb.draw(top, -Settings.WIDTH * 0.05F, H + DungeonMapScreen.offsetY + mapOffsetY, Settings.WIDTH * 1.1F, 1080.0F * Settings.scale);
        //         sb.draw(bot, -Settings.WIDTH * 0.05F, -mapMidDist + DungeonMapScreen.offsetY + mapOffsetY + 1.0F, Settings.WIDTH * 1.1F, 1080.0F * Settings.scale);
        //     }
        //
        //     renderMapBlender(sb);
        //     legend.render(sb);
        // }

        // public void renderBossIcon(SpriteBatch sb)
        // {
        //     if (boss != null)
        //     {
        //         sb.setColor(new Color(1.0F, 1.0F, 1.0F, bossNodeColor.a));
        //         if (!Settings.isMobile)
        //         {
        //             sb.draw(bossOutline, Settings.WIDTH / 2.0F - BOSS_W / 2.0F, DungeonMapScreen.offsetY + mapOffsetY + BOSS_OFFSET_Y, BOSS_W, BOSS_W);
        //             sb.setColor(bossNodeColor);
        //             sb.draw(boss, Settings.WIDTH / 2.0F - BOSS_W / 2.0F, DungeonMapScreen.offsetY + mapOffsetY + BOSS_OFFSET_Y, BOSS_W, BOSS_W);
        //         }
        //         else
        //         {
        //             sb.draw(bossOutline, Settings.WIDTH / 2.0F - BOSS_W / 2.0F, DungeonMapScreen.offsetY + mapOffsetY + BOSS_OFFSET_Y, BOSS_W, BOSS_W);
        //             sb.setColor(bossNodeColor);
        //             sb.draw(boss, Settings.WIDTH / 2.0F - BOSS_W / 2.0F, DungeonMapScreen.offsetY + mapOffsetY + BOSS_OFFSET_Y, BOSS_W, BOSS_W);
        //         }
        //     }
        //
        //     if (ADungeon.screen == CurrentScreen.MAP)
        //     {
        //         bossHb.render(sb);
        //         if (Settings.isControllerMode && ADungeon.dungeonMapScreen.map.bossHb.hovered)
        //             renderReticle(sb, ADungeon.dungeonMapScreen.map.bossHb);
        //     }
        // }
        //
        // void renderMapCenters(SpriteBatch sb)
        // {
        //     if (!Settings.isMobile)
        //     {
        //         sb.draw(mid, 0.0F, DungeonMapScreen.offsetY + mapOffsetY, Settings.WIDTH, 1080.0F * Settings.scale);
        //     }
        //     else
        //     {
        //         sb.draw(mid, -Settings.WIDTH * 0.05F, DungeonMapScreen.offsetY + mapOffsetY, Settings.WIDTH * 1.1F, 1080.0F * Settings.scale);
        //     }
        // }
        //
        // public void renderReticle(SpriteBatch sb, Hitbox hb)
        // {
        //     float offset = Interpolation.fade.apply(24.0F * Settings.scale, 12.0F * Settings.scale, reticleColor.a);
        //     sb.setColor(reticleColor);
        //     renderReticleCorner(sb, -hb.width / 2.0F + offset, hb.height / 2.0F - offset, hb, false, false);
        //     renderReticleCorner(sb, hb.width / 2.0F - offset, hb.height / 2.0F - offset, hb, true, false);
        //     renderReticleCorner(sb, -hb.width / 2.0F + offset, -hb.height / 2.0F + offset, hb, false, true);
        //     renderReticleCorner(sb, hb.width / 2.0F - offset, -hb.height / 2.0F + offset, hb, true, true);
        // }
        //
        // void renderReticleCorner(SpriteBatch sb, float x, float y, Hitbox hb, bool flipX, bool flipY)
        // {
        //     sb.draw(ImageMaster.RETICLE_CORNER, hb.cX + x - 18.0F, hb.cY + y - 18.0F, 18.0F, 18.0F, 36.0F, 36.0F, Settings.scale, Settings.scale, 0.0F, 0, 0, 36, 36, flipX, flipY);
        // }

        /*
        void renderMapBlender(SpriteBatch sb)
        {
            if (ADungeon.id != "TheEnding")
            {
                if (!Settings.isMobile)
                {
                    sb.draw(blend, 0.0F, DungeonMapScreen.offsetY + mapOffsetY + 800.0F * Settings.scale, Settings.WIDTH, BLEND_H);
                    sb.draw(blend, 0.0F, DungeonMapScreen.offsetY + mapOffsetY - 220.0F * Settings.scale, Settings.WIDTH, BLEND_H);
                }
                else
                {
                    sb.draw(blend, -Settings.WIDTH * 0.05F, DungeonMapScreen.offsetY + mapOffsetY + 800.0F * Settings.scale, Settings.WIDTH * 1.1F, BLEND_H);
                    sb.draw(blend, -Settings.WIDTH * 0.05F, DungeonMapScreen.offsetY + mapOffsetY - 220.0F * Settings.scale, Settings.WIDTH * 1.1F, BLEND_H);
                }
            }
        }
    */
    }
}