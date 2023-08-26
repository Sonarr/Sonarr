using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tags;

namespace NzbDrone.Core.MediaFiles
{
    public interface IImportScript
    {
        public ScriptImportDecision TryImport(string sourcePath, string destinationFilePath, LocalEpisode localEpisode, EpisodeFile episodeFile, TransferMode mode);
    }

    public class ImportScriptService : IImportScript
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IProcessProvider _processProvider;
        private readonly IConfigService _configService;
        private readonly ITagRepository _tagRepository;
        private readonly IExistingExtraFiles _existingExtraFiles;
        private readonly Logger _logger;

        public ImportScriptService(IProcessProvider processProvider,
                                   IVideoFileInfoReader videoFileInfoReader,
                                   IConfigService configService,
                                   IConfigFileProvider configFileProvider,
                                   ITagRepository tagRepository,
                                   IExistingExtraFiles existingExtraFiles,
                                   Logger logger)
        {
            _processProvider = processProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _configService = configService;
            _configFileProvider = configFileProvider;
            _tagRepository = tagRepository;
            _existingExtraFiles = existingExtraFiles;
            _logger = logger;
        }

        private (List<string> possibleMediaFiles, string mediaFile) ProcessTemporaryFile(string temporaryPath)
        {
            var lines = File.ReadAllLines(temporaryPath);

            if (lines.Length == 0 || string.IsNullOrWhiteSpace(lines[0]))
            {
                return (new List<string>(), null);
            }

            var possibleExtraFiles = new List<string>();
            string mediaFile = null;

            foreach (var line in lines)
            {
                if (MediaFileExtensions.Extensions.Contains(Path.GetExtension(line)))
                {
                    if (mediaFile is not null)
                    {
                        throw new ScriptImportException("Script output contains multiple media files. Only one media file can be returned.");
                    }
                    else
                    {
                        if (File.Exists(line))
                        {
                            mediaFile = line;
                        }
                        else
                        {
                            _logger.Warn("Script output contains invalid file: {0}", line);
                        }
                    }
                }
                else
                {
                    if (File.Exists(line))
                    {
                        possibleExtraFiles.Add(line);
                    }
                    else
                    {
                        _logger.Warn("Script output contains invalid file: {0}", line);
                    }
                }
            }

            if (mediaFile is not null)
            {
                throw new ScriptImportException("Script output does not contain a media file.");
            }

            return (possibleExtraFiles, mediaFile);
        }

