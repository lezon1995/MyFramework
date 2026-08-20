namespace MoreMountains;

public partial class OverlayMenu
{
    public BallTooltipItem BallTooltipItem => ballTooltipItem;
    OverlayMenuBinder binder;

    void initBinder()
    {
        binder = new(
            this,
            characterInfoView.CharacterBallsView.initBinder(this),
            characterInfoView.initBinder(this)
        );

        OverlayMenuService.Instance.Register(binder);
    }
}