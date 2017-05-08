using System.Collections.Generic;

namespace NzbDrone.Common.Disk
{
    public class MountOptions
    {
        private readonly Dictionary<string, string> _options;

        public MountOptions(Dictionary<string, string> options)
        {
            _options = options;
        }

        public bool IsReadOnly => _options.ContainsKey("ro");
    }
}
