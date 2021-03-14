using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public interface IMakeImportDecision
    {
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series, DownloadClientItem downloadClientItem, ParsedEpisodeInfo folderInfo, bool sceneSource);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series, DownloadClientItem downloadClientItem, ParsedEpisodeInfo folderInfo, bool sceneSource, bool filterExistingFiles);
        ImportDecision GetDecision(LocalEpisode localEpisode, DownloadClientItem downloadClientItem);
    }

    public class ImportDecisionMaker : IMakeImportDecision
    {
        private readonly IEnumerable<IImportDecisionEngineSpecification> _specifications;
        private readonly IMediaFileService _mediaFileService;
        private readonly IAggregationService _aggregationService;
        private readonly IDiskProvider _diskProvider;
        private readonly IAugmentMediaInfo _augmentMediaInfo;
        private readonly IDetectSample _detectSample;
        private readonly Logger _logger;

        public ImportDecisionMaker(IEnumerable<IImportDecisionEngineSpecification> specifications,
                                   IMediaFileService mediaFileService,
                                   IAggregationService aggregationService,
                                   IDiskProvider diskProvider,
                                   IAugmentMediaInfo augmentMediaInfo,
                                   IDetectSample detectSample,
                                   Logger logger)
        {
            _specifications = specifications;
            _mediaFileService = mediaFileService;
            _aggregationService = aggregationService;
            _diskProvider = diskProvider;
            _augmentMediaInfo = augmentMediaInfo;
            _detectSample = detectSample;
            _logger = logger;
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series)
        {
            return GetImportDecisions(videoFiles, series, null, null, false);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series, DownloadClientItem downloadClientItem, ParsedEpisodeInfo folderInfo, bool sceneSource)
        {
            return GetImportDecisions(videoFiles, series, downloadClientItem, folderInfo, sceneSource, true);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series, DownloadClientItem downloadClientItem, ParsedEpisodeInfo folderInfo, bool sceneSource, bool filterExistingFiles)
        {
            var newFiles = filterExistingFiles ? _mediaFileService.FilterExistingFiles(videoFiles.ToList(), series) : videoFiles.ToList();

            _logger.Debug("Analyzing {0}/{1} files.", newFiles.Count, videoFiles.Count());

            ParsedEpisodeInfo downloadClientItemInfo = null;

            if (downloadClientItem != null)
            {
                downloadClientItemInfo = Parser.Parser.ParseTitle(downloadClientItem.Title);
            }

            var decisions = new List<ImportDecision>();
            var localEpisodes = new List<LocalEpisode>();

            foreach (var file in newFiles)
            {
                var localEpisode = new LocalEpisode
                {
                    Series = series,
                    DownloadClientEpisodeInfo = downloadClientItemInfo,
                    FolderEpisodeInfo = folderInfo,
                    Path = file,
                    SceneSource = sceneSource,
                    ExistingFile = series.Path.IsParentPath(file)
                };

                _augmentMediaInfo.Augment(localEpisode);

                localEpisodes.Add(localEpisode);
            }

            // If not importing from a scene source (series folder for example), then assume all files are not samples
            // to avoid using media info on every file needlessly (especially if Analyse Media Files is disabled).
            var otherFiles = sceneSource && GetNonSampleVideoFileCount(localEpisodes, series, downloadClientItemInfo, folderInfo) > 1;

            // Set 'OtherVideoFiles' to true for each episode if there are other files.
            localEpisodes.ForEach(l => l.OtherVideoFiles = otherFiles);

            decisions.AddRange(localEpisodes.Select(l => GetDecision(l, downloadClientItem, otherFiles)));

            return decisions;
        }

        public ImportDecision GetDecision(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, localEpisode, downloadClientItem))
                                         .Where(c => c != null);

            return new ImportDecision(localEpisode, reasons.ToArray());
        }

        private ImportDecision GetDecision(LocalEpisode localEpisode, DownloadClientItem downloadClientItem, bool otherFiles)
        {
            ImportDecision decision = null;

            var fileEpisodeInfo = Parser.Parser.ParsePath(localEpisode.Path);

            localEpisode.FileEpisodeInfo = fileEpisodeInfo;
            localEpisode.Size = _diskProvider.GetFileSize(localEpisode.Path);

            try
            {
                _aggregationService.Augment(localEpisode, downloadClientItem);

                if (localEpisode.Episodes.Empty())
                {
                    if (IsPartialSeason(localEpisode))
                    {
                        decision = new ImportDecision(localEpisode, new Rejection("Partial season packs are not supported"));
                    }
                    else if (IsSeasonExtra(localEpisode))
                    {
                        decision = new ImportDecision(localEpisode, new Rejection("Extras are not supported"));
                    }
                    else
                    {
                        decision = new ImportDecision(localEpisode, new Rejection("Invalid season or episode"));
                    }
                }
                else
                {
                    decision = GetDecision(localEpisode, downloadClientItem);
                }
            }
            catch (AugmentingFailedException)
            {
                decision = new ImportDecision(localEpisode, new Rejection("Unable to parse file"));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Couldn't import file. {0}", localEpisode.Path);

                decision = new ImportDecision(localEpisode, new Rejection("Unexpected error processing file"));
            }

            if (decision == null)
            {
                _logger.Error("Unable to make a decision on {0}", localEpisode.Path);
            }
            else if (decision.Rejections.Any())
            {
                _logger.Debug("File rejected for the following reasons: {0}", string.Join(", ", decision.Rejections));
            }
            else
            {
                _logger.Debug("File accepted");
            }

            return decision;
        }

        private Rejection EvaluateSpec(IImportDecisionEngineSpecification spec, LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            try
            {
                var result = spec.IsSatisfiedBy(localEpisode, downloadClientItem);

                if (!result.Accepted)
                {
                    return new Rejection(result.Reason);
                }
            }
            catch (Exception e)
            {
                //e.Data.Add("report", remoteEpisode.Report.ToJson());
                //e.Data.Add("parsed", remoteEpisode.ParsedEpisodeInfo.ToJson());
                _logger.Error(e, "Couldn't evaluate decision on {0}", localEpisode.Path);
                return new Rejection($"{spec.GetType().Name}: {e.Message}");
            }

            return null;
        }

        private int GetNonSampleVideoFileCount(List<LocalEpisode> localEpisodes, Series series, ParsedEpisodeInfo downloadClientItemInfo, ParsedEpisodeInfo folderInfo)
        {
            var isPossibleSpecialEpisode = downloadClientItemInfo?.IsPossibleSpecialEpisode ?? false;
            // If we might already have a special, don't try to get it from the folder info.
            isPossibleSpecialEpisode = isPossibleSpecialEpisode || (folderInfo?.IsPossibleSpecialEpisode ?? false);

            return localEpisodes.Count(localEpisode =>
            {
                var sample = _detectSample.IsSample(series, localEpisode.Path, isPossibleSpecialEpisode, localEpisode.MediaInfo);

                if (sample == DetectSampleResult.Sample)
                {
                    return false;
                }

                return true;
            });
        }

        private bool IsPartialSeason(LocalEpisode localEpisode)
        {
            var downloadClientEpisodeInfo = localEpisode.DownloadClientEpisodeInfo;
            var folderEpisodeInfo = localEpisode.FolderEpisodeInfo;
            var fileEpisodeInfo = localEpisode.FileEpisodeInfo;

            if (downloadClientEpisodeInfo != null && downloadClientEpisodeInfo.IsPartialSeason)
            {
                return true;
            }

            if (folderEpisodeInfo != null && folderEpisodeInfo.IsPartialSeason)
            {
                return true;
            }

            if (fileEpisodeInfo != null && fileEpisodeInfo.IsPartialSeason)
            {
                return true;
            }

            return false;
        }

        private bool IsSeasonExtra(LocalEpisode localEpisode)
        {
            var downloadClientEpisodeInfo = localEpisode.DownloadClientEpisodeInfo;
            var folderEpisodeInfo = localEpisode.FolderEpisodeInfo;
            var fileEpisodeInfo = localEpisode.FileEpisodeInfo;

            if (downloadClientEpisodeInfo != null && downloadClientEpisodeInfo.IsSeasonExtra)
            {
                return true;
            }

            if (folderEpisodeInfo != null && folderEpisodeInfo.IsSeasonExtra)
            {
                return true;
            }

            if (fileEpisodeInfo != null && fileEpisodeInfo.IsSeasonExtra)
            {
                return true;
            }

            return false;
        }
    }
}
