using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public class UpdateSceneMappingCommand : ICommand
    {
        public String CommandId { get; private set; }

        public UpdateSceneMappingCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}