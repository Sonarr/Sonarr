using System;
using NzbDrone.Common;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeasonSearchCommand : Command
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }
    }
}