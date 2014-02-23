using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Model;

namespace NzbDrone.Common.Processes
{
    public interface IProcessProvider
    {
        ProcessInfo GetCurrentProcess();
        ProcessInfo GetProcessById(int id);
        List<ProcessInfo> FindProcessByName(string name);
        void OpenDefaultBrowser(string url);
        void WaitForExit(Process process);
        void SetPriority(int processId, ProcessPriorityClass priority);
        void KillAll(string processName);
        void Kill(int processId);
        bool Exists(string processName);
        ProcessPriorityClass GetCurrentProcessPriority();
        Process Start(string path, string args = null, Action<string> onOutputDataReceived = null, Action<string> onErrorDataReceived = null);
        Process SpawnNewProcess(string path, string args = null);
        ProcessOutput StartAndCapture(string path, string args = null);
    }

    public class ProcessProvider : IProcessProvider
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger();

        public const string NZB_DRONE_PROCESS_NAME = "NzbDrone";
        public const string NZB_DRONE_CONSOLE_PROCESS_NAME = "NzbDrone.Console";

        private static List<Process> GetProcessesByName(string name)
        {
            var monoProcesses = Process.GetProcessesByName("mono")
                                       .Where(process => process.Modules.Cast<ProcessModule>().Any(module => module.ModuleName.ToLower() == name.ToLower() + ".exe"));
            return Process.GetProcessesByName(name)
                .Union(monoProcesses).ToList();
        }

        public ProcessInfo GetCurrentProcess()
        {
            return ConvertToProcessInfo(Process.GetCurrentProcess());
        }

        public bool Exists(string processName)
        {
            return GetProcessesByName(processName).Any();
        }

        public ProcessPriorityClass GetCurrentProcessPriority()
        {
            return Process.GetCurrentProcess().PriorityClass;
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

        public List<ProcessInfo> FindProcessByName(string name)
        {
            return Process.GetProcessesByName(name).Select(ConvertToProcessInfo).Where(c => c != null).ToList();
        }

        public void OpenDefaultBrowser(string url)
        {
            Logger.Info("Opening URL [{0}]", url);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo(url)
                    {
                        UseShellExecute = true
                    }
            };

            process.Start();
        }

        public Process Start(string path, string args = null, Action<string> onOutputDataReceived = null, Action<string> onErrorDataReceived = null)
        {
            if (OsInfo.IsMono && path.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                args = path + " " + args;
                path = "mono";
            }

            var logger = LogManager.GetLogger(new FileInfo(path).Name);

            var startInfo = new ProcessStartInfo(path, args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            };


            logger.Info("Starting {0} {1}", path, args);

            var process = new Process
                {
                    StartInfo = startInfo
                };

            process.OutputDataReceived += (sender, eventArgs) =>
            {
                if (string.IsNullOrWhiteSpace(eventArgs.Data)) return;

                logger.Debug(eventArgs.Data);

                if (onOutputDataReceived != null)
                {
                    onOutputDataReceived(eventArgs.Data);
                }
            };

            process.ErrorDataReceived += (sender, eventArgs) =>
            {
                if (string.IsNullOrWhiteSpace(eventArgs.Data)) return;

                logger.Error(eventArgs.Data);

                if (onErrorDataReceived != null)
                {
                    onErrorDataReceived(eventArgs.Data);
                }
            };

            process.Start();

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            return process;
        }

        public Process SpawnNewProcess(string path, string args = null)
        {
            if (OsInfo.IsMono && path.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                args = path + " " + args;
                path = "mono";
            }

            Logger.Info("Starting {0} {1}", path, args);

            var startInfo = new ProcessStartInfo(path, args);
            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            return process;
        }

        public ProcessOutput StartAndCapture(string path, string args = null)
        {
            var output = new ProcessOutput();
            Start(path, args, s => output.Standard.Add(s), error => output.Error.Add(error)).WaitForExit();

            return output;
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
            var processToKill = GetProcessesByName(processName);

            foreach (var processInfo in processToKill)
            {
                Kill(processInfo.Id);
            }
        }

        private static ProcessInfo ConvertToProcessInfo(Process process)
        {
            if (process == null) return null;

            process.Refresh();

            ProcessInfo processInfo = null;

            try
            {
                if (process.Id <= 0) return null;

                processInfo = new ProcessInfo();
                processInfo.Id = process.Id;
                processInfo.Name = process.ProcessName;
                processInfo.StartPath = GetExeFileName(process);

                if (process.HasExited)
                {
                    processInfo = null;
                }
            }
            catch (Win32Exception e)
            {
                Logger.WarnException("Couldn't get process info for " + process.ProcessName, e);
            }

            return processInfo;

        }

        private static string GetExeFileName(Process process)
        {
            if (process.MainModule.FileName != "mono.exe")
            {
                return process.MainModule.FileName;
            }

            return process.Modules.Cast<ProcessModule>().FirstOrDefault(module => module.ModuleName.ToLower().EndsWith(".exe")).FileName;
        }

        public void Kill(int processId)
        {
            var process = Process.GetProcesses().FirstOrDefault(p => p.Id == processId);

            if (process == null)
            {
                Logger.Warn("Cannot find process with id: {0}", processId);
                return;
            }

            process.Refresh();

            if (process.HasExited)
            {
                Logger.Trace("Process has already exited");
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
