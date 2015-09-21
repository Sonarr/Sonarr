using System;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.MediaFiles.Movies;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieImportedEvent : IEvent
    {
        public LocalMovie MovieInfo { get; private set; }
        public MovieFile ImportedMovie { get; private set; }
        public Boolean NewDownload { get; private set; }
        public String DownloadClient { get; private set; }
        public String DownloadId { get; private set; }

        public MovieImportedEvent(LocalMovie movieInfo, MovieFile importedMovie, bool newDownload)
        {
            MovieInfo = movieInfo;
            ImportedMovie = importedMovie;
            NewDownload = newDownload;
        }

        public MovieImportedEvent(LocalMovie movieInfo, MovieFile importedMovie, bool newDownload, string downloadClient, string downloadId)
        {
            MovieInfo = movieInfo;
            ImportedMovie = importedMovie;
            NewDownload = newDownload;
            DownloadClient = downloadClient;
            DownloadId = downloadId;
        }
    }
}