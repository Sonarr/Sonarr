using System;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabSettings : IIndexerSetting
    {
        [FieldDefinition(0, Label = "URL", HelpText = "NewzNab Host Url")]
        public String Url { get; set; }

        [FieldDefinition(1, Label = "API Key", HelpText = "Your API Key")]
        public String ApiKey { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Url);
            }
        }
    }
}