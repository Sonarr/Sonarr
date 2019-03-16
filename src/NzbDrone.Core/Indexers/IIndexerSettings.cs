using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerSettings : IProviderConfig
    {
        string BaseUrl { get; set; }
        /// <summary>
        /// Used when searching for releases, a higher priority indexer will be used if releases found have the same quality
        /// </summary>
        int Priority { get; set; }
    }
}
