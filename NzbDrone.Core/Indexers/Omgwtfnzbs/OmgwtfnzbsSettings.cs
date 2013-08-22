using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsSetting : AbstractValidator<OmgwtfnzbsSetting>, IIndexerSetting
    {
        public OmgwtfnzbsSetting()
        {
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.ApiKey).NotEmpty();
        }

        [FieldDefinition(0, Label = "Username")]
        public String Username { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public String ApiKey { get; set; }


        public ValidationResult Validate()
        {
            return Validate(this);
        }
    }
}
