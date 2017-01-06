using System;
using SharpRaven.Data;

namespace NzbDrone.Common.Instrumentation.Sentry
{
    public class MachineNameUserFactory : ISentryUserFactory
    {
        public SentryUser Create()
        {
            return new SentryUser(Environment.MachineName);
        }
    }
}