namespace MarbleHero
{
    public class EnableEndTurnButtonAction : AGameAction
    {
        public override void update(float dt)
        {
            // ADungeon.overlayMenu.endTurnButton.enable();
            isDone = true;
        }
    }
}