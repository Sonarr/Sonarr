using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Model.Xbmc;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public interface IApiProvider
    {
        void Notify(XbmcSettings settings, string title, string message);
        void Update(XbmcSettings settings, Series series);
        void Clean(XbmcSettings settings);
        List<ActivePlayer> GetActivePlayers(XbmcSettings settings);
        bool CheckForError(string response);
        string GetSeriesPath(XbmcSettings settings, Series series);
        bool CanHandle(XbmcVersion version);
    }
}
