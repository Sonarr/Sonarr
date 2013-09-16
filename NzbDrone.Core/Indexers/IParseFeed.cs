using System.Collections.Generic;
using System.IO;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IParseFeed
    {
        IEnumerable<ReleaseInfo> Process(string source, string url);
    }
}