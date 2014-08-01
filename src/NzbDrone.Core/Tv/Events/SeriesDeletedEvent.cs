using System;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class SeriesDeletedEvent : IEvent
    {
        public Series Series { get; private set; }
        public Boolean DeleteFiles { get; private set; }

        public SeriesDeletedEvent(Series series, Boolean deleteFiles)
        {
            Series = series;
            DeleteFiles = deleteFiles;
        }
    }
}