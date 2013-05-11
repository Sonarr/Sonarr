namespace NzbDrone.Common.Messaging
{
    public class TestCommand : ICommand
    {
        public TestCommand()
        {
            Duration = 4000;
        }

        public int Duration { get; set; }

    }
}