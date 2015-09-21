using NzbDrone.Core.Datastore;
using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public class FileMoveResult
    {
        public FileMoveResult()
        {
            OldFiles = new List<MediaModelBase>();
        }

        public MediaModelBase File { get; set; }
        public List<MediaModelBase> OldFiles { get; set; }
    }
}
