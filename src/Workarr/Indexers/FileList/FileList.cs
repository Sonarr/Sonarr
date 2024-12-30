using NLog;
using Workarr.Configuration;
using Workarr.Http;
using Workarr.Localization;
using Workarr.Parser;

namespace Workarr.Indexers.FileList
{
    public class FileList : HttpIndexerBase<FileListSettings>
    {
        public override string Name => "FileList";
        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override bool SupportsRss => true;
        public override bool SupportsSearch => true;

        public FileList(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger, ILocalizationService localizationService)
            : base(httpClient, indexerStatusService, configService, parsingService, logger, localizationService)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new FileListRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new FileListParser(Settings);
        }
    }
}
