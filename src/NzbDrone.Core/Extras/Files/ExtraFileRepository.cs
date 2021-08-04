using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Files
{
    public interface IExtraFileRepository<TExtraFile> : IBasicRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        void DeleteForSeries(int seriesId);
        void DeleteForSeason(int seriesId, int seasonNumber);
        void DeleteForEpisodeFile(int episodeFileId);
        List<TExtraFile> GetFilesBySeries(int seriesId);
        List<TExtraFile> GetFilesBySeason(int seriesId, int seasonNumber);
        List<TExtraFile> GetFilesByEpisodeFile(int episodeFileId);
        TExtraFile FindByPath(int seriesId, string path);
    }

    public class ExtraFileRepository<TExtraFile> : BasicRepository<TExtraFile>, IExtraFileRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        public ExtraFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
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

        public List<TExtraFile> GetFilesBySeries(int seriesId)
        {
            return Query(c => c.SeriesId == seriesId);
        }

        public List<TExtraFile> GetFilesBySeason(int seriesId, int seasonNumber)
        {
            return Query(c => c.SeriesId == seriesId && c.SeasonNumber == seasonNumber);
        }

        public List<TExtraFile> GetFilesByEpisodeFile(int episodeFileId)
        {
            return Query(c => c.EpisodeFileId == episodeFileId);
        }

        public TExtraFile FindByPath(int seriesId, string path)
        {
            return Query(c => c.SeriesId == seriesId && c.RelativePath == path).SingleOrDefault();
        }
    }
}
