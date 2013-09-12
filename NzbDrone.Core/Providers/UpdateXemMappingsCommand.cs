using System;
using NzbDrone.Common;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Providers
{
    public class UpdateXemMappingsCommand : Command
    {
        public int? SeriesId { get; set; }

        public UpdateXemMappingsCommand(int? seriesId)
        {
            SeriesId = seriesId;
        }
    }
}