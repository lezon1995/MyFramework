namespace MoreMountains
{
    public interface IStatsGetter<in T> where T : struct
    {
        UniStats.Stat GetStat(T key);
        bool GetStat(T key, out UniStats.Stat stat);
    }
}