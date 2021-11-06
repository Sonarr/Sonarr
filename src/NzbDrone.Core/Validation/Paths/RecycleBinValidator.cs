using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Validation.Paths
{
    public class RecycleBinValidator : PropertyValidator
    {
        private readonly IConfigService _configService;

        public RecycleBinValidator(IConfigService configService)
            : base("Path is {relationship} configured recycle bin folder")
        {
            _configService = configService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var recycleBin = _configService.RecycleBin;
            var folder = context.PropertyValue.ToString();

            if (context.PropertyValue == null || recycleBin.IsNullOrWhiteSpace())
            {
                return true;
            }

            if (recycleBin.PathEquals(folder))
            {
                context.MessageFormatter.AppendArgument("relationship", "set to");

                return false;
            }

            if (recycleBin.IsParentPath(folder))
            {
                context.MessageFormatter.AppendArgument("relationship", "child of");

                return false;
            }

            return true;
        }
    }
}
