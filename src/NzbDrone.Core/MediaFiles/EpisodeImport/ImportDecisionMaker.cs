using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Languages;


namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public interface IMakeImportDecision
    {
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series, DownloadClientItem downloadClientItem, ParsedEpisodeInfo folderInfo, bool sceneSource);
    }

    public class ImportDecisionMaker : IMakeImportDecision
    {
        private readonly IEnumerable<IImportDecisionEngineSpecification> _specifications;
        private readonly IParsingService _parsingService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IDetectSample _detectSample;
        private readonly Logger _logger;

        public ImportDecisionMaker(IEnumerable<IImportDecisionEngineSpecification> specifications,
                                   IParsingService parsingService,
                                   IMediaFileService mediaFileService,
                                   IDiskProvider diskProvider,
                                   IVideoFileInfoReader videoFileInfoReader,
                                   IDetectSample detectSample,
                                   Logger logger)
        {
            _specifications = specifications;
            _parsingService = parsingService;
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _detectSample = detectSample;
            _logger = logger;
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series)
        {
            return GetImportDecisions(videoFiles, series, null, null, false);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series, DownloadClientItem downloadClientItem, ParsedEpisodeInfo folderInfo, bool sceneSource)
        {
            var newFiles = _mediaFileService.FilterExistingFiles(videoFiles.ToList(), series);

            _logger.Debug("Analyzing {0}/{1} files.", newFiles.Count, videoFiles.Count());

            var shouldUseFolderName = ShouldUseFolderName(videoFiles, series, folderInfo);
            var decisions = new List<ImportDecision>();

            foreach (var file in newFiles)
            {
                decisions.AddIfNotNull(GetDecision(file, series, downloadClientItem, folderInfo, sceneSource, shouldUseFolderName));
            }

            return decisions;
        }

        private ImportDecision GetDecision(string file, Series series, DownloadClientItem downloadClientItem, ParsedEpisodeInfo folderInfo, bool sceneSource, bool shouldUseFolderName)
        {
            ImportDecision decision = null;

            try
            {
                var localEpisode = _parsingService.GetLocalEpisode(file, series, shouldUseFolderName ? folderInfo : null, sceneSource);

                if (localEpisode != null)
                {
                    localEpisode.Quality = GetQuality(folderInfo, localEpisode.Quality, series);
                    localEpisode.Language = GetLanguage(folderInfo, localEpisode.Language, series);
                    localEpisode.Size = _diskProvider.GetFileSize(file);

                    _logger.Debug("Size: {0}", localEpisode.Size);

                    //TODO: make it so media info doesn't ruin the import process of a new series
                    if (sceneSource)
                    {
                        localEpisode.MediaInfo = _videoFileInfoReader.GetMediaInfo(file);
                    }

                    if (localEpisode.Episodes.Empty())
                    {
                        if (localEpisode.ParsedEpisodeInfo.IsPartialSeason)
                        {
                            decision = new ImportDecision(localEpisode, new Rejection("Partial season packs are not supported"));
                        }
                        else if (localEpisode.ParsedEpisodeInfo.IsSeasonExtra)
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

                else
                {
                    localEpisode = new LocalEpisode();
                    localEpisode.Path = file;

                    decision = new ImportDecision(localEpisode, new Rejection("Unable to parse file"));
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't import file. {0}", file);

                var localEpisode = new LocalEpisode { Path = file };
                decision = new ImportDecision(localEpisode, new Rejection("Unexpected error processing file"));
            }

            if (decision == null)
            {
                _logger.Error("Unable to make a decision on {0}", file);
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

        private ImportDecision GetDecision(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, localEpisode, downloadClientItem))
                                         .Where(c => c != null);

            return new ImportDecision(localEpisode, reasons.ToArray());
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

        private bool ShouldUseFolderName(List<string> videoFiles, Series series, ParsedEpisodeInfo folderInfo)
        {
            if (folderInfo == null)
            {
                return false;
            }

            if (folderInfo.FullSeason)
            {
                return false;
            }

            return videoFiles.Count(file =>
            {
                var sample = _detectSample.IsSample(series, file, folderInfo.IsPossibleSpecialEpisode);

                if (sample == DetectSampleResult.Sample)
                {
                    return false;
                }

                if (SceneChecker.IsSceneTitle(Path.GetFileName(file)))
                {
                    return false;
                }

                return true;
            }) == 1;
        }

        private QualityModel GetQuality(ParsedEpisodeInfo folderInfo, QualityModel fileQuality, Series series)
        {
            if (UseFolderQuality(folderInfo, fileQuality, series))
            {
                _logger.Debug("Using quality from folder: {0}", folderInfo.Quality);
                return folderInfo.Quality;
            }

            return fileQuality;
        }

      private Language GetLanguage(ParsedEpisodeInfo folderInfo, Language fileLanguage, Series series)
        {
            if (UseFolderLanguage (folderInfo, fileLanguage, series))
            {
                _logger.Debug("Using language from folder: {0}", folderInfo.Language);
                return folderInfo.Language;
            }

            return fileLanguage;
        }

        private bool UseFolderLanguage(ParsedEpisodeInfo folderInfo, Language fileLanguage, Series series)
        {
            if (folderInfo == null)
            {
                return false;
            }

            if (folderInfo.Language == Language.Unknown)
            {
                return false;
            }

            if (new LanguageComparer(series.LanguageProfile).Compare(folderInfo.Language, fileLanguage) > 0)
            {
                return true;
            }

            return false;
        }

        private bool UseFolderQuality(ParsedEpisodeInfo folderInfo, QualityModel fileQuality, Series series)
        {
            if (folderInfo == null)
            {
                return false;
            }

            if (folderInfo.Quality.Quality == Quality.Unknown)
            {
                return false;
            }

            if (fileQuality.QualitySource == QualitySource.Extension)
            {
                return true;
            }

            if (new QualityModelComparer(series.Profile).Compare(folderInfo.Quality, fileQuality) > 0)
            {
                return true;
            }

            return false;
        }
    }
}

