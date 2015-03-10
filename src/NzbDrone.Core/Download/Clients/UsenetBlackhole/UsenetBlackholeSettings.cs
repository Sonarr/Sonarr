using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Download.Clients.UsenetBlackhole
{
    public class UsenetBlackholeSettingsValidator : AbstractValidator<UsenetBlackholeSettings>
    {
        public UsenetBlackholeSettingsValidator()
        {
            RuleFor(c => c.NzbFolder).IsValidPath();
            RuleFor(c => c.WatchFolder).IsValidPath();
        }
    }

    public class UsenetBlackholeSettings : IProviderConfig
    {
        private static readonly UsenetBlackholeSettingsValidator Validator = new UsenetBlackholeSettingsValidator();

        [FieldDefinition(0, Label = "Nzb Folder", Type = FieldType.Path, HelpText = "Folder in which Sonarr will store the .nzb file")]
        public String NzbFolder { get; set; }

        [FieldDefinition(1, Label = "Watch Folder", Type = FieldType.Path, HelpText = "Folder from which Sonarr should import completed downloads")]
        public String WatchFolder { get; set; }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
