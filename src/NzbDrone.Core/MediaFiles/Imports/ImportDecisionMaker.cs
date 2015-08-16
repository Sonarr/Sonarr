using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.MediaFiles.Imports.Specifications;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;


namespace NzbDrone.Core.MediaFiles.Imports
{
    public interface IMakeImportDecision
    {
        List<ImportDecision> GetImportDecisions(List<String> videoFiles, Media media);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Media media, ParsedInfo folderInfo, bool sceneSource);
    }

    public class ImportDecisionMaker : IMakeImportDecision
    {
        private readonly IEnumerable<IImportDecisionEngineSpecification> _commonSpecifications;
        /*        private readonly IEnumerable<IImportSeriesDecisionEngineSpecification> _seriesSpecifications;
                private readonly IEnumerable<IImportMovieDecisionEngineSpecification> _movieSpecifications;*/
        private readonly IParsingService _parsingService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IDetectSample _detectSample;
        private readonly Logger _logger;

        public ImportDecisionMaker(IEnumerable<IImportDecisionEngineSpecification> commonSpecifications,
            /*                                   IEnumerable<IImportSeriesDecisionEngineSpecification> seriesSpecifications,
                                               IEnumerable<IImportMovieDecisionEngineSpecification> movieSpecifications,*/
                                   IParsingService parsingService,
                                   IMediaFileService mediaFileService,
                                   IDiskProvider diskProvider,
                                   IVideoFileInfoReader videoFileInfoReader,
                                   IDetectSample detectSample,
                                   Logger logger)
        {
            _commonSpecifications = commonSpecifications;
            /*            _seriesSpecifications = seriesSpecifications;
                        _movieSpecifications = movieSpecifications;*/
            _parsingService = parsingService;
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _detectSample = detectSample;
            _logger = logger;
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Media media)
        {
            return GetImportDecisions(videoFiles, media, null, false);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Media media, ParsedInfo folderInfo, bool sceneSource)
        {
            var newFiles = _mediaFileService.FilterExistingFiles(videoFiles.ToList(), media);

            _logger.Debug("Analyzing {0}/{1} files.", newFiles.Count, videoFiles.Count());

            var shouldUseFolderName = ShouldUseFolderName(videoFiles, media, folderInfo);
            var decisions = new List<ImportDecision>();

            foreach (var file in newFiles)
            {
                decisions.AddIfNotNull(GetDecision(file, media, folderInfo, sceneSource, shouldUseFolderName));
            }

            return decisions;
        }

        private ImportDecision GetDecision(string file, Media media, ParsedInfo folderInfo, bool sceneSource, bool shouldUseFolderName)
        {
            ImportDecision decision = null;

            try
            {
                var localItem = _parsingService.GetLocalItem(file, media, shouldUseFolderName ? folderInfo : null, sceneSource);

                if (localItem != null)
                {
                    localItem.Quality = GetQuality(folderInfo, localItem.Quality, media.Profile);
                    localItem.Size = _diskProvider.GetFileSize(file);

                    _logger.Debug("Size: {0}", localItem.Size);

                    //TODO: make it so media info doesn't ruin the import process of a new series
                    if (sceneSource)
                    {
                        localItem.MediaInfo = _videoFileInfoReader.GetMediaInfo(file);
                    }

                    if (localItem.IsEmpty())
                    {
                        decision = new ImportDecision(localItem, new Rejection("Unable to parse media items(s) from filename"));
                    }
                    else
                    {
                        decision = GetDecision(localItem);
                    }
                }

                else
                {
                    localItem = new LocalEpisode();
                    localItem.Path = file;

                    decision = new ImportDecision(localItem, new Rejection("Unable to parse file"));
                }
            }
            catch (Exception e)
            {
                _logger.ErrorException("Couldn't import file. " + file, e);
            }

            return decision;
        }

        private ImportDecision GetDecision(LocalItem localItem)
        {
            var reasons = _commonSpecifications.Select(c => EvaluateCommonSpec(c, localItem))
                                         .Where(c => c != null);

            return new ImportDecision(localItem, reasons.ToArray());
        }

        private Rejection EvaluateCommonSpec(IImportDecisionEngineSpecification spec, LocalItem localItem)
        {
            try
            {
                var result = spec.IsSatisfiedBy(localItem);

                if (!result.Accepted)
                {
                    return new Rejection(result.Reason);
                }
            }
            catch (Exception e)
            {
                //e.Data.Add("report", remoteEpisode.Report.ToJson());
                //e.Data.Add("parsed", remoteEpisode.ParsedEpisodeInfo.ToJson());
                _logger.ErrorException("Couldn't evaluate decision on " + localItem.Path, e);
                return new Rejection(String.Format("{0}: {1}", spec.GetType().Name, e.Message));
            }

            return null;
        }

        private bool ShouldUseFolderName(List<string> videoFiles, Media media, ParsedInfo folderInfo)
        {
            if (folderInfo == null)
            {
                return false;
            }

            var folderEpisodeInfo = folderInfo as ParsedEpisodeInfo;

            if (folderEpisodeInfo != null && folderEpisodeInfo.FullSeason)
            {
                return false;
            }

            return videoFiles.Count(file =>
            {
                var size = _diskProvider.GetFileSize(file);
                var fileQuality = QualityParser.ParseQuality(file);
                var sample = _detectSample.IsSample(media, GetQuality(folderInfo, fileQuality, media.Profile.Value), file, size, folderInfo);

                if (sample)
                {
                    return false;
                }

                if (SceneChecker.IsSeriesSceneTitle(Path.GetFileName(file)))
                {
                    return false;
                }

                return true;
            }) == 1;
        }

        private QualityModel GetQuality(ParsedInfo folderInfo, QualityModel fileQuality, Profile profile)
        {
            if (folderInfo != null &&
                folderInfo.Quality.Quality != Quality.Unknown &&
                new QualityModelComparer(profile).Compare(folderInfo.Quality, fileQuality) > 0)
            {
                _logger.Debug("Using quality from folder: {0}", folderInfo.Quality);
                return folderInfo.Quality;
            }

            return fileQuality;
        }
    }
}
