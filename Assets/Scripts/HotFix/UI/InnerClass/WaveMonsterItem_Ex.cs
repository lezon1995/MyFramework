using UnityEngine;

namespace MoreMountains;

public partial class WaveMonsterItem
{
    public void SetMonsterIcon(Sprite s)
    {
        icon.setSpriteOnly(s);
    }

    public void SetAtLeastSpawnCount(int c)
    {
        textAtLeastCount.setText(c.IToS());
    }
}