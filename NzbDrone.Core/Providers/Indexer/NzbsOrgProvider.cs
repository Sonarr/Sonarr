using System.ServiceModel.Syndication;
using NzbDrone.Core.Providers.Core;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NzbsOrgProvider : IndexerProviderBase
    {
        public NzbsOrgProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider, EpisodeProvider episodeProvider, ConfigProvider configProvider, HttpProvider httpProvider, IRepository repository, IndexerProvider indexerProvider)
            : base(seriesProvider, seasonProvider, episodeProvider, configProvider, httpProvider, repository, indexerProvider)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                                   {
                                       string.Format("http://nzbs.org/rss.php?type=1&i={0}&h={1}&num=50&dl=1", _configProvider.NzbsOrgUId, _configProvider.NzbsOrgHash)
                                   };
            }
        }

        public override string Name
        {
            get { return "Nzbs.org"; }
        }


        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Id;
        }


    }
}