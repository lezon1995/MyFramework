namespace MarbleHero;

public class ApplyPowerAction : AGameAction
{
    // static UIStrings uiStrings = Game.languagePack.getUIString("ApplyPowerAction");
    // public static string[] TEXT = uiStrings.TEXT;
    APower powerToApply;
    float startingDuration;
    int amount;

    public ApplyPowerAction(ACreature target, ACreature source, APower powerToApply, int stackAmount, bool isFast)
    {
        if (Settings.FAST_MODE)
            startingDuration = 0.1F;
        else if (isFast)
            startingDuration = Settings.ACTION_DUR_FASTER;
        else
            startingDuration = Settings.ACTION_DUR_FAST;

        duration = startingDuration;
        this.powerToApply = powerToApply;

        if (monsters.areMonstersBasicallyDead)
        {
            duration = 0.0F;
            startingDuration = 0.0F;
            isDone = true;
        }
    }

    public ApplyPowerAction(ACreature target, ACreature source, APower powerToApply) : this(target, source, powerToApply, powerToApply.amount)
    {
    }

    public ApplyPowerAction(ACreature target, ACreature source, APower powerToApply, int stackAmount) : this(target, source, powerToApply, stackAmount, false)
    {
    }


    public override void update(float dt)
    {
        if (target == null || target.isDeadOrEscaped())
        {
            isDone = true;
            return;
        }

        if (duration == startingDuration)
        {
            if (source != null)
                foreach (APower pow in source.powers)
                    pow.onApplyPower(powerToApply, target, source);

            ARelic relic;
            if (target is AMonster && target.isDeadOrEscaped())
            {
                duration = 0.0F;
                isDone = true;
                return;
            }

            // ADungeon.effectList.Add(new FlashAtkImgEffect(target.hb.cX, target.hb.cY, attackEffect));
            bool hasBuffAlready = false;
            foreach (APower p in target.powers)
            {
                if (p.ID == powerToApply.ID && p.ID != "Night Terror")
                {
                    p.stackPower(amount);
                    p.flash();
                    // if ((p is StrengthPower || p is DexterityPower) && amount <= 0)
                    // {
                    // ADungeon.effectList.Add(new PowerDebuffEffect(target.hb.cX - target.animX, target.hb.cY + target.hb.height / 2.0F, powerToApply.name + TEXT[3]));
                    // }
                    // else 
                    if (amount > 0)
                    {
                        // if (p.type == APower.PowerType.BUFF || p is StrengthPower || p is DexterityPower)
                        // ADungeon.effectList.Add(new PowerBuffEffect(target.hb.cX - target.animX, target.hb.cY + target.hb.height / 2.0F, "+" + amount + " " + powerToApply.name));
                        // else
                        // ADungeon.effectList.Add(new PowerDebuffEffect(target.hb.cX - target.animX, target.hb.cY + target.hb.height / 2.0F, "+" + amount + " " + powerToApply.name));
                    }
                    else if (p.type == APower.PowerType.BUFF)
                    {
                        // ADungeon.effectList.Add(new PowerBuffEffect(target.hb.cX - target.animX, target.hb.cY + target.hb.height / 2.0F, powerToApply.name + TEXT[3]));
                    }
                    else
                    {
                        // ADungeon.effectList.Add(new PowerDebuffEffect(target.hb.cX - target.animX, target.hb.cY + target.hb.height / 2.0F, powerToApply.name + TEXT[3]));
                    }

                    p.updateDescription();
                    hasBuffAlready = true;
                    ADungeon.onModifyPower();
                }
            }

            if (!hasBuffAlready)
            {
                target.powers.Add(powerToApply);
                target.powers.Sort();
                powerToApply.onInitialApplication();
                powerToApply.flash();

                if (amount < 0 && (powerToApply.ID == "Strength" || powerToApply.ID == "Dexterity" || powerToApply.ID == "Focus"))
                {
                    // ADungeon.effectList.Add(new PowerDebuffEffect(target.hb.cX - target.animX, target.hb.cY + target.hb.height / 2.0F, powerToApply.name + TEXT[3]));
                }
                else if (powerToApply.type == APower.PowerType.BUFF)
                {
                    // ADungeon.effectList.Add(new PowerBuffEffect(target.hb.cX - target.animX, target.hb.cY + target.hb.height / 2.0F, powerToApply.name));
                }
                else
                {
                    // ADungeon.effectList.Add(new PowerDebuffEffect(target.hb.cX - target.animX, target.hb.cY + target.hb.height / 2.0F, powerToApply.name));
                }

                ADungeon.onModifyPower();
                if (target.isPlayer)
                {
                    int buffCount = 0;
                    foreach (APower p in target.powers)
                    {
                        if (p.type == APower.PowerType.BUFF)
                            buffCount++;
                    }

                    if (buffCount >= 10)
                        UnlockTracker.unlockAchievement("POWERFUL");
                }
            }
        }

        tickDuration(dt);
    }
}