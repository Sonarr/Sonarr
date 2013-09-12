namespace NzbDrone.Core.Messaging.Commands
{
    public class TestCommand : Command
    {
        public int Duration { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }
        
        public TestCommand()
        {
            Duration = 4000;
        }
    }
}