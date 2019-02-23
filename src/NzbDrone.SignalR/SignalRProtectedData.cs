using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using NzbDrone.Common.EnvironmentInfo;
using System;
using System.Text;

namespace NzbDrone.SignalR
{
    // This class uses a per-startup key instead of the persistent keystore.
    public class SignalRProtectedData : IProtectedData
    {
        private static readonly UTF8Encoding _encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        
        public static void Register()
        {
            // On mono we're not guaranteed to have a DSAPI keystore, and we don't really need it, so use an alternate ProtectedData method.
            if (OsInfo.IsNotWindows)
            {
                GlobalHost.DependencyResolver.Register(typeof(IProtectedData), () => new SignalRProtectedData());
            }
        }

        public string Protect(string data, string purpose)
        {
            byte[] purposeBytes = _encoding.GetBytes(purpose);

            byte[] unprotectedBytes = _encoding.GetBytes(data);

            byte[] protectedBytes = NonPersistentManagedProtection.Protect(unprotectedBytes, purposeBytes);

            return Convert.ToBase64String(protectedBytes);
        }

        public string Unprotect(string protectedValue, string purpose)
        {
            byte[] purposeBytes = _encoding.GetBytes(purpose);

            byte[] protectedBytes = Convert.FromBase64String(protectedValue);

            byte[] unprotectedBytes = NonPersistentManagedProtection.Unprotect(protectedBytes, purposeBytes);

            return _encoding.GetString(unprotectedBytes);
        }
    }
}
