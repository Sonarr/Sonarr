using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public class InvalidSceneMappingException : NzbDroneException
    {
        public InvalidSceneMappingException(IEnumerable<SceneMapping> mappings)
            : base(FormatMessage(mappings))
        {

        }

        private static string FormatMessage(IEnumerable<SceneMapping> mappings)
        {
            return string.Format("Scene Mappings contains a conflict for tvdbids {0}, please notify Sonarr developers", mappings.Select(v => v.TvdbId));
        }
    }
}
