using System;
using NzbDrone.Common;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class CleanMediaFileDb : Command
    {
        public int SeriesId { get; private set; }

        public CleanMediaFileDb(int seriesId)
        {
            SeriesId = seriesId;
        }
    }
}