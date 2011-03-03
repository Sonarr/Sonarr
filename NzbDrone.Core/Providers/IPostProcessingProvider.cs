using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Providers
{
    public interface IPostProcessingProvider
    {
        void ProcessEpisode(string dir, string nzbName);
    }
}
