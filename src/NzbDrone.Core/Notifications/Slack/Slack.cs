using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Rest;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;
using RestSharp;

namespace NzbDrone.Core.Notifications.Slack
{
    public class Slack : NotificationBase<SlackSettings>
    {
        private readonly Logger _logger;
        private readonly ISlackService _service;

        public override bool SupportsOnUpgrade => false;

        public Slack(Logger logger, ISlackService service)
        {
            _logger = logger;
            _service = service;
        }

        public override string Link
        {
            get { return "https://github.com/Sonarr/Sonarr/wiki/Custom-Post-Processing-Scripts"; }
        }

        public override void OnGrab(GrabMessage message)
        {
            _logger.Trace("OnGrab: {0}", message);
            var series = message.Series;
            var remoteEpisode = message.Episode;
            var releaseGroup = remoteEpisode.ParsedEpisodeInfo.ReleaseGroup;
            var data = new Dictionary<string,string>();

            data.Add("$EventType", "Grab");
            data.Add("$Series_Id", series.Id.ToString());
            data.Add("$Series_Title", series.Title);
            data.Add("$Series_TvdbId", series.TvdbId.ToString());
            data.Add("$Series_Type", series.SeriesType.ToString());
            data.Add("$Release_SeasonNumber", remoteEpisode.ParsedEpisodeInfo.SeasonNumber.ToString());
            data.Add("$Release_EpisodeNumbers", string.Join(",", remoteEpisode.Episodes.Select(e => e.EpisodeNumber)));
            data.Add("$Release_Title", remoteEpisode.Release.Title);
            data.Add("$Release_Indexer", remoteEpisode.Release.Indexer);
            data.Add("$Release_Size", remoteEpisode.Release.Size.ToString());
            data.Add("$Release_ReleaseGroup", releaseGroup);

            _service.OnGrab(data, Settings);

        }

        public override void OnDownload(DownloadMessage message)
        {
            _logger.Trace("OnDownload: {0}", message);

            var series = message.Series;
            var episodeFile = message.EpisodeFile;
            var sourcePath = message.SourcePath;
            var data = new Dictionary<string, string>();

            data.Add("$EventType", "Download");
            data.Add("$Series_Id", series.Id.ToString());
            data.Add("$Series_Title", series.Title);
            data.Add("$Series_Path", series.Path);
            data.Add("$Series_TvdbId", series.TvdbId.ToString());
            data.Add("$Series_Type", series.SeriesType.ToString());
            data.Add("$EpisodeFile_Id", episodeFile.Id.ToString());
            data.Add("$EpisodeFile_RelativePath", episodeFile.RelativePath);
            data.Add("$EpisodeFile_Path", Path.Combine(series.Path, episodeFile.RelativePath));
            data.Add("$EpisodeFile_SeasonNumber", episodeFile.SeasonNumber.ToString());
            data.Add("$EpisodeFile_EpisodeNumbers", string.Join(",", episodeFile.Episodes.Value.Select(e => e.EpisodeNumber)));
            data.Add("$EpisodeFile_EpisodeAirDates", string.Join(",", episodeFile.Episodes.Value.Select(e => e.AirDate)));
            data.Add("$EpisodeFile_EpisodeAirDatesUtc", string.Join(",", episodeFile.Episodes.Value.Select(e => e.AirDateUtc)));
            data.Add("$EpisodeFile_Quality", episodeFile.Quality.Quality.Name);
            data.Add("$EpisodeFile_QualityVersion", episodeFile.Quality.Revision.Version.ToString());
            data.Add("$EpisodeFile_ReleaseGroup", episodeFile.ReleaseGroup ?? string.Empty);
            data.Add("$EpisodeFile_SceneName", episodeFile.SceneName ?? string.Empty);
            data.Add("$EpisodeFile_SourcePath", sourcePath);
            data.Add("$EpisodeFile_SourceFolder", Path.GetDirectoryName(sourcePath));

            _service.OnDownload(data, Settings);
        }

        public override void OnRename(Series series)
        {
            _logger.Trace("OnRename: {0}", series);
            var data = new Dictionary<string, string>();

            data.Add("$EventType", "Rename");
            data.Add("$Series_Id", series.Id.ToString());
            data.Add("$Series_Title", series.Title);
            data.Add("$Series_Path", series.Path);
            data.Add("$Series_TvdbId", series.TvdbId.ToString());
            data.Add("$Series_Type", series.SeriesType.ToString());

            _service.OnRename(data, Settings);
        }

        public override string Name
        {
            get
            {
                return "Slack";
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_service.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
