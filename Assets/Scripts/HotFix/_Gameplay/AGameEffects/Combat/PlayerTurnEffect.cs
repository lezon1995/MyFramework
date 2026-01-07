using Drawing;
using UnityEngine;

namespace MarbleHero
{
    public class PlayerTurnEffect : AGameEffect
    {
        const float DUR = 2.0F;
        string turnMessage;

        public override void onCreate()
        {
            duration = DUR;
            startingDuration = DUR;
            if (Settings.usesOrdinal)
            {
                // turnMessage = GameActionManager.turn + getOrdinalNaming(GameActionManager.turn) + BattleStartEffect.TURN_TXT;
            }
            else if (Settings.language == GameLanguage.VIE)
            {
                // turnMessage = BattleStartEffect.TURN_TXT + " " + GameActionManager.turn;
            }
            else
            {
                // turnMessage = GameActionManager.turn + BattleStartEffect.TURN_TXT;
            }

            sound.play("TURN_EFFECT");
            monsters.showIntent();

            scale = 1.0F;
        }

        public override bool update(float dt)
        {
            Draw.xy.Label2D(new Vector2(0, 0), "Player Turn Start", 20, LabelAlignment.Center, color);
            return base.update(dt);
        }
    }
}