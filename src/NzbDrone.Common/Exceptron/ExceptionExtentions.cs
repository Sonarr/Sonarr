using System;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Exceptron
{
    public static class ExceptionExtentions
    {
        private const string IGNORE_FLAG = "exceptron_ignore";

        public static Exception ExceptronIgnoreOnMono(this Exception exception)
        {
            if (OsInfo.IsNotWindows)
            {
                exception.ExceptronIgnore();
            }

            return exception;
        }

        public static Exception ExceptronIgnore(this Exception exception)
        {
            exception.Data.Add(IGNORE_FLAG, true);
            return exception;
        }

        public static bool ExceptronShouldIgnore(this Exception exception)
        {
            return exception.Data.Contains(IGNORE_FLAG);
        }
    }
}