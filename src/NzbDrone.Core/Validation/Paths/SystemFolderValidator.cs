using System;
using FluentValidation.Validators;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Validation.Paths
{
    public class SystemFolderValidator : PropertyValidator
    {
        public SystemFolderValidator()
            : base("Is {relationship} system folder {systemFolder}")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var folder = context.PropertyValue.ToString();

            if (OsInfo.IsWindows)
            {
                var windowsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                context.MessageFormatter.AppendArgument("systemFolder", windowsFolder);

                if (windowsFolder.PathEquals(folder))
                {
                    context.MessageFormatter.AppendArgument("relationship", "set to");

                    return false;
                }

                if (windowsFolder.IsParentPath(folder))
                {
                    context.MessageFormatter.AppendArgument("relationship", "child of");

                    return false;
                }
            }
            else if (OsInfo.IsOsx)
            {
                var systemFolder = "/System";
                context.MessageFormatter.AppendArgument("systemFolder", systemFolder);

                if (systemFolder.PathEquals(folder))
                {
                    context.MessageFormatter.AppendArgument("relationship", "child of");

                    return false;
                }

                if (systemFolder.IsParentPath(folder))
                {
                    context.MessageFormatter.AppendArgument("relationship", "child of");

                    return false;
                }
            }
            else
            {
                var folders = new[]
                              {
                                  "/bin",
                                  "/boot",
                                  "/lib",
                                  "/sbin",
                                  "/proc"
                              };

                foreach (var f in folders)
                {
                    context.MessageFormatter.AppendArgument("systemFolder", f);

                    if (f.PathEquals(folder))
                    {
                        context.MessageFormatter.AppendArgument("relationship", "child of");

                        return false;
                    }

                    if (f.IsParentPath(folder))
                    {
                        context.MessageFormatter.AppendArgument("relationship", "child of");

                        return false;
                    }
                }
            }

            return true;
        }
    }
}
