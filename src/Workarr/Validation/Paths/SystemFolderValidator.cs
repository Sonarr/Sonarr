using FluentValidation.Validators;
using Workarr.Disk;
using Workarr.Extensions;

namespace Workarr.Validation.Paths
{
    public class SystemFolderValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Path '{path}' is {relationship} system folder {systemFolder}";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var folder = context.PropertyValue.ToString();
            context.MessageFormatter.AppendArgument("path", folder);

            foreach (var systemFolder in SystemFolders.GetSystemFolders())
            {
                context.MessageFormatter.AppendArgument("systemFolder", systemFolder);

                if (systemFolder.PathEquals(folder))
                {
                    context.MessageFormatter.AppendArgument("relationship", "set to");

                    return false;
                }

                if (systemFolder.IsParentPath(folder))
                {
                    context.MessageFormatter.AppendArgument("relationship", "child of");

                    return false;
                }
            }

            return true;
        }
    }
}
