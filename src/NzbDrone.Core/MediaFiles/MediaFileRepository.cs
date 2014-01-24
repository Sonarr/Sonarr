using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<EpisodeFile>
    {
        List<EpisodeFile> GetFilesBySeries(int seriesId);
        List<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber);
        EpisodeFile FindFileByPath(string path, bool includeExtension = true);
    }


    public class MediaFileRepository : BasicRepository<EpisodeFile>, IMediaFileRepository
    {
        public MediaFileRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
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

        public EpisodeFile FindFileByPath(string path, bool includeExtension = true)
        {
            if (includeExtension)
            {
                return Query.SingleOrDefault(c => c.Path == path);
            }

            return Query.SingleOrDefault(c => c.Path.StartsWith(Path.ChangeExtension(path, "")));
        }
    }
}