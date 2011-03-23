using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public interface IUpcomingEpisodesProvider
    {
        UpcomingEpisodesModel Upcoming();
        List<Episode> Yesterday();
        List<Episode> Today();
        List<Episode> Week();
    }
}
