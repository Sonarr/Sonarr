using System;

namespace NzbDrone.Core.Indexers.FileSharingTalk
{
    public class FileSharingTalkSetting : IIndexerSetting
    {
        public String Uid { get; set; }
        public String Secret { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Uid) && !string.IsNullOrWhiteSpace(Secret);
            }
        }
    }
}
