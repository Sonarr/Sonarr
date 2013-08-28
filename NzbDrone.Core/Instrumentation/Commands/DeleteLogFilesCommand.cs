using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Instrumentation.Commands
{
    public class DeleteLogFilesCommand : ICommand
    {
        public String CommandId { get; set; }

        public DeleteLogFilesCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}