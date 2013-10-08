using System;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class ProwlSettings : IProviderConfig
    {
        [FieldDefinition(0, Label = "API Key", HelpLink = "https://www.prowlapp.com/api_settings.php")]
        public String ApiKey { get; set; }

        [FieldDefinition(1, Label = "Priority", Type = FieldType.Select, SelectOptions= typeof(ProwlPriority) )]
        public Int32 Priority { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ApiKey) && Priority != null & Priority >= -2 && Priority <= 2;
            }
        }

        public ValidationResult Validate()
        {
            throw new NotImplementedException();
        }
    }
}
