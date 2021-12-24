using System;

namespace NzbDrone.Core.Notifications
{
    public class ApplicationUpdateMessage
    {
        public string Message { get; set; }
        public Version PreviousVersion { get; set; }
        public Version NewVersion { get; set; }

        public override string ToString()
        {
            return NewVersion.ToString();
        }
    }
}
