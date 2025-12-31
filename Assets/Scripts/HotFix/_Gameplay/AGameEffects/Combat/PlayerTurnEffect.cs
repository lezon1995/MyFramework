namespace MarbleHero
{
    public class PlayerTurnEffect : AGameEffect
    {
        static float DUR = 2.0F;
        string turnMessage;

        public PlayerTurnEffect()
        {
            Duration = DUR;
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

            // sound.play("TURN_EFFECT");
            monsters.showIntent();

            scale = 1.0F;
        }

        public override void update(float dt)
        {
            if (Duration == DUR)
            {
                Toast.Show("Player Turn Start");
            }

            base.update(dt);
        }
    }
}