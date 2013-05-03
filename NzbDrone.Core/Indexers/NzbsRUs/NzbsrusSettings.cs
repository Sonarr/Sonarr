using System;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Indexers.NzbsRUs
{
    public class NzbsrusSetting : IIndexerSetting
    {
        [FieldDefinition(0, Label = "UID", HelpText = "Your NzbsRus User ID")]
        public String Uid { get; set; }

        [FieldDefinition(1, Label = "Hash", HelpText = "Your API Hash Key")]
        public String Hash { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Uid) && !string.IsNullOrWhiteSpace(Hash);
            }
        }
    }
}
