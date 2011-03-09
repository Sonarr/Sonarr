using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Providers
{
    public interface ISyncProvider
    {
        bool BeginSyncUnmappedFolders(List<string> paths);
        List<String> GetUnmappedFolders(string path);
    }
}