using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Instrumentation.Commands
{
    public class ClearLogCommand : ICommand
    {
        public String CommandId { get; set; }

        public ClearLogCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}