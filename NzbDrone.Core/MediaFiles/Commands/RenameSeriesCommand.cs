using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameSeriesCommand : ICommand
    {
        public String CommandId { get; set; }
        public int SeriesId { get; private set; }

        public RenameSeriesCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }

        public RenameSeriesCommand(int seriesId)
        {
            CommandId = HashUtil.GenerateCommandId();
            SeriesId = seriesId;
        }
    }
}