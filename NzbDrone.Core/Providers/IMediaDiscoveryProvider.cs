using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Providers
{
    interface IMediaDiscoveryProvider
    {
        void Discover();
        bool DiscoveredMedia { get; }
        List<IMediaProvider> Providers { get; }
    }
}
