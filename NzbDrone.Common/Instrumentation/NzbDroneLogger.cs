using System;
using System.Diagnostics;
using NLog;

namespace NzbDrone.Common.Instrumentation
{
    public static class NzbDroneLogger
    {
        public static Logger GetLogger()
        {
            string loggerName;
            Type declaringType;
            int framesToSkip = 1;
            do
            {
                var frame = new StackFrame(framesToSkip, false);
                var method = frame.GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    loggerName = method.Name;
                    break;
                }

                framesToSkip++;
                loggerName = declaringType.Name;
            } while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return LogManager.GetLogger(loggerName);
        }

        public static Logger GetLogger(object obj)
        {
            return LogManager.GetLogger(obj.GetType().Name);
        }

        public static Logger GetLogger<T>()
        {
            return LogManager.GetLogger(typeof(T).Name);
        }
    }
}