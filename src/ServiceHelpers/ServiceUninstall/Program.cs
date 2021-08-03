namespace ServiceUninstall
{
    public static class Program
    {
        private static void Main()
        {
            ServiceHelper.Run(@"/u");
        }
    }
}
