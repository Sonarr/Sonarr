using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Languages
{
    public class LanguagesBelowCutoff
    {
        public int ProfileId { get; set; }
        public IEnumerable<int> LanguageIds { get; set; }

        public LanguagesBelowCutoff(int profileId, IEnumerable<int> languageIds)
        {
            ProfileId = profileId;
            LanguageIds = languageIds;
        }
    }
}
