using System;
using System.Collections;
using System.Collections.Generic;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabSettings : IIndexerSetting
    {
        public NewznabSettings()
        {
            Categories = new [] { 5030, 5040 };
            UseRageTvId = true;
        }

        [FieldDefinition(0, Label = "URL")]
        public String Url { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public String ApiKey { get; set; }

        public IEnumerable<Int32> Categories { get; set; }

        public bool UseRageTvId { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Url);
            }
        }
    }
}