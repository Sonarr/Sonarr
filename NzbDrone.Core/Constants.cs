using System.Linq;

namespace NzbDrone.Core
{
    public static class Constants
    {
        public static long IgnoreFileSize
        {
            get
            {
                return 40.Megabytes();
            }
        }
    }
}
