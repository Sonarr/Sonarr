using System;
using System.Collections.Generic;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public interface IEpisodeProvider
    {
        Episode GetEpisode(long id);
        IList<Episode> GetEpisodeBySeries(long seriesId);
        String GetSabTitle(Episode episode);

        /// <summary>
        /// Comprehensive check on whether or not this episode is needed.
        /// </summary>
        /// <param name="episode">Episode that needs to be checked</param>
        /// <returns></returns>
        bool IsNeeded(EpisodeModel episode);

        void RefreshEpisodeInfo(int seriesId);
    }
}