namespace MarbleHero;

public class PlayerTurnEffect : ARenderEffect
{
    const float DUR = 2.0F;
    string turnMessage;

    public override void resetProperty()
    {
        base.resetProperty();
        turnMessage = null;
    }

    public override void onCreate()
    {
        duration = DUR;
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
    }

    public override bool update(float dt)
    {
        return base.update(dt);
    }
}