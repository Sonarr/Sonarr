using System;

namespace NzbDrone.Core.Indexers.NzbsRUs
{
    public class NzbsrusSetting : IIndexerSetting
    {
        public String Uid { get; set; }
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
