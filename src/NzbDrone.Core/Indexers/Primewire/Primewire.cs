using System.Linq;
using System.Text;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Indexers.Primewire
{
    public class Primewire : FilehosterIndexerBase<PrimewireSettings>
    {
        private readonly IEpisodeRepository episodeRepository;

        public Primewire(IEpisodeRepository episodeRepository, IProvideDownloadClient downloadClientProvider, IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger) : base(downloadClientProvider, httpClient, configService, parsingService, logger)
        {
            this.episodeRepository = episodeRepository;
        }

        public override bool SupportsRss
        {
            get
            {
                return false;
            }
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new PrimewireRequestGenerator(_parsingService, episodeRepository) { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new PrimewirePageParser(){ Settings = Settings};
        }


        public override string Name
        {
            get
            {
                return "Primewire";
            }
        }

        public override DownloadProtocol Protocol
        {
            get { return DownloadProtocol.Filehoster; }
        }

        protected override ValidationFailure TestConnection()
        {
            var parser = GetParser();
            var generator = GetRequestGenerator();
            return null;
        }
    }
}
