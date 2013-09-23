using System;
using NLog;
using NzbDrone.Common.Processes;

namespace NzbDrone.Host.AccessControl
{
    public interface INetshProvider
    {
        ProcessOutput Run(string arguments);
    }

    public class NetshProvider : INetshProvider
    {
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public NetshProvider(IProcessProvider processProvider, Logger logger)
        {
            _processProvider = processProvider;
            _logger = logger;
        }

        public ProcessOutput Run(string arguments)
        {
            try
            {
                var output = _processProvider.StartAndCapture("netsh.exe", arguments);

                return output;
            }
            catch (Exception ex)
            {
                _logger.WarnException("Error executing netsh with arguments: " + arguments, ex);
            }

            return null;
        }
    }
}
