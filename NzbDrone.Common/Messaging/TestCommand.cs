using System;

namespace NzbDrone.Common.Messaging
{
    public class TestCommand : ICommand
    {
        public int Duration { get; set; }
        public String CommandId { get; private set; }
        
        public TestCommand()
        {
            Duration = 4000;
        }
    }
}