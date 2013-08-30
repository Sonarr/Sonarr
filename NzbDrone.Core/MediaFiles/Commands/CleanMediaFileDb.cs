using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class CleanMediaFileDb : ICommand
    {
        public String CommandId { get; set; }
        public int SeriesId { get; private set; }

        public CleanMediaFileDb()
        {
            CommandId = HashUtil.GenerateCommandId();
        }

        public CleanMediaFileDb(int seriesId)
        {
            CommandId = HashUtil.GenerateCommandId();
            SeriesId = seriesId;
        }
    }
}