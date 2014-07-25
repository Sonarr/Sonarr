using System;

namespace NzbDrone.Common.Processes
{
    public interface IRuntimeProvider
    {
        String GetVersion();
    }
}
