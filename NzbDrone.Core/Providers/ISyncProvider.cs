using System;
using System.Collections.Generic;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers
{
    public interface ISyncProvider
    {

        List<String> GetUnmappedFolders(string path);
        bool BeginUpdateNewSeries();
    }
}