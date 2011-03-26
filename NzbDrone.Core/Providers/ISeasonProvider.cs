using System.Collections.Generic;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public interface ISeasonProvider
    {
        Season GetSeason(int seasonId);
        Season GetSeason(int seriesId, int seasonNumber);
        List<Season> GetSeasons(int seriesId);
        Season GetLatestSeason(int seriesId);
        void EnsureSeason(int seriesId, int seasonId, int seasonNumber);
        int SaveSeason(Season season);
        bool IsIgnored(int seasonId);
        bool IsIgnored(int seriesId, int seasonNumber);
        void DeleteSeason(int seasonId);
    }
}