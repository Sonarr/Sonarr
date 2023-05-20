using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Validation.Paths
{
    public class RecycleBinValidator : PropertyValidator
    {
        private readonly IConfigService _configService;

        public RecycleBinValidator(IConfigService configService)
        {
            _configService = configService;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' is {relationship} configured recycle bin folder";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var recycleBin = _configService.RecycleBin;

            if (context.PropertyValue == null || recycleBin.IsNullOrWhiteSpace())
            {
                return true;
            }

            var folder = context.PropertyValue.ToString();
            context.MessageFormatter.AppendArgument("path", folder);

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
