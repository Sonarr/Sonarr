using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public interface IScriptImportDecider
    {
        public ScriptImportDecision TryImport(string sourcePath, string destinationFilePath, ScriptImportDecisionInfo scriptImportDecisionInfo);
    }

    public class ScriptImportDecider : IScriptImportDecider
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IProcessProvider _processProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public ScriptImportDecider(IProcessProvider processProvider,
                                   IConfigService configService,
                                   IConfigFileProvider configFileProvider,
                                   Logger logger)
        {
            _processProvider = processProvider;
            _configService = configService;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public ScriptImportDecision TryImport(string sourcePath, string destinationFilePath, ScriptImportDecisionInfo scriptImportDecisionInfo)
        {
            EpisodeFile episodeFile = scriptImportDecisionInfo.episodeFile;
            Series series = scriptImportDecisionInfo.localEpisode.Series;
            TransferMode mode = scriptImportDecisionInfo.mode;
            LocalEpisode localEpisode = scriptImportDecisionInfo.localEpisode;
            List<EpisodeFile> oldFiles = scriptImportDecisionInfo.OldFiles;
            DownloadClientItemClientInfo downloadClientInfo = scriptImportDecisionInfo.downloadClientItemInfo;
            string downloadId = scriptImportDecisionInfo.downloadId;

            if (!_configService.UseScriptImport)
            {
                return ScriptImportDecision.DeferMove;
            }

            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Sonarr_EventType", "Download");
            environmentVariables.Add("Sonarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Sonarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Sonarr_TransferMode", mode.ToString());

            // always true
            // environmentVariables.Add("Sonarr_IsUpgrade", message.OldFiles.Any().ToString());
            environmentVariables.Add("Sonarr_Series_Id", series.Id.ToString());
            environmentVariables.Add("Sonarr_Series_Title", series.Title);
            environmentVariables.Add("Sonarr_Series_TitleSlug", series.TitleSlug);
            environmentVariables.Add("Sonarr_Series_Path", series.Path);
            environmentVariables.Add("Sonarr_Series_TvdbId", series.TvdbId.ToString());
            environmentVariables.Add("Sonarr_Series_TvMazeId", series.TvMazeId.ToString());
            environmentVariables.Add("Sonarr_Series_ImdbId", series.ImdbId ?? string.Empty);
            environmentVariables.Add("Sonarr_Series_Type", series.SeriesType.ToString());
            environmentVariables.Add("Sonarr_EpisodeFile_Id", episodeFile.Id.ToString());
            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeCount", episodeFile.Episodes.Value.Count.ToString());

            // not yet known
            // environmentVariables.Add("Sonarr_EpisodeFile_RelativePath", episodeFile.RelativePath);
            // environmentVariables.Add("Sonarr_EpisodeFile_Path", Path.Combine(series.Path, episodeFile.RelativePath));
            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeIds", string.Join(",", episodeFile.Episodes.Value.Select(e => e.Id)));
            environmentVariables.Add("Sonarr_EpisodeFile_SeasonNumber", episodeFile.SeasonNumber.ToString());
            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeNumbers", string.Join(",", episodeFile.Episodes.Value.Select(e => e.EpisodeNumber)));
            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeAirDates", string.Join(",", episodeFile.Episodes.Value.Select(e => e.AirDate)));
            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeAirDatesUtc", string.Join(",", episodeFile.Episodes.Value.Select(e => e.AirDateUtc)));
            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeTitles", string.Join("|", episodeFile.Episodes.Value.Select(e => e.Title)));
            environmentVariables.Add("Sonarr_EpisodeFile_EpisodeOverviews", string.Join("|", episodeFile.Episodes.Value.Select(e => e.Overview)));
            environmentVariables.Add("Sonarr_EpisodeFile_Quality", episodeFile.Quality.Quality.Name);
            environmentVariables.Add("Sonarr_EpisodeFile_QualityVersion", episodeFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Sonarr_EpisodeFile_ReleaseGroup", episodeFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Sonarr_EpisodeFile_SceneName", episodeFile.SceneName ?? string.Empty);

            environmentVariables.Add("Sonarr_Download_Client", downloadClientInfo?.Name ?? string.Empty);
            environmentVariables.Add("Sonarr_Download_Client_Type", downloadClientInfo?.Type ?? string.Empty);
            environmentVariables.Add("Sonarr_Download_Id", downloadId ?? string.Empty);
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_AudioChannels", MediaInfoFormatter.FormatAudioChannels(episodeFile.MediaInfo).ToString());
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_AudioCodec", MediaInfoFormatter.FormatAudioCodec(episodeFile.MediaInfo, null));
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_AudioLanguages", episodeFile.MediaInfo.AudioLanguages.Distinct().ConcatToString(" / "));
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_Languages", episodeFile.MediaInfo.AudioLanguages.ConcatToString(" / "));
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_Height", episodeFile.MediaInfo.Height.ToString());
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_Width", episodeFile.MediaInfo.Width.ToString());
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_Subtitles", episodeFile.MediaInfo.Subtitles.ConcatToString(" / "));
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_VideoCodec", MediaInfoFormatter.FormatVideoCodec(episodeFile.MediaInfo, null));
            environmentVariables.Add("Sonarr_EpisodeFile_MediaInfo_VideoDynamicRangeType", MediaInfoFormatter.FormatVideoDynamicRangeType(episodeFile.MediaInfo));

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

            switch (processOutput.ExitCode)
            {
            case 0: // Copy complete
                return ScriptImportDecision.MoveComplete;
            case 2: // Copy complete, file potentially changed, should try renaming again
                if (scriptImportDecisionInfo.isEpisodeFile)
                {
                    // TODO: verify extras get renamed fine
                    return ScriptImportDecision.RenameRequested;
                }
                else
                {
                    return ScriptImportDecision.RejectExtra;
                }

            case 3: // Let Sonarr handle it
                return ScriptImportDecision.DeferMove;
            default: // Error, fail to import
                throw new ScriptImportException("Moving with script failed! Exit code {0}", processOutput.ExitCode);
            }
        }
    }
}
