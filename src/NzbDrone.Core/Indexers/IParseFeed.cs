using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IParseFeed
    {
        IEnumerable<ReleaseInfo> Process(string source, string url);
    }
}