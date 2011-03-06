using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers
{
    public interface IExtenalNotificationProvider
    {
        void OnGrab(string message);
        void OnDownload(EpisodeRenameModel erm);
        void OnRename(EpisodeRenameModel erm);
    }
}
