using System;

namespace NzbDrone.Common.Messaging
{
    public interface ICommand : IMessage
    {
        String CommandId { get; }
    }
}