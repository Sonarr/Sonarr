using System.Threading;

namespace NzbDrone.Common.Messaging
{
    public class TestCommandExecutor : IExecute<TestCommand>
    {
        public void Execute(TestCommand message)
        {
            Thread.Sleep(message.Duration);
        }
    }
}