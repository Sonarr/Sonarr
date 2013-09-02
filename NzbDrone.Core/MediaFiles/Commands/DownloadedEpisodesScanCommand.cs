using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class DownloadedEpisodesScanCommand : ICommand
    {
        public String CommandId { get; private set; }

        public DownloadedEpisodesScanCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}