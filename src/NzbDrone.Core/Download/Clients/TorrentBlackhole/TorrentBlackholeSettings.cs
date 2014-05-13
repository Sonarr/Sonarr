using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Download.Clients.TorrentBlackhole
{
    public class TorrentBlackholeSettingsValidator : AbstractValidator<TorrentBlackholeSettings>
    {
        public TorrentBlackholeSettingsValidator()
        {
            //Todo: Validate that the path actually exists
            RuleFor(c => c.TorrentFolder).IsValidPath();
        }
    }

    public class TorrentBlackholeSettings : IProviderConfig
    {
        private static readonly TorrentBlackholeSettingsValidator validator = new TorrentBlackholeSettingsValidator();

        [FieldDefinition(0, Label = "Torrent Folder", Type = FieldType.Path)]
        public String TorrentFolder { get; set; }

        [FieldDefinition(1, Label = "Watch Folder", Type = FieldType.Path)]
        public String WatchFolder { get; set; }

        public ValidationResult Validate()
        {
            return validator.Validate(this);
        }
    }
}
