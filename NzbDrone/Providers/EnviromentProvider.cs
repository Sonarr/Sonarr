using System;

namespace NzbDrone.Providers
{
    public class EnviromentProvider
    {
        public virtual String LogPath
        {
            get { return Environment.CurrentDirectory; }
        }

        public virtual bool IsUserInteractive
        {
            get { return Environment.UserInteractive; }
        }
    }
}