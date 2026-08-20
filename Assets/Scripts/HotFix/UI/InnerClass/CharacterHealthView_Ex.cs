
namespace MoreMountains;

public partial class CharacterHealthView
{
    public void SetHealth(int cur, int max)
    {
        curExp.setText(cur);
        maxExp.setText(max);
        expBar.setFillPercent(cur / (float)max);
    }
}
