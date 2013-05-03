using System;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsSetting : IIndexerSetting
    {
        [FieldDefinition(0, Label = "Username", HelpText = "Your Username")]
        public String Username { get; set; }

        [FieldDefinition(1, Label = "API Key", HelpText = "Your API Key")]
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
