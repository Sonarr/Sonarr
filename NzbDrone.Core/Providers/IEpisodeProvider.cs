using System;
using System.Collections.Generic;
using NzbDrone.Core.Entities.Episode;

namespace NzbDrone.Core.Providers
{
    public interface IEpisodeProvider
    {
        EpisodeInfo GetEpisode(long id);
        void UpdateEpisode(EpisodeInfo episode);
        IList<EpisodeInfo> GetEpisodesBySeason(long seasonId);
        IList<EpisodeInfo> GetEpisodeBySeries(long seriesId);
        String GetSabTitle(BasicEpisode episode);

        /// <summary>
        /// Comprehensive check on whether or not this episode is needed.
        /// </summary>
        /// <param name="episode">Episode that needs to be checked</param>
        /// <returns></returns>
        bool IsNeeded(RemoteEpisode episode);

        void RefreshSeries(int seriesId);
    }
}