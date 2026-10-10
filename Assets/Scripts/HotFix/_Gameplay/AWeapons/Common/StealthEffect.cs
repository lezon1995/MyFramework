namespace MoreMountains;

public class StealthEffect : ALogicEffect, IArgs<APlayer, float>
{
    APlayer _player;

    public override void resetProperty()
    {
        base.resetProperty();
        _player = null;
    }

    public void onCreate(APlayer p, float d)
    {
        _player = p;
        _player.inStealth = true;
        _player.playerRenderer.setStealthEffect(true);
        duration = d;
    }

    public override void onFinished()
    {
        _player.inStealth = false;
        _player.playerRenderer.setStealthEffect(false);
    }
}