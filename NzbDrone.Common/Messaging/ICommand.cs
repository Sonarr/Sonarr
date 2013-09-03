using System;
using System.Collections.Generic;

namespace NzbDrone.Common.Messaging
{
    public interface ICommand : IMessage
    {
        String CommandId { get; }
    }
}