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
            if (isFloatEqual(duration, DUR))
            {
                Toast.Show("Player Turn Start");
            }

            return base.update(dt);
        }
    }
}