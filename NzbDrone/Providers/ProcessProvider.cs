using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NLog;

namespace NzbDrone.Providers
{
    public class ProcessProvider
    {
        private static readonly Logger Logger = LogManager.GetLogger("ProcessProvider");


        public virtual void SetPriority(int processId, ProcessPriorityClass priority)
        {
            var process = Process.GetProcessById(processId);

            Logger.Info("Updating [{0}] process priority from {1} to {2}",
                         process.ProcessName,
                         process.PriorityClass,
                         priority);

            process.PriorityClass = priority;
        }

        public virtual ProcessPriorityClass GetProcessPriority(int processId)
        {
            return Process.GetProcessById(processId).PriorityClass;
        }

        public virtual int GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        public virtual Process Start(string path)
        {
            return Process.Start(path);
        }
    }
}
