using System;

namespace MoreMountains
{
    public interface IPublisherIntegration
    {
        bool isInitialized();
        void dispose();
        void deleteAllCloudFiles();
        bool setStat(string paramString, int paramInt);
        int getStat(string paramString);
        bool incrementStat(string paramString, int paramInt);
        long getGlobalStat(string paramString);
        void uploadDailyLeaderboardScore(string paramString, int paramInt);
        void uploadLeaderboardScore(string paramString, int paramInt);
        void unlockAchievement(string paramString);
        // void getLeaderboardEntries(APlayer.PlayerClass paramPlayerClass, FilterButton.RegionSetting paramRegionSetting, FilterButton.LeaderboardType paramLeaderboardType, int paramInt1, int paramInt2);
        void getDailyLeaderboard(long paramLong, int paramInt1, int paramInt2);
        void setRichPresenceDisplayPlaying(int paramInt, string paramString);
        void setRichPresenceDisplayPlaying(int paramInt1, int paramInt2, string paramString);
        void setRichPresenceDisplayInMenu();
        int getNumUnlockedAchievements();
        DistributorFactory.Distributor getType();
    }

    public class DistributorFactory
    {
        public enum Distributor
        {
            STEAM,
            DISCORD,
            WEGAME,
            GOG,
            EA,
            MICROSOFT
        }

        public static IPublisherIntegration getEnabledDistributor(string distributor)
        {
            switch (distributor)
            {
                case "steam":
                    return new SteamIntegration();
                case "discord":
                    // return new DiscordIntegration();
                case "wegame":
                    // return new WeGameIntegration();
                case "gog":
                    // return new GogIntegration();
                case "ea":
                    // return new EaIntegration();
                case "microsoft":
                    // return new MicrosoftIntegration();
                default:
                    return null;
            }

            throw new DistributorFactoryException("Unrecognized distributor=" + distributor);
        }

        public static bool isLeaderboardEnabled()
        {
            return false;
            // return (Game.publisherIntegration.getType() == Distributor.STEAM);
        }
    }

    public class DistributorFactoryException : Exception
    {
        public DistributorFactoryException(String message) : base(message)
        {
        }
    }
}