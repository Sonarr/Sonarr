using System.IO;
using System.Text.RegularExpressions;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;

namespace NzbDrone.Api.MediaCovers
{
    public class MediaCoverModule : NzbDroneApiModule
    {
        private static readonly Regex RegexResizedImage = new Regex(@"-\d+\.jpg$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const string MEDIA_COVER_SERIES_ROUTE = @"/(?<seriesId>\d+)/(?<filename>(.+)\.(jpg|png|gif))";
        private const string MEDIA_COVER_MOVIES_ROUTE = @"/(?<moviesTag>movies)/(?<movieId>\d+)/(?<filename>(.+)\.(jpg|png|gif))";

        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;

        public MediaCoverModule(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider) : base("MediaCover")
        {
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;

            Get[MEDIA_COVER_SERIES_ROUTE] = options => GetMediaCover(options.seriesId, options.filename, MediaCoverOrigin.Series);
            Get[MEDIA_COVER_MOVIES_ROUTE] = options => GetMediaCover(options.movieId, options.filename, MediaCoverOrigin.Movie);
        }

        private Response GetMediaCover(int id, string filename, MediaCoverOrigin coverOrigin)
        {
            var movie = coverOrigin == MediaCoverOrigin.Movie ? "movies" : "";
            var filePath = Path.Combine(_appFolderInfo.GetAppDataPath(), "MediaCover", movie, id.ToString(), filename);

            if (!_diskProvider.FileExists(filePath) || _diskProvider.GetFileSize(filePath) == 0)
            {
                // Return the full sized image if someone requests a non-existing resized one.
                // TODO: This code can be removed later once everyone had the update for a while.
                var basefilePath = RegexResizedImage.Replace(filePath, ".jpg");
                if (basefilePath == filePath || !_diskProvider.FileExists(basefilePath))
                {
                    return new NotFoundResponse();
                }
                filePath = basefilePath;
            }

            return new StreamResponse(() => File.OpenRead(filePath), MimeTypes.GetMimeType(filePath));
        }
    }
}
