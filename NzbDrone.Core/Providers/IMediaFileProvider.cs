using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public interface IMediaFileProvider
    {
        /// <summary>
        /// Scans the specified series folder for media files
        /// </summary>
        /// <param name="series">The series to be scanned</param>
        void Scan(Series series);
    }
}