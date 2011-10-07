using System;
using NLog;
using Ninject;

namespace NzbDrone
{
    internal static class Program
    {
        public static readonly StandardKernel Kernel = new StandardKernel();

        private static readonly Logger Logger = LogManager.GetLogger("Main");

        private static void Main()
        {
            try
            {
                Console.WriteLine("Starting Console.");
                Kernel.Get<Application>().Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Logger.Fatal(e.ToString());
            }

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}