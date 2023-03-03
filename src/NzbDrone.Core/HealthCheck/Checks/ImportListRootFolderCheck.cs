using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(SeriesDeletedEvent))]
    [CheckOn(typeof(SeriesMovedEvent))]
    [CheckOn(typeof(EpisodeImportedEvent), CheckOnCondition.FailedOnly)]
    [CheckOn(typeof(EpisodeImportFailedEvent), CheckOnCondition.SuccessfulOnly)]
    public class ImportListRootFolderCheck : HealthCheckBase
    {
        private readonly IImportListFactory _importListFactory;
        private readonly IDiskProvider _diskProvider;

        public ImportListRootFolderCheck(IImportListFactory importListFactory, IDiskProvider diskProvider)
        {
            _importListFactory = importListFactory;
            _diskProvider = diskProvider;
        }

        public override HealthCheck Check()
        {
            var importLists = _importListFactory.All();
            var missingRootFolders = new Dictionary<string, List<ImportListDefinition>>();

            foreach (var importList in importLists)
            {
                var rootFolderPath = importList.RootFolderPath;

                if (missingRootFolders.ContainsKey(rootFolderPath))
                {
                    missingRootFolders[rootFolderPath].Add(importList);

                    continue;
                }

                if (!_diskProvider.FolderExists(rootFolderPath))
                {
                    missingRootFolders.Add(rootFolderPath, new List<ImportListDefinition> { importList });
                }
            }

            if (missingRootFolders.Any())
            {
                if (missingRootFolders.Count == 1)
                {
                    var missingRootFolder = missingRootFolders.First();
                    return new HealthCheck(GetType(), HealthCheckResult.Error, $"Missing root folder for import list(s): {FormatRootFolder(missingRootFolder.Key, missingRootFolder.Value)}", "#import-list-missing-root-folder");
                }

                var message = string.Format("Multiple root folders are missing for import lists: {0}", string.Join(" | ", missingRootFolders.Select(m => FormatRootFolder(m.Key, m.Value))));
                return new HealthCheck(GetType(), HealthCheckResult.Error, message, "#import-list-missing-root-folder");
            }

            return new HealthCheck(GetType());
        }

        private string FormatRootFolder(string rootFolderPath, List<ImportListDefinition> importLists)
        {
            return $"{rootFolderPath} ({string.Join(", ", importLists.Select(l => l.Name))})";
        }
    }
}
