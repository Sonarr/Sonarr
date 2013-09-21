using System;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexServerSettings : IProviderConfig
    {
        public PlexServerSettings()
        {
            Port = 32400;
        }

        [FieldDefinition(0, Label = "Host")]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Update Library", Type = FieldType.Checkbox)]
        public Boolean UpdateLibrary { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Host);
            }
        }

        public ValidationResult Validate()
        {
            throw new NotImplementedException();
        }
    }
}
