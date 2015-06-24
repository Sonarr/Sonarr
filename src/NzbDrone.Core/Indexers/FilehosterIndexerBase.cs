using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public abstract class FilehosterIndexerBase<TSettings>: HttpIndexerBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        private readonly IProvideDownloadClient downloadClientProvider;

        protected FilehosterIndexerBase(IProvideDownloadClient downloadClientProvider, IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger) 
            : base(httpClient, configService, parsingService, logger)
        {
            this.downloadClientProvider = downloadClientProvider;
        }

        protected override IList<ReleaseInfo> CleanupReleases(IEnumerable<ReleaseInfo> releases)
        {
            var cleanReleases = base.CleanupReleases(releases);

            var filehostLinkChecker = downloadClientProvider.GetDownloadClient(Protocol) as IFilehostLinkChecker;
            if (filehostLinkChecker == null)
            {
                return cleanReleases;
            }
            var result = new List<ReleaseInfo>(cleanReleases.Count);
            foreach (var linkCheck in filehostLinkChecker.CheckLinks(cleanReleases).Where(a => a.IsOnline))
            {
                var release = linkCheck.Release;
                release.Title = linkCheck.Title;
                release.Size = linkCheck.Size;
                result.Add(release);
            }
            return result;
        }
        protected override bool IsFullPage(IList<ReleaseInfo> page)
        {
            return true;
        }
    }
}
