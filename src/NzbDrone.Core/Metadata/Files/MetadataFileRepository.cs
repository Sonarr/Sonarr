using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Metadata.Files
{
    public interface IMetadataFileRepository : IBasicRepository<MetadataFile>
    {
        void DeleteForSeries(int seriesId);
        void DeleteForSeason(int seriesId, int seasonNumber);
        void DeleteForEpisodeFile(int episodeFileId);
        List<MetadataFile> GetFilesBySeries(int seriesId);
        List<MetadataFile> GetFilesBySeason(int seriesId, int seasonNumber);
        List<MetadataFile> GetFilesByEpisodeFile(int episodeFileId);
        MetadataFile FindByPath(string path);
    }

    public class MetadataFileRepository : BasicRepository<MetadataFile>, IMetadataFileRepository
    {
        public MetadataFileRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void DeleteForSeries(int seriesId)
        {
            Delete(c => c.SeriesId == seriesId);
        }

        public void DeleteForSeason(int seriesId, int seasonNumber)
        {
            Delete(c => c.SeriesId == seriesId && c.SeasonNumber == seasonNumber);
        }

        public void DeleteForEpisodeFile(int episodeFileId)
        {
            Delete(c => c.EpisodeFileId == episodeFileId);
        }

        public List<MetadataFile> GetFilesBySeries(int seriesId)
        {
            return Query.Where(c => c.SeriesId == seriesId);
        }

        public List<MetadataFile> GetFilesBySeason(int seriesId, int seasonNumber)
        {
            return Query.Where(c => c.SeriesId == seriesId && c.SeasonNumber == seasonNumber);
        }

        public List<MetadataFile> GetFilesByEpisodeFile(int episodeFileId)
        {
            return Query.Where(c => c.EpisodeFileId == episodeFileId);
        }

        public MetadataFile FindByPath(string path)
        {
            return Query.Where(c => c.RelativePath == path).SingleOrDefault();
        }
    }
}
