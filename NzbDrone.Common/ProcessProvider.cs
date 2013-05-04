using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;
using NzbDrone.Common.Model;

namespace NzbDrone.Common
{
    public class ProcessProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public const string NzbDroneProcessName = "NzbDrone";
        public const string NzbDroneConsoleProcessName = "NzbDrone.Console";

        public virtual ProcessInfo GetCurrentProcess()
        {
            return ConvertToProcessInfo(Process.GetCurrentProcess());
        }

        public virtual ProcessInfo GetProcessById(int id)
        {
            Logger.Trace("Finding process with Id:{0}", id);

            var processInfo = ConvertToProcessInfo(Process.GetProcesses().Where(p => p.Id == id).FirstOrDefault());

            if (processInfo == null)
            {
                Logger.Warn("Unable to find process with ID {0}", id);
            }
            else
            {
                Logger.Trace("Found process {0}", processInfo.ToString());
            }

            return processInfo;
        }

        public virtual IEnumerable<ProcessInfo> GetProcessByName(string name)
        {
            return Process.GetProcessesByName(name).Select(ConvertToProcessInfo).Where(p => p != null);
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

        public virtual void WaitForExit(Process process)
        {
            Logger.Trace("Waiting for process {0} to exit.", process.ProcessName);
            process.WaitForExit();
        }

        public virtual void Kill(int processId)
        {
            if (processId == 0 || Process.GetProcesses().All(p => p.Id != processId))
            {
                Logger.Warn("Cannot find process with id: {0}", processId);
                return;
            }

            var process = Process.GetProcessById(processId);

            if (process.HasExited)
            {
                return;
            }

            Logger.Info("[{0}]: Killing process", process.Id);
            process.Kill();
            Logger.Info("[{0}]: Waiting for exit", process.Id);
            process.WaitForExit();
            Logger.Info("[{0}]: Process terminated successfully", process.Id);
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
            if (process == null || process.Id <= 0 || process.HasExited) return null;

            return new ProcessInfo
                       {
                           Id = process.Id,
                           Priority = process.PriorityClass,
                           StartPath = process.MainModule.FileName,
                           Name = process.ProcessName
                       };
        }

        public void KillAll(string nzbdrone)
        {
            throw new System.NotImplementedException();
        }
    }
}