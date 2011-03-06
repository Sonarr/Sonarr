using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public interface IExtenalNotificationProvider
    {
        void OnGrab(string message);
        void OnDownload(EpisodeFile episodeFile);
        void OnRename(EpisodeFile episodeFile);
    }
}
