using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using NLog;
using NzbDrone.Common.Processes;

namespace NzbDrone.Mono
{
    public class MonoProcessProvider : ProcessProviderBase
    {
        private readonly Logger _logger;

        public MonoProcessProvider(Logger logger)
            :base(logger)
        {
            _logger = logger;
        }
        
        public override Process Start(string path, string args = null, StringDictionary environmentVariables = null, System.Action<string> onOutputDataReceived = null, System.Action<string> onErrorDataReceived = null)
        {
            if (path.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                args = GetMonoArgs(path, args);
                path = "mono";
            }
            
            return base.Start(path, args, environmentVariables, onOutputDataReceived, onErrorDataReceived);
        }
        
        public override Process SpawnNewProcess(string path, string args = null, StringDictionary environmentVariables = null)
        {
            if (path.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                args = GetMonoArgs(path, args);
                path = "mono";
            }
            
            return base.SpawnNewProcess(path, args, environmentVariables);
        }
        
        protected new string GetExeFileName(Process process)
        {
            return process.Modules.Cast<ProcessModule>().FirstOrDefault(module => module.ModuleName.ToLower().EndsWith(".exe")).FileName;
        }
        
        protected new List<Process> GetProcessesByName(string name)
        {
            var processes = Process.GetProcessesByName("mono")
                                   .Union(Process.GetProcessesByName("mono-sgen"))
                                   .Where(process =>
                                          process.Modules.Cast<ProcessModule>()
                                                 .Any(module =>
                                                      module.ModuleName.ToLower() == name.ToLower() + ".exe")).ToList();

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
                // Don't crash on gettings some log data.
            }

            return processes;
        }
        
        private string GetMonoArgs(string path, string args)
        {
            return string.Format("--debug {0} {1}", path, args);
        }
    }
}

