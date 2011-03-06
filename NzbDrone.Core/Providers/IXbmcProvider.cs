using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Providers
{
    public interface IXbmcProvider
    {
        void Notify(string header, string message);
        void Update(int seriesId);
        void Clean();
    }
}
