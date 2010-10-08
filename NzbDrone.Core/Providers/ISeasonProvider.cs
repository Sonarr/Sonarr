using System.Collections.Generic;
using NzbDrone.Core.Entities;

namespace NzbDrone.Core.Providers
{
    public interface ISeasonProvider
    {
        Season GetSeason(int seasonId);
        List<Season> GetSeasongs(int seriesId);
        void EnsureSeason(int seriesId, int seasonId, int seasonNumber);
        int SaveSeason(Season season);
    }
}