using NLog;
using Workarr.Download;
using Workarr.Extensions;
using Workarr.Extras.Subtitles;
using Workarr.Parser;
using Workarr.Parser.Model;

namespace Workarr.MediaFiles.EpisodeImport.Aggregation.Aggregators
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
            var isSubtitleFile = SubtitleFileExtensions.Extensions.Contains(Path.GetExtension((string)path));

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
            localEpisode.SubtitleInfo = CleanSubtitleTitleInfo(episodeFile, path, localEpisode.FileNameBeforeRename);

            return localEpisode;
        }

        public SubtitleTitleInfo CleanSubtitleTitleInfo(EpisodeFile episodeFile, string path, string fileNameBeforeRename)
        {
            var subtitleTitleInfo = LanguageParser.ParseSubtitleLanguageInformation(path);

            var episodeFileTitle = Path.GetFileNameWithoutExtension(fileNameBeforeRename ?? episodeFile.RelativePath);
            var originalEpisodeFileTitle = Path.GetFileNameWithoutExtension(episodeFile.OriginalFilePath) ?? string.Empty;

            if (subtitleTitleInfo.TitleFirst && (episodeFileTitle.Contains((string)subtitleTitleInfo.RawTitle, StringComparison.OrdinalIgnoreCase) || originalEpisodeFileTitle.Contains((string)subtitleTitleInfo.RawTitle, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.Debug<string, string>("Subtitle title '{0}' is in episode file title '{1}'. Removing from subtitle title.", subtitleTitleInfo.RawTitle, episodeFileTitle);

                subtitleTitleInfo = LanguageParser.ParseBasicSubtitle(path);
            }

            var cleanedTags = Enumerable.Where<string>(subtitleTitleInfo.LanguageTags, t => !episodeFileTitle.Contains((string)t, StringComparison.OrdinalIgnoreCase)).ToList();

            if (cleanedTags.Count != subtitleTitleInfo.LanguageTags.Count)
            {
                _logger.Debug<string, string>("Removed language tags '{0}' from subtitle title '{1}'.", string.Join(", ", Enumerable.Except(subtitleTitleInfo.LanguageTags, cleanedTags)), subtitleTitleInfo.RawTitle);
                subtitleTitleInfo.LanguageTags = cleanedTags;
            }

            return subtitleTitleInfo;
        }
    }
}
