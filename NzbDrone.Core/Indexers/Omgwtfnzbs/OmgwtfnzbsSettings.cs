using System;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsSetting : IIndexerSetting
    {
        public String Username { get; set; }
        public String ApiKey { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(ApiKey);
            }
        }
    }
}
