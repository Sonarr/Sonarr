using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Download.Clients
{
    public class FolderSettingsValidator : AbstractValidator<FolderSettings>
    {
        public FolderSettingsValidator()
        {
            //Todo: Validate that the path actually exists
            RuleFor(c => c.Folder).IsValidPath();
        }
    }

    public class FolderSettings : IProviderConfig
    {
        private static readonly FolderSettingsValidator Validator = new FolderSettingsValidator();

        [FieldDefinition(0, Label = "Folder", Type = FieldType.Path)]
        public String Folder { get; set; }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
