using System;
using System.Collections.Generic;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers
{
    public interface ISyncProvider
    {
        bool BeginSyncUnmappedFolders(List<SeriesMappingModel> unmapped);
        List<String> GetUnmappedFolders(string path);
    }
}