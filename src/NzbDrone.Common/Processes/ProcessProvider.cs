using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
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
        bool Exists(int processId);
        bool Exists(string processName);
        ProcessPriorityClass GetCurrentProcessPriority();
        Process Start(string path, IEnumerable<string> args = null, StringDictionary environmentVariables = null, Action<string> onOutputDataReceived = null, Action<string> onErrorDataReceived = null);
        Process SpawnNewProcess(string path, IEnumerable<string> args = null, StringDictionary environmentVariables = null, bool noWindow = false);
        ProcessOutput StartAndCapture(string path, IEnumerable<string> args = null, StringDictionary environmentVariables = null);
        List<string> ParseCommandLineArguments(string commandLine);
    }

    public class ProcessProvider : IProcessProvider
    {
        private readonly Logger _logger;

        public const string SONARR_PROCESS_NAME = "Sonarr";
        public const string SONARR_CONSOLE_PROCESS_NAME = "Sonarr.Console";

        public ProcessProvider(Logger logger)
        {
            _logger = logger;
        }

        public static int GetCurrentProcessId()
        {
            return Environment.ProcessId;
        }

        public ProcessInfo GetCurrentProcess()
        {
            return ConvertToProcessInfo(Process.GetCurrentProcess());
        }

        public bool Exists(int processId)
        {
            return GetProcessById(processId) != null;
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
            _logger.Debug("Finding process with Id:{0}", id);

            var processInfo = ConvertToProcessInfo(Process.GetProcesses().FirstOrDefault(p => p.Id == id));

            if (processInfo == null)
            {
                _logger.Warn("Unable to find process with ID {0}", id);
            }
            else
            {
                _logger.Debug("Found process {0}", processInfo.ToString());
            }

            return processInfo;
        }

        public List<ProcessInfo> FindProcessByName(string name)
        {
            return GetProcessesByName(name).Select(ConvertToProcessInfo).Where(c => c != null).ToList();
        }

        public void OpenDefaultBrowser(string url)
        {
            _logger.Info("Opening URL [{0}]", url);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                }
            };

            process.Start();
        }

        public Process Start(string path, IEnumerable<string> args = null, StringDictionary environmentVariables = null, Action<string> onOutputDataReceived = null, Action<string> onErrorDataReceived = null)
        {
            (path, args) = GetPathAndArgs(path, args);

            var logger = LogManager.GetLogger(new FileInfo(path).Name);

            var startInfo = new ProcessStartInfo(path)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            if (args != null)
            {
                foreach (var arg in args)
                {
                    startInfo.ArgumentList.Add(arg);
                }
            }

            if (environmentVariables != null)
            {
                foreach (DictionaryEntry environmentVariable in environmentVariables)
                {
                    try
                    {
                        _logger.Trace("Setting environment variable '{0}' to '{1}'", environmentVariable.Key, environmentVariable.Value);

                        var key = environmentVariable.Key.ToString();
                        var value = environmentVariable.Value?.ToString();

                        startInfo.EnvironmentVariables[key] = value;
                    }
                    catch (Exception e)
                    {
                        if (environmentVariable.Value == null)
                        {
                            _logger.Error(e, "Unable to set environment variable '{0}', value is null", environmentVariable.Key);
                        }
                        else
                        {
                            _logger.Error(e, "Unable to set environment variable '{0}'", environmentVariable.Key);
                        }

                        throw;
                    }
                }
            }

            logger.Debug("Starting {0} {1}", path, args);

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.OutputDataReceived += (sender, eventArgs) =>
            {
                if (string.IsNullOrWhiteSpace(eventArgs.Data))
                {
                    return;
                }

                logger.Debug(eventArgs.Data);

                if (onOutputDataReceived != null)
                {
                    onOutputDataReceived(eventArgs.Data);
                }
            };

            process.ErrorDataReceived += (sender, eventArgs) =>
            {
                if (string.IsNullOrWhiteSpace(eventArgs.Data))
                {
                    return;
                }

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

        public Process SpawnNewProcess(string path, IEnumerable<string> args = null, StringDictionary environmentVariables = null, bool noWindow = false)
        {
            (path, args) = GetPathAndArgs(path, args);

            _logger.Debug("Starting {0} {1}", path, args);

            var startInfo = new ProcessStartInfo(path, args);
            startInfo.CreateNoWindow = noWindow;
            startInfo.UseShellExecute = !noWindow;

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            return process;
        }

        public ProcessOutput StartAndCapture(string path, IEnumerable<string> args = null, StringDictionary environmentVariables = null)
        {
            var output = new ProcessOutput();
            var process = Start(path,
                                args,
                                environmentVariables,
                                s => output.Lines.Add(new ProcessOutputLine(ProcessOutputLevel.Standard, s)),
                                error => output.Lines.Add(new ProcessOutputLine(ProcessOutputLevel.Error, error)));

            process.WaitForExit();
            output.ExitCode = process.ExitCode;

            return output;
        }

        public void WaitForExit(Process process)
        {
            _logger.Debug("Waiting for process {0} to exit.", process.ProcessName);

            process.WaitForExit();
        }

        public void SetPriority(int processId, ProcessPriorityClass priority)
        {
            var process = Process.GetProcessById(processId);

            _logger.Info("Updating [{0}] process priority from {1} to {2}",
                        process.ProcessName,
                        process.PriorityClass,
                        priority);

            process.PriorityClass = priority;
        }

        public void Kill(int processId)
        {
            var process = Process.GetProcesses().FirstOrDefault(p => p.Id == processId);

            if (process == null)
            {
                _logger.Warn("Cannot find process with id: {0}", processId);
                return;
            }

            process.Refresh();

            if (process.Id != GetCurrentProcessId() && process.HasExited)
            {
                _logger.Debug("Process has already exited");
                return;
            }

            _logger.Info("[{0}]: Killing process", process.Id);
            process.Kill();
            _logger.Info("[{0}]: Waiting for exit", process.Id);
            process.WaitForExit();
            _logger.Info("[{0}]: Process terminated successfully", process.Id);
        }

        public void KillAll(string processName)
        {
            var processes = GetProcessesByName(processName);

            _logger.Debug("Found {0} processes to kill", processes.Count);

            foreach (var processInfo in processes)
            {
                if (processInfo.Id == GetCurrentProcessId())
                {
                    _logger.Debug("Tried killing own process, skipping: {0} [{1}]", processInfo.Id, processInfo.ProcessName);
                    continue;
                }

                _logger.Debug("Killing process: {0} [{1}]", processInfo.Id, processInfo.ProcessName);
                Kill(processInfo.Id);
            }
        }

        public List<string> ParseCommandLineArguments(string commandLine)
        {
            var args = new List<string>();
            if (string.IsNullOrWhiteSpace(commandLine))
            {
                return args;
            }

            var inQuotes = false;
            var currentArg = new System.Text.StringBuilder();

            for (var i = 0; i < commandLine.Length; i++)
            {
                var c = commandLine[i];

                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (currentArg.Length > 0)
                    {
                        args.Add(currentArg.ToString());
                        currentArg.Clear();
                    }
                }
                else
                {
                    currentArg.Append(c);
                }
            }

            if (currentArg.Length > 0)
            {
                args.Add(currentArg.ToString());
            }

            return args;
        }

        private ProcessInfo ConvertToProcessInfo(Process process)
        {
            if (process == null)
            {
                return null;
            }

            process.Refresh();

            ProcessInfo processInfo = null;

            try
            {
                if (process.Id <= 0)
                {
                    return null;
                }

                processInfo = new ProcessInfo();
                processInfo.Id = process.Id;
                processInfo.Name = process.ProcessName;
                processInfo.StartPath = process.MainModule?.FileName;

                if (process.Id != GetCurrentProcessId() && process.HasExited)
                {
                    processInfo = null;
                }
            }
            catch (Win32Exception e)
            {
                _logger.Warn(e, "Couldn't get process info for " + process.ProcessName);
            }

            return processInfo;
        }

        private List<Process> GetProcessesByName(string name)
        {
            var processes = Process.GetProcessesByName(name).ToList();

            _logger.Debug("Found {0} processes with the name: {1}", processes.Count, name);

            try
            {
                foreach (var process in processes)
                {
                    _logger.Debug(" - [{0}] {1}", process.Id, process.ProcessName);
                }
            }
            catch
            {
                // Don't crash on getting some log data.
            }

            return processes;
        }

        private (string Path, IEnumerable<string> Args) GetPathAndArgs(string path, IEnumerable<string> args)
        {
            var argList = args?.ToList() ?? new List<string>();

            if (OsInfo.IsWindows && path.EndsWith(".bat", StringComparison.InvariantCultureIgnoreCase))
            {
                var newArgs = new List<string> { "/c", path };
                newArgs.AddRange(argList);
                return ("cmd.exe", newArgs);
            }

            if (OsInfo.IsWindows && path.EndsWith(".ps1", StringComparison.InvariantCultureIgnoreCase))
            {
                var newArgs = new List<string> { "-ExecutionPolicy", "Bypass", "-NoProfile", "-File", path };
                newArgs.AddRange(argList);
                return ("powershell.exe", newArgs);
            }

            if (OsInfo.IsWindows && path.EndsWith(".py", StringComparison.InvariantCultureIgnoreCase))
            {
                var newArgs = new List<string> { path };
                newArgs.AddRange(argList);
                return ("python.exe", newArgs);
            }

            return (path, args);
        }
    }
}
