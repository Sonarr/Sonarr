using System;

namespace NzbDrone.Console
{
    public static class AppMain
    {
        public static void Main(string[] args)
        {
            try
            {

                NzbDrone.AppMain.Main(args);

            }
            catch(Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }

            System.Console.WriteLine("Press enter to exit...");
            System.Console.ReadLine();
        }
    }
}