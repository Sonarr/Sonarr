using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Instrumentation.Commands
{
    public class TrimLogCommand : ICommand
    {
        public String CommandId { get; set; }

        public TrimLogCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}