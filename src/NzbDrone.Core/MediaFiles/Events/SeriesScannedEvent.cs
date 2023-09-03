using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class SeriesScannedEvent : IEvent
    {
        public Series Series { get; private set; }
        public List<string> PossibleExtraFiles { get; set; }

        public SeriesScannedEvent(Series series, List<string> possibleExtraFiles)
        {
            Series = series;
            PossibleExtraFiles = possibleExtraFiles;
        }
    }
}
