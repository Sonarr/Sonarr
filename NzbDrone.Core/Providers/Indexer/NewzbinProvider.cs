using System.ServiceModel.Syndication;
using NzbDrone.Core.Providers.Core;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NewzbinProvider : IndexerProviderBase
    {
        public NewzbinProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider, EpisodeProvider episodeProvider, ConfigProvider configProvider, HttpProvider httpProvider, IRepository repository, IndexerProvider indexerProvider)
            : base(seriesProvider, seasonProvider, episodeProvider, configProvider, httpProvider, repository, indexerProvider)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                                   {
                                       string.Format("http://www.newzbin.com", _configProvider.NewzbinUsername, _configProvider.NewzbinPassword)
                                   };
            }
        }

        public override string Name
        {
            get { return "Newzbin"; }
        }


        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Id;
        }
    }
}