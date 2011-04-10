using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class HistoryProvider
    {
        private readonly IRepository _sonicRepo;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public HistoryProvider(IRepository sonicRepo)
        {
            _sonicRepo = sonicRepo;
        }

        public HistoryProvider()
        {
        }

        #region HistoryProvider Members

        public virtual List<History> AllItems()
        {
            return _sonicRepo.All<History>().ToList();
        }

        public virtual void Purge()
        {
            var all = _sonicRepo.All<History>();
            _sonicRepo.DeleteMany(all);
            Logger.Info("History has been Purged");
        }

        public virtual void Trim()
        {
            var old = _sonicRepo.All<History>().Where(h => h.Date < DateTime.Now.AddDays(-30));
            _sonicRepo.DeleteMany(old);
            Logger.Info("History has been trimmed, items older than 30 days have been removed");
        }

        public virtual void Insert(History item)
        {
            _sonicRepo.Add(item);
            //Logger.Info("Item added to history: {0} - {1}x{2:00}", item.Episode.Series.Title, item.Episode.SeasonNumber, item.Episode.EpisodeNumber);
        }

        public virtual bool Exists(int episodeId, QualityTypes quality, bool proper)
        {
            //Looks for the existance of this episode in History
            if (_sonicRepo.Exists<History>(h => h.EpisodeId == episodeId && (QualityTypes)h.Quality == quality && h.IsProper == proper))
                return true;

            Logger.Debug("Episode not in History: {0}", episodeId);
            return false;
        }

        #endregion
    }
}