        public ScriptImportDecision TryImport(string sourcePath, string destinationFilePath, LocalEpisode localEpisode, EpisodeFile episodeFile, TransferMode mode)
        {
            var series = localEpisode.Series;
            var oldFiles = localEpisode.OldFiles;
            var downloadClientInfo = localEpisode.DownloadItem?.DownloadClientInfo;
            var downloadId = localEpisode.DownloadItem?.DownloadId;

            if (!_configService.UseScriptImport)
            {
                return ScriptImportDecision.DeferMove;
            }

            var temporaryPath = Path.GetTempFileName();

            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Sonarr_SourcePath", sourcePath);
            environmentVariables.Add("Sonarr_DestinationPath", destinationFilePath);
            environmentVariables.Add("Sonarr_InfoFilePath", temporaryPath);

            environmentVariables.Add("Sonarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Sonarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Sonarr_TransferMode", mode.ToString());

            environmentVariables.Add("Sonarr_Series_Id", series.Id.ToString());
            environmentVariables.Add("Sonarr_Series_Title", series.Title);
            environmentVariables.Add("Sonarr_Series_TitleSlug", series.TitleSlug);
            environmentVariables.Add("Sonarr_Series_Path", series.Path);
            environmentVariables.Add("Sonarr_Series_TvdbId", series.TvdbId.ToString());
            environmentVariables.Add("Sonarr_Series_TvMazeId", series.TvMazeId.ToString());
            environmentVariables.Add("Sonarr_Series_ImdbId", series.ImdbId ?? string.Empty);
            environmentVariables.Add("Sonarr_Series_Type", series.SeriesType.ToString());
            environmentVariables.Add("Sonarr_Series_OriginalLanguage", IsoLanguages.Get(series.OriginalLanguage).ThreeLetterCode);
            environmentVariables.Add("Sonarr_Series_Genres", string.Join("|", series.Genres));
            environmentVariables.Add("Sonarr_Series_Tags", string.Join("|", series.Tags.Select(t => _tagRepository.Get(t).Label)));

            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeCount", localEpisode.Episodes.Count.ToString());
            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeIds", string.Join(",", localEpisode.Episodes.Select(e => e.Id)));
            environmentVariables.Add("Sonarr_EpisodeFile_SeasonNumber", localEpisode.SeasonNumber.ToString());
            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeNumbers", string.Join(",", localEpisode.Episodes.Select(e => e.EpisodeNumber)));
            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeAirDates", string.Join(",", localEpisode.Episodes.Select(e => e.AirDate)));
            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeAirDatesUtc", string.Join(",", localEpisode.Episodes.Select(e => e.AirDateUtc)));
            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeTitles", string.Join("|", localEpisode.Episodes.Select(e => e.Title)));
            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeOverviews", string.Join("|", localEpisode.Episodes.Select(e => e.Overview)));
            environmentVariables.Add("Sonarr_EpisodeFile_Quality", localEpisode.Quality.Quality.Name);
            environmentVariables.Add("Sonarr_EpisodeFile_QualityVersion", localEpisode.Quality.Revision.Version.ToString());
            environmentVariables.Add("Sonarr_EpisodeFile_ReleaseGroup", localEpisode.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Sonarr_EpisodeFile_SceneName", localEpisode.SceneName ?? string.Empty);

            environmentVariables.Add("Sonarr_Download_Client", downloadClientInfo?.Name ?? string.Empty);
            environmentVariables.Add("Sonarr_Download_Client_Type", downloadClientInfo?.Type ?? string.Empty);
            environmentVariables.Add("Sonarr_Download_Id", downloadId ?? string.Empty);
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_AudioChannels", MediaInfoFormatter.FormatAudioChannels(localEpisode.MediaInfo).ToString());
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_AudioCodec", MediaInfoFormatter.FormatAudioCodec(localEpisode.MediaInfo, null));
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_AudioLanguages", localEpisode.MediaInfo.AudioLanguages.Distinct().ConcatToString(" / "));
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_Languages", localEpisode.MediaInfo.AudioLanguages.ConcatToString(" / "));
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_Height", localEpisode.MediaInfo.Height.ToString());
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_Width", localEpisode.MediaInfo.Width.ToString());
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_Subtitles", localEpisode.MediaInfo.Subtitles.ConcatToString(" / "));
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_VideoCodec", MediaInfoFormatter.FormatVideoCodec(localEpisode.MediaInfo, null));
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_VideoDynamicRangeType", MediaInfoFormatter.FormatVideoDynamicRangeType(localEpisode.MediaInfo));

            environmentVariables.Add("Sonarr_EpisodeFile_CustomFormat", string.Join("|", localEpisode.CustomFormats));
            environmentVariables.Add("Sonarr_EpisodeFile_CustomFormatScore", localEpisode.CustomFormatScore.ToString());

            if (oldFiles.Any())
            {
                environmentVariables.Add("Sonarr_DeletedRelativePaths", string.Join("|", oldFiles.Select(e => e.RelativePath)));
                environmentVariables.Add("Sonarr_DeletedPaths", string.Join("|", oldFiles.Select(e => Path.Combine(series.Path, e.RelativePath))));
                environmentVariables.Add("Sonarr_DeletedDateAdded", string.Join("|", oldFiles.Select(e => e.DateAdded)));
            }

            _logger.Debug("Executing external script: {0}", _configService.ScriptImportPath);

            var processOutput = _processProvider.StartAndCapture(_configService.ScriptImportPath, $"\"{sourcePath}\" \"{destinationFilePath}\"", environmentVariables);

            _logger.Debug("Executed external script: {0} - Status: {1}", _configService.ScriptImportPath, processOutput.ExitCode);
            _logger.Debug("Script Output: \r\n{0}", string.Join("\r\n", processOutput.Lines));

            var (possibleExtraFiles, mediaFile) = ProcessTemporaryFile(temporaryPath);

            localEpisode.PossibleExtraFiles = possibleExtraFiles;

            destinationFilePath = mediaFile ?? destinationFilePath;
            episodeFile.RelativePath = series.Path.GetRelativePath(destinationFilePath);
            episodeFile.Path = destinationFilePath;

            if ((processOutput.ExitCode & 0x4) == 0x4)
            {
                localEpisode.ShouldImportExtras = true;
            }

            switch (processOutput.ExitCode & 0x3)
            {
                case 0: // Copy complete
                    localEpisode.ScriptImported = true;
                    return ScriptImportDecision.MoveComplete;
                case 2: // Copy complete, file potentially changed, should try renaming again
                    localEpisode.ScriptImported = true;
                    episodeFile.MediaInfo = _videoFileInfoReader.GetMediaInfo(destinationFilePath);
                    episodeFile.Path = null;
                    return ScriptImportDecision.RenameRequested;
                case 3: // Let Sonarr handle it
                    return ScriptImportDecision.DeferMove;
                default: // Error, fail to import
                    throw new ScriptImportException("Moving with script failed! Exit code {0}", processOutput.ExitCode);
            }
        }
    }
}
