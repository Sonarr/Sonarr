using System;
using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsSettings : IIndexerSetting
    {
//        public OmgwtfnzbsSettings()
//        {
//            RuleFor(c => c.Username).NotEmpty();
//            RuleFor(c => c.ApiKey).NotEmpty();
//        }

        [FieldDefinition(0, Label = "Username")]
        public String Username { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public String ApiKey { get; set; }

        public ValidationResult Validate()
        {
            return new ValidationResult();
            //return Validate(this);
        }
    }
}
