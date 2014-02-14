using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download.Clients
{
    public class FolderSettingsValidator : AbstractValidator<FolderSettings>
    {
        public FolderSettingsValidator()
        {
            RuleFor(c => c.Folder).NotEmpty();
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
