using Workarr.Indexers;
using Workarr.Parser.Model;
using Workarr.ThingiProvider;

namespace Workarr.Download
{
    public interface IDownloadClient : IProvider
    {
        DownloadProtocol Protocol { get; }
        Task<string> Download(RemoteEpisode remoteEpisode, IIndexer indexer);
        IEnumerable<DownloadClientItem> GetItems();
        DownloadClientItem GetImportItem(DownloadClientItem item, DownloadClientItem previousImportAttempt);
        void RemoveItem(DownloadClientItem item, bool deleteData);
        DownloadClientInfo GetStatus();
        void MarkItemAsImported(DownloadClientItem downloadClientItem);
    }
}
