﻿using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.CustomScript
{
    public class CustomScript : NotificationBase<CustomScriptSettings>
    {
        private readonly ICustomScriptService _customScriptService;

        public CustomScript(ICustomScriptService customScriptService)
        {
            _customScriptService = customScriptService;
        }

        public override string Link
        {
            get { return "https://github.com/Sonarr/Sonarr/wiki/Custom-Post-Processing-Scripts"; }
        }

        public override void OnGrab(GrabMessage grabMessage)
        {
        }

        public override void OnGrabMovie(GrabMovieMessage grabMessage)
        {
        }

        public override void OnDownload(DownloadMessage message)
        {
            _customScriptService.OnDownload(message.Series, message.EpisodeFile, message.SourcePath, Settings);
        }

        public override void OnDownloadMovie(DownloadMovieMessage message)
        {
            _customScriptService.OnDownloadMovie(message.Movie, message.MovieFile, message.SourcePath, Settings);
        }

        public override void OnRename(Series series)
        {
            _customScriptService.OnRename(series, Settings);
        }

        public override void OnRenameMovie(Movie movie)
        {
            _customScriptService.OnRenameMovie(movie, Settings);
        }

        public override string Name
        {
            get
            {
                return "Custom Script";
            }
        }

        public override bool SupportsOnGrab
        {
            get
            {
                return false;
            }
        }

        public override bool SupportsOnGrabMovie
        {
            get
            {
                return false;
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            return new ValidationResult(failures);
        }
    }
}
