using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerSettings : IProviderConfig
    {
        string BaseUrl { get; set; }

        int SearchPriority { get; set; }
    }
}
