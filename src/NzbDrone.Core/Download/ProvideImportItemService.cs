namespace NzbDrone.Core.Download
{
    public interface IProvideImportItemService
    {
        DownloadClientItem ProvideImportItem(DownloadClientItem item, DownloadClientItem previousImportAttempt);
    }

    public class ProvideImportItemService : IProvideImportItemService
    {
        private readonly IProvideDownloadClient _downloadClientProvider;

        public ProvideImportItemService(IProvideDownloadClient downloadClientProvider)
        {
            _downloadClientProvider = downloadClientProvider;
        }

        public DownloadClientItem ProvideImportItem(DownloadClientItem item, DownloadClientItem previousImportAttempt)
        {
            var client = _downloadClientProvider.Get(item.DownloadClientInfo.Id);

            return client.GetImportItem(item, previousImportAttempt);
        }
    }
}
