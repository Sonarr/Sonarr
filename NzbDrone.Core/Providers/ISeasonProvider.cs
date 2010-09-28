using System.Collections.Generic;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public interface ISeasonProvider
    {
        Season GetSeason(long seasonId);
        List<Season> GetSeasongs(long seriesId);

        int SaveSeason(Season season);
    }

}