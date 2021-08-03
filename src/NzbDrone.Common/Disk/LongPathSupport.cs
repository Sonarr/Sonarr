using System;
using System.IO;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Disk
{
    public static class LongPathSupport
    {
        private static int MAX_PATH;
        private static int MAX_NAME;

        public static void Enable()
        {
            // Mono has an issue with enabling long path support via app.config.
            // This works for both mono and .net on Windows.
            AppContext.SetSwitch("Switch.System.IO.UseLegacyPathHandling", false);
            AppContext.SetSwitch("Switch.System.IO.BlockLongPaths", false);

            DetectLongPathLimits();
        }

        private static void DetectLongPathLimits()
        {
            if (!int.TryParse(Environment.GetEnvironmentVariable("MAX_PATH"), out MAX_PATH))
            {
                if (OsInfo.IsLinux)
                {
                    MAX_PATH = 4096;
                }
                else
                {
                    try
                    {
                        // Windows paths can be up to 32,767 characters long, but each component of the path must be less than 255.
                        // If the OS does not have Long Path enabled, then the following will throw an exception
                        // ref: https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation
                        Path.GetDirectoryName($@"C:\{new string('a', 254)}\{new string('a', 254)}");
                        MAX_PATH = 4096;
                    }
                    catch
                    {
                        MAX_PATH = 260 - 1;
                    }
                }
            }

            if (!int.TryParse(Environment.GetEnvironmentVariable("MAX_NAME"), out MAX_NAME))
            {
                MAX_NAME = 255;
            }
        }

        public static int MaxFilePathLength
        {
            get
            {
                if (MAX_PATH == 0)
                {
                    DetectLongPathLimits();
                }

                return MAX_PATH;
            }
        }

        public static int MaxFileNameLength
        {
            get
            {
                if (MAX_NAME == 0)
                {
                    DetectLongPathLimits();
                }

                return MAX_NAME;
            }
        }
    }
}
