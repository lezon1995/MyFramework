namespace MoreMountains
{
    public interface IStatsGetter<in T> : IStatsGetter where T : struct
    {
        UniStats.Stat GetStat(T key);
        bool GetStat(T key, out UniStats.Stat stat);
    }

    public interface IStatsGetter
    {
        UniStats.Stat GetStat(string key);
        bool GetStat(string key, out UniStats.Stat stat);
    }
}