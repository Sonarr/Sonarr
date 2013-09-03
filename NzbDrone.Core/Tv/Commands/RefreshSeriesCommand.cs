using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Commands
{
    public class RefreshSeriesCommand : ICommand
    {
        public String CommandId { get; private set; }
        public int? SeriesId { get; set; }

        public RefreshSeriesCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }

        public RefreshSeriesCommand(int? seriesId)
        {
            CommandId = HashUtil.GenerateCommandId();

            SeriesId = seriesId;
        }
    }
}