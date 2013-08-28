using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Providers
{
    public class UpdateXemMappingsCommand : ICommand
    {
        public String CommandId { get; set; }
        public int? SeriesId { get; private set; }

        public UpdateXemMappingsCommand(int? seriesId)
        {
            CommandId = HashUtil.GenerateCommandId();

            SeriesId = seriesId;
        }
    }
}