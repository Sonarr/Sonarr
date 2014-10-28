using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IParseIndexerResponse
    {
        IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse);
    }
}
