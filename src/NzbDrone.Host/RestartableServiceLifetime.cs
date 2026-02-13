using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Host
{
    public class RestartableServiceLifetime : IHostLifetime, IDisposable
    {
        private static readonly Lock StaticLock = new();
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(RestartableServiceLifetime));
        private static WindowsServiceWrapper _singletonService;
        private static bool _serviceBaseRunCalled;

        private readonly IHostEnvironment _environment;
        private readonly bool _isWindowsService;
        private readonly ConsoleLifetime _consoleLifetime;
        private readonly TaskCompletionSource<object> _waitForStartTask;
        private readonly ManualResetEventSlim _hostStoppedEvent;

        private CancellationTokenRegistration _stoppedRegistration;
        private bool _isDisposed;

        public IHostApplicationLifetime ApplicationLifetime { get; private set; }
        internal TaskCompletionSource<object> WaitForStartTask => _waitForStartTask;

        public RestartableServiceLifetime(
            IHostEnvironment environment,
            IHostApplicationLifetime applicationLifetime,
            ILoggerFactory loggerFactory,
            IOptions<ConsoleLifetimeOptions> consoleOptions,
            IOptions<HostOptions> hostOptions)
        {
            _environment = environment;
            _isWindowsService = WindowsServiceHelpers.IsWindowsService();
            ApplicationLifetime = applicationLifetime;

            if (_isWindowsService)
            {
                _waitForStartTask = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                _hostStoppedEvent = new ManualResetEventSlim(false);
            }
            else
            {
                _consoleLifetime = new ConsoleLifetime(consoleOptions, environment, applicationLifetime, hostOptions, loggerFactory);
            }
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(RestartableServiceLifetime));
            }

            if (!_isWindowsService)
            {
                return _consoleLifetime.WaitForStartAsync(cancellationToken);
            }

            lock (StaticLock)
            {
                _singletonService ??= new WindowsServiceWrapper(_environment.ApplicationName);

                if (_singletonService.CurrentLifetime != this)
                {
                    Logger.Trace("In-process restart detected, preparing host");
                    _singletonService.PrepareForRestart(this);
                }

                if (!_serviceBaseRunCalled)
                {
                    _serviceBaseRunCalled = true;
                    Logger.Debug("Starting Windows Service");

                    var serviceThread = new Thread(_singletonService.RunServiceBase)
                    {
                        IsBackground = false,
                        Name = "Windows.Service.Lifetime"
                    };
                    serviceThread.Start();
                }
            }

            RegisterStoppedCallback();
            return _waitForStartTask.Task.WaitAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_isDisposed)
            {
                return Task.CompletedTask;
            }

            if (!_isWindowsService)
            {
                return _consoleLifetime.StopAsync(cancellationToken);
            }

            try
            {
                _hostStoppedEvent.Wait(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Logger.Warn("Host stop was cancelled");
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (!_isWindowsService)
            {
                _consoleLifetime?.Dispose();
            }
            else
            {
                _stoppedRegistration.Dispose();
                if (_singletonService?.CurrentLifetime != this)
                {
                    _hostStoppedEvent?.Dispose();
                }
            }
        }

        internal void CleanupForRestart()
        {
            _stoppedRegistration.Dispose();
        }

        internal void PrepareForRestart(IHostApplicationLifetime newApplicationLifetime)
        {
            ApplicationLifetime = newApplicationLifetime;
            _hostStoppedEvent.Reset();
            _waitForStartTask.TrySetResult(null);
        }

        private void OnServiceStart()
        {
            try
            {
                _waitForStartTask.TrySetResult(null);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Service start failed");
                _waitForStartTask.TrySetException(ex);
                throw;
            }
        }

        private void OnServiceStop()
        {
            try
            {
                ApplicationLifetime?.StopApplication();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during service stop");
                throw;
            }
        }

        private void RegisterStoppedCallback()
        {
            _stoppedRegistration = ApplicationLifetime.ApplicationStopped.Register(state => ((RestartableServiceLifetime)state)._hostStoppedEvent.Set(), this);
        }

        private sealed class WindowsServiceWrapper : ServiceBase
        {
            public RestartableServiceLifetime CurrentLifetime { get; private set; }

            public WindowsServiceWrapper(string serviceName)
            {
                ServiceName = serviceName;
            }

            public void PrepareForRestart(RestartableServiceLifetime newLifetime)
            {
                CurrentLifetime?.CleanupForRestart();
                CurrentLifetime = newLifetime;
                newLifetime.PrepareForRestart(newLifetime.ApplicationLifetime);
            }

            public void RunServiceBase()
            {
                try
                {
                    Run(this);
                    CurrentLifetime?.WaitForStartTask.TrySetException(new InvalidOperationException("Service stopped without starting"));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Service base execution failed");
                    CurrentLifetime?.WaitForStartTask.TrySetException(ex);
                }
            }

            protected override void OnStart(string[] args)
            {
                CurrentLifetime?.OnServiceStart();
                base.OnStart(args);
            }

            protected override void OnStop()
            {
                CurrentLifetime?.OnServiceStop();
                base.OnStop();
            }

            protected override void OnShutdown()
            {
                try
                {
                    CurrentLifetime?.ApplicationLifetime?.StopApplication();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error during system shutdown");
                }

                base.OnShutdown();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
            }
        }
    }
}
