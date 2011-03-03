using System.Collections.Generic;
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
        List<EpisodeFile> Scan(Series series);
        List<EpisodeFile> Scan(Series series, string path);
        EpisodeFile ImportFile(Series series, string filePath);
        string GenerateEpisodePath(EpisodeModel episode);
        void CleanUp(List<EpisodeFile> files);
        void DeleteFromDb(int fileId);
        void DeleteFromDisk(int fileId, string path);
        void Update(EpisodeFile episodeFile);
        EpisodeFile GetEpisodeFile(int episodeFileId);
        List<EpisodeFile> GetEpisodeFiles();
    }
}