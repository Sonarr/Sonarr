using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Extras.Subtitles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateSubtitleInfo : IAggregateLocalEpisode
    {
        public int Order => 2;

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

            if (localEpisode.Episodes.Empty())
            {
                return localEpisode;
            }

            var firstEpisode = localEpisode.Episodes.First();
            var episodeFile = firstEpisode.EpisodeFile.Value;
            localEpisode.SubtitleInfo = CleanSubtitleTitleInfo(episodeFile, path);

            return localEpisode;
        }

        public SubtitleTitleInfo CleanSubtitleTitleInfo(EpisodeFile episodeFile, string path)
        {
            var subtitleTitleInfo = LanguageParser.ParseSubtitleLanguageInformation(path);

            var episodeFileTitle = Path.GetFileNameWithoutExtension(episodeFile.RelativePath);
            var originalEpisodeFileTitle = Path.GetFileNameWithoutExtension(episodeFile.OriginalFilePath) ?? string.Empty;

            if (subtitleTitleInfo.TitleFirst && (episodeFileTitle.Contains(subtitleTitleInfo.RawTitle, StringComparison.OrdinalIgnoreCase) || originalEpisodeFileTitle.Contains(subtitleTitleInfo.RawTitle, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.Debug("Subtitle title '{0}' is in episode file title '{1}'. Removing from subtitle title.", subtitleTitleInfo.RawTitle, episodeFileTitle);

                subtitleTitleInfo = LanguageParser.ParseBasicSubtitle(path);
            }

            var cleanedTags = subtitleTitleInfo.LanguageTags.Where(t => !episodeFileTitle.Contains(t, StringComparison.OrdinalIgnoreCase)).ToList();

            if (cleanedTags.Count != subtitleTitleInfo.LanguageTags.Count)
            {
                _logger.Debug("Removed language tags '{0}' from subtitle title '{1}'.", string.Join(", ", subtitleTitleInfo.LanguageTags.Except(cleanedTags)), subtitleTitleInfo.RawTitle);
                subtitleTitleInfo.LanguageTags = cleanedTags;
            }

            return subtitleTitleInfo;
        }
    }
}
