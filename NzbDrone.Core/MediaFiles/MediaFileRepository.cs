using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<EpisodeFile>
    {
        EpisodeFile GetFileByPath(string path);
        List<EpisodeFile> GetFilesBySeries(int seriesId);
        List<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber);
        bool Exists(string path);
    }


    public class MediaFileRepository : BasicRepository<EpisodeFile>, IMediaFileRepository
    {
        public MediaFileRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public EpisodeFile GetFileByPath(string path)
        {
            return Query.SingleOrDefault(c => c.Path == path);
        }

        public List<EpisodeFile> GetFilesBySeries(int seriesId)
        {
            return Query.Where(c => c.SeriesId == seriesId).ToList();
        }

        public List<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber)
        {
            return Query.Where(c => c.SeriesId == seriesId)
                        .AndWhere(c => c.SeasonNumber == seasonNumber)
                        .ToList();
        }

        public bool Exists(string path)
        {
            return Query.Any(c => c.Path == path);
        }
    }
}