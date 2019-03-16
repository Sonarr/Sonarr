namespace NzbDrone.Core.Indexers
{
    public static class IndexerDefaults
    {
        public const int MINIMUM_SEEDERS = 1;
        public const int PRIORITY = 100; //default > 0 so it allows users to specify higher or lower prio indexers without having to go trough all of them
    }
}
