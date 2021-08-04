namespace NzbDrone.Core.Indexers
{
    public interface ITorrentIndexerSettings : IIndexerSettings
    {
        int MinimumSeeders { get; set; }

        // TODO: System.Text.Json requires setter be public for sub-object deserialization in 3.0. https://github.com/dotnet/corefx/issues/42515
        SeedCriteriaSettings SeedCriteria { get; set; }
    }
}
