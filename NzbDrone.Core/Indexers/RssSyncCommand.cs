using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Indexers
{
    public class RssSyncCommand : ICommand
    {
        public String CommandId { get; set; }

        public RssSyncCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}