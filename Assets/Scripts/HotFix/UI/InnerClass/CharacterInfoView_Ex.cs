namespace MoreMountains;

public partial class CharacterInfoView
{
    public CharacterStatsView CharacterStatsView => characterStatsView;
    public CharacterBallsView CharacterBallsView => characterBallsView;
    public CharacterHealthView CharacterHealthView => characterHealthView;
    public CharacterExpView CharacterExpView => characterExpView;

    CharacterInfoBinder _binder;
    
    public CharacterInfoBinder initBinder(OverlayMenu overlayMenu)
    {
        _binder ??= new(this);
        return _binder;
    }
}