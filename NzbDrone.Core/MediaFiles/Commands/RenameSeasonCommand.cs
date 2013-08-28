using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameSeasonCommand : ICommand
    {
        public int SeriesId { get; private set; }
        public int SeasonNumber { get; private set; }

        public String CommandId { get; set; }

        public RenameSeasonCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }

        public RenameSeasonCommand(int seriesId, int seasonNumber)
        {
            CommandId = HashUtil.GenerateCommandId();
            SeriesId = seriesId;
            SeasonNumber = seasonNumber;
        }
    }
}