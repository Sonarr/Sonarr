using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using FluentValidation.Results;

namespace NzbDrone.Core.Validation
{
    public class NzbDroneValidationState
    {
        public static NzbDroneValidationState Warning = new NzbDroneValidationState { IsWarning = true };

        public bool IsWarning { get; set; }
    }
}
