namespace MoreMountains;

public partial class CharacterHealthView
{
    public DamageChunkHealthBarUI DamageChunkHealthBarUI => damageChunkHealthBarUI;

    public void SetHealth(int cur, int max)
    {
        curExp.setText(cur);
        maxExp.setText(max);
    }

    public void SetShield(int pre, int cur)
    {
        if (cur > 0)
        {
            curShield.setText(cur);
        }
        else
        {
            curShield.setText(null);
        }
    }
}