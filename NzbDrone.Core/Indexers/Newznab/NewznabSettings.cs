using System;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabSettings : IIndexerSetting
    {
        public String Url { get; set; }
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