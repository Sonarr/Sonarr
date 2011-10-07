using System;
using System.Diagnostics;
using System.Threading;
using NLog;

namespace NzbDrone.Providers
{
    public class DebuggerProvider
    {
        private static readonly Logger Logger = LogManager.GetLogger("Host.DebuggerProvider");


        public virtual void Attach()
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                Logger.Info("Trying to attach to debugger");

                int count = 0;

                while (true)
                {
                    try
                    {
                        ProcessAttacher.Attach();
                        Logger.Info("Debugger Attached");
                        return;
                    }
                    catch (Exception e)
                    {
                        count++;
                        if (count > 20)
                        {
                            Logger.WarnException("Unable to attach to debugger", e);
                            return;
                        }

                        Thread.Sleep(100);
                    }
                }
            }
#endif
        }
    }
}