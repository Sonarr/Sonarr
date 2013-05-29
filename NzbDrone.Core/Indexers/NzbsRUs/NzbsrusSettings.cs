using System;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Indexers.NzbsRUs
{
    public class NzbsrusSetting : IIndexerSetting
    {
        [FieldDefinition(0, Label = "UID")]
        public String Uid { get; set; }

        [FieldDefinition(1, Label = "Hash")]
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
