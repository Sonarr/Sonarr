using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class HistoryProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _repository;

        public HistoryProvider(IRepository repository)
        {
            _repository = repository;
        }

        public HistoryProvider()
        {
        }

        public virtual IQueryable<History> AllItems()
        {
            return _repository.All<History>();
        }

        public virtual void Purge()
        {
            _repository.DeleteMany(AllItems());
            Logger.Info("History has been Purged");
        }

        public virtual void Trim()
        {
            var old = AllItems().Where(h => h.Date < DateTime.Now.AddDays(-30));
            _repository.DeleteMany(old);
            Logger.Info("History has been trimmed, items older than 30 days have been removed");
        }

        public virtual void Add(History item)
        {
            _repository.Add(item);
            Logger.Debug("Item added to history: {0}", item.NzbTitle);
        }

        public virtual Quality GetBestQualityInHistory(long episodeId)
        {
            var history = AllItems().Where(c => c.EpisodeId == episodeId).ToList().Select(d => new Quality(d.Quality, d.IsProper));
            history.OrderBy(q => q);
            return history.FirstOrDefault();
        }

        public virtual void Delete(int historyId)
        {
            _repository.Delete<History>(historyId);
        }

        public virtual void DeleteForEpisode(int episodeId)
        {
            _repository.DeleteMany<History>(h => h.EpisodeId == episodeId);
        }
    }
}