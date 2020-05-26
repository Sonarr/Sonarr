using System;
using FluentValidation;
using Mono.Unix.Native;

namespace NzbDrone.Core.Validation
{
    public static class FileChmodValidator
    {
        public static IRuleBuilderOptions<T, string> ValidFileChmod<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(x =>
            {
                try
                {
                    var permissions = NativeConvert.FromOctalPermissionString(x);
                    return (permissions & (FilePermissions.S_ISGID | FilePermissions.S_ISUID | FilePermissions.S_ISVTX |
                                           FilePermissions.S_IXUSR | FilePermissions.S_IXGRP |
                                           FilePermissions.S_IXOTH)) == 0;
                }
                catch (FormatException)
                {
                    return false;
                }
            }).WithMessage("Must contain a valid octal of Unix permissions");
        }
    }
}