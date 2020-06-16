﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Discordnotifier
{
    public class Discordnotifier : NotificationBase<DiscordnotifierSettings>
    {
        private readonly IDiscordnotifierProxy _proxy;

        public Discordnotifier(IDiscordnotifierProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Link => "https://discordnotifier.com";
        public override string Name => "Discord Notifier";

        public override void OnGrab(GrabMessage message)
        {
            var series = message.Series;
            var remoteEpisode = message.Episode;
            var releaseGroup = remoteEpisode.ParsedEpisodeInfo.ReleaseGroup;
            var variables = new StringDictionary();

            variables.Add("Sonarr_EventType", "Grab");
            variables.Add("Sonarr_Series_Id", series.Id.ToString());
            variables.Add("Sonarr_Series_Title", series.Title);
            variables.Add("Sonarr_Series_TvdbId", series.TvdbId.ToString());
            variables.Add("Sonarr_Series_TvMazeId", series.TvMazeId.ToString());
            variables.Add("Sonarr_Series_ImdbId", series.ImdbId ?? string.Empty);
            variables.Add("Sonarr_Series_Type", series.SeriesType.ToString());
            variables.Add("Sonarr_Release_EpisodeCount", remoteEpisode.Episodes.Count.ToString());
            variables.Add("Sonarr_Release_SeasonNumber", remoteEpisode.Episodes.First().SeasonNumber.ToString());
            variables.Add("Sonarr_Release_EpisodeNumbers", string.Join(",", remoteEpisode.Episodes.Select(e => e.EpisodeNumber)));
            variables.Add("Sonarr_Release_AbsoluteEpisodeNumbers", string.Join(",", remoteEpisode.Episodes.Select(e => e.AbsoluteEpisodeNumber)));
            variables.Add("Sonarr_Release_EpisodeAirDates", string.Join(",", remoteEpisode.Episodes.Select(e => e.AirDate)));
            variables.Add("Sonarr_Release_EpisodeAirDatesUtc", string.Join(",", remoteEpisode.Episodes.Select(e => e.AirDateUtc)));
            variables.Add("Sonarr_Release_EpisodeTitles", string.Join("|", remoteEpisode.Episodes.Select(e => e.Title)));
            variables.Add("Sonarr_Release_Title", remoteEpisode.Release.Title);
            variables.Add("Sonarr_Release_Indexer", remoteEpisode.Release.Indexer ?? string.Empty);
            variables.Add("Sonarr_Release_Size", remoteEpisode.Release.Size.ToString());
            variables.Add("Sonarr_Release_Quality", remoteEpisode.ParsedEpisodeInfo.Quality.Quality.Name);
            variables.Add("Sonarr_Release_QualityVersion", remoteEpisode.ParsedEpisodeInfo.Quality.Revision.Version.ToString());
            variables.Add("Sonarr_Release_ReleaseGroup", releaseGroup ?? string.Empty);
            variables.Add("Sonarr_Download_Client", message.DownloadClient ?? string.Empty);
            variables.Add("Sonarr_Download_Id", message.DownloadId ?? string.Empty);
			
            _proxy.SendNotification(variables, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var series = message.Series;
            var episodeFile = message.EpisodeFile;
            var sourcePath = message.SourcePath;
            var variables = new StringDictionary();

            variables.Add("Sonarr_EventType", "Download");
            variables.Add("Sonarr_IsUpgrade", message.OldFiles.Any().ToString());
            variables.Add("Sonarr_Series_Id", series.Id.ToString());
            variables.Add("Sonarr_Series_Title", series.Title);
            variables.Add("Sonarr_Series_Path", series.Path);
            variables.Add("Sonarr_Series_TvdbId", series.TvdbId.ToString());
            variables.Add("Sonarr_Series_TvMazeId", series.TvMazeId.ToString());
            variables.Add("Sonarr_Series_ImdbId", series.ImdbId ?? string.Empty);
            variables.Add("Sonarr_Series_Type", series.SeriesType.ToString());
            variables.Add("Sonarr_EpisodeFile_Id", episodeFile.Id.ToString());
            variables.Add("Sonarr_EpisodeFile_EpisodeCount", episodeFile.Episodes.Value.Count.ToString());
            variables.Add("Sonarr_EpisodeFile_RelativePath", episodeFile.RelativePath);
            variables.Add("Sonarr_EpisodeFile_Path", Path.Combine(series.Path, episodeFile.RelativePath));
            variables.Add("Sonarr_EpisodeFile_EpisodeIds", string.Join(",", episodeFile.Episodes.Value.Select(e => e.Id)));
            variables.Add("Sonarr_EpisodeFile_SeasonNumber", episodeFile.SeasonNumber.ToString());
            variables.Add("Sonarr_EpisodeFile_EpisodeNumbers", string.Join(",", episodeFile.Episodes.Value.Select(e => e.EpisodeNumber)));
            variables.Add("Sonarr_EpisodeFile_EpisodeAirDates", string.Join(",", episodeFile.Episodes.Value.Select(e => e.AirDate)));
            variables.Add("Sonarr_EpisodeFile_EpisodeAirDatesUtc", string.Join(",", episodeFile.Episodes.Value.Select(e => e.AirDateUtc)));
            variables.Add("Sonarr_EpisodeFile_EpisodeTitles", string.Join("|", episodeFile.Episodes.Value.Select(e => e.Title)));
            variables.Add("Sonarr_EpisodeFile_Quality", episodeFile.Quality.Quality.Name);
            variables.Add("Sonarr_EpisodeFile_QualityVersion", episodeFile.Quality.Revision.Version.ToString());
            variables.Add("Sonarr_EpisodeFile_ReleaseGroup", episodeFile.ReleaseGroup ?? string.Empty);
            variables.Add("Sonarr_EpisodeFile_SceneName", episodeFile.SceneName ?? string.Empty);
            variables.Add("Sonarr_EpisodeFile_SourcePath", sourcePath);
            variables.Add("Sonarr_EpisodeFile_SourceFolder", Path.GetDirectoryName(sourcePath));
            variables.Add("Sonarr_Download_Client", message.DownloadClient ?? string.Empty);
            variables.Add("Sonarr_Download_Id", message.DownloadId ?? string.Empty);

            if (message.OldFiles.Any())
            {
                variables.Add("Sonarr_DeletedRelativePaths", string.Join("|", message.OldFiles.Select(e => e.RelativePath)));
                variables.Add("Sonarr_DeletedPaths", string.Join("|", message.OldFiles.Select(e => Path.Combine(series.Path, e.RelativePath))));
            }

            _proxy.SendNotification(variables, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck message)
        {
            var variables = new StringDictionary();

            variables.Add("Sonarr_EventType", "HealthIssue");
            variables.Add("Sonarr_Health_Issue_Level", nameof(message.Type));
            variables.Add("Sonarr_Health_Issue_Message", message.Message);
            variables.Add("Sonarr_Health_Issue_Type", message.Source.Name);
            variables.Add("Sonarr_Health_Issue_Wiki", message.WikiUrl.ToString() ?? string.Empty);
			
            _proxy.SendNotification(variables, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
