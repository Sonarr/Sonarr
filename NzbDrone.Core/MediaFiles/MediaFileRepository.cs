using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<EpisodeFile>
    {
        EpisodeFile GetFileByPath(string path);
        List<EpisodeFile> GetFilesBySeries(int seriesId);
        List<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber);
    }


    public class MediaFileRepository : BasicRepository<EpisodeFile>, IMediaFileRepository
    {
        public MediaFileRepository(IObjectDatabase objectDatabase)
                : base(objectDatabase)
        {
        }


        public EpisodeFile GetFileByPath(string path)
        {
            return Queryable.SingleOrDefault(c => c.Path == path);
        }

        public List<EpisodeFile> GetFilesBySeries(int seriesId)
        {
            return Queryable.Where(c => c.SeriesId == seriesId).ToList();
        }

        public List<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber)
        {
            return Queryable.Where(c => c.SeriesId == seriesId && c.SeasonNumber == seasonNumber).ToList();

        }
    }
}