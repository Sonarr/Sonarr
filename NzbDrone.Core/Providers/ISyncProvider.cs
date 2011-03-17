using System;
using System.Collections.Generic;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers
{
    public interface ISyncProvider
    {
        bool BeginSyncUnmappedFolders(List<SeriesMappingModel> unmapped);
        bool BeginAddNewSeries(string dir, int seriesId, string seriesName);
        bool BeginAddExistingSeries(string path, int seriesId);
        List<String> GetUnmappedFolders(string path);
    }
}