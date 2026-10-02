using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Download
{
    public interface IMissingYearImportValidator
    {
        bool IsValid(TrackedDownload download, Series series, List<EpisodeHistory> history);
    }

    public class MissingYearImportValidator : IMissingYearImportValidator
    {
        private static readonly Regex YearSuffix = new(@" \((?:19|20)\d{2}\)$", RegexOptions.Compiled);
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly ISceneMappingService _sceneMappingService;

        public MissingYearImportValidator(IDiskProvider diskProvider,
                                          IConfigService configService,
                                          ISeriesService seriesService,
                                          IEpisodeService episodeService,
                                          ISceneMappingService sceneMappingService)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _seriesService = seriesService;
            _episodeService = episodeService;
            _sceneMappingService = sceneMappingService;
        }

        public bool IsValid(TrackedDownload download, Series series, List<EpisodeHistory> history)
        {
            if (!_configService.AllowMissingYearImport || series.SeriesType != SeriesTypes.Standard || series.UseSceneNumbering ||
                !YearSuffix.IsMatch(series.Title) || series.TvdbId <= 0 ||
                download.RemoteEpisode?.Series?.Id != series.Id || download.RemoteEpisode.Episodes.Count != 1 ||
                string.IsNullOrEmpty(download.DownloadItem.DownloadId))
            {
                return false;
            }

            var episode = _episodeService.GetEpisode(download.RemoteEpisode.Episodes[0].Id);
            if (episode == null || episode.SeriesId != series.Id || episode.EpisodeFileId != 0 || episode.SeasonNumber == 0)
            {
                return false;
            }

            var title = YearSuffix.Replace(series.Title, string.Empty).CleanSeriesTitle();
            var parsed = Parser.Parser.ParseTitle(download.DownloadItem.Title);
            if (!Matches(parsed, title, episode) || history.Count == 0 || history.Any(h =>
                h.EventType != EpisodeHistoryEventType.Grabbed || h.SeriesId != series.Id || h.EpisodeId != episode.Id ||
                h.DownloadId != download.DownloadItem.DownloadId || h.SourceTitle != download.DownloadItem.Title ||
                h.Data.GetValueOrDefault(EpisodeHistory.SERIES_MATCH_TYPE) != SeriesMatchType.Id.ToString() ||
                !int.TryParse(h.Data.GetValueOrDefault("tvdbId"), out var tvdbId) || tvdbId != series.TvdbId))
            {
                return false;
            }

            var mappedId = _sceneMappingService.FindTvdbId(parsed.SeriesTitle, parsed.ReleaseTitle, episode.SeasonNumber);
            if (mappedId.HasValue && mappedId.Value != series.TvdbId)
            {
                return false;
            }

            foreach (var other in _seriesService.GetAllSeries().Where(s => s.Id != series.Id))
            {
                var titles = _sceneMappingService.GetSceneNames(other.TvdbId, [episode.SeasonNumber], [episode.SeasonNumber]);
                if (titles.Append(other.Title).Any(t => YearSuffix.Replace(t, string.Empty).CleanSeriesTitle() == title))
                {
                    return false;
                }
            }

            var path = download.ImportItem.OutputPath.FullPath;
            var files = _diskProvider.FileExists(path)
                ? new[] { path }
                : _diskProvider.FolderExists(path)
                    ? _diskProvider.GetFiles(path, true).Where(f => MediaFileExtensions.Extensions.Contains(Path.GetExtension(f))).ToArray()
                    : Array.Empty<string>();

            // Keep ambiguous downloads and obfuscated file names blocked. The regular
            // import pipeline still checks quality, samples and other import rules.
            return files.Length == 1 && Matches(Parser.Parser.ParseTitle(Path.GetFileName(files[0])), title, episode);
        }

        private static bool Matches(ParsedEpisodeInfo parsed, string title, Episode episode)
        {
            return parsed != null && parsed.ReleaseType == ReleaseType.SingleEpisode &&
                   parsed.SeriesTitle.CleanSeriesTitle() == title &&
                   parsed.SeasonNumber == episode.SeasonNumber &&
                   parsed.EpisodeNumbers.SequenceEqual(new[] { episode.EpisodeNumber });
        }
    }
}
