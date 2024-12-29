using Workarr.ThingiProvider;

namespace Workarr.Indexers
{
    public interface IIndexerSettings : IProviderConfig
    {
        string BaseUrl { get; set; }

        IEnumerable<int> MultiLanguages { get; set; }

        IEnumerable<int> FailDownloads { get; set; }
    }
}
