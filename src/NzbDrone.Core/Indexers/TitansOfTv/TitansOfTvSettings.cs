using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.TitansOfTv
{

       public class TitansOfTvSettingsValidator : AbstractValidator<TitansOfTvSettings>
    {
  
    }

    public class TitansOfTvSettings : IProviderConfig
    {
     private static readonly TitansOfTvSettingsValidator Validator = new TitansOfTvSettingsValidator();

     public TitansOfTvSettings()
        {
            ApiKey = "";
        }

        [FieldDefinition(0, Label = "API key", HelpText = "Enter your ToTV API key.", HelpLink = "https://titansof.tv/user/edit#api")]
        public String ApiKey { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
