using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Providers
{
    public interface IRenameProvider
    {
        void RenameAll();
        void RenameSeries(int seriesId);
        void RenameSeason(int seasonId);
        void RenameEpisode(int episodeId);
    }
}
