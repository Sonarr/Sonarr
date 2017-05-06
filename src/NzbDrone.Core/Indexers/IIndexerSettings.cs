using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerSettings : IProviderConfig
    {
        string BaseUrl { get; set; }
    }
}
