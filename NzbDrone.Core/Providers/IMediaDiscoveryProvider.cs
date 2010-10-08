using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Providers
{
    public interface IMediaDiscoveryProvider
    {
        bool DiscoveredMedia { get; }
        List<IMediaProvider> Providers { get; }
    }
}
