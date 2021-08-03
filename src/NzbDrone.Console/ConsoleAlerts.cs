using NzbDrone.Host;

namespace NzbDrone.Console
{
    public class ConsoleAlerts : IUserAlert
    {
        public void Alert(string message)
        {
            System.Console.WriteLine();
            System.Console.WriteLine(message);
            System.Console.WriteLine("Press enter to continue");
            System.Console.ReadLine();
        }
    }
}
