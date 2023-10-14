using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Extras.Subtitles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateSubtitleInfo : IAggregateLocalEpisode
    {
        public int Order => 6;

        private readonly Logger _logger;

        public AggregateSubtitleInfo(Logger logger)
        {
            _logger = logger;
        }

        public LocalEpisode Aggregate(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            var path = localEpisode.Path;
            var isSubtitleFile = SubtitleFileExtensions.Extensions.Contains(Path.GetExtension(path));
            if (!isSubtitleFile)
            {
                return localEpisode;
            }

            var firstEpisode = localEpisode.Episodes.First();
            var subtitleTitleInfo = LanguageParser.ParseSubtitleLanguageInformation(path, firstEpisode);

            subtitleTitleInfo.LanguageTags ??= LanguageParser.ParseLanguageTags(path);
            subtitleTitleInfo.Language ??= LanguageParser.ParseSubtitleLanguage(path);

            localEpisode.SubtitleInfo = subtitleTitleInfo;

            return localEpisode;
        }
    }
}
