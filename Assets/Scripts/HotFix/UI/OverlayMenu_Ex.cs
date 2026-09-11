namespace MoreMountains;

public partial class OverlayMenu
{
    public BallTooltipItem BallTooltipItem => ballTooltipItem;
    public BossHealthBarView BossHealthBarView => bossHealthBarView;
    public CharacterInfoView CharacterInfoView => characterInfoView;
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