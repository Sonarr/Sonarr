using System;
using System.Collections.Generic;
using NzbDrone.Core.Entities.Episode;

namespace NzbDrone.Core.Providers
{
    public interface IEpisodeProvider
    {
        BasicEpisode GetEpisode(long id);
        BasicEpisode UpdateEpisode(BasicEpisode episode);
        IList<BasicEpisode> GetEpisodesBySeason(long seasonId);
        IList<BasicEpisode> GetEpisodeBySeries(long seriesId);
        String GetSabTitle(BasicEpisode episode);

        /// <summary>
        /// Comprehensive check on whether or not this episode is needed.
        /// </summary>
        /// <param name="episode">Episode that needs to be checked</param>
        /// <returns></returns>
        bool IsNeeded(BasicEpisode episode);

        void RefreshSeries(int seriesId);
    }
}