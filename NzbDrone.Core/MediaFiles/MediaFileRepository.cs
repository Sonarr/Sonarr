using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<EpisodeFile>
    {
        EpisodeFile GetFileByPath(string path);
        List<EpisodeFile> GetFilesBySeries(int seriesId);
        bool Exists(string path);
    }


    public class MediaFileRepository : BasicRepository<EpisodeFile>, IMediaFileRepository
    {
        public MediaFileRepository(IDatabase database, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
        {
        }


        public EpisodeFile GetFileByPath(string path)
        {
            return Query.SingleOrDefault(c => c.Path == path);
        }

        public bool Exists(string path)
        {
            return Query.Any(c => c.Path == path);
        }

        public List<EpisodeFile> GetFilesBySeries(int seriesId)
        {
            return Query.Where(c => c.SeriesId == seriesId).ToList();
        }

    }
}