using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Update.Commands
{
    public class ApplicationUpdateCommand : ICommand
    {
        public String CommandId { get; private set; }

        public ApplicationUpdateCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}