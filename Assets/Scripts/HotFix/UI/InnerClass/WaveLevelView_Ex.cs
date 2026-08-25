namespace MoreMountains;

public partial class WaveLevelView
{
    int remainSeconds = -1;

    public void setRemainSeconds(int v)
    {
        if (remainSeconds == v)
            return;

        textRemainSeconds.setText(v);
        remainSeconds = v;
    }

    int waveNumber = -1;

    public void setWaveNumber(int cur, int max)
    {
        if (waveNumber == cur)
            return;

        _stringWaveNumber.setInt("waveNumber", cur, "maxWaveNumber", max);
        waveNumber = cur;
    }

    int activeMonsterCount = -1;

    public void setActiveMonsterCount(int v)
    {
        if (activeMonsterCount == v)
            return;

        textActiveMonsterCount.setText(v);
        activeMonsterCount = v;
    }

    int killMonsterCount = -1;

    public void setKillMonsterCount(int v)
    {
        if (killMonsterCount == v)
            return;

        textKillMonsterCount.setText(v);
        killMonsterCount = v;
    }
}