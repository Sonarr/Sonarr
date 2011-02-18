using NzbDrone.Core.Model;
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

        EpisodeFile ImportFile(Series series, string filePath);
        string GenerateEpisodePath(EpisodeModel episode);
        void DeleteFromDb(int fileId);
        void DeleteFromDisk(int fileId, string path);
    }
}