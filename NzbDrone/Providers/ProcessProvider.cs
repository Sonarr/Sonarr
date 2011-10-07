using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;

namespace NzbDrone.Providers
{
    public class ProcessProvider
    {
        private static readonly Logger Logger = LogManager.GetLogger("Host.ProcessProvider");


        public virtual ProcessInfo GetCurrentProcess()
        {
            return ConvertToProcessInfo(Process.GetCurrentProcess());
        }

        public virtual ProcessInfo GetProcessById(int id)
        {
            return ConvertToProcessInfo(Process.GetProcesses().Where(p => p.Id == id).FirstOrDefault());
        }

        public virtual IEnumerable<ProcessInfo> GetProcessByName(string name)
        {
            return Process.GetProcessesByName(name).Select(ConvertToProcessInfo);
        }

        public virtual void Start(string path)
        {
            Process.Start(path);
        }

        public virtual Process Start(ProcessStartInfo startInfo)
        {
            Logger.Info("Starting process. [{0}]", startInfo.FileName);

            var process = new Process
                              {
                                  StartInfo = startInfo
                              };
            process.Start();
            return process;
        }

        public virtual void Kill(int processId)
        {
            if (processId == 0) return;
            if (!Process.GetProcesses().Any(p => p.Id == processId)) return;

            var process = Process.GetProcessById(processId);

            if (!process.HasExited)
            {
                Logger.Info("[{0}]Killing process", process.Id);
                process.Kill();
                Logger.Info("[{0}]Waiting for exit", process.Id);
                process.WaitForExit();
                Logger.Info("[{0}]Process terminated successfully", process.Id);
            }
        }

        public virtual void SetPriority(int processId, ProcessPriorityClass priority)
        {
            var process = Process.GetProcessById(processId);

            Logger.Info("Updating [{0}] process priority from {1} to {2}",
                        process.ProcessName,
                        process.PriorityClass,
                        priority);

            process.PriorityClass = priority;
        }

        private static ProcessInfo ConvertToProcessInfo(Process process)
        {
            if (process == null) return null;

            return new ProcessInfo
                           {
                               Id = process.Id,
                               Priority = process.PriorityClass,
                               StartPath = process.MainModule.FileName
                           };
        }
    }
}
