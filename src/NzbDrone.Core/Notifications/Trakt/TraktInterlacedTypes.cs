using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Trakt
{
    public static class TraktInterlacedTypes
    {
        private static HashSet<string> _interlacedTypes;

        static TraktInterlacedTypes()
        {
            _interlacedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                              {
                                  "Interlaced", "MBAFF", "PAFF"
                              };
        }

        public static HashSet<string> interlacedTypes => _interlacedTypes;
    }
}
