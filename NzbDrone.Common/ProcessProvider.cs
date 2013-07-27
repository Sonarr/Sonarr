using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Model;

namespace NzbDrone.Common
{
    public interface IProcessProvider
    {
        ProcessInfo GetCurrentProcess();
        ProcessInfo GetProcessById(int id);
        IEnumerable<ProcessInfo> GetProcessByName(string name);
        Process Start(string path);
        Process Start(ProcessStartInfo startInfo);
        void WaitForExit(Process process);
        void SetPriority(int processId, ProcessPriorityClass priority);
        void KillAll(string processName);
    }

    public class ProcessProvider : IProcessProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public const string NzbDroneProcessName = "NzbDrone";
        public const string NzbDroneConsoleProcessName = "NzbDrone.Console";

        public ProcessInfo GetCurrentProcess()
        {
            return ConvertToProcessInfo(Process.GetCurrentProcess());
        }

        public ProcessInfo GetProcessById(int id)
        {
            Logger.Trace("Finding process with Id:{0}", id);

            var processInfo = ConvertToProcessInfo(Process.GetProcesses().FirstOrDefault(p => p.Id == id));

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

        public IEnumerable<ProcessInfo> GetProcessByName(string name)
        {
            if (OsInfo.IsMono)
            {
                var mono = Process.GetProcessesByName("mono");

                return mono.Where(process => process.Modules.Cast<ProcessModule>().Any(module => module.ModuleName.ToLower() == name + ".exe"))
                                     .Select(ConvertToProcessInfo);
            }

            return Process.GetProcessesByName(name).Select(ConvertToProcessInfo).Where(p => p != null);
        }

        public Process Start(string path)
        {
            return Start(new ProcessStartInfo(path));
        }

        public Process Start(ProcessStartInfo startInfo)
        {
            Logger.Info("Starting process. [{0}]", startInfo.FileName);

            var process = new Process
                              {
                                  StartInfo = startInfo
                              };
            process.Start();

            return process;
        }

        public void WaitForExit(Process process)
        {
            Logger.Trace("Waiting for process {0} to exit.", process.ProcessName);
            process.WaitForExit();
        }



        public void SetPriority(int processId, ProcessPriorityClass priority)
        {
            var process = Process.GetProcessById(processId);

            Logger.Info("Updating [{0}] process priority from {1} to {2}",
                        process.ProcessName,
                        process.PriorityClass,
                        priority);

            process.PriorityClass = priority;
        }

        public void KillAll(string processName)
        {
            var processToKill = GetProcessByName(processName);

            foreach (var processInfo in processToKill)
            {
                Kill(processInfo.Id);
            }
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



        private void Kill(int processId)
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
    }
}