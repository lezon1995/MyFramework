namespace MarbleHero
{
    public class ExhaustSpecificCardAction : AGameAction
    {
        ACard targetCard;
        CardGroup group;
        float startingDuration;

        public ExhaustSpecificCardAction(ACard targetCard, CardGroup group, bool isFast)
        {
            this.targetCard = targetCard;
            this.group = group;

            setValues(player, player, amount);
            actionType = ActionType.EXHAUST;
            startingDuration = Settings.ACTION_DUR_FAST;
            duration = startingDuration;
        }

        public ExhaustSpecificCardAction(ACard targetCard, CardGroup group) : this(targetCard, group, false)
        {
        }

        public override void update(float dt)
        {
            if (duration == startingDuration && group.contains(targetCard))
            {
                group.moveToExhaustPile(targetCard);
                _dungeon.checkForPactAchievement();
                targetCard.exhaustOnUseOnce = false;
                targetCard.freeToPlayOnce = false;
            }

            tickDuration(dt);
        }
    }
}