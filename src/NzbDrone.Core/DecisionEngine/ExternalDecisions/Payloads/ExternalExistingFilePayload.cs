using System;
using System.Collections.Generic;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads
{
    public class ExternalExistingFilePayload
    {
        public string Quality { get; set; }
        public long Size { get; set; }
        public List<Language> Languages { get; set; }
        public string RelativePath { get; set; }
        public string ReleaseGroup { get; set; }
        public string SceneName { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
